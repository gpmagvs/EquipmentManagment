using CIMComponent;
using EquipmentManagment.Device.Options;
using EquipmentManagment.MainEquipment;
using EquipmentManagment.Manager;
using EquipmentManagment.PLC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentManagment.Emu
{
    internal class clsPLCMCProtocolEmu : clsDIOModuleEmu
    {
        internal clsPLCMCProtocolEmu() : base() { }
        MemoryTable EQMemoryTable;  clsPLCMemOption PLCMemOption;


        public override async Task StartEmu(clsEndPointOptions value)
        {
            this.options = value;

            clsEQ eqInstance = StaEQPManagager.MainEQList.FirstOrDefault(eq => eq.EndPointOptions.Name == value.Name);
            if (eqInstance != null)
            {
                EQMemoryTable = eqInstance.EQPLCMemoryTb;
                PLCMemOption = eqInstance.PLCMemOption;
            }
            
            DefaultInputsSetting();

        }

        protected override void DefaultInputsSetting()
        {
            if (EQMemoryTable == null)
                return;
            string bitStart = PLCMemOption.EQP_Bit_Start_Address;
            EQMemoryTable.WriteOneBit(bitStart,true);
            //base.DefaultInputsSetting();
        }
        public override void ModifyInputs(int startIndex, bool[] value)
        {
            int index = startIndex;
            foreach (var val in value)
            {
                ModifyInput(index,val);
                index += 1;
            }
        }
        public override void ModifyInput(int index, bool value)
        {
            string speficAddHex = PLCMemOption.EQPBitAreaName;
            // to hex
            if (PLCMemOption.IsEQP_Bit_Hex)
            {
                int values = Convert.ToInt32(PLCMemOption.EQPBitStartAddressName, 16);  // Returns 255
                speficAddHex += (values + index).ToString("X2");
            }
            else
            {
                int.TryParse(PLCMemOption.EQPBitStartAddressName, out int startAdd);
                speficAddHex+=(startAdd + 13).ToString("X2");
            }
            EQMemoryTable.WriteOneBit(speficAddHex, value);
        }
    }
}
