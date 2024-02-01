using Modbus.Data;
using Modbus.Device;
using Modbus.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EquipmentManagment.MainEquipment
{
    public class clsStatusIOModbusGateway
    {
        private ModbusTcpSlave slave;
        public int Port { get; set; }

        public event EventHandler<bool[]> OnAGVOutputsChanged;

        public virtual void StartGateway(int port)
        {
            Port = port;
            slave = ModbusTcpSlave.CreateTcp(0, new TcpListener(Port));
            slave.ModbusSlaveRequestReceived += Slave_ModbusSlaveRequestReceived;
            slave.DataStore = DataStoreFactory.CreateDefaultDataStore();
            slave.ListenAsync();
            WatchMasterClientConnectionStatus();


        }
        private Dictionary<TcpClient, DateTime> tcpClients = new Dictionary<TcpClient, DateTime>();
        private int _clients_num = 0;
        public int clients_num
        {
            get => _clients_num;
            set
            {
                if (_clients_num != value)
                {
                    _clients_num = value;
                    Console.WriteLine($"AGV Modbus Getway - {Port}| Client Number changed to -{_clients_num}");
                }
            }
        }
        private void WatchMasterClientConnectionStatus()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                    clients_num = slave.Masters.Count();
                    foreach (var master in slave.Masters)
                    {
                        if (!tcpClients.ContainsKey(master))
                            tcpClients.Add(master, DateTime.Now);
                    }
                    if (tcpClients.Count > 1)
                    {
                        for (int i = 0; i < tcpClients.Count - 1; i++)
                        {
                            tcpClients.Keys.ToList()[i].Close();
                        }
                        Console.WriteLine($"Close {tcpClients.Count - 1} connection.");
                    }
                }
            });
        }

        public void StoreEQOutpus(bool[] outputs)
        {
            for (int i = 0; i < outputs.Length; i++)
            {
                slave.DataStore.InputDiscretes[i + 1] = outputs[i];
            }
        }
        private string last_coil_str = "";
        private void Slave_ModbusSlaveRequestReceived(object sender, ModbusSlaveRequestEventArgs e)
        {
            if (e.Message is WriteMultipleCoilsRequest request)
            {
                ushort startAddress = request.StartAddress;
                bool[] coils = request.Data.ToArray();
                ushort coilCount = (ushort)coils.Length;
                string coilval_str = string.Join(",", coils);

                if (coilval_str != last_coil_str)
                    OnAGVOutputsChanged?.Invoke(this, coils);
                last_coil_str = coilval_str;
                try
                {
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
    }
}
