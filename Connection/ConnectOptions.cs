using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.Connection
{
    public class ConnectOptions
    {
        public CONN_METHODS ConnMethod { get; set; } = CONN_METHODS.MODBUS_TCP;
        public string IP { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 502;
        public int AGVModbusGatewayPort { get; set; } = 502;
        public string ComPort { get; set; } = "COM1";

        public ushort Input_StartRegister { get; set; } = 0;
        public ushort Input_RegisterNum { get; set; } = 1;

        public IO_VALUE_TYPE IO_Value_Type { get; set; } = IO_VALUE_TYPE.INPUT_REGISTER;
    }
}
