using EquipmentManagment.Device.Options;
using EquipmentManagment.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EquipmentManagment.Emu
{
    public class clsWIPEmu : clsDIOModuleEmu
    {
        

        public void StartEmu(clsRackOptions value)
        {
            Console.WriteLine($"WIP Emulator Start(127.0.0.1:{value.ConnOptions.Port})");
            base.StartEmu(value);
        }
        protected override void DefaultInputsSetting()
        {
            slave.DataStore.InputDiscretes[1] = true;
            slave.DataStore.InputDiscretes[2] = true;
            slave.DataStore.InputDiscretes[3] = false;
            slave.DataStore.InputDiscretes[4] = true;
            var ushortVal = slave.DataStore.InputDiscretes.Skip(1).Take(16).ToArray().GetUshort();
            slave.DataStore.InputRegisters[1] = ushortVal;
        }
    }
}
