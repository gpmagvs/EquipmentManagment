using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EquipmentManagment.ChargeStation;
using EquipmentManagment.Connection;
using EquipmentManagment.Device.Options;
using EquipmentManagment.Tool;

namespace EquipmentManagment.Emu
{
    public class clsChargeStationEmu : clsDIOModuleEmu
    {
        Socket _socket;
        bool _IsCharging = true;
        private double CC = 33;
        private double CV = 48;
        private double FV = 44;
        private double TC = 21;
        private byte[] CC_Bytes => (Convert.ToInt32(Math.Round(CC * 10))).GetHighLowBytes();
        private byte[] CV_Bytes => (Convert.ToInt32(Math.Round(CV * 10))).GetHighLowBytes();
        private byte[] FV_Bytes => (Convert.ToInt32(Math.Round(FV * 10))).GetHighLowBytes();
        private byte[] TC_Bytes => (Convert.ToInt32(Math.Round(TC * 10))).GetHighLowBytes();
        public override async Task StartEmu(clsEndPointOptions value)
        {

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, value.ConnOptions.Port));
            _socket.Listen(10);
            StartAcceptClientConnectIn();
        }

        private class clsSocketState
        {
            public Socket socket;
            public byte[] buffer = new byte[1024];
        }
        private bool SettingFlag = false;
        private void StartAcceptClientConnectIn()
        {
            Task.Run(() =>
            {
                var client = _socket.Accept();
                clsSocketState socketState = new clsSocketState()
                {
                    socket = client,
                };

                //Task.Factory.StartNew(() =>
                //{
                //    var data = new byte[57];
                //    data[13] = 0x51;
                //    data[clsChargeStation.Indexes.VIN_H] = (byte)DateTime.Now.Second;
                //    data[clsChargeStation.Indexes.VOUT_H] = 0x4F;
                //    data[clsChargeStation.Indexes.Status_1] = 0xF0;
                //    data[clsChargeStation.Indexes.CC_L] = CC_Bytes[0];
                //    data[clsChargeStation.Indexes.CC_H] = CC_Bytes[1];
                //    data[clsChargeStation.Indexes.CV_L] = CV_Bytes[0];
                //    data[clsChargeStation.Indexes.CV_H] = CV_Bytes[1];
                //    data[clsChargeStation.Indexes.FV_L] = FV_Bytes[0];
                //    data[clsChargeStation.Indexes.FV_H] = FV_Bytes[1];
                //    data[clsChargeStation.Indexes.TC_L] = TC_Bytes[0];
                //    data[clsChargeStation.Indexes.TC_H] = TC_Bytes[1];

                //    client.Send(data);
                //});
                Thread _timer = new Thread(() =>
                {
                    Thread.Sleep(1000);
                    while (true)
                    {
                        Thread.Sleep(1000);
                        if (!_IsCharging)
                            continue;
                        byte[] data = new byte[57];
                        if (!SettingFlag)
                        {
                            data[13] = 0x51;
                            data[clsChargeStation.Indexes.VIN_H] = (byte)DateTime.Now.Second;
                            data[clsChargeStation.Indexes.VOUT_H] = 0x4F;
                            data[clsChargeStation.Indexes.Status_1] = 0xF0;
                            data[clsChargeStation.Indexes.CC_L] = CC_Bytes[0];
                            data[clsChargeStation.Indexes.CC_H] = CC_Bytes[1];
                            data[clsChargeStation.Indexes.CV_L] = CV_Bytes[0];
                            data[clsChargeStation.Indexes.CV_H] = CV_Bytes[1];
                            data[clsChargeStation.Indexes.FV_L] = FV_Bytes[0];
                            data[clsChargeStation.Indexes.FV_H] = FV_Bytes[1];
                            data[clsChargeStation.Indexes.TC_L] = TC_Bytes[0];
                            data[clsChargeStation.Indexes.TC_H] = TC_Bytes[1];
                        }
                        else
                        {
                            data[13] = 0x61;
                            byte[] cc_bytes = new byte[2] { data[clsChargeStation.Indexes.CC_L], data[clsChargeStation.Indexes.CC_H] };
                            byte[] cv_bytes = new byte[2] { data[clsChargeStation.Indexes.CV_L], data[clsChargeStation.Indexes.CV_H] };
                            byte[] fv_bytes = new byte[2] { data[clsChargeStation.Indexes.FV_L], data[clsChargeStation.Indexes.FV_H] };
                            byte[] tc_bytes = new byte[2] { data[clsChargeStation.Indexes.TC_L], data[clsChargeStation.Indexes.TC_H] };

                            CC = cc_bytes.GetInt() / 10.0;
                            CV = cv_bytes.GetInt() / 10.0;
                            FV = fv_bytes.GetInt() / 10.0;
                            TC = tc_bytes.GetInt() / 10.0;
                            SettingFlag = false;
                        }

                        try
                        {
                            var _num = client.Send(data);
                        }
                        catch (Exception)
                        {
                            return;
                        }


                    };
                });

                _timer.Start();
                client.BeginReceive(socketState.buffer, 0, 1024, SocketFlags.None, new AsyncCallback(RecieveCallback), socketState);
                StartAcceptClientConnectIn();
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
                    if (data[13] == 0x50)
                    {
                        data[13] = 0x51;
                        data[clsChargeStation.Indexes.VIN_H] = (byte)DateTime.Now.Second;
                        data[clsChargeStation.Indexes.VOUT_H] = 0x4F;
                        data[clsChargeStation.Indexes.Status_1] = 0xF0;
                        data[clsChargeStation.Indexes.CC_L] = CC_Bytes[0];
                        data[clsChargeStation.Indexes.CC_H] = CC_Bytes[1];
                        data[clsChargeStation.Indexes.CV_L] = CV_Bytes[0];
                        data[clsChargeStation.Indexes.CV_H] = CV_Bytes[1];
                        data[clsChargeStation.Indexes.FV_L] = FV_Bytes[0];
                        data[clsChargeStation.Indexes.FV_H] = FV_Bytes[1];
                        data[clsChargeStation.Indexes.TC_L] = TC_Bytes[0];
                        data[clsChargeStation.Indexes.TC_H] = TC_Bytes[1];
                    }
                    if (data[13] == 0x60)
                    {
                        data[13] = 0x61;
                        byte[] cc_bytes = new byte[2] { data[clsChargeStation.Indexes.CC_L], data[clsChargeStation.Indexes.CC_H] };
                        byte[] cv_bytes = new byte[2] { data[clsChargeStation.Indexes.CV_L], data[clsChargeStation.Indexes.CV_H] };
                        byte[] fv_bytes = new byte[2] { data[clsChargeStation.Indexes.FV_L], data[clsChargeStation.Indexes.FV_H] };
                        byte[] tc_bytes = new byte[2] { data[clsChargeStation.Indexes.TC_L], data[clsChargeStation.Indexes.TC_H] };

                        CC = cc_bytes.GetInt() / 10.0;
                        CV = cv_bytes.GetInt() / 10.0;
                        FV = fv_bytes.GetInt() / 10.0;
                        TC = tc_bytes.GetInt() / 10.0;

                        SettingFlag = true;
                    }

                    Task.Factory.StartNew(() =>
                    {
                        state.socket.BeginReceive(state.buffer, 0, 1024, SocketFlags.None, new AsyncCallback(RecieveCallback), state);
                    });
                }
            }
            catch (Exception ex)
            {
                state.socket?.Dispose();
            }
        }

        public void AGVNoCharging()
        {
            _IsCharging = false;
        }
        public void AGVCharging()
        {
            _IsCharging = true;
        }
    }
}
