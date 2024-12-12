using EquipmentManagment.Device.Options;
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentManagment.Emu
{
    public abstract class EQEmulatorBase: IDisposable
    {
        public clsEndPointOptions options;
        protected ModbusTcpSlave slave;
        protected bool disposedValue;



        public EQEmulatorBase() { }

        public abstract  Task StartEmu(clsEndPointOptions value);
        protected abstract  void DefaultInputsSetting();


        public abstract bool GetInput(int index);
        public abstract void ModifyInput(int index, bool value);

        public abstract void ModifyInputs(int startIndex, bool[] value);

        public abstract void ModifyHoldingRegist(int address, ushort value);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                disposedValue = true;
            }
        }

        // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
        // ~EmulatorBase()
        // {
        //     // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public abstract bool SetStatusLoadable();
        public abstract bool SetStatusUnloadable();
        public abstract bool SetStatusBUSY();
        public abstract void SetPartsReplacing(bool isReplacing);
        public abstract void SetCarrierIDRead(string carrierID);

        public abstract bool SetHS_L_REQ(bool state);
        public abstract bool SetHS_U_REQ(bool state);
        public abstract bool SetHS_READY(bool state);
        public abstract bool SetHS_UP_READY(bool state);
        public abstract bool SetHS_LOW_READY(bool state);
        public abstract bool SetHS_BUSY(bool state);
        public abstract void SetUpPose();
        public abstract void SetDownPose();
        public abstract void SetUnknownPose();
        public abstract void SetPortType(int portType);
        public abstract void SetPortExist(int portExist);
    }
}
