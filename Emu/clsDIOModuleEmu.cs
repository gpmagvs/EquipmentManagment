using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Modbus.Data;
using Modbus.Device;
using Modbus.Message;

namespace EquipmentManagment.Emu
{
    public class clsDIOModuleEmu : IDisposable
    {
        private ModbusTcpSlave slave;
        private bool disposedValue;

        public void StartEmu(int port = 502)
        {

            slave = ModbusTcpSlave.CreateTcp(0, new TcpListener(port));
            slave.ModbusSlaveRequestReceived += Master_ModbusSlaveRequestReceived;
            slave.DataStore = DataStoreFactory.CreateDefaultDataStore();
            bool[] inputs = new bool[4096];
            if (port % 2 == 0)
            {
                slave.DataStore.InputDiscretes[1] = false;
                slave.DataStore.InputDiscretes[2] = false;
                slave.DataStore.InputDiscretes[3] = false;
                slave.DataStore.InputDiscretes[4] = false;
                slave.DataStore.InputDiscretes[5] = true;
                slave.DataStore.InputDiscretes[6] = true;
                slave.DataStore.InputDiscretes[7] = false;
                slave.DataStore.InputDiscretes[8] = true;
            }
            else
            {
                slave.DataStore.InputDiscretes[1] = true;
                slave.DataStore.InputDiscretes[2] = false;
                slave.DataStore.InputDiscretes[3] = false;
                slave.DataStore.InputDiscretes[4] = false;
                slave.DataStore.InputDiscretes[5] = true;
                slave.DataStore.InputDiscretes[6] = true;
                slave.DataStore.InputDiscretes[7] = false;
                slave.DataStore.InputDiscretes[8] = true;
            }
            slave.ListenAsync();

        }

        public bool SetStatusBUSY()
        {
            ModifyInputs(0, new bool[8] { false, false, true, false, true, true, true, false });
            return true;
        }
        public bool SetStatusLoadable()
        {
            ModifyInputs(0, new bool[8] { true, false, false, false, true, true, true, false });
            return true;

        }
        public bool SetStatusUnloadable()
        {
            ModifyInputs(0, new bool[8] { false, true, true, false, true, true, true, false });
            return true;
        }
        public void ModifyInput(int index, bool value)
        {
            slave.DataStore.InputDiscretes[index + 1] = value;
        }

        public void ModifyInputs(int startIndex, bool[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                slave.DataStore.InputDiscretes[i + 1] = value[i];
            }
        }
        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Master_ModbusSlaveRequestReceived(object sender, ModbusSlaveRequestEventArgs e)
        {

            if (e.Message is WriteMultipleCoilsRequest request)
            {
                ushort startAddress = request.StartAddress;
                bool[] coils = request.Data.ToArray();
                ushort coilCount = (ushort)coils.Length;

                try
                {
                    // 執行寫操作，這裡只是示例，你需要根據你的實際需求進行處理
                    for (int i = 0; i < coilCount; i++)
                    {
                        bool value = coils[i];
                        slave.DataStore.CoilDiscretes[startAddress + i] = value;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                }
                slave?.Dispose();
                disposedValue = true;
            }
        }
      
    }
}
