using EquipmentManagment.MainEquipment;
using EquipmentManagment.WIP;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.Device
{
    public abstract class PortStatusAbstract
    {
        public static event EventHandler<(string newValue, string oldValue, bool isUpdateByVehicleLoadUnLoad)> CarrierIDChanged;

        public string NickName { get; set; } = "TEST";

        private string _CarrierID = "";
        public virtual string CarrierID
        {
            get => _CarrierID;
            set
            {
                if (_CarrierID != value)
                {
                    string oldValue = _CarrierID;
                    _CarrierID = value;

                    if (GetType().Name == typeof(clsPortOfRack).Name)
                    {
                        CarrierIDChanged?.Invoke(this, (value, oldValue, VehicleLoadToPortFlag || VehicleUnLoadFromPortFlag));
                    }
                    else
                    {

                    }
                }
            }
        }
        public bool CarrierExist { get; set; }
        public DateTime InstallTime { get; set; } = DateTime.MinValue;

        public RACK_CONTENT_STATE RackContentState { get; set; } = RACK_CONTENT_STATE.UNKNOWN;

        public bool VehicleLoadToPortFlag { get; set; } = false;
        public bool VehicleUnLoadFromPortFlag { get; set; } = false;
    }


}
