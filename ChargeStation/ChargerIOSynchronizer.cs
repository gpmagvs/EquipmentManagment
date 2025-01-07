using EquipmentManagment.Emu;
using Modbus.Data;
using Modbus.Device;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentManagment.ChargeStation
{
    public partial class ChargerIOSynchronizer
    {
        private bool _Connected = false;
        public bool Connected
        {
            get => _Connected;
            private set
            {
                if (_Connected != value)
                {
                    _Connected = value;
                    if (value)
                        OnConnected?.Invoke(this, ChargerOption.Name);
                    else
                        OnDisconnected?.Invoke(this, ChargerOption.Name);
                }
            }
        }
        private ModbusIpMaster modbusMaster;
        private TcpClient tcpClient;
        public ChargerIOStates IOStates = new ChargerIOStates();
        public static event EventHandler<string> OnEMO;
        public static event EventHandler<string> OnSmokeDetected;
        public static event EventHandler<string> OnAirError;
        public static event EventHandler<string> OnTemperatureModuleError;
        public static event EventHandler<string> OnDisconnected;
        public static event EventHandler<string> OnConnected;
        public ChargerIOSynchronizer()
        {
        }
        public ChargerIOSynchronizer(clsChargeStationOptions chargerOption)
        {
            ChargerOption = chargerOption;
        }

        public clsChargeStationOptions ChargerOption { get; }

        internal async Task StartAsync()
        {

            IOStates.OnEMO += IOStates_OnEMO;
            IOStates.OnSmokeDetected += IOStates_OnSmokeDetected;
            IOStates.OnAirError += IOStates_OnAirError;
            IOStates.OnTemperatureError += IOStates_OnTemperatureError;

            await Task.Delay(1).ContinueWith(async tk =>
            {
                while (true)
                {

                    await Task.Delay(10);


                    if (!Connected)
                    {
                        (bool connected, string message) = await ConnectToAsync();
                        Connected = connected;
                        continue;
                    }

                    try
                    {
                        bool[] inputs = modbusMaster.ReadCoils(0, 16);

                        bool _emo = inputs[ChargerOption.IOLocation.Inputs.EMO];
                        bool _smoke = inputs[ChargerOption.IOLocation.Inputs.SMOKE_DETECT_ERROR];
                        bool _air = inputs[ChargerOption.IOLocation.Inputs.AIR_ERROR];
                        bool _temperature = inputs[ChargerOption.IOLocation.Inputs.TEMPERABURE_ABN];
                        bool _cylinder_fw = inputs[ChargerOption.IOLocation.Inputs.CYLINDER_FORWARD];
                        bool _cylinder_bw = inputs[ChargerOption.IOLocation.Inputs.CYLINDER_BACKWARD];
                        IOStates.UpdateIOAcutalState(_emo, _smoke, _air, _temperature, _cylinder_fw, _cylinder_bw);
                        IOStates.EMO = !_emo;
                        IOStates.SMOKE_DECTECTED = _smoke;
                        IOStates.AIR_ERROR = !_air;
                        IOStates.TEMPERATURE_MODULE_ABN = _temperature;
                        IOStates.CYLINDER_FORWARD = _cylinder_fw;
                        IOStates.CYLINDER_BACKWARD = _cylinder_bw;
                    }
                    catch (Exception ex)
                    {
                        Connected = false;
                        await Task.Delay(1000);
                    }
                }
            });
        }

        private void IOStates_OnAirError(object sender, EventArgs e)
        {
            OnAirError?.Invoke(this, ChargerOption.Name);
        }
        private void IOStates_OnTemperatureError(object sender, EventArgs e)
        {
            OnTemperatureModuleError?.Invoke(this, ChargerOption.Name);
        }

        private void IOStates_OnSmokeDetected(object sender, EventArgs e)
        {
            OnSmokeDetected?.Invoke(this, ChargerOption.Name);
        }

        private void IOStates_OnEMO(object sender, EventArgs e)
        {
            OnEMO?.Invoke(this, ChargerOption.Name);
        }

        private async Task<(bool confirm, string message)> ConnectToAsync()
        {
            try
            {
                tcpClient?.Dispose();
                modbusMaster?.Dispose();
            }
            catch (Exception ex)
            {
            }


            await Task.Delay(1000);
            try
            {
                tcpClient = new TcpClient(ChargerOption.IsEmulation ? "127.0.0.1" : ChargerOption.IOModubleConnOptions.IP, ChargerOption.IOModubleConnOptions.Port);
                modbusMaster = ModbusIpMaster.CreateIp(tcpClient);
                modbusMaster.Transport.WaitToRetryMilliseconds = 200;
                modbusMaster.Transport.RetryOnOldResponseThreshold = 10;
                modbusMaster.Transport.ReadTimeout = 300;
                modbusMaster.Transport.WriteTimeout = 1000;
                modbusMaster.Transport.Retries = 3;
                return (true, "");

            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

    }
}
