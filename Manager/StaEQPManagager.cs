using EquipmentManagment.BatteryExchanger;
using EquipmentManagment.ChargeStation;
using EquipmentManagment.Device;
using EquipmentManagment.Device.Options;
using EquipmentManagment.Emu;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.MainEquipment.EQGroup;
using EquipmentManagment.WIP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EquipmentManagment.Manager
{
    public partial class StaEQPManagager
    {
        public static clsEQManagementConfigs Configs { get; set; }
        public static List<EndPointDeviceAbstract> EQPDevices = new List<EndPointDeviceAbstract>();
        /// <summary>
        /// 客戶端主設備
        /// </summary>
        public static List<clsEQ> MainEQList
        {
            get
            {
                return EQPDevices.FindAll(device => device.EndPointOptions.IsProdution_EQ).Select(eq => eq as clsEQ).ToList();
            }
        }
        public static List<clsChargeStation> ChargeStations = new List<clsChargeStation>();
        public static List<clsRack> RacksList = new List<clsRack>();

        public static List<EqGroup> EQGroupsStore = new List<EqGroup>();

        public static Dictionary<string, clsEndPointOptions> EQOptions = new Dictionary<string, clsEndPointOptions>();
        public static Dictionary<string, clsChargeStationOptions> ChargeStationsOptions = new Dictionary<string, clsChargeStationOptions>();
        public static Dictionary<string, clsRackOptions> RacksOptions = new Dictionary<string, clsRackOptions>();
        private static string _LoadConfigJson(string configPath)
        {
            if (File.Exists(configPath))
                return File.ReadAllText(configPath);
            else
            {
                return "";
            }
        }
        public static void TryRemoveAndCreateNewEQ()
        {
            //找出被移除的設備
            var newOptionKeys = EQOptions.Values.Select(opt => _createKeyOfOption(opt)).ToList();
            IEnumerable<clsEQ> removedEQ = MainEQList.Where(eq => !newOptionKeys.Contains(_createKeyOfOption(eq.EndPointOptions)));

            string _createKeyOfOption(clsEndPointOptions opt)
            {
                return opt.TagID + "H" + opt.Height;
            }

            if (removedEQ.Any())
            {
                foreach (var item in removedEQ)
                {
                    item.DisConnect();
                    item.Dispose();
                    EQPDevices.Remove(item);
                    StaEQPEmulatorsManagager.EqEmulators.Remove(item.EQName);
                }
            }

            foreach (KeyValuePair<string, clsEndPointOptions> item in EQOptions)
            {
                var _eq = MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == item.Value.TagID && eq.EndPointOptions.Height == item.Value.Height);
                if (_eq == null)
                {
                    if (item.Value.IsEmulation && !StaEQPEmulatorsManagager.EqEmulators.ContainsKey(item.Key))
                    {
                        clsDIOModuleEmu emu = new clsDIOModuleEmu();
                        int modbusPort = StaEQPEmulatorsManagager.EqEmulators.Values.Select(_emu => _emu.options.ConnOptions.Port).OrderBy(port => port).LastOrDefault() + 2;
                        item.Value.ConnOptions.Port = modbusPort;
                        emu.StartEmu(item.Value);
                        StaEQPEmulatorsManagager.EqEmulators.Add(item.Key, emu);
                    }
                    clsEQ NewEQ = new clsEQ(item.Value);
                    NewEQ.StartSyncData();
                    EQPDevices.Add(NewEQ);
                }
            }
        }
        public static void SaveEqConfigs()
        {

            foreach (KeyValuePair<string, clsEndPointOptions> item in EQOptions)
            {
                var _eq = MainEQList.FirstOrDefault(eq => eq.EndPointOptions.TagID == item.Value.TagID && eq.EndPointOptions.Height == item.Value.Height);
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
            return EQPDevices.FindAll(eq => eq.EndPointOptions.IsProdution_EQ).Select(eq => (eq as clsEQ).GetEQStatusDTO()).OrderBy(eq => eq.EQName).ToList();
        }

        public static bool TryGetEQByEqName(string eqName, out clsEQ eQ, out string errorMsg)
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
                return _EQ.GetEQStatusDTO();
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
        public static clsRack GetRackByTag(int eQTag)
        {
            return RacksList.FirstOrDefault(eq => eq.EndPointOptions.TagID == eQTag) as clsRack;
        }
        public static List<clsPortOfRack> GetRackColumnByTag(int tag)
        {

            List<clsPortOfRack> portsofrack = new List<clsPortOfRack>();
            foreach (clsRack rack in RacksList)
            {
                var rackportscontaintag = rack.PortsStatus.Select(x => x).Where(x => x.TagNumbers.Contains(tag)).ToList();
                if (rackportscontaintag.Count() > 0)
                {
                    portsofrack.AddRange(rackportscontaintag);
                    break;
                }
            }
            return portsofrack;
        }

        public static RACK_CONTENT_STATE CargoStartTransferToDestineHandler(clsEQ sourceEQ, clsEQ destineEQ)
        {
            RACK_CONTENT_STATE raack_content_state = sourceEQ.RackContentState;
            if (raack_content_state == RACK_CONTENT_STATE.FULL)
            {
                destineEQ.Full_RACK_To_EQ();
            }
            else if (raack_content_state == RACK_CONTENT_STATE.EMPTY)
            {
                destineEQ.Empty_RACK_To_EQ();
            }
            else
            {

            }
            return raack_content_state;
        }

        public static List<int> GetBlockedTagsByEqIsMainain()
        {
            IEnumerable<clsEQ> maintainingEqCollection = MainEQList.Where(eq => eq.IsMaintaining);
            if (!maintainingEqCollection.Any())
                return new List<int>();

            return maintainingEqCollection.Select(eq => eq.EndPointOptions.TagID).ToList();

        }

        public static IEnumerable<int> GetUsableChargeStationTags(string agv_name)
        {

            if (ChargeStations.Count == 0)
            {
                return Enumerable.Empty<int>();
            }
            return ChargeStations.Where(station => station.IsAGVUsable(agv_name)).Select(station => station.EndPointOptions.TagID);

        }

        public static void ConfigurateEqGroupStore(List<EqGroupConfiguration> groupsConfigs)
        {
            EQGroupsStore = groupsConfigs.Select(config => new EqGroup(config)).ToList();
            File.WriteAllText(Configs.EQGroupConfigPath, JsonConvert.SerializeObject(groupsConfigs, Formatting.Indented));
        }


    }
}
