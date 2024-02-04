using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.Exceptions
{
    public class ChargeStationNoResponseException : Exception
    {
        public ChargeStationNoResponseException() { }
        public ChargeStationNoResponseException(string message) : base(message) { }

    }
}
