using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using EquipmentManagment.Connection;
using EquipmentManagment.Tool;
using Modbus.Data;
using Modbus.Device;
using Modbus.Message;

namespace EquipmentManagment.Emu
{
    public class clsDIOModuleEmu : IDisposable
    {

        private ModbusTcpSlave slave;
        private bool disposedValue;


        public virtual void StartEmu(int port = 502)
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

            slave.DataStore.InputDiscretes[9] = false; //LREQ
            slave.DataStore.InputDiscretes[10] = false;//UREQ
            slave.DataStore.InputDiscretes[11] = false;//READY
            slave.DataStore.InputDiscretes[12] = true;//BUSY
            slave.DataStore.InputDiscretes[13] = false;
            slave.DataStore.InputDiscretes[14] = false;
            slave.DataStore.InputDiscretes[15] = false;
            slave.DataStore.InputDiscretes[16] = false;


            var ushortVal = slave.DataStore.InputDiscretes.Skip(1).Take(16).ToArray().GetUshort();
            slave.DataStore.InputRegisters[1] = ushortVal;


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
            var ushortVal = slave.DataStore.InputDiscretes.Skip(1).Take(16).ToArray().GetUshort();
            slave.DataStore.InputRegisters[1] = ushortVal;

        }

        public void ModifyInputs(int startIndex, bool[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                slave.DataStore.InputDiscretes[i + 1] = value[i];
            }
            var ushortVal = slave.DataStore.InputDiscretes.Skip(1).Take(16).ToArray().GetUshort();
            slave.DataStore.InputRegisters[1] = ushortVal;
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

        public bool SetHS_L_REQ(bool state)
        {
            ModifyInput(8, state);
            return true;
        }
        public bool SetHS_U_REQ(bool state)
        {
            ModifyInput(9, state);
            return true;
        }
        public bool SetHS_READY(bool state)
        {
            ModifyInput(10, state);
            return true;
        }
        public bool SetHS_BUSY(bool state)
        {
            ModifyInput(11, state);
            return true;
        }



    }
}
