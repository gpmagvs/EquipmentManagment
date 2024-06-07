using System;
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
            PLACED_BUT_ASYMMETRIC,
            /// <summary>
            /// 訊號閃爍/Rack
            /// </summary>
            NO_CARGO_BUT_CLICK
        }
        public enum SENSOR_LOCATION
        {
            TRAY_1, TRAY_2,
            RACK_1, RACK_2
        }

        public enum SENSOR_STATUS
        {
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
                var tagMap = ParentRack.RackOption.ColumnTagMap;

                if (tagMap.TryGetValue(Properties.Column, out int[] _tags))
                    return _tags;
                else
                    return new int[0];

            }
        }
        public int Layer => Properties.Row;

        public static event EventHandler<(clsRack rack, clsPortOfRack port)> OnRackPortSensorFlash;
        public static event EventHandler<(clsRack rack, clsPortOfRack port)> OnRackPortSensorStatusChanged;

        [NonSerialized]
        public ConcurrentQueue<Dictionary<SENSOR_LOCATION, bool>> QueExistSensorStates = new ConcurrentQueue<Dictionary<SENSOR_LOCATION, bool>>();

        public Dictionary<SENSOR_LOCATION, SENSOR_STATUS> ExistSensorStates { get; set; } = new Dictionary<SENSOR_LOCATION, SENSOR_STATUS>()
        {
            { SENSOR_LOCATION.TRAY_1 ,SENSOR_STATUS.OFF },
            { SENSOR_LOCATION.TRAY_2 ,SENSOR_STATUS.OFF },
            { SENSOR_LOCATION.RACK_1 ,SENSOR_STATUS.OFF },
            { SENSOR_LOCATION.RACK_2 ,SENSOR_STATUS.OFF },
        };
        public bool CargoExist
        {
            get
            {
                return ExistSensorStates.Values.Any(state => state == SENSOR_STATUS.ON || state == SENSOR_STATUS.FLASH);
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


            //if (sensor1 && sensor2)
            //    return CARGO_PLACEMENT_STATUS.PLACED_NORMAL;
            //else if ((sensor1 && !sensor2) || (!sensor1 && sensor2))
            //    return CARGO_PLACEMENT_STATUS.PLACED_BUT_ASYMMETRIC;
            //else if (NO_CARGO_BUT_CLICK)
            //    return CARGO_PLACEMENT_STATUS.NO_CARGO_BUT_CLICK;
            //else
            //    return CARGO_PLACEMENT_STATUS.NO_CARGO;
        }

        private Dictionary<SENSOR_LOCATION, bool> temp_statuscounter = new Dictionary<SENSOR_LOCATION, bool>(){
                            { SENSOR_LOCATION.TRAY_1 ,false },
                            { SENSOR_LOCATION.TRAY_2 ,false },
                            { SENSOR_LOCATION.RACK_1 ,false },
                            { SENSOR_LOCATION.RACK_2 ,false },
        };

        private void CheckSensorClick2()
        {
            if (DateTime.Now.Second % 5 == 0)
            {
                try
                {
                    Dictionary<SENSOR_LOCATION, bool> finall_statuscounter = new Dictionary<SENSOR_LOCATION, bool>(){
                            { SENSOR_LOCATION.TRAY_1 ,false },
                            { SENSOR_LOCATION.TRAY_2 ,false },
                            { SENSOR_LOCATION.RACK_1 ,false },
                            { SENSOR_LOCATION.RACK_2 ,false },
                        };
                    Dictionary<SENSOR_LOCATION, int[]> statuscounter = new Dictionary<SENSOR_LOCATION, int[]>(){
                            { SENSOR_LOCATION.TRAY_1 ,new int[]{0,0,0 } },
                            { SENSOR_LOCATION.TRAY_2 ,new int[]{0,0,0 } },
                            { SENSOR_LOCATION.RACK_1 ,new int[]{0,0,0 } },
                            { SENSOR_LOCATION.RACK_2 ,new int[]{0,0,0 } },
                        };
                    int count = 0;
                    int flashcount = 0;
                    while (QueExistSensorStates.TryDequeue(out Dictionary<SENSOR_LOCATION, bool> temp_ExistSensorStates))
                    {
                        foreach (var item in temp_ExistSensorStates)
                        {
                            statuscounter[item.Key][0]++;
                            if (item.Value != temp_statuscounter[item.Key])
                            {
                                finall_statuscounter[item.Key] = temp_ExistSensorStates[item.Key];

                                flashcount += 1;
                            }
                            if (temp_ExistSensorStates[item.Key] == true)
                                statuscounter[item.Key][1]++;
                            else
                                statuscounter[item.Key][2]++;


                            count += 1;
                        }

                    }
                    foreach (var item in finall_statuscounter)
                    {
                        if ((statuscounter[SENSOR_LOCATION.TRAY_1][0] == statuscounter[SENSOR_LOCATION.TRAY_1][1] && statuscounter[SENSOR_LOCATION.TRAY_2][0] == statuscounter[SENSOR_LOCATION.TRAY_2][1]) || (statuscounter[SENSOR_LOCATION.RACK_1][0] == statuscounter[SENSOR_LOCATION.RACK_1][1] && statuscounter[SENSOR_LOCATION.RACK_2][0] == statuscounter[SENSOR_LOCATION.RACK_2][1]))
                            ExistSensorStates[item.Key] = SENSOR_STATUS.ON;
                        if ((statuscounter[SENSOR_LOCATION.TRAY_1][0] == statuscounter[SENSOR_LOCATION.TRAY_1][2] && statuscounter[SENSOR_LOCATION.TRAY_2][0] == statuscounter[SENSOR_LOCATION.TRAY_2][2]) || (statuscounter[SENSOR_LOCATION.RACK_1][0] == statuscounter[SENSOR_LOCATION.RACK_1][2] && statuscounter[SENSOR_LOCATION.RACK_2][0] == statuscounter[SENSOR_LOCATION.RACK_2][2]))
                            ExistSensorStates[item.Key] = SENSOR_STATUS.OFF;
                        else if (flashcount >= count / 2)
                            ExistSensorStates[item.Key] = SENSOR_STATUS.FLASH;
                    }

                    //foreach (var item in ExistSensorStates)
                    //{
                    //    ExistSensorStates[item.Key] = current_ExistSensorStates[item.Key] == true ? SENSOR_STATUS.ON : SENSOR_STATUS.OFF;
                    //}
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        Dictionary<SENSOR_LOCATION, bool> current_ExistSensorStates = new Dictionary<SENSOR_LOCATION, bool>()
                {
                    { SENSOR_LOCATION.TRAY_1 ,true },
                    { SENSOR_LOCATION.TRAY_2 ,true },
                    { SENSOR_LOCATION.RACK_1 ,true },
                    { SENSOR_LOCATION.RACK_2 ,true },
                };
        Dictionary<SENSOR_LOCATION, bool> _previousSensorStates = new Dictionary<SENSOR_LOCATION, bool>();
        internal void UpdateIO(ref bool[] inputBuffer)
        {
            clsRackPortProperty.clsPortIOLocation ioLocation = Properties.IOLocation;
            try
            {
                HandleSensorStatus(inputBuffer, SENSOR_LOCATION.TRAY_1, ioLocation.Tray_Sensor1);
                HandleSensorStatus(inputBuffer, SENSOR_LOCATION.TRAY_2, ioLocation.Tray_Sensor2);
                HandleSensorStatus(inputBuffer, SENSOR_LOCATION.RACK_1, ioLocation.Box_Sensor1);
                HandleSensorStatus(inputBuffer, SENSOR_LOCATION.RACK_2, ioLocation.Box_Sensor2);

                //_previousSensorStates = current_ExistSensorStates;
                //QueExistSensorStates.Enqueue(current_ExistSensorStates);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"clsWIPPort -> UpdateIO From inputs buffer Fail: {ex.Message}");
            }

            async void HandleSensorStatus(bool[] _inputBuffer, SENSOR_LOCATION _sensorLocation, ushort _ioLocation)
            {
                bool currentSensorStatus = _inputBuffer[_ioLocation];

                if (current_ExistSensorStates[_sensorLocation] != currentSensorStatus)
                {
                    CancellationTokenSource _cts = _StatusDelayCancellationTks[_sensorLocation];
                    if (_cts != null)
                    {
                        _cts.Cancel();
                        await Task.Delay(100);
                        SensorStatusChangedDelayAsync(_sensorLocation, currentSensorStatus);
                    }
                    else
                    {
                        SensorStatusChangedDelayAsync(_sensorLocation, currentSensorStatus);
                    }
                    current_ExistSensorStates[_sensorLocation] = currentSensorStatus;
                }
            }
            // CheckSensorClick2();
        }
        Dictionary<SENSOR_LOCATION, CancellationTokenSource> _StatusDelayCancellationTks = new Dictionary<SENSOR_LOCATION, CancellationTokenSource>()
        {
            { SENSOR_LOCATION.TRAY_1  ,  null},
            { SENSOR_LOCATION.TRAY_2  ,  null},
            { SENSOR_LOCATION.RACK_1  ,  null},
            { SENSOR_LOCATION.RACK_2  ,  null},
        };

        public DateTime timestamp { get; set; } = DateTime.MinValue;

        private async Task SensorStatusChangedDelayAsync(SENSOR_LOCATION location, bool currentSatus)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            _StatusDelayCancellationTks[location] = new CancellationTokenSource();
            while (stopwatch.ElapsedMilliseconds < 700)
            {
                if (_StatusDelayCancellationTks[location].IsCancellationRequested)
                {
                    stopwatch.Stop();
                    var previosState = ExistSensorStates[location];

                    if (previosState != SENSOR_STATUS.FLASH)
                    {
                        ExistSensorStates[location] = SENSOR_STATUS.FLASH;
                        OnRackPortSensorFlash?.Invoke(this, (ParentRack, this));
                    }
                    Console.WriteLine($"{this.ParentRack.EQName}-Port [{this.Properties.ID}]-{location} chagne to {ExistSensorStates[location]}");
                    return;
                }
                await Task.Delay(1);
            }
            ExistSensorStates[location] = currentSatus ? SENSOR_STATUS.OFF : SENSOR_STATUS.ON;
            OnRackPortSensorStatusChanged?.Invoke(this, (ParentRack, this));
            Console.WriteLine($"{this.ParentRack.EQName}-Port [{this.Properties.ID}]-{location} chagne to {ExistSensorStates[location]}");
            timestamp = DateTime.Now;
        }
    }
}
