using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.Device
{
    public abstract class PortStatusAbstract
    {
        public string CarrierID { get; set; }
        public bool CarrierExist { get; set; }
        public DateTime InstallTime { get; set; } = DateTime.MinValue;
    }

    
}
