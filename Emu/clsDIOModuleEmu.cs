﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EquipmentManagment.Connection;
using EquipmentManagment.Device.Options;
using EquipmentManagment.Tool;
using Modbus.Data;
using Modbus.Device;
using Modbus.Message;

namespace EquipmentManagment.Emu
{
    public class clsDIOModuleEmu : IDisposable
    {
        public clsEndPointOptions options;
        protected ModbusTcpSlave slave;
        private bool disposedValue;


        public virtual async void StartEmu(clsEndPointOptions value)
        {
            try
            {
                this.options = value;
                slave = ModbusTcpSlave.CreateTcp(0, new TcpListener(value.ConnOptions.Port));
                slave.ModbusSlaveRequestReceived += Master_ModbusSlaveRequestReceived;
                slave.DataStore = DataStoreFactory.CreateDefaultDataStore();
                DefaultInputsSetting();
                Console.WriteLine($"DIO Moudle Emulator Start({options.ConnOptions.IP}:{options.ConnOptions.Port})");
                await slave.ListenAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected virtual void DefaultInputsSetting()
        {
            slave.DataStore.InputDiscretes[5] = true;
            slave.DataStore.InputDiscretes[6] = true;
            slave.DataStore.InputDiscretes[7] = false;
            slave.DataStore.InputDiscretes[8] = false;
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
        }

        public bool SetStatusBUSY()
        {
            bool[] input = new bool[32];
            input[options.IOLocation.Up_Pose] = true;
            input[options.IOLocation.Port_Exist] = true;
            input[options.IOLocation.Eqp_Status_Down] = true;
            ModifyInputs(0, input);
            return true;
        }
        public bool SetStatusLoadable()
        {
            bool[] input = new bool[32];
            input[options.IOLocation.Load_Request] = true;
            input[options.IOLocation.Down_Pose] = true;
            input[options.IOLocation.Eqp_Status_Down] = true;
            ModifyInputs(0, input);
            return true;

        }

        public bool SetStatusUnloadable()
        {
            bool[] input = new bool[32];
            input[options.IOLocation.Unload_Request] = true;
            input[options.IOLocation.Up_Pose] = true;
            input[options.IOLocation.Eqp_Status_Down] = true;
            input[options.IOLocation.Port_Exist] = true;
            ModifyInputs(0, input);

            return true;
        }

        public bool GetInput(int index)
        {
            return slave.DataStore.InputDiscretes[index + 1];
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

        public void ModifyHoldingRegist(int address, ushort value)
        {
            slave.DataStore.HoldingRegisters[address + 1] = value;
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
            ModifyInput(options.IOLocation.HS_EQ_L_REQ, state);
            return true;
        }
        public bool SetHS_U_REQ(bool state)
        {
            ModifyInput(options.IOLocation.HS_EQ_U_REQ, state);
            return true;
        }
        public bool SetHS_READY(bool state)
        {
            ModifyInput(options.IOLocation.HS_EQ_READY, state);
            return true;
        }
        public bool SetHS_UP_READY(bool state)
        {
            ModifyInput(options.IOLocation.HS_EQ_UP_READY, state);
            return true;
        }
        public bool SetHS_LOW_READY(bool state)
        {
            ModifyInput(options.IOLocation.HS_EQ_LOW_READY, state);
            return true;
        }
        public bool SetHS_BUSY(bool state)
        {
            ModifyInput(options.IOLocation.HS_EQ_BUSY, state);
            return true;
        }

        public void SetUpPose()
        {
            ModifyInput(options.IOLocation.Up_Pose, true);
            ModifyInput(options.IOLocation.Down_Pose, false);

        }

        public void SetDownPose()
        {
            ModifyInput(options.IOLocation.Up_Pose, false);
            ModifyInput(options.IOLocation.Down_Pose, true);
        }

        public void SetUnknownPose()
        {
            ModifyInput(options.IOLocation.Up_Pose, false);
            ModifyInput(options.IOLocation.Down_Pose, false);
        }

        public void SetPortType(int portType)
        {
            ModifyHoldingRegist(options.IOLocation.HoldingRegists.PortTypeStatus, (ushort)portType);
        }

        internal void SetPartsReplacing(bool isReplacing)
        {
            ModifyInput(options.IOLocation.Eqp_PartsReplacing, isReplacing);
        }
    }
}
