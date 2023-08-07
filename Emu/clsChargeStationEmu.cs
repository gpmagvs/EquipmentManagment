using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EquipmentManagment.ChargeStation;

namespace EquipmentManagment.Emu
{
    public class clsChargeStationEmu : clsDIOModuleEmu
    {
        Socket _socket;
        public override void StartEmu(int port = 502)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, port));
            _socket.Listen(10);
            StartAcceptClientConnectIn();
        }
        private class clsSocketState
        {
            public Socket socket;
            public byte[] buffer = new byte[1024];
        }

        private void StartAcceptClientConnectIn()
        {
            Task.Run(() =>
            {
                var client = _socket.Accept();
                clsSocketState socketState = new clsSocketState()
                {
                    socket = client,
                };
                client.BeginReceive(socketState.buffer, 0, 1024, SocketFlags.None, new AsyncCallback(RecieveCallback), socketState);
                Task.Factory.StartNew(() =>
                {
                    StartAcceptClientConnectIn();
                });
            });
        }

        private void RecieveCallback(IAsyncResult ar)
        {

            clsSocketState state = ar.AsyncState as clsSocketState;
            try
            {
                int rev = state.socket.EndReceive(ar);
                if (rev == 57)
                {
                    byte[] data = new ArraySegment<byte>(state.buffer, 0, 57).ToArray();
                    data[clsChargeStation.Indexes.VIN_H] = (byte)DateTime.Now.Second;
                    data[clsChargeStation.Indexes.VOUT_H] = 0x4F;
                    data[clsChargeStation.Indexes.Status_1] = 0xF0;
                    state.socket.Send(data);
                }
                Task.Factory.StartNew(() =>
                {
                    state.socket.BeginReceive(state.buffer, 0, 1024, SocketFlags.None, new AsyncCallback(RecieveCallback), state);
                });
            }
            catch (Exception ex)
            {
                state.socket?.Dispose();
            }
        }
    }
}
