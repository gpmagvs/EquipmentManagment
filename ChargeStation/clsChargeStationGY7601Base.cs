using EquipmentManagment.Device.Options;
using EquipmentManagment.Exceptions;
using EquipmentManagment.Tool;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
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
            STATUS_TEMPERATURE = 0x7D,
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
                //    if (!_OperationONOFF_Donw_flag)
                //    {
                //        //if (!await OperationONOFFCtrlAsync())
                //        //{
                //        //    throw new Exception("Operation ON/OFF Fail");
                //        //}
                //        if (!await SetChargeConfigAsync())
                //        {
                //            throw new Exception("Operation ON/OFF Fail");
                //        }
                //        _OperationONOFF_Donw_flag = true;
                //    }

                await ReadDataSemaphoreSlim.WaitAsync();
                Datas.Vin = ReadVin();
                Datas.Vout = Math.Round(ReadVout(), 2);
                Datas.Iout = ReadIout();
                Datas.CC = ReadCC();
                Datas.CV = ReadCV();
                Datas.FV = ReadFV();
                Datas.TC = ReadTC();
                Datas.Fan_Speed_1 = ReadFAN_Speed_1();
                Datas.Fan_Speed_2 = ReadFAN_Speed_2();
                Datas.Temperature = ReadTemperature();
                ReadCURVE_CONFIG();
                ReadChargeStatus();
                Datas.Time = DateTime.Now;
                //Console.WriteLine($"{JsonConvert.SerializeObject(Datas, Formatting.Indented)}");
                return true;
            }
            catch (SocketException ex)
            {
                throw new ChargeStationNoResponseException();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }


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
        private void ReadChargeStatus()
        {
            var raw_data = SendReadCommnad((byte)COMMAND_CODES.CHG_STATUS, 2);
        }
        private void ReadCURVE_CONFIG()
        {
            var raw_data = SendReadCommnad((byte)COMMAND_CODES.CURVE_CONFIG, 2);
        }

        private double ReadVin()
        {
            var raw_data = SendReadCommnad((byte)COMMAND_CODES.READ_VIN, 2);
            return raw_data.Linear11ToDouble(-1);
        }
        private double ReadVout()
        {
            var raw_data = SendReadCommnad((byte)COMMAND_CODES.READ_VOUT, 2);
            return raw_data.Linear16ToDouble(-9);
        }
        private double ReadIout()
        {
            var raw_data = SendReadCommnad((byte)COMMAND_CODES.READ_IOUT, 2);
            return raw_data.Linear11ToDouble(-2);

        }
        private double ReadTemperature()
        {
            var raw_data = SendReadCommnad((byte)COMMAND_CODES.READ_TEMPERATURE, 2);
            double temperature = raw_data.Linear11ToDouble(-2);
            return temperature;
        }
        private double ReadCC()
        {
            var raw_data = SendReadCommnad((byte)COMMAND_CODES.CURVE_CC, 2);
            return raw_data.Linear11ToDouble(-2);
        }
        private double ReadCV()
        {
            var raw_data = SendReadCommnad((byte)COMMAND_CODES.CURVE_CV, 2);
            return raw_data.Linear16ToDouble(-9);
        }
        private double ReadFV()
        {
            var raw_data = SendReadCommnad((byte)COMMAND_CODES.CURVE_FV, 2);
            return raw_data.Linear16ToDouble(-9);

        }
        private double ReadTC()
        {
            var raw_data = SendReadCommnad((byte)COMMAND_CODES.CURVE_TC, 2);
            return raw_data.Linear11ToDouble(-2);
        }
        private double ReadFAN_Speed_1()
        {
            var raw_data = SendReadCommnad((byte)COMMAND_CODES.READ_FAN_SPEED_1, 2);
            return raw_data.Linear11ToDouble(5);
        }

        private double ReadFAN_Speed_2()
        {
            var raw_data = SendReadCommnad((byte)COMMAND_CODES.READ_FAN_SPEED_2, 2);
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
        private byte[] SendReadCommnad(byte command_code, int data_len)
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
                    Thread.Sleep(1);
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
