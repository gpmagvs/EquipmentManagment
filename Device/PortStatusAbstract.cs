using EquipmentManagment.MainEquipment;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.Device
{
    public abstract class PortStatusAbstract
    {
        public static event EventHandler<string> CarrierIDChanged;

        public string NickName { get; set; } = "TEST";

        private string _CarrierID = "";
        public string CarrierID
        {
            get => _CarrierID;
            set
            {
                if (_CarrierID != value)
                {
                    _CarrierID = value;
                    CarrierIDChanged?.Invoke(this, value);
                }
            }
        }
        public bool CarrierExist { get; set; }
        public DateTime InstallTime { get; set; } = DateTime.MinValue;

        public RACK_CONTENT_STATE RackContentState { get; set; } = RACK_CONTENT_STATE.UNKNOWN;
    }


}
