using EquipmentManagment.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            Battery_Disconnect,
            CC_Timeout,
            CV_Timeout,
            FV_Timeout,
            Touch_Pad_OT,
            CML,
            Vin_UV_Fault,
            Output_OFF,
            Busy,
            Other,
            Fans,
            Power_Good,
            MFR,
            Input,
            Iout_Pout,
            Vout
        }
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
        public clsChargerData Datas = new clsChargerData();
        public clsChargeStation(clsEndPointOptions options) : base(options)
        {
        }
        public override PortStatusAbstract PortStatus { get; set; } = new clsRackPort();
        ManualResetEvent readStop = new ManualResetEvent(true);
        protected override  void ReadDataUseTCPIP()
        {
            try
            {
                // ClearBuffer();
                //SendSettingsToCharger(out var msg);
                //定義充電站的通訊交握
                if (WriteSettingFlag)
                {

                    tcp_client.Client.Send(ReadChargerStatesCmd, 0, 57, SocketFlags.None);
                    WriteSettingFlag = false;
                }
                TcpDataBuffer.Clear();
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

                while (TcpDataBuffer.Count != 57)
                {
                    Thread.Sleep(1);
                    byte[] buffer = new byte[57];
                    if (tcp_client.Available == 0)
                    {
                        //tcp_client.Client.Send(ReadChargerStatesCmd);
                        if (cts.IsCancellationRequested)
                        {
                            throw new SocketException();
                        }
                        continue;
                    }
                    int recLength = tcp_client.Client.Receive(buffer);
                    if (recLength == 0)
                        continue;

                    ArraySegment<byte> recvData = new ArraySegment<byte>(buffer, 0, recLength);
                    TcpDataBuffer.AddRange(recvData.ToArray());
                }
            }
            catch (SocketException sckex)
            {
                tcp_client.Dispose();
                throw sckex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

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
        protected override void DefineInputData()
        {

            if (TcpDataBuffer.Count != 57)
                return;
            if (TcpDataBuffer[13] == 0x61 | TcpDataBuffer[13] == 0x60)
            {
                Datas.CC = GetValue(Indexes_Write.CC_H, Indexes_Write.CC_L) / 10.0;
                Datas.CV = GetValue(Indexes_Write.CV_H, Indexes_Write.CV_L) / 10.0;
                Datas.FV = GetValue(Indexes_Write.FV_H, Indexes_Write.FV_L) / 10.0;
                Datas.TC = GetValue(Indexes_Write.TC_H, Indexes_Write.TC_L) / 10.0;
                return;
            }
            //解析封包取得充電器狀態
            Datas.Vin = GetValue(Indexes.VIN_H, Indexes.VIN_L) / 10.0;
            Datas.Vout = GetValue(Indexes.VOUT_H, Indexes.VOUT_L) / 10.0;
            Datas.Iout = GetValue(Indexes.IOUT_H, Indexes.IOUT_L) / 10.0;

            Datas.Temperature = TcpDataBuffer[Indexes.TEMPERATURE];
            Datas.Time = DateTime.FromBinary(GetValue(Indexes.TIME_L1, Indexes.TIME_L2, Indexes.TIME_H1, Indexes.TIME_H2));
            Datas.UpdateTime = DateTime.Now;
            //Errors
            CheckStatus(TcpDataBuffer[Indexes.Status_1], 0, ERROR_CODE.EEPRROM_DATA_ERROR);
            CheckStatus(TcpDataBuffer[Indexes.Status_1], 1, ERROR_CODE.Temp_Sensor_Short);
            CheckStatus(TcpDataBuffer[Indexes.Status_1], 2, ERROR_CODE.Battery_Disconnect);
            CheckStatus(TcpDataBuffer[Indexes.Status_1], 3, ERROR_CODE.CC_Timeout);
            CheckStatus(TcpDataBuffer[Indexes.Status_1], 4, ERROR_CODE.CV_Timeout);
            CheckStatus(TcpDataBuffer[Indexes.Status_1], 5, ERROR_CODE.FV_Timeout);
            CheckStatus(TcpDataBuffer[Indexes.Status_1], 6, ERROR_CODE.Touch_Pad_OT);
            //TODO確認ErrorStatus
            CheckStatus(TcpDataBuffer[Indexes.Status_2], 1, ERROR_CODE.CML);
            CheckStatus(TcpDataBuffer[Indexes.Status_2], 2, ERROR_CODE.Temp_OT_Warning);
            CheckStatus(TcpDataBuffer[Indexes.Status_2], 3, ERROR_CODE.Vin_UV_Fault);
            CheckStatus(TcpDataBuffer[Indexes.Status_2], 4, ERROR_CODE.Iout_OC_Fault);
            CheckStatus(TcpDataBuffer[Indexes.Status_2], 5, ERROR_CODE.Vout_OV_Fault);
            CheckStatus(TcpDataBuffer[Indexes.Status_2], 6, ERROR_CODE.Output_OFF);
            CheckStatus(TcpDataBuffer[Indexes.Status_2], 7, ERROR_CODE.Busy);

            CheckStatus(TcpDataBuffer[Indexes.Status_3], 1, ERROR_CODE.Other);
            CheckStatus(TcpDataBuffer[Indexes.Status_3], 1, ERROR_CODE.Fans);
            CheckStatus(TcpDataBuffer[Indexes.Status_3], 1, ERROR_CODE.Power_Good);
            CheckStatus(TcpDataBuffer[Indexes.Status_3], 1, ERROR_CODE.MFR);
            CheckStatus(TcpDataBuffer[Indexes.Status_3], 1, ERROR_CODE.Input);
            CheckStatus(TcpDataBuffer[Indexes.Status_3], 1, ERROR_CODE.Iout_Pout);
            CheckStatus(TcpDataBuffer[Indexes.Status_3], 1, ERROR_CODE.Vout);

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
        public bool SetCC(double val, out string message)
        {
            int valToWrite = int.Parse(Math.Round(val * 10) + "");
            Datas.CC_Setting = valToWrite;
            return SendSettingsToCharger(out message);
        }
        public bool SetCV(double val, out string message)
        {
            int valToWrite = int.Parse(Math.Round(val * 10) + "");
            Datas.CV_Setting = valToWrite;
            return SendSettingsToCharger(out message);

        }

        public bool SetFV(double val, out string message)
        {
            int valToWrite = int.Parse(Math.Round(val * 10) + "");
            Datas.FV_Setting = valToWrite;
            return SendSettingsToCharger(out message);

        }
        public bool SetTC(double val, out string message)
        {
            int valToWrite = int.Parse(Math.Round(val * 10) + "");
            Datas.TC_Setting = valToWrite;

            return SendSettingsToCharger(out message);
        }

        private bool SendSettingsToCharger(out string message)
        {
            message = "";
            Task.Factory.StartNew(() =>
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
                settingCmd = cmd_.ToArray();
                WriteSettingFlag = true;
            });

            CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            while (Datas.CC_Setting / 10.0 != Datas.CC | Datas.CV_Setting / 10.0 != Datas.CV | Datas.FV_Setting / 10.0 != Datas.FV | Datas.TC_Setting / 10.0 != Datas.TC)
            {
                Thread.Sleep(1);
                if (timeout.IsCancellationRequested)
                {
                    message = "Timeout!";
                    return false;
                }
                if (tcp_client == null)
                {
                    message = "Connection Error";
                    return false;
                }
            }

            return true;

        }
        private short GetValue(int LowByteIndex, int HighByteIndex)
        {
            byte l = TcpDataBuffer[LowByteIndex];
            byte h = TcpDataBuffer[HighByteIndex];
            return (new byte[2] { h, l }).GetInt();
        }

        private int GetValue(int LowByteIndex, int LowByteIndex2, int HighByteIndex, int HighByteIndex2)
        {
            byte l = TcpDataBuffer[LowByteIndex];
            byte l2 = TcpDataBuffer[LowByteIndex2];
            byte h = TcpDataBuffer[HighByteIndex];
            byte h2 = TcpDataBuffer[HighByteIndex2];
            return BitConverter.ToInt32(new byte[4] { h, h2, l, l2 }, 0);
        }


    }
}
