﻿using EquipmentManagment.BatteryExchanger;
using EquipmentManagment.ChargeStation;
using EquipmentManagment.Device.Options;
using EquipmentManagment.Device;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.WIP;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using EquipmentManagment.MainEquipment.EQGroup;
using static EquipmentManagment.MainEquipment.clsEQ;

namespace EquipmentManagment.Manager
{
    public partial class StaEQPManagager
    {

        public static event EventHandler<Dictionary<string, clsRackOptions>> OnWIPConfigLoaded;
        public static event EventHandler<Dictionary<string, clsEndPointOptions>> OnEqConfigLoaded;
        public static async void InitializeAsync()
        {
            InitializeAsync(Configs == null ? new clsEQManagementConfigs() : Configs);
        }

        public static void InitializeAsync(clsEQManagementConfigs _Configs)
        {
            try
            {
                //DisposeEQs();
                Configs = _Configs;
                _LoadChargeStationConfigs(_Configs.ChargeStationConfigPath);
                _LoadEqConfigs(_Configs.EQConfigPath);
                _LoadWipConfigs(_Configs.WIPConfigPath);

                foreach (KeyValuePair<string, clsChargeStationOptions> item in ChargeStationsOptions)
                {
                    var eqName = item.Key;
                    var options = item.Value;

                    if (options.Name != item.Key)
                        options.Name = eqName;

                    var charge_station = options.chip_brand == clsChargeStationOptions.CHARGER_INTERFACE.PMBUS_REV_1_1 ? new clsChargeStationGY7601Base(options) : new clsChargeStation(options);
                    if (charge_station != null)
                    {
                        ChargeStations.Add(charge_station);
                    }
                }

                foreach (KeyValuePair<string, clsRackOptions> item in RacksOptions)
                {
                    var eqName = item.Key;
                    var options = item.Value;

                    if (options.Name != item.Key)
                        options.Name = eqName;

                    clsRack Rack = new clsRack(options);
                    RacksList.Add(Rack);
                }


                foreach (KeyValuePair<string, clsEndPointOptions> item in EQOptions)
                {
                    var eqName = item.Key;
                    var options = item.Value;

                    if (options.Name != item.Key)
                        options.Name = eqName;

                    EndPointDeviceAbstract EQ = null;
                    if (item.Value.IsProdution_EQ)
                    {
                        EQ = new clsEQ(options);
                    }
                    else if (item.Value.EqType == EQ_TYPE.BATTERY_EXCHANGER)
                        EQ = new clsBatteryExchanger(options);

                    if (EQ == null)
                        continue;
                    EQPDevices.Add(EQ);
                }

                InitEQGroup(_Configs.EQGroupConfigPath);

                clsEQ.OnPortCarrierIDChanged += ClsEQ_OnPortCarrierIDChanged;
                clsEQ.OnCheckEQPortBelongTwoLayersEQOrNot += ClsEQ_OnCheckEQPortBelongTwoLayersEQOrNot;
                clsRack.OnRackPortCarrierIDChanged += ClsRack_OnRackPortCarrierIDChanged;
                EmulatorsInitialize(_Configs);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static async Task DevicesConnectToAsync()
        {
            Console.WriteLine("Start Device Connect To..");
            List<EndPointDeviceAbstract> deviceList = new List<EndPointDeviceAbstract>();
            deviceList.AddRange(EQPDevices);
            deviceList.AddRange(RacksList);
            deviceList.AddRange(ChargeStations);

            foreach (var item in deviceList)
                Task.Run(() => item.StartSyncData());
        }

        private static bool ClsEQ_OnCheckEQPortBelongTwoLayersEQOrNot(clsEQ eq, EventArgs e)
        {
            int portTag = eq.EndPointOptions.TagID;
            return MainEQList.Count(_eq => _eq.EndPointOptions.TagID == portTag) > 1;
        }

        private static void InitEQGroup(string eQGroupConfigPath)
        {
            if (!File.Exists(eQGroupConfigPath))
            {
                string _json = JsonConvert.SerializeObject(EQGroupsStore, Formatting.Indented);
                File.WriteAllText(eQGroupConfigPath, _json);
            }
            else
            {
                string json = _LoadConfigJson(eQGroupConfigPath);
                List<EqGroupConfiguration> EQGroupConfig = JsonConvert.DeserializeObject<List<EqGroupConfiguration>>(json);
                ConfigurateEqGroupStore(EQGroupConfig);
            }
        }

        private static void InitRackGroup(string rackGroupConfigPath)
        {

        }

        private static void ClsRack_OnRackPortCarrierIDChanged(object sender, (string portID, string carrierID) e)
        {
            clsRack _rack = (clsRack)sender;
            if (_rack.RackOption.MaterialInfoFromEquipment)
            {
                clsPortOfRack rackPort = _rack.PortsStatus.FirstOrDefault(port => port.Properties.ID == e.portID);
                if (rackPort != null)
                {
                    int[] tags = _rack.RackOption.ColumnTagMap[rackPort.Properties.Column];

                    var modified = MainEQList.Where(eq => tags.Contains(eq.EndPointOptions.TagID))
                                              .Select(eq =>
                                              {
                                                  eq.PortStatus.CarrierID = e.carrierID;
                                                  return eq;
                                              });

                    Console.WriteLine($"EQ Carrier ID Modified By WIP Cariier ID Changed = {modified.Count()}");
                }
            }
        }

        private static void ClsEQ_OnPortCarrierIDChanged(object sender, string carrierID)
        {
            clsEQ eq = (clsEQ)sender;
            IEnumerable<clsRack> rackUseEqMatterals = RacksList.Where(rack => rack.RackOption.MaterialInfoFromEquipment);
            clsRack rackAssign = rackUseEqMatterals.FirstOrDefault(rack => rack.RackOption.ColumnTagMap.Values.SelectMany(tags => tags).Contains(eq.EndPointOptions.TagID));
            if (rackAssign != null)
            {
                int column = rackAssign.RackOption.ColumnTagMap.FirstOrDefault(tags => tags.Value.Contains(eq.EndPointOptions.TagID)).Key;
                rackAssign.ModifyPortCargoID($"0-{column}", carrierID, triggerByEqCarrierIDChanged: true);
            }
        }


        private static void EmulatorsInitialize(clsEQManagementConfigs _Configs)
        {
            //if (!_Configs.UseEqEmu)
            //    return;
            int emu_port = 5600;
            foreach (var option in EQOptions.Values)
            {
                if (option.IsEmulation == false)
                    continue;
                option.ConnOptions.IP = "127.0.0.1";
                option.ConnOptions.Port = emu_port;
                emu_port += 2;
            }
            emu_port = 6600;
            foreach (var option in ChargeStationsOptions.Values)
            {
                if (option.IsEmulation == false)
                    continue;
                option.ConnOptions.IP = "127.0.0.1";
                option.ConnOptions.Port = emu_port;
                emu_port += 2;
            }

            int rack_emu_port = 6300;
            foreach (var option in RacksOptions.Values)
            {
                if (option.IsEmulation == false)
                    continue;
                option.ConnOptions.IP = "127.0.0.1";
                option.ConnOptions.Port = rack_emu_port;
                rack_emu_port += 1;

                ushort port_io = default(ushort);
                foreach (clsRackPortProperty port in option.PortsOptions)
                {
                    port.IOLocation.Tray_Sensor1 = port_io;
                    port_io++;
                    port.IOLocation.Tray_Sensor2 = port_io;
                    port_io++;
                    port.IOLocation.Box_Sensor1 = port_io;
                    port_io++;
                    port.IOLocation.Box_Sensor2 = port_io;
                    port_io++;
                }
            }

            StaEQPEmulatorsManagager.InitEmu(
                    EQOptions.Where(opt => opt.Value.IsEmulation || opt.Value.EmulationMode == 1).ToDictionary(kp => kp.Key, kp => kp.Value),
                    ChargeStationsOptions.Where(opt => opt.Value.IsEmulation || opt.Value.EmulationMode == 1).ToDictionary(kp => kp.Key, kp => kp.Value),
                    RacksOptions.Where(opt => opt.Value.IsEmulation || opt.Value.EmulationMode == 1).ToDictionary(kp => kp.Key, kp => kp.Value)
                );
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
            OnEqConfigLoaded?.Invoke(string.Empty, EQOptions);
            SaveEqConfigs();

        }
        private static void _LoadWipConfigs(string wIPConfigPath)
        {
            if (!File.Exists(wIPConfigPath))
            {
                clsRackOptions _defaultRackOption = new clsRackOptions
                {
                    Name = "WIP-1",
                    TagID = 1,
                    Columns = 3,
                    Rows = 3,
                    ColumnTagMap = new Dictionary<int, int[]>() { { 0, new int[] { -1 } } }
                };
                for (int row = 0; row < _defaultRackOption.Rows; row++)
                {
                    for (int col = 0; col < _defaultRackOption.Columns; col++)
                    {
                        _defaultRackOption.PortsOptions.Add(new clsRackPortProperty
                        {
                            Row = row,
                            Column = col,
                        });
                    }
                }
                RacksOptions.Add(_defaultRackOption.Name, _defaultRackOption);
                string _json = JsonConvert.SerializeObject(RacksOptions, Formatting.Indented);
                File.WriteAllText(wIPConfigPath, _json);
            }
            else
            {
                string json = _LoadConfigJson(wIPConfigPath);
                RacksOptions = JsonConvert.DeserializeObject<Dictionary<string, clsRackOptions>>(json);
                foreach (var rackOpt in RacksOptions.Values)
                {
                    if (rackOpt.ColumnTagMap.Count == 0)
                    {
                        for (int i = 0; i < rackOpt.Columns; i++)
                        {
                            rackOpt.ColumnTagMap.Add(i, new int[1] { i });
                        }
                    }
                }
                File.WriteAllText(wIPConfigPath, JsonConvert.SerializeObject(RacksOptions, Formatting.Indented));//write back
            }

            OnWIPConfigLoaded?.Invoke(string.Empty, RacksOptions);
            File.WriteAllText(wIPConfigPath, JsonConvert.SerializeObject(RacksOptions, Formatting.Indented));//write back
        }
    }
}
