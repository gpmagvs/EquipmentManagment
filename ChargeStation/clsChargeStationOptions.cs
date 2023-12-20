using EquipmentManagment.Device;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.ChargeStation
{
    public class clsChargeStationOptions : clsEndPointOptions
    {
        public int chip_brand { get; set; } = 2;
        public int pmbus_slave_id { get; set; } = 7;
    }
}
