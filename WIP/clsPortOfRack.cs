using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            ON, OFF, FLASH
        }

        public clsRackPortProperty Properties = new clsRackPortProperty();

        [NonSerialized]
        public clsRack ParentRack;

        public ConcurrentQueue<Dictionary<SENSOR_LOCATION, bool>> QueExistSensorStates = new ConcurrentQueue<Dictionary<SENSOR_LOCATION, bool>>();

        public Dictionary<SENSOR_LOCATION, SENSOR_STATUS> ExistSensorStates = new Dictionary<SENSOR_LOCATION, SENSOR_STATUS>()
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
                return ExistSensorStates.Values.Any(state => state == SENSOR_STATUS.ON);
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

        private CARGO_PLACEMENT_STATUS GetPlacementState(SENSOR_STATUS sensor1, SENSOR_STATUS sensor2)
        {

            if (sensor1 == SENSOR_STATUS.ON && sensor2 == SENSOR_STATUS.ON)
                return CARGO_PLACEMENT_STATUS.PLACED_NORMAL;
            else if ((sensor1 == SENSOR_STATUS.ON && sensor2 == SENSOR_STATUS.OFF) || (sensor1 == SENSOR_STATUS.ON && sensor2 == SENSOR_STATUS.OFF))
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

                            temp_statuscounter[item.Key] = item.Value;
                            count += 1;
                        }

                    }
                    foreach (var item in finall_statuscounter)
                    {
                        if (statuscounter[item.Key][0] == statuscounter[item.Key][1])
                            ExistSensorStates[item.Key] = SENSOR_STATUS.ON;
                        else if (statuscounter[item.Key][0] == statuscounter[item.Key][2])
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
                    { SENSOR_LOCATION.TRAY_1 ,false },
                    { SENSOR_LOCATION.TRAY_2 ,false },
                    { SENSOR_LOCATION.RACK_1 ,false },
                    { SENSOR_LOCATION.RACK_2 ,false },
                };
        Dictionary<SENSOR_LOCATION, bool> _previousSensorStates = new Dictionary<SENSOR_LOCATION, bool>();
        internal void UpdateIO(ref bool[] inputBuffer)
        {
            clsRackPortProperty.clsPortIOLocation ioLocation = Properties.IOLocation;
            try
            {
                current_ExistSensorStates[SENSOR_LOCATION.TRAY_1] = inputBuffer[ioLocation.Tray_Sensor1];
                current_ExistSensorStates[SENSOR_LOCATION.TRAY_2] = inputBuffer[ioLocation.Tray_Sensor2];
                current_ExistSensorStates[SENSOR_LOCATION.RACK_1] = inputBuffer[ioLocation.Box_Sensor1];
                current_ExistSensorStates[SENSOR_LOCATION.RACK_2] = inputBuffer[ioLocation.Box_Sensor2];

                if (_previousSensorStates.Values.Where(s => !s).Count() != current_ExistSensorStates.Values.Where(s => !s).Count())
                { }
                if (Properties.Row == 1 && Properties.Column == 2)
                {

                }
                _previousSensorStates = current_ExistSensorStates;
                QueExistSensorStates.Enqueue(current_ExistSensorStates);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"clsWIPPort -> UpdateIO From inputs buffer Fail: {ex.Message}");
            }
            CheckSensorClick2();
        }
        //bool NO_CARGO_BUT_CLICK = false;
        //private void CheckSensorClick()
        //{
        //    bool Traysensor1 = ExistSensorStates[SENSOR_LOCATION.TRAY_1];
        //    bool Traysensor2 = ExistSensorStates[SENSOR_LOCATION.TRAY_2];
        //    bool Racksensor1 = ExistSensorStates[SENSOR_LOCATION.RACK_1];
        //    bool Racksensor2 = ExistSensorStates[SENSOR_LOCATION.RACK_2];
        //    int count = 0;
        //    int noalarmcount = 0;
        //    while (ExistSensorStates[SENSOR_LOCATION.TRAY_1] || ExistSensorStates[SENSOR_LOCATION.TRAY_2] || ExistSensorStates[SENSOR_LOCATION.RACK_1] || ExistSensorStates[SENSOR_LOCATION.RACK_2])
        //    {
        //        if (Traysensor1 != ExistSensorStates[SENSOR_LOCATION.TRAY_1] || Traysensor2 != ExistSensorStates[SENSOR_LOCATION.TRAY_2] || Racksensor1 != ExistSensorStates[SENSOR_LOCATION.RACK_1] || Racksensor2 != ExistSensorStates[SENSOR_LOCATION.RACK_2])
        //        {
        //            Traysensor1 = ExistSensorStates[SENSOR_LOCATION.TRAY_1];
        //            Traysensor2 = ExistSensorStates[SENSOR_LOCATION.TRAY_2];
        //            Racksensor1 = ExistSensorStates[SENSOR_LOCATION.RACK_1];
        //            Racksensor2 = ExistSensorStates[SENSOR_LOCATION.RACK_2];
        //            noalarmcount = count;
        //            count += 1;
        //        }
        //        if (count == 5)
        //        {
        //            NO_CARGO_BUT_CLICK = true;
        //            Console.WriteLine($"NO_CARGO_BUT_CLICK={NO_CARGO_BUT_CLICK}");
        //            break;
        //        }
        //        if (noalarmcount == count)
        //        { break; }
        //        Thread.Sleep(400);
        //    }
        //}
    }
}
