using EquipmentManagment.Device.Options;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace EquipmentManagment.ChargeStation
{
    public class clsChargeStationOptions : clsEndPointOptions
    {
        public int chip_brand { get; set; } = 2;
        public int pmbus_slave_id { get; set; } = 7;

        public  string[] usableAGVList { get;set; } = new string[0];
    }
}
