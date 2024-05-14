using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EquipmentManagment.Device;
using EquipmentManagment.Device.Options;

namespace EquipmentManagment.WIP
{
    /// <summary>
    /// 表示一個儲存格
    /// </summary>
    public class clsPortOfRack : PortStatusAbstract
    {
        public enum CARGO_PLACEMENT_STATUS
        {
            /// <summary>
            /// 沒有貨物
            /// </summary>
            NO_CARGO,
            /// <summary>
            /// 正常放置
            /// </summary>
            PLACED_NORMAL,
            /// <summary>
            /// 騎框/Rack
            /// </summary>
            PLACED_BUT_ASYMMETRIC
        }
        public enum SENSOR_LOCATION
        {
            TRAY_1, TRAY_2,
            RACK_1, RACK_2
        }

        public clsRackPortProperty Properties = new clsRackPortProperty();

        [NonSerialized]
        public readonly clsRack ParentRack;

        public Dictionary<SENSOR_LOCATION, bool> ExistSensorStates = new Dictionary<SENSOR_LOCATION, bool>()
        {
            { SENSOR_LOCATION.TRAY_1 ,false },
            { SENSOR_LOCATION.TRAY_2 ,false },
            { SENSOR_LOCATION.RACK_1 ,false },
            { SENSOR_LOCATION.RACK_2 ,false },
        };
        public bool CargoExist
        {
            get
            {
                return ExistSensorStates.Values.Any(state => state);
            }
        }
        public CARGO_PLACEMENT_STATUS TrayPlacementState
        {
            get
            {
                return GetPlacementState(ExistSensorStates[SENSOR_LOCATION.TRAY_1], ExistSensorStates[SENSOR_LOCATION.TRAY_2]);
            }
        }

        public CARGO_PLACEMENT_STATUS RackPlacementState
        {
            get
            {
                return GetPlacementState(ExistSensorStates[SENSOR_LOCATION.RACK_1], ExistSensorStates[SENSOR_LOCATION.RACK_2]);
            }
        }
        public clsPortOfRack()
        {
        }
        public clsPortOfRack(clsRackPortProperty option, clsRack parentRack)
        {
            this.Properties = option;
            ParentRack = parentRack;
        }

        private CARGO_PLACEMENT_STATUS GetPlacementState(bool sensor1, bool sensor2)
        {
            if (sensor1 && sensor2)
                return CARGO_PLACEMENT_STATUS.PLACED_NORMAL;
            else if ((sensor1 && !sensor2) || (!sensor1 && sensor2))
                return CARGO_PLACEMENT_STATUS.PLACED_BUT_ASYMMETRIC;
            else
                return CARGO_PLACEMENT_STATUS.NO_CARGO;
        }



        internal void UpdateIO(ref bool[] inputBuffer)
        {
            clsRackPortProperty.clsPortIOLocation ioLocation = Properties.IOLocation;
            try
            {
                ExistSensorStates[SENSOR_LOCATION.TRAY_1] = inputBuffer[ioLocation.Tray_Sensor1];
                ExistSensorStates[SENSOR_LOCATION.TRAY_2] = inputBuffer[ioLocation.Tray_Sensor2];
                ExistSensorStates[SENSOR_LOCATION.RACK_1] = inputBuffer[ioLocation.Box_Sensor1];
                ExistSensorStates[SENSOR_LOCATION.RACK_2] = inputBuffer[ioLocation.Box_Sensor2];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"clsWIPPort -> UpdateIO From inputs buffer Fail: {ex.Message}");
            }
        }
    }
}
