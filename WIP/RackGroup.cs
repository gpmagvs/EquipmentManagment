using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.WIP
{
    public class RackGroup
    {
        public string Display { get; set; } = "";

        public List<string> SubRacksNames { get; set; } = new List<string>();

        public RackGroup() { }
    }
}
