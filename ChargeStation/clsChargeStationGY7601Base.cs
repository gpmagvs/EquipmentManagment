using EquipmentManagment.Device.Options;
using EquipmentManagment.Exceptions;
using EquipmentManagment.Tool;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EquipmentManagment.ChargeStation
{
    public class clsChargeStationGY7601Base : clsChargeStation
    {
        private bool _OperationONOFF_Donw_flag = false;

        private SemaphoreSlim SocketSemaphoreSlim = new SemaphoreSlim(1, 1);
        private SemaphoreSlim ReadDataSemaphoreSlim = new SemaphoreSlim(1, 1);
        public enum COMMAND_CODES : byte
        {
            /// <summary>
            /// Data:2Byte
            /// </summary>
            FAULT_CODE = 0x40,
            STATUS_BYTES = 0x78,
            STATUS_WORD = 0x79,
            STATUS_TEMPERATURE = 0x7D,
            STATUS_MFR_SPECIFIC = 0x80,
            READ_VIN = 0x88,
            READ_VOUT = 0x8B,
            READ_IOUT = 0x8C,
            READ_TEMPERATURE = 0x8D,
            READ_FAN_SPEED_1 = 0x90,
            READ_FAN_SPEED_2 = 0x91,
            CURVE_CC = 0xB0,
            CURVE_CV,
            CURVE_FV,
            CURVE_TC,
            CURVE_CONFIG = 0xB4,
            CHG_STATUS = 0xB8,
            MFR_MODEL = 0x9A,
            MFR_SERIAL = 0x9E
        }
        public clsChargeStationGY7601Base(clsEndPointOptions options) : base(options)
        {
        }

        protected override void InputsHandler()
        {

        }
        protected override void ReadDataUseSerial()
        {
            ReadDatasAsync();
        }

        protected override void ReadInputsUseTCPIP()
        {
            try
            {
                ReadDatasAsync().Wait();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ReadDataSemaphoreSlim.Release();
            }
        }

        private async Task<bool> ReadDatasAsync()
        {

            try
            {
                await ReadDataSemaphoreSlim.WaitAsync();

                Datas.Vin = await ReadVin();
                Datas.Vout = Math.Round(await ReadVout(), 2);
                Datas.Iout = await ReadIout();
                Datas.CC = await ReadCC();
                Datas.CV = await ReadCV();
                Datas.FV = await ReadFV();
                Datas.TC = await ReadTC();
                Datas.Fan_Speed_1 = await ReadFAN_Speed_1();
                Datas.Fan_Speed_2 = await ReadFAN_Speed_2();
                Datas.Temperature = await ReadTemperature();
                //ReadCURVE_CONFIG();
                (CHARGE_MODE currentMode, bool isFull, List<ERROR_CODE> errorCodes) = await ReadChargeStatus();

                Datas.IsBatteryFull = IsFull = isFull;
                Datas.CurrentChargeMode = currentChargeMode = currentMode;
                #region Get Error Codes
                List<ERROR_CODE> errorCodesFromStatusWord = await ReadSTATUS_WORD();
                List<ERROR_CODE> allErrorCodes = new List<ERROR_CODE>();
                allErrorCodes.AddRange(errorCodes);
                allErrorCodes.AddRange(errorCodesFromStatusWord);
                UpdateErrorCodes(allErrorCodes);
                #endregion
                //await ReadSTATUS_MFR_SPECIFIC();
                if (Datas.ErrorCodes.Any())
                {
                    Console.WriteLine($"Error Codes={string.Join(",", Datas.ErrorCodes)}");
                }
                Datas.UseVehicleName = UseVehicleName;
                Datas.SetAsUsing();
                Datas.Time = DateTime.Now;

                return true;
            }
            catch (SocketException ex)
            {
                //觸發此例外表示充電器已斷電=>沒有在充電

                Datas.SetAsNotUsing();
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }


        }

        private void UpdateErrorCodes(List<ERROR_CODE> allErrorCodes)
        {
            bool batteryNotConnectErrrorExist = allErrorCodes.Any(error => error == ERROR_CODE.Battery_Disconnect);
            bool previousBatteryNotConnectErrorExist = Datas.ErrorCodes.Any(error => error == ERROR_CODE.Battery_Disconnect);
            if (!previousBatteryNotConnectErrorExist && batteryNotConnectErrrorExist)
            {
                InvokeBatteryNotConnectEvent();
            }
            Datas.ErrorCodes.Clear();
            Datas.ErrorCodes = allErrorCodes;
        }

        private async Task ReadFaultCodes()
        {
            var result = await SendReadCommnad((byte)COMMAND_CODES.FAULT_CODE, 2);
            Console.WriteLine($"FAULT_CODE= {string.Join(",", result)}");
        }

        private async Task ReadSTATUS_MFR_SPECIFIC()
        {
            var result = await SendReadCommnad((byte)COMMAND_CODES.STATUS_MFR_SPECIFIC, 2);
            Console.WriteLine($"STATUS_MFR_SPECIFIC= {string.Join(",", result)}");
        }

        private async Task ReadSTATUS_BYTES()
        {
            var result = await SendReadCommnad((byte)COMMAND_CODES.STATUS_BYTES, 2);
            Console.WriteLine($"STATUS_BYTES = {string.Join(",", result)}");
        }
        private async Task<List<ERROR_CODE>> ReadSTATUS_WORD()
        {
            var result = await SendReadCommnad((byte)COMMAND_CODES.STATUS_WORD, 2);
            byte statusLowByte = result[0];
            byte statusHighByte = result[1];

            int[] statusLowByteBits = statusLowByte.ToBitArray();
            int[] statusHighByteBits = statusHighByte.ToBitArray();
            Console.WriteLine($"STATUS_LOW Byte = {string.Join(",", statusLowByteBits)}({statusLowByte})");
            Console.WriteLine($"STATUS_HIGHT Byte = {string.Join(",", statusHighByteBits)}({statusHighByte})");

            Dictionary<int, ERROR_CODE> errorCodesMapOf78H = new Dictionary<int, ERROR_CODE>()
            {
                {0, ERROR_CODE.NONE_OF_THE_ABOVE },
                {1, ERROR_CODE.CML },
                {2, ERROR_CODE.TEMP},
                {3, ERROR_CODE.Vin_UV_Fault},
                {4, ERROR_CODE.Iout_OC_Fault},
                {5, ERROR_CODE.Vout_OV_Fault},
                {6, ERROR_CODE.OFF},
                {7, ERROR_CODE.BUSY},
            };
            Dictionary<int, ERROR_CODE> errorCodesMapOf79H = new Dictionary<int, ERROR_CODE>()
            {
                {0, ERROR_CODE.RESERVE },
                {1, ERROR_CODE.Other },
                {2, ERROR_CODE.RESERVE},
                {3, ERROR_CODE.Power_Good},
                {4, ERROR_CODE.MFR},
                {5, ERROR_CODE.Input},
                {6, ERROR_CODE.IOUT},
                {7, ERROR_CODE.VOUT},
            };

            IEnumerable<int> errorOnBitsOf78H = statusLowByteBits.Where(val => val == 1)
                                                                 .Select(val => statusLowByteBits.ToList().IndexOf(val));
            List<ERROR_CODE> errorCodesFrom78H = errorCodesMapOf78H.Where(pair => errorOnBitsOf78H.Contains(pair.Key))
                                         .Select(pair => pair.Value).ToList().ToList();

            IEnumerable<int> errorOnBitsOf79H = statusHighByteBits.Where(val => val == 1)
                                                                  .Select(val => statusHighByteBits.ToList().IndexOf(val));
            List<ERROR_CODE> errorCodesFrom79H = errorCodesMapOf79H.Where(pair => errorOnBitsOf79H.Contains(pair.Key))
                                         .Select(pair => pair.Value).ToList().ToList();

            List<ERROR_CODE> outputErrorCodes = new List<ERROR_CODE>();
            outputErrorCodes.AddRange(errorCodesFrom78H);
            outputErrorCodes.AddRange(errorCodesFrom79H);
            //Console.WriteLine($"STATUS_WORD = {string.Join(",", result)}({Encoding.ASCII.GetString(result)})");
            return outputErrorCodes;
        }

        private async Task ReadMFR_MODEL()
        {
            var result = await SendReadCommnad((byte)COMMAND_CODES.MFR_MODEL, 12);
            string mfrModel = Encoding.ASCII.GetString(result);
            //Console.WriteLine($"MFR_MODEL = {mfrModel}");
        }

        private async Task ReadMFR_SERIALL()
        {
            var result = await SendReadCommnad((byte)COMMAND_CODES.MFR_SERIAL, 12);
            string MFR_SERIALL = Encoding.ASCII.GetString(result);
            //Console.WriteLine($"MFR_MODEL = {MFR_SERIALL}");
        }
        private async Task<bool> SetChargeConfigAsync()
        {
            var result = await SendWriteCommnadAsync((byte)COMMAND_CODES.CURVE_CONFIG, new byte[2] { 44, 00 });
            return result.Item1;
        }
        private async Task<bool> OperationONOFFCtrlAsync()
        {
            bool _success = (await SendWriteCommnadAsync(0x01, new byte[] { 0x80 })).Item1;
            if (!_success)
                return false;
            var result = await SendWriteCommnadAsync(0x01, new byte[] { 0x00 });
            if (!result.Item1)
                return false;
            else
                return true;
        }
        private async Task<(CHARGE_MODE currentMode, bool isFull, List<ERROR_CODE> errorCodes)> ReadChargeStatus()
        {
            var raw_data = await SendReadCommnad((byte)COMMAND_CODES.CHG_STATUS, 2);
            Console.WriteLine($"CHG_STATUS = {string.Join(",", raw_data)}");
            byte chargeStatusByte = raw_data[0];
            byte faultStatusByte = raw_data[1];
            int[] chargeStatusBits = chargeStatusByte.ToBitArray();
            int[] faultStatusBits = faultStatusByte.ToBitArray();

            CHARGE_MODE _chargeMode = _GetCurrentChargeMode(chargeStatusBits);
            bool _isFull = chargeStatusBits[0] == 1;

            Dictionary<int, ERROR_CODE> errorCodeMap = new Dictionary<int, ERROR_CODE>()
            {
                { 0, ERROR_CODE.EEPRROM_DATA_ERROR},
                { 1, ERROR_CODE.RESERVE},
                { 2, ERROR_CODE.NTCER},
                { 3, ERROR_CODE.Battery_Disconnect},
                { 4, ERROR_CODE.RESERVE},
                { 5, ERROR_CODE.CCTOF},
                { 6, ERROR_CODE.CVTOF},
                { 7, ERROR_CODE.FFTOF},
            };
            IEnumerable<int> errorOnBits = faultStatusBits.Where(val => val == 1).Select(val => faultStatusBits.ToList().IndexOf(val));
            List<ERROR_CODE> errorCodes = errorCodeMap.Where(pair => errorOnBits.Contains(pair.Key))
                                         .Select(pair => pair.Value).ToList().ToList();
            return (_chargeMode, _isFull, errorCodes);
        }

        private CHARGE_MODE _GetCurrentChargeMode(int[] chargeStatusBits)
        {
            int[] bits = new int[3] { chargeStatusBits[1], chargeStatusBits[2], chargeStatusBits[3] };
            string bitsString = string.Join("", bits);
            switch (bitsString)
            {
                case "100":
                    return CHARGE_MODE.CCM;
                case "010":
                    return CHARGE_MODE.CVM;
                case "001":
                    return CHARGE_MODE.FVM;
                default:
                    return CHARGE_MODE.CCM;
            }
        }

        private async Task ReadCURVE_CONFIG()
        {
            var raw_data = await SendReadCommnad((byte)COMMAND_CODES.CURVE_CONFIG, 2);
            Console.WriteLine($"CURVE_CONFIG = {string.Join(",", raw_data)}");
        }

        private async Task<double> ReadVin()
        {
            var raw_data = await SendReadCommnad((byte)COMMAND_CODES.READ_VIN, 2);
            return raw_data.Linear11ToDouble(-1);
        }
        private async Task<double> ReadVout()
        {
            var raw_data = await SendReadCommnad((byte)COMMAND_CODES.READ_VOUT, 2);
            return raw_data.Linear16ToDouble(-9);
        }
        private async Task<double> ReadIout()
        {
            var raw_data = await SendReadCommnad((byte)COMMAND_CODES.READ_IOUT, 2);
            return raw_data.Linear11ToDouble(-2);

        }
        private async Task<double> ReadTemperature()
        {
            var raw_data = await SendReadCommnad((byte)COMMAND_CODES.READ_TEMPERATURE, 2);
            double temperature = raw_data.Linear11ToDouble(-2);
            return temperature;
        }
        private async Task<double> ReadCC()
        {
            var raw_data = await SendReadCommnad((byte)COMMAND_CODES.CURVE_CC, 2);
            return raw_data.Linear11ToDouble(-2);
        }
        private async Task<double> ReadCV()
        {
            var raw_data = await SendReadCommnad((byte)COMMAND_CODES.CURVE_CV, 2);
            return raw_data.Linear16ToDouble(-9);
        }
        private async Task<double> ReadFV()
        {
            var raw_data = await SendReadCommnad((byte)COMMAND_CODES.CURVE_FV, 2);
            return raw_data.Linear16ToDouble(-9);

        }
        private async Task<double> ReadTC()
        {
            var raw_data = await SendReadCommnad((byte)COMMAND_CODES.CURVE_TC, 2);
            return raw_data.Linear11ToDouble(-2);
        }
        private async Task<double> ReadFAN_Speed_1()
        {
            var raw_data = await SendReadCommnad((byte)COMMAND_CODES.READ_FAN_SPEED_1, 2);
            return raw_data.Linear11ToDouble(5);
        }

        private async Task<double> ReadFAN_Speed_2()
        {
            var raw_data = await SendReadCommnad((byte)COMMAND_CODES.READ_FAN_SPEED_2, 2);
            return raw_data.Linear11ToDouble(5);
        }
        public new async Task<(bool, string message)> SetCCAsync(double val)
        {
            var bytes = val.DoubleToLinear11(-2);
            return await SendWriteCommnadAsync((byte)COMMAND_CODES.CURVE_CC, bytes);
        }
        public new async Task<(bool, string message)> SetCVAsync(double val)
        {
            var bytes = val.DoubleToLinear16(-2);
            return await SendWriteCommnadAsync((byte)COMMAND_CODES.CURVE_CV, bytes);
        }
        public new async Task<(bool, string message)> SetFV(double val)
        {
            var bytes = val.DoubleToLinear16(-2);
            return await SendWriteCommnadAsync((byte)COMMAND_CODES.CURVE_FV, bytes);
        }
        public new async Task<(bool, string message)> SetTCAsync(double val)
        {
            var bytes = val.DoubleToLinear11(-2);
            return await SendWriteCommnadAsync((byte)COMMAND_CODES.CURVE_TC, bytes);
        }
        /// <summary>
        /// 讀取指定位址數據
        /// </summary>
        /// <param name="command_code">位址</param>
        /// <param name="data_len">回傳的數據長度</param>
        /// <returns>byte陣列，0->N 對應 Low Byte->High Byte</returns>
        private async Task<byte[]> SendReadCommnad(byte command_code, int data_len)
        {
            Connection.CONN_METHODS connection_method = EndPointOptions.ConnOptions.ConnMethod;
            var command = new byte[] { 0x44, 0x8F, command_code, (byte)(data_len - 1) }; //TODO  8F為 Slave id(7bit) + 讀(1)bit ex, 10001111,其中 1000111 為充電器地址
            byte[] return_val = new byte[data_len];
            if (connection_method == Connection.CONN_METHODS.TCPIP)
            {
                this.tcp_client.Client.Send(command, command.Length, System.Net.Sockets.SocketFlags.None);
                int dateLenRev = 0;
                List<byte> bytes = new List<byte>();
                while (dateLenRev != data_len)
                {
                    await Task.Delay(10);
                    int _ava = tcp_client.Client.Available;
                    byte[] buffer = new byte[_ava];
                    int _num = this.tcp_client.Client.Receive(buffer, _ava, System.Net.Sockets.SocketFlags.None);
                    dateLenRev += _num;
                    bytes.AddRange(buffer);
                }
                return_val = bytes.ToArray();
            }
            else if (connection_method == Connection.CONN_METHODS.SERIAL_PORT)
            {
                serial.ReadTimeout = 3000;
                serial.Write(command, 0, command.Length);
                Thread.Sleep(100);
                serial.Read(return_val, 0, return_val.Length);
            }
            await Task.Delay(100);
            return return_val;
        }
        private async Task<(bool, string error_message)> SendWriteCommnadAsync(byte command_code, IEnumerable<byte> dataBytes)
        {
            await ReadDataSemaphoreSlim.WaitAsync();
            try
            {
                Connection.CONN_METHODS connection_method = EndPointOptions.ConnOptions.ConnMethod;
                var command = new byte[dataBytes.Count() + 3];
                command[0] = 0x44;
                command[1] = 0x8E;
                command[2] = command_code;
                Array.Copy(dataBytes.ToArray(), 0, command, 3, dataBytes.Count());
                byte[] return_val = new byte[1];
                if (connection_method == Connection.CONN_METHODS.TCPIP)
                {
                    tcp_client.Client.Send(command, command.Length, System.Net.Sockets.SocketFlags.None);
                    Thread.Sleep(100);
                    tcp_client.Client.Receive(return_val, 1, System.Net.Sockets.SocketFlags.None);

                }
                else if (connection_method == Connection.CONN_METHODS.SERIAL_PORT)
                {
                    serial.WriteTimeout = 3000;
                    serial.ReadTimeout = 3000;
                    serial.Write(command, 0, command.Length);
                    Thread.Sleep(100);
                    serial.Read(return_val, 0, return_val.Length);
                }
                Console.WriteLine($"Command Code:{command_code.ToString("X2")} Write:{string.Join(",", dataBytes.Select(b => b.ToString("X2")))},Module Return:{return_val[0].ToString("X2")}");
                bool _isReturnOk = return_val[0] == 0xAA;
                return (_isReturnOk, _isReturnOk ? "" : "Return value incorrect");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                ReadDataSemaphoreSlim.Release();
            }
        }
    }
}
