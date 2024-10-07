using EquipmentManagment.Connection;
using EquipmentManagment.Device.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace EquipmentManagment.ChargeStation
{
    public class clsChargeStationOptions : clsEndPointOptions
    {

        public override Dictionary<string, string> Notes { get; set; } = new Dictionary<string, string>()
        {
        };

        public int chip_brand { get; set; } = 2;
        public int pmbus_slave_id { get; set; } = 7;

        public string[] usableAGVList { get; set; } = new string[0];

        public new IOLocation IOLocation { get; set; } = new IOLocation();
        public ConnectOptions IOModubleConnOptions { get; set; } = new ConnectOptions();

        public bool hasIOModule { get; set; } = false;
    }
    public class IOLocation
    {

        public INPUT Inputs { get; set; } = new INPUT();
        public OUTPUT Outputs { get; set; } = new OUTPUT();

        public class INPUT
        {
            public int EMO { get; set; } = 0;
            public int SMOKE_DETECT_ERROR { get; set; } = 1;
            public int AIR_ERROR { get; set; } = 2;
            public int CYLINDER_FORWARD { get; set; } = 3;
            public int CYLINDER_BACKWARD { get; set; } = 4;

        }

        public class OUTPUT
        {
            public int CHARGER_POWER_SWITCH { get; set; } = 0;
        }
    }
}
