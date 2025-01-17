﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EquipmentManagment.Device;
using EquipmentManagment.Device.Options;
using Newtonsoft.Json;

namespace EquipmentManagment.WIP
{


    public class PortOfRackViewModel : clsPortOfRack
    {
        private string carrierID = "";
        public override string CarrierID { get => carrierID; set => carrierID = value; }

        public new int[] TagNumbers { get; set; } = new int[0];

    }
    /// <summary>
    /// 表示一個儲存格
    /// </summary>
    public class clsPortOfRack : PortStatusAbstract
    {
        public enum PRUDUCTION_QUALITY
        {
            OK,
            NG,
        }

        public enum CARGO_TYPE
        {
            ONLY_TRAY,
            ONLY_RACK,
            MIXED
        }

        public enum Port_Enable
        {
            Enable,
            Disable,
        }

        public enum PORT_USABLE
        {
            NOT_USABLE = 0,
            USABLE = 1,
        }

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
            PLACED_BUT_ASYMMETRIC,
            /// <summary>
            /// 訊號閃爍/Rack
            /// </summary>
            NO_CARGO_BUT_CLICK
        }
        public enum SENSOR_LOCATION
        {
            TRAY_1, TRAY_2,
            RACK_1, RACK_2,
            TRAY_DIRECTION,
            RACK_AREA
        }

        public enum SENSOR_STATUS
        {
            /// <summary>
            /// OFF 表示沒有貨物
            /// </summary>
            OFF = 0,
            ON = 1,
            FLASH = 2
        }

        public clsRackPortProperty Properties { get; set; } = new clsRackPortProperty();

        [JsonIgnore]
        [NonSerialized]
        private clsRack ParentRack;

        public clsRack GetParentRack() => ParentRack;

        public int[] TagNumbers
        {
            get
            {
                if (ParentRack == null)
                    return new int[0];
                var tagMap = ParentRack.RackOption.ColumnTagMap;

                if (tagMap.TryGetValue(Properties.Column, out int[] _tags))
                    return _tags;
                else
                    return new int[0];

            }
        }
        public int Layer => Properties.Row;
        public string PortNo => Properties.PortNo;

        public static event EventHandler<(clsRack rack, clsPortOfRack port)> OnRackPortSensorFlash;
        public static event EventHandler<(clsRack rack, clsPortOfRack port)> OnRackPortSensorStatusChanged;
        public static event EventHandler<clsPortOfRack> OnPortCargoChangeToDisappear;
        public static event EventHandler<clsPortOfRack> OnPortCargoChangedToExist;

        /// <summary>
        /// 這是所有sensor的狀態
        /// </summary>
        public Dictionary<SENSOR_LOCATION, SENSOR_STATUS> SensorStates { get; set; } = new Dictionary<SENSOR_LOCATION, SENSOR_STATUS>()
        {
            { SENSOR_LOCATION.TRAY_1 ,SENSOR_STATUS.OFF },
            { SENSOR_LOCATION.TRAY_2 ,SENSOR_STATUS.OFF },
            { SENSOR_LOCATION.RACK_1 ,SENSOR_STATUS.OFF },
            { SENSOR_LOCATION.RACK_2 ,SENSOR_STATUS.OFF },
            { SENSOR_LOCATION.TRAY_DIRECTION ,SENSOR_STATUS.OFF },
            { SENSOR_LOCATION.RACK_AREA ,SENSOR_STATUS.OFF },
        };

        /// <summary>
        /// 這是產品在席狀態檢知Sensor的狀態
        /// </summary>
        public Dictionary<SENSOR_LOCATION, SENSOR_STATUS> MaterialExistSensorStates
        {
            get
            {
                return SensorStates.Where(k => k.Key != SENSOR_LOCATION.RACK_AREA && k.Key != SENSOR_LOCATION.TRAY_DIRECTION)
                                   .ToDictionary(k => k.Key, k => k.Value);
            }
        }
        private bool PreviousCargoExist = false;
        /// <summary>
        /// 儲格是否有貨物存在
        /// </summary>
        public bool CargoExist
        {
            get
            {
                bool hasTray = Properties.HasTraySensor && MaterialExistSensorStates.Any(r => (r.Key == SENSOR_LOCATION.TRAY_1 || r.Key == SENSOR_LOCATION.TRAY_2) && r.Value != SENSOR_STATUS.OFF);
                bool hasRack = Properties.HasRackSensor && MaterialExistSensorStates.Any(r => (r.Key == SENSOR_LOCATION.RACK_1 || r.Key == SENSOR_LOCATION.RACK_2) && r.Value != SENSOR_STATUS.OFF);
                return hasTray || hasRack;
            }
        }
        public CARGO_PLACEMENT_STATUS TrayPlacementState
        {
            get
            {
                return GetPlacementState(SensorStates[SENSOR_LOCATION.TRAY_1], SensorStates[SENSOR_LOCATION.TRAY_2]);
            }
        }

        public CARGO_PLACEMENT_STATUS RackPlacementState
        {
            get
            {
                return GetPlacementState(SensorStates[SENSOR_LOCATION.RACK_1], SensorStates[SENSOR_LOCATION.RACK_2]);
            }
        }
        public clsPortOfRack()
        { }
        public clsPortOfRack(clsRackPortProperty option, clsRack parentRack)
        {
            this.Properties = option;
            ParentRack = parentRack;
        }

        private CARGO_PLACEMENT_STATUS GetPlacementState(SENSOR_STATUS sensor1, SENSOR_STATUS sensor2)
        {

            if (sensor1 == SENSOR_STATUS.ON && sensor2 == SENSOR_STATUS.ON)
                return CARGO_PLACEMENT_STATUS.PLACED_NORMAL;
            else if ((sensor1 == SENSOR_STATUS.ON && sensor2 == SENSOR_STATUS.OFF) || (sensor2 == SENSOR_STATUS.ON && sensor1 == SENSOR_STATUS.OFF))
                return CARGO_PLACEMENT_STATUS.PLACED_BUT_ASYMMETRIC;
            else if (sensor1 == SENSOR_STATUS.FLASH || sensor2 == SENSOR_STATUS.FLASH)
                return CARGO_PLACEMENT_STATUS.NO_CARGO_BUT_CLICK;
            else
                return CARGO_PLACEMENT_STATUS.NO_CARGO;
        }

        Dictionary<SENSOR_LOCATION, bool> current_ExistSensorStates = new Dictionary<SENSOR_LOCATION, bool>()
                {
                    { SENSOR_LOCATION.TRAY_1 ,true },
                    { SENSOR_LOCATION.TRAY_2 ,true},
                    { SENSOR_LOCATION.RACK_1 ,true },
                    { SENSOR_LOCATION.RACK_2 ,true },
                    { SENSOR_LOCATION.TRAY_DIRECTION,true},
                    { SENSOR_LOCATION.RACK_AREA,true },
                };
        internal void UpdateIO(ref bool[] inputBuffer)
        {
            clsRackPortProperty.clsPortIOLocation ioLocation = Properties.IOLocation;
            try
            {
                HandleSensorStatus(inputBuffer, SENSOR_LOCATION.TRAY_1, ioLocation.Tray_Sensor1);
                HandleSensorStatus(inputBuffer, SENSOR_LOCATION.TRAY_2, ioLocation.Tray_Sensor2);
                HandleSensorStatus(inputBuffer, SENSOR_LOCATION.RACK_1, ioLocation.Box_Sensor1);
                HandleSensorStatus(inputBuffer, SENSOR_LOCATION.RACK_2, ioLocation.Box_Sensor2);
                HandleSensorStatus(inputBuffer, SENSOR_LOCATION.TRAY_DIRECTION, ioLocation.Tray_Direction_Sensor);
                HandleSensorStatus(inputBuffer, SENSOR_LOCATION.RACK_AREA, ioLocation.Rack_Area_Sensor);

                //_previousSensorStates = current_ExistSensorStates;
                //QueExistSensorStates.Enqueue(current_ExistSensorStates);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"clsWIPPort -> UpdateIO From inputs buffer Fail: {ex.Message}");
            }

            void HandleSensorStatus(bool[] _inputBuffer, SENSOR_LOCATION _sensorLocation, ushort _ioLocation)
            {
                bool currentSensorStatus = _inputBuffer[_ioLocation];

                if (current_ExistSensorStates[_sensorLocation] != currentSensorStatus)
                {
                    CancellationTokenSource _cts = _StatusDelayCancellationTks[_sensorLocation];
                    _cts?.Cancel();
                    SensorStatusChangedDelayAsync(_sensorLocation, currentSensorStatus);
                    current_ExistSensorStates[_sensorLocation] = currentSensorStatus;
                }
            }
            // CheckSensorClick2();
        }
        Dictionary<SENSOR_LOCATION, CancellationTokenSource> _StatusDelayCancellationTks = new Dictionary<SENSOR_LOCATION, CancellationTokenSource>()
        {
            { SENSOR_LOCATION.TRAY_1 , new CancellationTokenSource()},
            { SENSOR_LOCATION.TRAY_2 , new CancellationTokenSource()},
            { SENSOR_LOCATION.RACK_1 , new CancellationTokenSource()},
            { SENSOR_LOCATION.RACK_2 , new CancellationTokenSource()},
            { SENSOR_LOCATION.TRAY_DIRECTION , new CancellationTokenSource()},
            { SENSOR_LOCATION.RACK_AREA , new CancellationTokenSource()},
        };

        /// <summary>
        /// 修改Port狀態為OK/NG Port
        /// </summary>
        /// <param name="IsNormal">True = OK; False = NG</param>
        internal void ChangePortStatus(bool IsNormal)
        {
            if (IsNormal)
                Properties.ProductionQualityStore = PRUDUCTION_QUALITY.OK;
            else
                Properties.ProductionQualityStore = PRUDUCTION_QUALITY.NG;
        }

        public DateTime timestamp { get; set; } = DateTime.MinValue;

        private async Task SensorStatusChangedDelayAsync(SENSOR_LOCATION location, bool currentSatus)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            _StatusDelayCancellationTks[location] = new CancellationTokenSource();
            try
            {
                await Task.Delay(700, _StatusDelayCancellationTks[location].Token);
            }
            catch (TaskCanceledException)
            {
                var previosState = SensorStates[location];
                if (previosState != SENSOR_STATUS.FLASH)
                {
                    SensorStates[location] = SENSOR_STATUS.FLASH;
                    OnRackPortSensorFlash?.Invoke(this, (ParentRack, this));
                }
                Console.WriteLine($"{this.ParentRack.EQName}-Port [{this.Properties.ID}]-{location} chagne to {SensorStates[location]}");
                return;
            }

            SensorStates[location] = currentSatus ? SENSOR_STATUS.OFF : SENSOR_STATUS.ON;
            OnRackPortSensorStatusChanged?.Invoke(this, (ParentRack, this));
            Console.WriteLine($"{this.ParentRack.EQName}-Port [{this.Properties.ID}]-{location} chagne to {SensorStates[location]}");
            timestamp = DateTime.Now;

            if (!PreviousCargoExist && CargoExist)
                OnPortCargoChangedToExist?.Invoke(this, this);
            else if (PreviousCargoExist && !CargoExist)
                OnPortCargoChangeToDisappear?.Invoke(this, this);

            PreviousCargoExist = CargoExist;
        }
    }
}
