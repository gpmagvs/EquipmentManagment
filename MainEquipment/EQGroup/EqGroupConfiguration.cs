using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.MainEquipment.EQGroup
{
    public class EqGroupConfiguration
    {
        public string EqGroupName { get; set; } = "";
        public List<int> LoadPortEqTags { get; set; } = new List<int>();
        public List<int> UnloadPortEqTags { get; set; } = new List<int>();
    }
}
