using EquipmentManagment.BatteryExchanger;
using EquipmentManagment.ChargeStation;
using EquipmentManagment.Device;
using EquipmentManagment.Emu;
using EquipmentManagment.MainEquipment;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EquipmentManagment.Manager
{
    public class StaEQPManagager
    {
        public static clsEQManagementConfigs Configs { get; set; }
        public static List<EndPointDeviceAbstract> EQPDevices = new List<EndPointDeviceAbstract>();
        /// <summary>
        /// 客戶端主設備
        /// </summary>
        public static List<clsEQ> EQList
        {
            get
            {
                return EQPDevices.FindAll(device => device.EndPointOptions.EqType == EQ_TYPE.EQ).Select(eq => eq as clsEQ).ToList();
            }
        }
        public static List<clsChargeStation> ChargeStations = new List<clsChargeStation>();
        public static Dictionary<string, clsEndPointOptions> EQOptions = new Dictionary<string, clsEndPointOptions>();
        public static Dictionary<string, clsChargeStationOptions> ChargeStationsOptions = new Dictionary<string, clsChargeStationOptions>();
        public static async Task InitializeAsync()
        {
            await InitializeAsync(Configs == null ? new clsEQManagementConfigs
            {
                EQConfigPath = "EQConfigs.json",
                ChargeStationConfigPath = "ChargStationConfigs.json",
                WIPConfigPath = "WIPConfigs.json"
            } : Configs);
        }

        public static async Task InitializeAsync(clsEQManagementConfigs _Configs)
        {
            try
            {
                //DisposeEQs();
                Configs = _Configs;
                _LoadChargeStationConfigs(_Configs.ChargeStationConfigPath);
                _LoadEqConfigs(_Configs.EQConfigPath);
                _LoadWipConfigs(_Configs.WIPConfigPath);

                if (_Configs.UseEqEmu)
                {
                    StaEQPEmulatorsManagager.InitEmu(EQOptions);
                }
                foreach (KeyValuePair<string, clsChargeStationOptions> item in ChargeStationsOptions)
                {
                    var eqName = item.Key;
                    var options = item.Value;
                    var charge_station = options.chip_brand==2? new clsChargeStationGY7601Base(options): new clsChargeStation(options);
                    if (charge_station != null)
                    {
                        ChargeStations.Add(charge_station);
                        await charge_station.Connect();
                    }
                }
                foreach (KeyValuePair<string, clsEndPointOptions> item in EQOptions)
                {
                    var eqName = item.Key;
                    var options = item.Value;

                    EndPointDeviceAbstract EQ = null;
                    if (item.Value.EqType == EQ_TYPE.EQ)
                    {
                        EQ = new clsEQ(options);
                    }
                    else if (item.Value.EqType == EQ_TYPE.BATTERY_EXCHANGER)
                        EQ = new clsBatteryExchanger(options);
                    if (EQ != null)
                    {
                        EQPDevices.Add(EQ);
                        _ = Task.Factory.StartNew(async () =>
                        {
                            bool connected = EQ.EndPointOptions.EqType == EQ_TYPE.BATTERY_EXCHANGER ? await (EQ as clsBatteryExchanger).Connect() : await EQ.Connect();
                            //if (connected)
                            //{
                            //    if (EQ.EndPointOptions.EqType == EQ_TYPE.EQ)
                            //        ((clsEQ)EQ).ReserveUp();
                            //}
                        });

                    }
                }
            }
            catch (Exception ex)
            {

            }

        }

        public static void DisposeEQs()
        {
            StaEQPEmulatorsManagager.DisposeEQEmus();
            foreach (EndPointDeviceAbstract eq in EQPDevices)
            {
                eq.Dispose();
            }
            EQPDevices.Clear();
        }

        private static void _LoadChargeStationConfigs(string charge_station_ConfigPath)
        {
            string json = _LoadConfigJson(charge_station_ConfigPath);
            if (json != "")
            {
                ChargeStationsOptions = JsonConvert.DeserializeObject<Dictionary<string, clsChargeStationOptions>>(json);
            }
            else
            {
                ChargeStationsOptions = new Dictionary<string, clsChargeStationOptions>()
                {
                    {"Charge_1", new clsChargeStationOptions
                    {
                         ConnOptions = new Connection.ConnectOptions(),
                         Name = "Charge_1",
                         TagID = 1,
                          EqType = EQ_TYPE.CHARGE,
                           LdULdType = EQLDULD_TYPE.Charge,

                    } }
                };
            }
            SaveChargeStationConfigs();

        }
        private static void _LoadEqConfigs(string eQConfigPath)
        {
            string json = _LoadConfigJson(eQConfigPath);
            if (json != "")
            {
                EQOptions = JsonConvert.DeserializeObject<Dictionary<string, clsEndPointOptions>>(json);
            }
            else
            {
                EQOptions = new Dictionary<string, clsEndPointOptions>()
                {
                    {"EQ-1", new clsEndPointOptions
                    {
                         ConnOptions = new Connection.ConnectOptions(),
                         Name = "EQ-1",
                         TagID = 1
                    } }
                };
            }
            var alleqnames = EQOptions.Keys.ToList();
            var fin = EQOptions.Values.Where(val => val.ValidDownStreamEndPointNames.Any(name => !alleqnames.Contains(name)));
            if (fin.Any())
            {
                foreach (var item in fin)
                {
                    item.ValidDownStreamEndPointNames = item.ValidDownStreamEndPointNames.FindAll(nam => alleqnames.Contains(nam)).ToList();
                }
            }
            SaveEqConfigs();

        }
        private static void _LoadWipConfigs(string wIPConfigPath)
        {
            string json = _LoadConfigJson(wIPConfigPath);
        }

        private static string _LoadConfigJson(string configPath)
        {
            if (File.Exists(configPath))
                return File.ReadAllText(configPath);
            else
            {
                return "";
            }
        }
        public static void SaveEqConfigs()
        {
            foreach (KeyValuePair<string, clsEndPointOptions> item in EQOptions)
            {
                var _eq = EQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == item.Value.TagID);
                if (_eq != null)
                {
                    var oriName = _eq.EndPointOptions.Name;
                    if (StaEQPEmulatorsManagager.EqEmulators.TryGetValue(oriName, out var emulators))
                    {
                        if (!StaEQPEmulatorsManagager.EqEmulators.ContainsKey(item.Value.Name))
                        {

                            StaEQPEmulatorsManagager.EqEmulators.Add(item.Value.Name, emulators);
                            StaEQPEmulatorsManagager.EqEmulators.Remove(oriName);
                        }
                    }
                    _eq.EndPointOptions = item.Value;

                }
            }
            Directory.CreateDirectory(Path.GetDirectoryName(Configs.EQConfigPath));
            File.WriteAllText(Configs.EQConfigPath, JsonConvert.SerializeObject(EQOptions, Formatting.Indented));
        }
        public static void SaveChargeStationConfigs()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Configs.ChargeStationConfigPath));
            File.WriteAllText(Configs.ChargeStationConfigPath, JsonConvert.SerializeObject(ChargeStationsOptions, Formatting.Indented));
        }


        public static Dictionary<string, clsChargerData> GetChargeStationStates()
        {
            return ChargeStations.ToDictionary(eq => eq.EQName, eq => eq.Datas);
        }
        public static List<EQStatusDIDto> GetEQStates()
        {
            return EQPDevices.FindAll(eq => eq.EndPointOptions.EqType == EQ_TYPE.EQ).Select(eq => new EQStatusDIDto
            {
                IsConnected = eq.IsConnected,
                EQName = eq.EQName,
                Load_Request = (eq as clsEQ).Load_Request,
                Unload_Request = (eq as clsEQ).Unload_Request,
                Port_Exist = (eq as clsEQ).Port_Exist,
                Up_Pose = (eq as clsEQ).Up_Pose,
                Down_Pose = (eq as clsEQ).Down_Pose,
                Eqp_Status_Down = (eq as clsEQ).Eqp_Status_Down,
                HS_EQ_BUSY = (eq as clsEQ).HS_EQ_BUSY,
                HS_EQ_READY = (eq as clsEQ).HS_EQ_READY,
                HS_EQ_L_REQ = (eq as clsEQ).HS_EQ_L_REQ,
                HS_EQ_U_REQ = (eq as clsEQ).HS_EQ_U_REQ,
                HS_AGV_VALID = (eq as clsEQ).HS_AGV_VALID,
                HS_AGV_TR_REQ = (eq as clsEQ).HS_AGV_TR_REQ,
                HS_AGV_BUSY = (eq as clsEQ).HS_AGV_BUSY,
                HS_AGV_READY = (eq as clsEQ).HS_AGV_READY,
                HS_AGV_COMPT = (eq as clsEQ).HS_AGV_COMPT,
                Region = eq.EndPointOptions.Region,
                Tag = eq.EndPointOptions.TagID
            }).OrderBy(eq=>eq.EQName).ToList();
        }

        public static bool TryGetEQByEqName(string eqName, out clsEQ eQ , out string errorMsg)
        {
            errorMsg = string.Empty;
            eQ = null;
            try
            {
                eQ = GetEQByName(eqName);
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            return eQ != null;
        }
        public static EQStatusDIDto GetEQStatesByTagID(int tag)
        {
            var endpoint = EQPDevices.FirstOrDefault(eq => eq.EndPointOptions.TagID == tag);
            if (endpoint != null)
            {
                var _EQ = endpoint as clsEQ;
                return new EQStatusDIDto()
                {
                    Load_Request = _EQ.Load_Request,
                    Down_Pose = _EQ.Down_Pose,
                    EQName = _EQ.EQName,
                    Eqp_Status_Down = _EQ.Eqp_Status_Down,
                    Port_Exist = _EQ.Port_Exist,
                    Up_Pose = _EQ.Up_Pose,
                    Unload_Request = _EQ.Unload_Request,
                    IsConnected = _EQ.IsConnected,
                    Region = _EQ.EndPointOptions.Region,
                };
            }
            else
                return null;
        }

        public static clsEQ GetEQByName(string eqName)
        {
            return EQPDevices.FirstOrDefault(eq => eq.EQName == eqName) as clsEQ;
        }

        public static clsEQ GetEQByTag(int eQTag)
        {
            return EQPDevices.FirstOrDefault(eq => eq.EndPointOptions.TagID == eQTag) as clsEQ;
        }

    }
}
