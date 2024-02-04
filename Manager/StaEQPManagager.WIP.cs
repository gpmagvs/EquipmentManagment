using EquipmentManagment.WIP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EquipmentManagment.Manager
{
    public partial class StaEQPManagager
    {

        public struct WIPController
        {
            public static bool TryGetWIPByWIPName(string name, out clsRack wip)
            {
                try
                {
                    wip = RacksList.FirstOrDefault(rack => rack.EQName == name);
                    return wip != null;
                }
                catch (Exception)
                {
                    wip = null;
                    return false;
                }
            }

            public static void ModifyCargoID(string wIPID, string portID, string newCargoID)
            {
                if (TryGetWIPByWIPName(wIPID, out var WIP))
                {
                    WIP.ModifyPortCargoID(portID,newCargoID);
                }
            }
        }
    }
}
