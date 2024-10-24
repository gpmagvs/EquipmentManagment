using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentManagment.Device.TemperatureModuleDevice
{
    /// <summary>
    /// 溫控模組
    /// </summary>
    public abstract class TemperatureModuleAbstract
    {
        public SerialPort serialPort { get; protected set; }
        public Socket socket { get; protected set; }

        public TemperatureModuleSetupOptions SetupOptions { get; set; } = new TemperatureModuleSetupOptions();

        public bool Connected { get; protected set; } = false;
        private double _CurrentTemperature = -1;
        public double CurrentTemperature
        {
            get => _CurrentTemperature;
            set
            {
                if (_CurrentTemperature != value)
                {
                    _CurrentTemperature = value;
                    OnTemperatureChanged?.Invoke(this, value);
                }
            }
        }

        public event EventHandler<double> OnTemperatureChanged;

        protected TemperatureModuleAbstract(TemperatureModuleSetupOptions setupOptions)
        {
            SetupOptions = setupOptions;
        }

        public virtual async Task<bool> BeginAsync()
        {
            if (!SetupOptions.Enable)
            {
                Console.WriteLine("Temperature Module is disabled.");
                return false;
            }

            await Task.Delay(1000);
            bool _connected = await ConnectTest();
            Connected = _connected;
            if (_connected)
            {
                StartDataPolling();
            }
            else
            {
                await Task.Delay(1000);
                _ = Task.Factory.StartNew(() => BeginAsync());
            }
            return _connected;
        }

        public async Task<bool> ConnectTest()
        {
            bool _connected = SetupOptions.Protocol == TemperatureModuleSetupOptions.COMMUNICATION_PROTOCOL.SERIAL_PORT ? await _ConnectWithSerialPort() : await _ConnectWithTcpSocket();
            return _connected;
        }

        protected virtual async Task<double> GetTemperature()
        {
            return 0;
        }

        private async Task StartDataPolling()
        {
            await Task.Delay(1000).ContinueWith(async tk =>
            {
                Console.WriteLine("Temperature Module is polling data...");
                while (Connected)
                {
                    await Task.Delay(1000);

                    try
                    {
                        CurrentTemperature = await GetTemperature();
                        Console.WriteLine($"Temperature read from {GetType().Name}: {CurrentTemperature}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Connected = false;
                    }

                }
                Console.WriteLine("Temperature Module is reconnecting...");
                BeginAsync();

            });
        }

        protected async Task<byte[]> SendDataToDevice(byte[] command, bool waitReply = true)
        {
            byte[] buffer = new byte[1024];
            if (SetupOptions.Protocol == TemperatureModuleSetupOptions.COMMUNICATION_PROTOCOL.SERIAL_PORT)
            {
                serialPort?.Write(command, 0, command.Length);
                if (!waitReply)
                    return new byte[0];
                await Task.Delay(100);
                serialPort.Read(buffer, 0, buffer.Length);
            }
            else
            {
                socket?.Send(command);
                if (!waitReply)
                    return new byte[0];
                await Task.Delay(100);
                socket.Receive(buffer);
            }
            return buffer;
        }

        private async Task<bool> _ConnectWithSerialPort()
        {
            try
            {
                serialPort = new SerialPort(SetupOptions.COM, SetupOptions.BaudRate, Parity.Even);
                serialPort.Open();
                return serialPort.IsOpen;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private async Task<bool> _ConnectWithTcpSocket()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(SetupOptions.IP, SetupOptions.Port);
                return socket.Connected;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public class TemperatureModuleSetupOptions
        {
            public enum COMMUNICATION_PROTOCOL
            {
                SERIAL_PORT, TCPIP
            }

            public COMMUNICATION_PROTOCOL Protocol { get; set; } = COMMUNICATION_PROTOCOL.SERIAL_PORT;
            public string COM { get; set; } = "COM5";
            public string IP { get; set; } = "192.168.0.23";
            public int Port { get; set; } = 10001;
            public int BaudRate { get; set; } = 9600;
            public bool Enable { get; set; } = false;

        }

    }
}
