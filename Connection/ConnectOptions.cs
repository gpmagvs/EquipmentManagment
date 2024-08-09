using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.Connection
{
    public class ConnectOptions
    {
        public Dictionary<string, string> Notes { get; set; } = new Dictionary<string, string> {
            {"ConnMethod","0:MODBUS_TCP | 1:MODBUS_RTU | 2:TCPIP | 3:SERIAL_PORT | 4:MX | 5:MC" },
            {"IO_VALUE_TYPE","0:INPUT | 1:INPUT_REGISTER" },
        };
        public CONN_METHODS ConnMethod { get; set; } = CONN_METHODS.MODBUS_TCP;
        public string IP { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 502;
        public int AGVModbusGatewayPort { get; set; } = 502;
        public string ComPort { get; set; } = "COM1";

        public byte byteSlaveId = 1;

        public bool IsPLCAddress_Base_1 { get; set; } = true;
        public ushort Input_StartRegister { get; set; } = 0;
        public ushort Input_RegisterNum { get; set; } = 32;
        /// <summary>
        /// Output起始位址
        /// </summary>
        public ushort Output_Start_Address { get; set; } = 0;

        public IO_VALUE_TYPE IO_Value_Type { get; set; } = IO_VALUE_TYPE.INPUT;
    }
}
