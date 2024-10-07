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
        public bool Connected { get; private set; } = false;
        private ModbusIpMaster modbusMaster;
        private TcpClient tcpClient;
        public ChargerIOStates IOStates = new ChargerIOStates();
        public static event EventHandler<string> OnEMO;
        public static event EventHandler<string> OnSmokeDetected;
        public static event EventHandler<string> OnAirError;
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

                        IOStates.EMO = inputs[ChargerOption.IOLocation.Inputs.EMO];
                        IOStates.SMOKE_DECTECTED = inputs[ChargerOption.IOLocation.Inputs.SMOKE_DETECT_ERROR];
                        IOStates.AIR_ERROR = inputs[ChargerOption.IOLocation.Inputs.AIR_ERROR];
                        IOStates.CYLINDER_FORWARD = inputs[ChargerOption.IOLocation.Inputs.CYLINDER_FORWARD];
                        IOStates.CYLINDER_BACKWARD = inputs[ChargerOption.IOLocation.Inputs.CYLINDER_BACKWARD];

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

            //模擬server
            if (ChargerOption.IsEmulation)
                StartEmulator();

            await Task.Delay(1000);
            try
            {
                tcpClient = new TcpClient(ChargerOption.IOModubleConnOptions.IP, ChargerOption.IOModubleConnOptions.Port);
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
