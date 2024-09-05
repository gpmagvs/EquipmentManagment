using EquipmentManagment.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EquipmentManagment.MainEquipment.EQGroup
{
    public class EqGroup
    {
        public readonly EqGroupConfiguration configuration;
        public EqGroup(EqGroupConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// 取得所有Unload Port EQ
        /// </summary>
        public List<clsEQ> UnloadPorts
        {
            get
            {
                if (this.configuration.UnloadPortEqTags.Any())
                {
                    return this.configuration.UnloadPortEqTags
                                              .Select(tag => StaEQPManagager.GetEQByTag(tag))
                                              .Where(eq => eq != null)
                                              .ToList();
                }
                else
                    return new List<clsEQ>();
            }
        }
        /// <summary>
        /// 取得所有Load Port EQ
        /// </summary>
        public List<clsEQ> LoadPorts
        {
            get
            {
                if (this.configuration.LoadPortEqTags.Any())
                {
                    return this.configuration.LoadPortEqTags
                                             .Select(tag => StaEQPManagager.GetEQByTag(tag))
                                             .Where(eq => eq != null)
                                             .ToList();
                }
                else
                    return new List<clsEQ>();
            }
        }
    }
}
