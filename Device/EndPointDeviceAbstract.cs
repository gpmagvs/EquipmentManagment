using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EquipmentManagment.Connection;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.Tool;
using Modbus.Device;

namespace EquipmentManagment.Device
{
    public abstract class EndPointDeviceAbstract : IDisposable
    {
        public EndPointDeviceAbstract(clsEndPointOptions options)
        {
            EndPointOptions = options;
        }
        public string EQName => EndPointOptions.Name;

        public TcpClient tcp_client { get; private set; }
        protected ModbusIpMaster master;
        public CIMComponent.clsMC_EthIF McInterface { get; private set; } = new CIMComponent.clsMC_EthIF();

        public clsEndPointOptions EndPointOptions { get; set; } = new clsEndPointOptions();

        /// <summary>
        /// 下游設備
        /// </summary>
        public List<clsEQ> DownstremEQ
        {
            get
            {
                return StaEQPManagager.EQList.FindAll(eq => EndPointOptions.ValidDownStreamEndPointNames.Contains(eq.EQName));
            }
        }
        private CONN_METHODS _ConnectionMethod => EndPointOptions.ConnOptions.ConnMethod;
        public abstract PortStatusAbstract PortStatus { get; set; }

        protected bool _IsConnected;
        public virtual bool IsConnected
        {
            get => _IsConnected;
            set => _IsConnected = value;
        }

        public List<bool> InputBuffer = new List<bool>();
        public List<byte> TcpDataBuffer { get; protected set; } = new List<byte>();

        private bool disposedValue;

        /// <summary>
        /// 使用Modbus Tcp 連線
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="Port"></param>
        /// <returns></returns>
        public async Task<bool> Connect(bool use_for_conn_test = false)
        {
            await Task.Delay(1);

            bool connected = false;
            if (_ConnectionMethod == CONN_METHODS.MODBUS_TCP)
                connected = await ModbusTcpConnect(EndPointOptions.ConnOptions.IP, EndPointOptions.ConnOptions.Port);
            else if (_ConnectionMethod == CONN_METHODS.TCPIP)
            {
                connected = await TCPIPConnect(EndPointOptions.ConnOptions.IP, EndPointOptions.ConnOptions.Port);
            }
            else if (_ConnectionMethod == CONN_METHODS.MODBUS_RTU)
                connected = await SerialPortConnect(EndPointOptions.ConnOptions.ComPort);

            else if (_ConnectionMethod == CONN_METHODS.MC)
            {
                connected = await MCProtocolConnect(EndPointOptions.ConnOptions.IP, EndPointOptions.ConnOptions.Port);
            }
            if (connected)
            {
                IsConnected = true;
                if (!use_for_conn_test)
                    StartSyncData();
                return IsConnected;
            }
            else
            {
                IsConnected = false;
                if (!use_for_conn_test)
                    _StartRetry();
                return IsConnected;
            }
        }

        private async Task _StartRetry()
        {
            await Connect();
        }

        /// <summary>
        /// 建立TCP連線
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="Port"></param>
        /// <returns></returns>
        protected virtual async Task<bool> ModbusTcpConnect(string IP, int Port)
        {
            try
            {
                tcp_client = new TcpClient(IP, Port);
                master = ModbusIpMaster.CreateIp(tcp_client);
                master.Transport.ReadTimeout = 5000;
                master.Transport.WriteTimeout = 5000;
                master.Transport.Retries = 10;
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
        protected virtual async Task<bool> SerialPortConnect(string comport)
        {
            return false;
        }

        public void StartSyncData()
        {
            Thread thread = new Thread(async () =>
            {
                try
                {
                    while (IsConnected)
                    {
                        Thread.Sleep(10);
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
                        DefineInputData();
                    }
                }
                catch (NullReferenceException ex)
                {
                    IsConnected = false;
                    _StartRetry();
                }
                catch (Exception ex)
                {
                    IsConnected = false;
                    _StartRetry();
                }

            });
            thread.Start();
        }


        /// <summary>
        /// 使用MC Interface將PLC Data讀回
        /// </summary>
        private void ReadDataUseMCProtocol()
        {
            McInterface.ReadBit(ref EQPLCMemoryTb, PLCMemOption.EQPBitAreaName,PLCMemOption.EQPBitStartAddressName,PLCMemOption.EQP_Bit_Size);
            McInterface.ReadWord(ref EQPLCMemoryTb, PLCMemOption.EQPWordAreaName, PLCMemOption.EQPWordStartAddressName, PLCMemOption.EQP_Word_Size);
        }

        protected virtual void ReadInputsUseTCPIP()
        {
            throw new NotImplementedException();
        }
        protected virtual void ReadInputsUseModbusTCP()
        {
            ushort startRegister = EndPointOptions.ConnOptions.Input_StartRegister;
            ushort registerNum = EndPointOptions.ConnOptions.Input_RegisterNum;
            if (EndPointOptions.ConnOptions.IO_Value_Type == IO_VALUE_TYPE.INPUT)
            {
                var inputs = master.ReadInputs(startRegister, registerNum);
                InputBuffer = inputs.ToList();
            }
            else
            {
                ushort[] input_registers = master.ReadInputRegisters(0, 1);
                InputBuffer = input_registers[0].GetBoolArray().ToList();
            }
        }
        public void WriteInputsUseModbusTCP(bool[] outputs)
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


        protected abstract void DefineInputData();

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
    }
}
