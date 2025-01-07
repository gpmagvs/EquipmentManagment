using Modbus.Data;
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentManagment.ChargeStation
{
    public partial class ChargerIOSynchronizer
    {
        private ModbusTcpSlave emuSlave;
        public void EMOEmulate(bool isEMO)
        {
            emuSlave.DataStore.CoilDiscretes[ChargerOption.IOLocation.Inputs.EMO + 1] = isEMO;
        }
        public void SmokeDetectedEmulate(bool isSmokeDetected)
        {
            emuSlave.DataStore.CoilDiscretes[ChargerOption.IOLocation.Inputs.SMOKE_DETECT_ERROR + 1] = isSmokeDetected;
        }
        public void AirErrorEmulate(bool isAirError)
        {
            emuSlave.DataStore.CoilDiscretes[ChargerOption.IOLocation.Inputs.AIR_ERROR + 1] = isAirError;
        }
        public void StartEmulator()
        {
            CloseEmulator();
            _ = Task.Run(async () =>
            {
                emuSlave = ModbusTcpSlave.CreateTcp(0, new TcpListener(ChargerOption.IOModubleConnOptions.Port));
                emuSlave.ModbusSlaveRequestReceived += Master_ModbusSlaveRequestReceived;
                emuSlave.DataStore = DataStoreFactory.CreateDefaultDataStore();
                await emuSlave.ListenAsync();
            });
        }
        public void CloseEmulator()
        {
            emuSlave?.Dispose();
        }
        private void Master_ModbusSlaveRequestReceived(object sender, ModbusSlaveRequestEventArgs e)
        {
        }


    }
}
