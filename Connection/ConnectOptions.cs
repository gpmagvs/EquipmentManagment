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

        public string ComPort { get; set; } = "COM1";
    }
}
