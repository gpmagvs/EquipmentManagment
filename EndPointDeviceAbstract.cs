using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EquipmentManagment.Connection;
using Modbus.Device;

namespace EquipmentManagment
{
    public abstract class EndPointDeviceAbstract : IDisposable
    {
        public EndPointDeviceAbstract(clsEndPointOptions options)
        {
            this.EndPointOptions = options;
        }
        public string EQName => EndPointOptions.Name;

        protected ModbusIpMaster master;

        public clsEndPointOptions EndPointOptions { get; set; } = new clsEndPointOptions();

        public abstract PortStatusAbstract PortStatus { get; set; }

        public bool IsConnected { get; set; }

        public List<bool> InputBuffer = new List<bool>();
        private bool disposedValue;

        /// <summary>
        /// 使用Modbus Tcp 連線
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="Port"></param>
        /// <returns></returns>
        public async Task<bool> Connect(bool use_for_conn_test = false)
        {
            await Task.Delay(1);

            bool connected = false;
            if (EndPointOptions.ConnOptions.ConnMethod == CONN_METHODS.MODBUS_TCP)
                connected = await _Connect(EndPointOptions.ConnOptions.IP, EndPointOptions.ConnOptions.Port);
            else
                connected = await _Connect(EndPointOptions.ConnOptions.ComPort);
            if (connected)
            {
                IsConnected = true;
                if (!use_for_conn_test)
                    StartSyncData();
                return IsConnected;
            }
            else
            {
                IsConnected = false;
                if (!use_for_conn_test)
                    _StartRetry();
                return IsConnected;
            }
        }

        private async Task _StartRetry()
        {
            await Connect();
        }

        private async Task<bool> _Connect(string IP, int Port)
        {
            try
            {
                var client = new TcpClient(IP, Port);
                master = ModbusIpMaster.CreateIp(client);
                master.Transport.ReadTimeout = 5000;
                master.Transport.WriteTimeout = 5000;
                master.Transport.Retries = 10;
                return true;
            }
            catch (Exception ex)
            {
                master = null;
                return false;
            }
        }

        /// <summary>
        /// 使用Modbus RTU
        /// </summary>
        /// <param name="comport"></param>
        /// <returns></returns>
        public virtual async Task<bool> _Connect(string comport)
        {
            return false;
        }

        public void StartSyncData()
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    while (IsConnected)
                    {

                        await Task.Delay(10);
                        var inputs = master.ReadInputs(0, 8);
                        InputBuffer = inputs.ToList();
                        DefineInputData();
                    }
                }
                catch (NullReferenceException ex)
                {
                    IsConnected = false;
                    _StartRetry();
                }
                catch (Exception ex)
                {
                    IsConnected = false;
                    _StartRetry();
                }
            });
        }

        protected abstract void DefineInputData();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                }
                master?.Transport.Dispose();
                master?.Dispose();
                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                disposedValue = true;
            }
        }

        // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
        // ~EndPointDeviceAbstract()
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
    }
}
