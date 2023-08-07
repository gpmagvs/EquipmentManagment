using System;
using System.Collections.Generic;

namespace EquipmentManagment.ChargeStation
{
    public class clsChargerData
    {
        public bool Connected { get; set; } = false;
        public double Vin { get; set; }
        public double Vout { get; set; }
        public double Iout { get; set; }
        public double CC { get; set; }
        public double CV { get; set; }
        public double FV { get; set; }
        public double TC { get; set; }
        public DateTime Time { get; set; }
        public List<clsChargeStation.ERROR_CODE> ErrorCodes { get; set; } = new List<clsChargeStation.ERROR_CODE>();
        public byte Temperature { get; internal set; }
    }
}
