using EquipmentManagment.Device;
using EquipmentManagment.Device.Options;
using EquipmentManagment.WIP;
using EquipmentManagment.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EquipmentManagment.Exceptions;
using EquipmentManagment.Connection;
using System.Net.Http.Headers;
using System.Diagnostics;

namespace EquipmentManagment.ChargeStation
{
    public partial class clsChargeStation : EndPointDeviceAbstract
    {
        public struct Indexes
        {
            public const int VIN_L = 27;
            public const int VIN_H = 28;
            public const int VOUT_L = 29;
            public const int VOUT_H = 30;
            public const int IOUT_L = 31;
            public const int IOUT_H = 32;
            public const int CC_L = 33;
            public const int CC_H = 34;
            public const int CV_L = 35;
            public const int CV_H = 36;
            public const int FV_L = 37;
            public const int FV_H = 38;
            public const int TC_L = 39;
            public const int TC_H = 40;
            public const int TEMPERATURE = 41;
            public const int TIME_L1 = 47;
            public const int TIME_L2 = 48;
            public const int TIME_H1 = 49;
            public const int TIME_H2 = 50;

            public const int Error = 51;
            public const int Status_1 = 52;
            public const int Status_2 = 53;
            public const int Status_3 = 54;

        }
        public struct Indexes_Write
        {

            public const int CC_L = 27;
            public const int CC_H = 28;
            public const int CV_L = 29;
            public const int CV_H = 30;
            public const int FV_L = 31;
            public const int FV_H = 32;
            public const int TC_L = 33;
            public const int TC_H = 34;
        }
        public override int TcpSocketRecieveTimeout { get; set; } = 3000;
        public override int TcpSocketSendTimeout { get; set; } = 1000;
        public override bool IsConnected
        {
            get => _IsConnected;
            set
            {
                Datas.Connected = _IsConnected = value;
            }
        }
        public enum ERROR_CODE
        {
            EEPRROM_DATA_ERROR,
            Vout_OV_Warning,
            Vout_OV_Fault,
            Vout_UV_Warning,
            Vout_UV_Fault,
            Iout_OC_Warning,
            Iout_OC_Fault,
            Iout_UV_Warning,
            Iout_UV_Fault,
            Input_OC_Warning,
            Input_OC_Fault,
            Input_UV_Warning,
            Input_UV_Fault,
            Temp_OT_Warning,
            Temp_OT_Fault,
            Temp_UT_Warning,
            Temp_UT_Fault,
            FAN1_Warning,
            FAN1_Fault,
            FAN2_Warning,
            FAN2_Fault,
            FAN1_Speed_OR,
            FAN2_Speed_OR,
            Airflow_Warning,
            Airflow_Fault,
            Temp_Sensor_Short,
            /// <summary>
            /// 未偵測到電池
            /// </summary>
            Battery_Disconnect,
            CC_Timeout,
            CV_Timeout,
            FV_Timeout,
            Touch_Pad_OT,
            CML,
            Vin_UV_Fault,
            Output_OFF,
            BUSY,
            Other,
            Fans,
            Power_Good,
            MFR,
            Input,
            Iout_Pout,
            VOUT,
            /// <summary>
            /// 溫度補償線路發生短路
            /// </summary>
            NTCER,
            /// <summary>
            /// 保留-無定義
            /// </summary>
            RESERVE,
            /// <summary>
            /// 定電流階段充電超時
            /// </summary>
            CCTOF,
            /// <summary>
            /// 浮充階段充電超時
            /// </summary>
            FFTOF,
            /// <summary>
            /// 定電壓階段充電超時
            /// </summary>
            CVTOF,
            NONE_OF_THE_ABOVE,
            TEMP,
            OFF,
            IOUT,
        }
        public enum CHARGE_MODE
        {
            /// <summary>
            /// 定電壓充電模式
            /// </summary>
            CVM,
            /// <summary>
            /// 浮充模式
            /// </summary>
            FVM,
            /// <summary>
            /// 定電流充電模式
            /// </summary>
            CCM
        }


        public bool _IsFull = false;
        public bool IsFull
        {
            get => _IsFull;
            set
            {
                if (_IsFull != value)
                {
                    _IsFull = value;
                    if (_IsFull)
                    {
                        OnBatteryChargeFull?.Invoke(this, this);
                    }
                }
            }
        }

        public string UseVehicleName { get; private set; } = "";

        public CHARGE_MODE currentChargeMode = CHARGE_MODE.CCM;
        private bool WriteSettingFlag = false;
        private byte[] settingCmd = null;
        private byte[] ReadChargerStatesCmd
        {
            get
            {
                if (WriteSettingFlag)
                {
                    return settingCmd;
                }
                else
                {

                    byte[] cmd = new byte[57]
                    {
                   0xAA,0xAA,0xAA,0xAA,0xAA,0xAA,0xAA,0xAB,
                   0x01,0x2B,0x00,0x00,0x00,0x50,
                   0x01,0x08,0x00,0x00,0x00,0x00,0x00,0x00,
                   0x01,0x15,0x01,0x1,0x01,
                   0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                   0x00,0x00 //CRC16, need to calculate
                    };
                    ArraySegment<byte> toCalCRC = new ArraySegment<byte>(cmd, 8, 47);
                    ushort crc = CRCCalculator.GetCRC16(toCalCRC.ToArray());
                    byte[] crcbytes = BitConverter.GetBytes(crc);
                    //cmd[55] = crcbytes[0];
                    //cmd[56] = crcbytes[1];
                    cmd[55] = 0x22;
                    cmd[56] = 0xEC;
                    return cmd;
                }
            }
        }

        public static event EventHandler<clsChargeStation> OnBatteryNotConnected;
        public static event EventHandler<clsChargeStation> OnBatteryChargeFull;
        public clsChargerData Datas = new clsChargerData();
        public ChargerIOSynchronizer chargerIOSynchronizer = new ChargerIOSynchronizer();
        public clsChargeStationOptions chargerOptions => EndPointOptions as clsChargeStationOptions;
        public clsChargeStation(clsEndPointOptions options) : base(options)
        {
            Datas.UsableAGVNames = (options as clsChargeStationOptions).usableAGVList;
            Datas.TagNumber = options.TagID;
        }
        public override PortStatusAbstract PortStatus { get; set; } = new clsPortOfRack();
        public override bool IsMaintaining { get => throw new NotImplementedException(); }
        protected void InvokeBatteryNotConnectEvent()
        {
            OnBatteryNotConnected?.Invoke(this, this);
        }

        public override async Task StartSyncData()
        {
            if ((EndPointOptions as clsChargeStationOptions).hasIOModule)
                SyncIOState();

            await Task.Run(async () =>
            {
                while (true)
                {
                    if (disposedValue)
                    {
                        DisConnect();
                        Console.WriteLine($"{this.EndPointOptions.Name} ChargerStation Instance Disposed");
                        return;
                    }
                    await Task.Delay(300);
                    try
                    {
                        if (!_IsConnected)
                        {
                            bool _connected = Connect().GetAwaiter().GetResult();
                            Console.WriteLine($"Connect Result={_connected}");
                            continue;
                        }
                        ReadInputsUseTCPIP();
                        if (DataBuffer.Count > 0)
                            InputsHandler();

                    }
                    catch (ChargeStationNoResponseException ex)
                    {
                        //若觸發這個例外，表示充電站沒AGV在充電.
                        //Datas.SetAsNotUsing();
                        //IsConnected = false;
                        await Task.Delay(5000);
                        Console.WriteLine($"Charge Station No Charging action...");
                        continue;
                    }
                    catch (SocketException ex)
                    {
                        IsConnected = false;
                        await Task.Delay(1000);
                        Console.WriteLine(ex.Message);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        IsConnected = false;
                        await Task.Delay(3000);
                        Console.WriteLine(ex.Message);
                        continue;
                    }
                }


            });
        }

        private async Task SyncIOState()
        {
            await Task.Delay(1).ContinueWith(async task =>
            {
                clsChargeStationOptions chargerOption = this.EndPointOptions as clsChargeStationOptions;
                chargerIOSynchronizer = new ChargerIOSynchronizer(chargerOption);
                chargerIOSynchronizer.StartAsync();

            });
        }

        protected override async Task _StartRetry()
        {
            //do noting;
        }
        internal class ChargerSocketState
        {
            public byte[] buffer = new byte[57];
            public Socket socket = null;
        }
        protected override void ReadInputsUseTCPIP()
        {

            try
            {
                ManualResetEvent manualResetEvent = new ManualResetEvent(false);
                bool _socketError = false;
                void RecieveCallBack(IAsyncResult ar)
                {
                    try
                    {
                        ChargerSocketState _state = (ChargerSocketState)ar.AsyncState;
                        int received = _state.socket.EndReceive(ar);
                        ArraySegment<byte> recvData = new ArraySegment<byte>(_state.buffer, 0, received);
                        DataBuffer.AddRange(recvData);
                        if (_IsDataRecievieDone(DataBuffer))
                        {
                            manualResetEvent.Set();
                        }
                        else
                        {
                            DataBuffer.Clear();
                            _state.buffer = new byte[57];
                            Task.Run(async () =>
                            {
                                await Task.Delay(10);
                                _state.socket.BeginReceive(_state.buffer, 0, _state.buffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallBack), _state);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _socketError = true;
                        manualResetEvent.Set();
                    }
                }
                ChargerSocketState sckState = new ChargerSocketState() { socket = tcp_client.Client };
                tcp_client.Client.BeginReceive(sckState.buffer, 0, sckState.buffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallBack), sckState);

                bool done = manualResetEvent.WaitOne(Debugger.IsAttached ? 8000 : 8000, true);
                if (_socketError)
                {
                    throw new SocketException();
                }
                if (!done)
                {
                    DataBuffer.Clear();
                    //timeout
                    throw new ChargeStationNoResponseException();
                }
                Datas.SetAsUsing();
            }
            catch (SocketException sckex)
            {
                throw sckex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private bool _IsDataRecievieDone(List<byte> dataBuffer)
        {
            //條件 data len = 57 & [0]~[6] = 0xAA & [7] = 0xAB
            if (dataBuffer.Count < 57)
                return false;

            if (dataBuffer[7] != 0xAB)
                return false;
            return true;
        }

        private void ClearBuffer()
        {
            try
            {

                if (tcp_client.Available != 0)
                {
                    byte[] buffer = new byte[tcp_client.Available];
                    int recLength = tcp_client.Client.Receive(buffer);
                }
            }
            catch (Exception)
            {
            }
        }
        protected override void InputsHandler()
        {

            if (DataBuffer.Count % 57 != 0)
                return;

            List<byte> lastData = DataBuffer.GetRange(DataBuffer.Count - 57, 57);

            if (lastData[13] == 0x61 || lastData[13] == 0x60)
            {
                Datas.CC = GetValue(ref lastData, Indexes_Write.CC_H, Indexes_Write.CC_L) / 10.0;
                Datas.CV = GetValue(ref lastData, Indexes_Write.CV_H, Indexes_Write.CV_L) / 10.0;
                Datas.FV = GetValue(ref lastData, Indexes_Write.FV_H, Indexes_Write.FV_L) / 10.0;
                Datas.TC = GetValue(ref lastData, Indexes_Write.TC_H, Indexes_Write.TC_L) / 10.0;
                DataBuffer.Clear();
                return;
            }
            //解析封包取得充電器狀態
            Datas.Vin = GetValue(ref lastData, Indexes.VIN_H, Indexes.VIN_L) / 10.0;
            Datas.Vout = GetValue(ref lastData, Indexes.VOUT_H, Indexes.VOUT_L) / 10.0;
            Datas.Iout = GetValue(ref lastData, Indexes.IOUT_H, Indexes.IOUT_L) / 10.0;
            Datas.CC = GetValue(ref lastData, Indexes.CC_H, Indexes.CC_L) / 10.0;
            Datas.TC = GetValue(ref lastData, Indexes.TC_H, Indexes.TC_L) / 10.0;
            Datas.FV = GetValue(ref lastData, Indexes.FV_H, Indexes.FV_L) / 10.0;
            Datas.CV = GetValue(ref lastData, Indexes.CV_H, Indexes.CV_L) / 10.0;
            Datas.Temperature = lastData[Indexes.TEMPERATURE];
            Datas.Time = DateTime.FromBinary(GetValue(ref lastData, Indexes.TIME_L1, Indexes.TIME_L2, Indexes.TIME_H1, Indexes.TIME_H2));
            Datas.UpdateTime = DateTime.Now;
            //Errors
            CheckStatus(lastData[Indexes.Status_1], 0, ERROR_CODE.EEPRROM_DATA_ERROR);
            CheckStatus(lastData[Indexes.Status_1], 1, ERROR_CODE.Temp_Sensor_Short);
            CheckStatus(lastData[Indexes.Status_1], 2, ERROR_CODE.Battery_Disconnect);
            CheckStatus(lastData[Indexes.Status_1], 3, ERROR_CODE.CC_Timeout);
            CheckStatus(lastData[Indexes.Status_1], 4, ERROR_CODE.CV_Timeout);
            CheckStatus(lastData[Indexes.Status_1], 5, ERROR_CODE.FV_Timeout);
            CheckStatus(lastData[Indexes.Status_1], 6, ERROR_CODE.Touch_Pad_OT);
            //TODO確認ErrorStatus
            CheckStatus(lastData[Indexes.Status_2], 1, ERROR_CODE.CML);
            CheckStatus(lastData[Indexes.Status_2], 2, ERROR_CODE.Temp_OT_Warning);
            CheckStatus(lastData[Indexes.Status_2], 3, ERROR_CODE.Vin_UV_Fault);
            CheckStatus(lastData[Indexes.Status_2], 4, ERROR_CODE.Iout_OC_Fault);
            CheckStatus(lastData[Indexes.Status_2], 5, ERROR_CODE.Vout_OV_Fault);
            CheckStatus(lastData[Indexes.Status_2], 6, ERROR_CODE.Output_OFF);
            CheckStatus(lastData[Indexes.Status_2], 7, ERROR_CODE.BUSY);

            CheckStatus(lastData[Indexes.Status_3], 1, ERROR_CODE.Other);
            CheckStatus(lastData[Indexes.Status_3], 2, ERROR_CODE.Fans);
            CheckStatus(lastData[Indexes.Status_3], 3, ERROR_CODE.Power_Good);
            CheckStatus(lastData[Indexes.Status_3], 4, ERROR_CODE.MFR);
            CheckStatus(lastData[Indexes.Status_3], 5, ERROR_CODE.Input);
            CheckStatus(lastData[Indexes.Status_3], 6, ERROR_CODE.Iout_Pout);
            CheckStatus(lastData[Indexes.Status_3], 7, ERROR_CODE.VOUT);
            DataBuffer.Clear();
        }

        private void CheckStatus(byte status, int status_bit, ERROR_CODE StatusErrorCode)
        {
            bool[] boolArray = new bool[8];
            for (int i = 0; i < 8; i++)
                boolArray[i] = (status & 1 << i) != 0;

            if (boolArray[status_bit])
            {
                if (!Datas.ErrorCodes.Contains(StatusErrorCode))
                    Datas.ErrorCodes.Add(StatusErrorCode);
            }
            else
                Datas.ErrorCodes.Remove(StatusErrorCode);
        }
        public virtual async Task<(bool success, string message)> SetCCAsync(double val)
        {
            int valToWrite = int.Parse(Math.Round(val * 10) + "");
            Datas.CC_Setting = valToWrite;
            return await SendSettingsToCharger();
        }
        public virtual async Task<(bool success, string message)> SetCVAsync(double val)
        {
            int valToWrite = int.Parse(Math.Round(val * 10) + "");
            Datas.CV_Setting = valToWrite;
            return await SendSettingsToCharger();


        }

        public virtual async Task<(bool success, string message)> SetFV(double val)
        {
            int valToWrite = int.Parse(Math.Round(val * 10) + "");
            Datas.FV_Setting = valToWrite;
            return await SendSettingsToCharger();

        }
        public virtual async Task<(bool success, string message)> SetTCAsync(double val)
        {
            int valToWrite = int.Parse(Math.Round(val * 10) + "");
            Datas.TC_Setting = valToWrite;

            return await SendSettingsToCharger();
        }
        public override void SetTag(int newTag)
        {
            base.SetTag(newTag);
            this.Datas.TagNumber = newTag;
        }
        private async Task<(bool success, string message)> SendSettingsToCharger()
        {
            byte[] cc_LH = Datas.CC_Setting.GetHighLowBytes();
            var cmd_ = ReadChargerStatesCmd.ToArray();
            cmd_[13] = 0x60;
            cmd_[Indexes_Write.CC_L] = cc_LH[0];
            cmd_[Indexes_Write.CC_H] = cc_LH[1];

            byte[] cv_LH = Datas.CV_Setting.GetHighLowBytes();
            cmd_[Indexes_Write.CV_L] = cv_LH[0];
            cmd_[Indexes_Write.CV_H] = cv_LH[1];

            byte[] FV_LH = Datas.FV_Setting.GetHighLowBytes();
            cmd_[Indexes_Write.FV_L] = FV_LH[0];
            cmd_[Indexes_Write.FV_H] = FV_LH[1];

            byte[] TC_LH = Datas.TC_Setting.GetHighLowBytes();
            cmd_[Indexes_Write.TC_L] = TC_LH[0];
            cmd_[Indexes_Write.TC_H] = TC_LH[1];

            ArraySegment<byte> toCalCRC = new ArraySegment<byte>(cmd_, 8, 47);
            ushort crc = CRCCalculator.GetCRC16(toCalCRC.ToArray());
            byte[] crcbytes = BitConverter.GetBytes(crc);
            cmd_[55] = crcbytes[0];
            cmd_[56] = crcbytes[1];
            var settingCmd = cmd_.ToArray();

            CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            tcp_client?.Client.Send(settingCmd);
            while (Datas.CC_Setting / 10.0 != Datas.CC || Datas.CV_Setting / 10.0 != Datas.CV || Datas.FV_Setting / 10.0 != Datas.FV || Datas.TC_Setting / 10.0 != Datas.TC)
            {
                await Task.Delay(1000);
                if (timeout.IsCancellationRequested)
                {
                    return (false, "Timeout!");
                }
                if (tcp_client == null)
                {
                    return (false, "Connection Error");
                }
            }

            return (true, "");

        }
        private short GetValue(ref List<byte> dataRef, int LowByteIndex, int HighByteIndex)
        {
            byte l = dataRef[LowByteIndex];
            byte h = dataRef[HighByteIndex];
            return (new byte[2] { h, l }).GetInt();
        }

        private int GetValue(ref List<byte> dataRef, int LowByteIndex, int LowByteIndex2, int HighByteIndex, int HighByteIndex2)
        {
            byte l = dataRef[LowByteIndex];
            byte l2 = dataRef[LowByteIndex2];
            byte h = dataRef[HighByteIndex];
            byte h2 = dataRef[HighByteIndex2];
            return BitConverter.ToInt32(new byte[4] { h, h2, l, l2 }, 0);
        }

        protected override void WriteOutuptsData()
        {
        }

        public bool IsAGVUsable(string agv_name)
        {
            return (EndPointOptions as clsChargeStationOptions).usableAGVList.Contains(agv_name);
        }

        public void SetUsableAGVList(string[] agvNames)
        {
            Datas.UsableAGVNames = (EndPointOptions as clsChargeStationOptions).usableAGVList = agvNames;
        }

        public void UpdateUserVehicleName(string vehicleName)
        {
            Datas.UseVehicleName = UseVehicleName = vehicleName;
        }

        public override void UpdateCarrierInfo(int tagNumber, string carrierID, int height)
        {
        }
    }
}
