using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.Exceptions
{
    public class ModbusReadInputException : Exception
    {
        public ModbusReadInputException() : base() { }
        public ModbusReadInputException(string message) : base(message)
        {

        }
    }
}
