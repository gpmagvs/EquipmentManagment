using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.Connection
{
    public enum CONN_METHODS
    {
        MODBUS_TCP,
        MODBUS_RTU,
        TCPIP,
        SERIAL_PORT,
        MX,
        MC,
    }

    /// <summary>
    /// IO模組廠牌
    /// </summary>
    public enum IO_VALUE_TYPE
    {
        /// <summary>
        /// Inputs 讀 / CoilS 寫 
        /// </summary>
        INPUT,
        /// <summary>
        /// 歐迪爾,使用 InputRegist 讀/ SingleRegister 寫 
        /// </summary>
        INPUT_REGISTER

    }
}
