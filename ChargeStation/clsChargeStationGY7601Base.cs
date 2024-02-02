using EquipmentManagment.Device.Options;
using EquipmentManagment.Tool;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace EquipmentManagment.ChargeStation
{
    public class clsChargeStationGY7601Base : clsChargeStation
    {
        private bool _write_setting_flag = false;
        private bool _ready_to_write_flag = false;
        private bool _OperationONOFF_Donw_flag = false;
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
            ReadDatas();
        }

        protected override void ReadInputsUseTCPIP()
        {
            ReadDatas();
        }

        private void ReadDatas()
        {
            if (!_OperationONOFF_Donw_flag)
            {
                if (!OperationONOFFCtrl())
                {
                    throw new Exception("Operation ON/OFF Fail");
                }
                _OperationONOFF_Donw_flag = true;
                _write_setting_flag = false;
            }
            if (_write_setting_flag)
            {
                _ready_to_write_flag = true;
                return;
            }
            _ready_to_write_flag = false;
            Datas.Vin = ReadVin();
            Datas.Vout = ReadVout();
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
            Console.WriteLine($"{JsonConvert.SerializeObject(Datas, Formatting.Indented)}");
        }
        private bool OperationONOFFCtrl()
        {
            _ready_to_write_flag = true;
            bool _success = SendWriteCommnad(0x01, new byte[] { 0x80 }, out string msg);
            if (!_success)
                return false;
            _success = SendWriteCommnad(0x01, new byte[] { 0x00 }, out msg);
            _ready_to_write_flag = false;
            if (!_success)
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
            return raw_data.Reverse().Linear11ToDouble(-1);
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
        public override bool SetCC(double val, out string message)
        {
            var bytes = val.DoubleToLinear11(-2);
            return SendWriteCommnad((byte)COMMAND_CODES.CURVE_CC, bytes, out message);
        }
        public override bool SetCV(double val, out string message)
        {
            var bytes = val.DoubleToLinear16(-2).Reverse().ToArray();
            return SendWriteCommnad((byte)COMMAND_CODES.CURVE_CV, bytes, out message);
        }
        public override bool SetFV(double val, out string message)
        {
            var bytes = val.DoubleToLinear16(-2);
            return SendWriteCommnad((byte)COMMAND_CODES.CURVE_FV, bytes, out message);
        }
        public override bool SetTC(double val, out string message)
        {
            var bytes = val.DoubleToLinear11(-2);
            return SendWriteCommnad((byte)COMMAND_CODES.CURVE_TC, bytes, out message);
        }
        private byte[] SendReadCommnad(byte command_code, int data_len)
        {
            Connection.CONN_METHODS connection_method = EndPointOptions.ConnOptions.ConnMethod;
            var command = new byte[] { 0x44, 0x8F, command_code, (byte)(data_len - 1) }; //TODO  8F為 Slave id(7bit) + 讀(1)bit ex, 10001111,其中 1000111 為充電器地址
            byte[] return_val = new byte[data_len];
            if (connection_method == Connection.CONN_METHODS.TCPIP)
            {

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
        private bool SendWriteCommnad(byte command_code, IEnumerable<byte> dataBytes, out string error_message)
        {
            error_message = string.Empty;
            _write_setting_flag = true;
            while (!_ready_to_write_flag)
            {
                Thread.Sleep(1);
            }
            try
            {
                Connection.CONN_METHODS connection_method = EndPointOptions.ConnOptions.ConnMethod;
                var command = new byte[dataBytes.Count() + 2];
                command[0] = 0x44;
                command[1] = 0x8E;
                Array.Copy(dataBytes.ToArray(), 0, command, 2, dataBytes.Count());
                byte[] return_val = new byte[1];
                if (connection_method == Connection.CONN_METHODS.TCPIP)
                {

                }
                else if (connection_method == Connection.CONN_METHODS.SERIAL_PORT)
                {
                    serial.WriteTimeout = 3000;
                    serial.ReadTimeout = 3000;
                    serial.Write(command, 0, command.Length);
                    Thread.Sleep(100);
                    serial.Read(return_val, 0, return_val.Length);
                }
                _write_setting_flag = false;
                return return_val[0] == 0xAA;

            }
            catch (Exception ex)
            {
                error_message = ex.Message;
                return false;
            }

        }
    }
}
