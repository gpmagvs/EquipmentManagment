using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EquipmentManagment.Connection;
using EquipmentManagment.Device.Options;
using EquipmentManagment.Exceptions;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.Tool;
using Modbus.Device;
using Newtonsoft.Json.Converters;

namespace EquipmentManagment.Device
{
    public abstract partial class EndPointDeviceAbstract : IDisposable
    {
        public static event EventHandler<EndPointDeviceAbstract> OnEQDisconnected;
        public static event EventHandler<EndPointDeviceAbstract> OnEQConnected;
        /// <summary>
        /// 數據長度不足
        /// </summary>
        public static event EventHandler<EndPointDeviceAbstract> OnEQInputDataSizeNotEnough;
        public EndPointDeviceAbstract(clsEndPointOptions options)
        {
            EndPointOptions = options;
        }
        public string EQName => EndPointOptions.Name;

        public TcpClient tcp_client { get; private set; }
        protected ModbusIpMaster master;
        protected SerialPort serial;
        public CIMComponent.clsMC_EthIF McInterface { get; private set; } = new CIMComponent.clsMC_EthIF();

        public clsEndPointOptions EndPointOptions { get; set; } = new clsEndPointOptions();

        /// <summary>
        /// 下游設備
        /// </summary>
        public List<clsEQ> DownstremEQ
        {
            get
            {
                return StaEQPManagager.MainEQList.FindAll(eq => EndPointOptions.ValidDownStreamEndPointNames.Contains(eq.EQName));
            }
        }
        private CONN_METHODS _ConnectionMethod => EndPointOptions.ConnOptions.ConnMethod;
        public abstract PortStatusAbstract PortStatus { get; set; }

        protected bool _IsConnected;
        public virtual bool IsConnected
        {
            get => _IsConnected;
            set
            {
                if (_IsConnected != value)
                {
                    _IsConnected = value;
                    if (!_IsConnected)
                    {
                        OnEQDisconnected?.Invoke(this, this);
                    }
                    else
                        OnEQConnected?.Invoke(this, this);
                }
            }
        }

        public List<bool> InputBuffer = new List<bool>();
        public List<byte> DataBuffer { get; protected set; } = new List<byte>();

        private bool disposedValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="Port"></param>
        /// <returns></returns>
        public async Task<bool> Connect(bool use_for_conn_test = false, bool retry = false)
        {
            await Task.Delay(1);

            if (_ConnectionMethod == CONN_METHODS.MODBUS_TCP)
                IsConnected = ModbusTcpConnect(EndPointOptions.ConnOptions.IP, EndPointOptions.ConnOptions.Port);
            else if (_ConnectionMethod == CONN_METHODS.TCPIP)
            {
                IsConnected = await TCPIPConnect(EndPointOptions.ConnOptions.IP, EndPointOptions.ConnOptions.Port);
            }
            else if (_ConnectionMethod == CONN_METHODS.MODBUS_RTU | _ConnectionMethod == CONN_METHODS.SERIAL_PORT)
                IsConnected = await SerialPortConnect(EndPointOptions.ConnOptions.ComPort);

            else if (_ConnectionMethod == CONN_METHODS.MC)
            {
                IsConnected = await MCProtocolConnect(EndPointOptions.ConnOptions.IP, EndPointOptions.ConnOptions.Port);
            }
            if (IsConnected)
            {
                if (!use_for_conn_test)
                    StartSyncData();
            }
            else
            {
                if (!retry)
                    OnEQDisconnected?.Invoke(this, this);
                if (!use_for_conn_test)
                    _StartRetry();
            }
            return IsConnected;
        }

        private async Task _StartRetry()
        {
            await Task.Delay(1000);
            await Connect(retry: true);
        }

        /// <summary>
        /// 建立TCP連線
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="Port"></param>
        /// <returns></returns>
        protected virtual bool ModbusTcpConnect(string IP, int Port)
        {
            try
            {
                tcp_client = new TcpClient(IP, Port);
                master = ModbusIpMaster.CreateIp(tcp_client);
                master.Transport.WaitToRetryMilliseconds = 200;
                master.Transport.RetryOnOldResponseThreshold = 10;
                master.Transport.ReadTimeout = 300;
                master.Transport.WriteTimeout = 1000;
                master.Transport.Retries = 3;
                return true;
            }
            catch (Exception ex)
            {
                master = null;
                return false;
            }
        }
        private async Task<bool> TCPIPConnect(string IP, int Port)
        {
            try
            {
                tcp_client = new TcpClient();
                tcp_client.ReceiveTimeout = 15000;
                await tcp_client.ConnectAsync(IP, Port);
                return tcp_client.Connected;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> MCProtocolConnect(string IP, int Port)
        {
            try
            {
                int ret_code = McInterface.Open(IP, Port, 5000, 5000);
                return ret_code == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 使用Modbus RTU
        /// </summary>
        /// <param name="comport"></param>
        /// <returns></returns>
        protected virtual async Task<bool> SerialPortConnect(string comport, int baudrate = 115200)
        {
            try
            {
                serial.Close();
            }
            catch (Exception ex)
            {
            }

            try
            {
                serial = new SerialPort(comport, baudrate);
                serial.Open();
                return serial.IsOpen;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public virtual void StartSyncData()
        {

            Thread thread = new Thread(async () =>
            {
                while (true)
                {
                    Thread.Sleep(300);
                    try
                    {
                        if (!_IsConnected)
                        {
                            await Connect();
                            continue;
                        }

                        if (_ConnectionMethod == CONN_METHODS.MODBUS_TCP)
                        {
                            ReadInputsUseModbusTCP();
                        }
                        if (_ConnectionMethod == CONN_METHODS.TCPIP)
                        {
                            ReadInputsUseTCPIP();
                        }
                        if (_ConnectionMethod == CONN_METHODS.MC)
                        {
                            ReadDataUseMCProtocol();
                        }
                        if (_ConnectionMethod == CONN_METHODS.SERIAL_PORT)
                            ReadDataUseSerial();

                        if (InputBuffer.Count > 0 || DataBuffer.Count > 0)
                            InputsHandler();

                    }
                    catch (ModbusReadInputException ex)
                    {
                        _IsConnected = false;
                        Console.WriteLine(ex.Message);
                        continue;
                    }
                    catch (ChargeStationNoResponseException ex)
                    {
                        //若觸發這個例外，表示充電站沒AGV在充電.
                        await Task.Delay(5000);
                        Console.WriteLine($"Charge Station No Charging action...");
                        continue;
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        Console.WriteLine($"{EndPointOptions.Name}- Error:Out of Range(Current InputBuffer Size:{InputBuffer.Count}/ DataBuffer Size:{this.DataBuffer.Count} )");
                        await Task.Delay(1000);
                        OnEQInputDataSizeNotEnough?.Invoke(this, this);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        IsConnected = false;
                        await Task.Delay(3000);
                        continue;
                    }
                }

            });
            thread.Start();
        }

        protected virtual void ReadDataUseSerial() { }


        /// <summary>
        /// 使用MC Interface將PLC Data讀回
        /// </summary>
        private void ReadDataUseMCProtocol()
        {
            McInterface.ReadBit(ref EQPLCMemoryTb, PLCMemOption.EQPBitAreaName, PLCMemOption.EQPBitStartAddressName, PLCMemOption.EQP_Bit_Size);
            McInterface.ReadWord(ref EQPLCMemoryTb, PLCMemOption.EQPWordAreaName, PLCMemOption.EQPWordStartAddressName, PLCMemOption.EQP_Word_Size);
        }

        protected virtual void ReadInputsUseTCPIP()
        {
            throw new NotImplementedException();
        }
        protected virtual void ReadInputsUseModbusTCP()
        {
            try
            {
                ushort startRegister = EndPointOptions.ConnOptions.Input_StartRegister;
                ushort registerNum = EndPointOptions.ConnOptions.Input_RegisterNum;

                if (EndPointOptions.ConnOptions.IO_Value_Type == IO_VALUE_TYPE.INPUT)
                {
                    var inputs = master.ReadInputs(startRegister, registerNum);
                    if (!inputs.SequenceEqual(InputBuffer))
                        InputBuffer = inputs.ToList();
                }
                else
                {
                    try
                    {
                        ushort[] input_registers = master.ReadInputRegisters(0, 1);
                        InputBuffer = input_registers[0].GetBoolArray().ToList();
                    }
                    catch (Exception ex)
                    {
                        throw new ModbusReadInputException(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void WriteInputsUseModbusTCP(bool[] outputs)
        {
            try
            {
                var IO_Module_Brand = EndPointOptions.ConnOptions.IO_Value_Type;
                if (IO_Module_Brand == IO_VALUE_TYPE.INPUT)
                    master.WriteMultipleCoils(1, outputs);

                if (IO_Module_Brand == IO_VALUE_TYPE.INPUT_REGISTER)
                {
                    ushort WriteInValue = outputs.GetUshort();
                    master.WriteSingleRegister(0, WriteInValue); //EasyModbus從0開始計算
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WriteInputsUseModbusTCP Fail... {ex.Message}");
            }

        }

        protected abstract void WriteOutuptsData();
        protected abstract void InputsHandler();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                }
                master?.Transport.Dispose();
                master?.Dispose();
                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        public class IOChangedEventArgs : EventArgs
        {
            public IOChangedEventArgs(EndPointDeviceAbstract Device, string IOName, bool IOState)
            {
                this.IOName = IOName;
                this.Device = Device;
                this.IOState = IOState;
            }
            public EndPointDeviceAbstract Device { get; }
            public string IOName { get; }
            public bool IOState { get; }
        }
    }

}
