using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.MainEquipment
{
    public class clsEQManagementConfigs
    {
        public string EQConfigPath { get; set; } = "EQConfigs.json";
        public string ChargeStationConfigPath { get; set; } = "ChargStationConfigs.json";
        public string WIPConfigPath { get; set; } = "WIPConfigs.json";

        public string EQGroupConfigPath { get; set; } = "EQGroupConfigs.json";
        public string RackGroupConfigPath { get; set; } = "RackGroupConfigs.json";
    }
}
