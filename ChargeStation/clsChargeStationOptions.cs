using EquipmentManagment.Connection;
using EquipmentManagment.Device.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using static EquipmentManagment.Device.TemperatureModuleDevice.TemperatureModuleAbstract;

namespace EquipmentManagment.ChargeStation
{
    public class clsChargeStationOptions : clsEndPointOptions
    {
        /// <summary>
        /// 充電器通訊界面
        /// </summary>
        public enum CHARGER_INTERFACE
        {
            /// <summary>
            /// 充電器通訊交換器
            /// </summary>
            GANG_HAO_C36500Z1E_V3 = 1,
            /// <summary>
            /// PMBUS通用協議 ,使用 RS232/485轉I2C模組傳送指令
            /// </summary>
            PMBUS_REV_1_1 = 2
        }

        public override Dictionary<string, string> Notes { get; set; } = new Dictionary<string, string>()
        {
            {"chip_brand","充電器通訊界面: 1_罡豪充電器通訊交換器, 2_PMBUS通用協議 ,使用 RS232/485轉I2C模組傳送指令" }
        };

        public CHARGER_INTERFACE chip_brand { get; set; } = CHARGER_INTERFACE.PMBUS_REV_1_1;
        public int pmbus_slave_id { get; set; } = 7;

        public string[] usableAGVList { get; set; } = new string[0];

        public new IOLocation IOLocation { get; set; } = new IOLocation();
        public ConnectOptions IOModubleConnOptions { get; set; } = new ConnectOptions();

        public bool hasIOModule { get; set; } = false;

        public TemperatureModuleSetupOptions TemperatureModuleSettings { get; set; } = new TemperatureModuleSetupOptions();

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
            public int TEMPERABURE_ABN { get; internal set; } = 5;
        }

        public class OUTPUT
        {
            public int CHARGER_POWER_SWITCH { get; set; } = 0;
        }
    }
}
