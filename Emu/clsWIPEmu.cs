//#define InitRackSensorStateEmu
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
        private clsRackOptions rackoption;
        public void StartEmu(clsRackOptions value)
        {
            this.rackoption = value;
            Console.WriteLine($"WIP Emulator Start(127.0.0.1:{value.ConnOptions.Port})");
            base.StartEmu(value);
        }
        protected override void DefaultInputsSetting()
        {
            foreach (clsRackPortProperty item in this.rackoption.PortsOptions)
            {
                slave.DataStore.InputDiscretes[item.IOLocation.Tray_Sensor1 + 1] = false;
                slave.DataStore.InputDiscretes[item.IOLocation.Tray_Sensor2 + 1] = false;
                slave.DataStore.InputDiscretes[item.IOLocation.Box_Sensor1 + 1] = false;
                slave.DataStore.InputDiscretes[item.IOLocation.Box_Sensor2 + 1] = false;
            }
#if InitRackSensorStateEmu
            try
            {
                Random rd = new Random();
                foreach (clsRackPortProperty item in this.rackoption.PortsOptions)
                {
                    slave.DataStore.InputDiscretes[item.IOLocation.Tray_Sensor1 + 1] = rd.Next(10) % 2 == 0 ? false : true;
                    slave.DataStore.InputDiscretes[item.IOLocation.Tray_Sensor2 + 1] = rd.Next(10) % 2 == 0 ? false : true;
                    slave.DataStore.InputDiscretes[item.IOLocation.Box_Sensor1 + 1] = rd.Next(10) % 2 == 0 ? false : true;
                    slave.DataStore.InputDiscretes[item.IOLocation.Box_Sensor2 + 1] = rd.Next(10) % 2 == 0 ? false : true;

                    //slave.DataStore.InputDiscretes[item.IOLocation.Tray_Sensor1 + 1] =  true;
                    //slave.DataStore.InputDiscretes[item.IOLocation.Tray_Sensor2 + 1] = true;
                    //slave.DataStore.InputDiscretes[item.IOLocation.Box_Sensor1 + 1] = true;
                    //slave.DataStore.InputDiscretes[item.IOLocation.Box_Sensor2 + 1] = true;

                }

            }
            catch (Exception ex)
            {
            }
            //slave.DataStore.InputDiscretes[1] = true;
            //slave.DataStore.InputDiscretes[2] = true;
            //slave.DataStore.InputDiscretes[3] = false;
            //slave.DataStore.InputDiscretes[4] = true;
           
#endif 
            var ushortVal = slave.DataStore.InputDiscretes.Skip(1).Take(16).ToArray().GetUshort();
            slave.DataStore.InputRegisters[1] = ushortVal;

        }
    }
}
