using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EquipmentManagment.Device;

namespace EquipmentManagment.MainEquipment
{
    public enum EQLDULD_TYPE
    {
        LD,
        ULD,
        LDULD,
        Charge
    }
    public class clsEQ : EndPointDeviceAbstract
    {
        public clsEQ(clsEndPointOptions options) : base(options)
        {
            AGVModbusGateway = new clsStatusIOModbusGateway();
            AGVModbusGateway.StartGateway(options.ConnOptions.AGVModbusGatewayPort);
            AGVModbusGateway.OnAGVOutputsChanged += AGVModbusGateway_OnAGVOutputsChanged;
        }

        #region EQ->AGVS
        private bool _Load_Reuest = false;
        private bool _Unload_Request = false;
        public clsStatusIOModbusGateway AGVModbusGateway { get; set; } = new clsStatusIOModbusGateway();
        public static event EventHandler<clsEQ> OnEqUnloadRequesting;
        public bool Load_Request
        {
            get => _Load_Reuest;
            set
            {
                if (_Load_Reuest != value)
                {
                    _Load_Reuest = value;
                }
            }
        }
        public bool Unload_Request
        {
            get => _Unload_Request;
            set
            {
                if (_Unload_Request != value)
                {
                    _Unload_Request = value;
                    if (value)
                        OnEqUnloadRequesting?.Invoke(this, this);
                }
            }
        }
        public bool Port_Exist { get; set; }
        public bool Up_Pose { get; set; }
        public bool Down_Pose { get; set; }
        public bool Eqp_Status_Down { get; set; }


        public bool HS_EQ_L_REQ { get; set; }
        public bool HS_EQ_U_REQ { get; set; }
        public bool HS_EQ_READY { get; set; }
        public bool HS_EQ_BUSY { get; set; }



        private bool _HS_AGV_VALID;
        private bool _HS_AGV_TR_REQ;
        private bool _HS_AGV_BUSY;
        private bool _HS_AGV_READY;
        private bool _HS_AGV_COMPT;

        public bool HS_AGV_VALID
        {
            get => _HS_AGV_VALID;
            set
            {
                if (_HS_AGV_VALID != value)
                {
                    _HS_AGV_VALID = value;
                    Console.WriteLine($"AGV_VALID Changed to :{value}");
                    _WriteOutputSiganls();
                }
            }
        }
        public bool HS_AGV_TR_REQ
        {
            get => _HS_AGV_TR_REQ;
            set
            {
                if (_HS_AGV_TR_REQ != value)
                {
                    _HS_AGV_TR_REQ = value;
                    Console.WriteLine($"AGV_TR_REQ Changed to :{value}");
                    _WriteOutputSiganls();
                }
            }
        }
        public bool HS_AGV_BUSY
        {
            get => _HS_AGV_BUSY;
            set
            {
                if (_HS_AGV_BUSY != value)
                {
                    _HS_AGV_VALID = value;
                    Console.WriteLine($"AGV_BUSY Changed to :{value}");
                    _WriteOutputSiganls();
                }
            }
        }
        public bool HS_AGV_READY
        {
            get => _HS_AGV_READY;
            set
            {
                if (_HS_AGV_READY != value)
                {
                    _HS_AGV_READY = value;
                    Console.WriteLine($"AGV_READY Changed to :{value}");
                    _WriteOutputSiganls();
                }
            }
        }
        public bool HS_AGV_COMPT
        {
            get => _HS_AGV_COMPT;
            set
            {
                if (_HS_AGV_COMPT != value)
                {
                    _HS_AGV_COMPT = value;
                    Console.WriteLine($"AGV_COMPT Changed to :{value}");
                    _WriteOutputSiganls();
                }
            }
        }

        #endregion

        #region AGVS->EQ

        public bool To_EQ_Up { get; set; }
        public bool To_EQ_Low { get; set; }
        public bool CMD_Reserve_Up { get; set; } = false;
        public bool CMD_Reserve_Low { get; set; }

        #endregion
        public EQLDULD_TYPE lduld_type
        {
            get => EndPointOptions.LdULdType;
            set
            {
                EndPointOptions.LdULdType = value;
            }
        }
        private void AGVModbusGateway_OnAGVOutputsChanged(object sender, bool[] agv_outputs)
        {
            HS_AGV_VALID = agv_outputs[0];
            HS_AGV_TR_REQ = agv_outputs[1];
            HS_AGV_BUSY = agv_outputs[2];
            HS_AGV_READY = agv_outputs[3];
            HS_AGV_COMPT = agv_outputs[4];
        }

        public override PortStatusAbstract PortStatus { get; set; } = new clsEQPort();

        protected override void DefineInputData()
        {
            var io_location = EndPointOptions.IOLocation;
            Load_Request = InputBuffer[io_location.Load_Request];
            Unload_Request = InputBuffer[io_location.Unload_Request];
            Port_Exist = InputBuffer[io_location.Port_Exist];
            Up_Pose = InputBuffer[io_location.Up_Pose];
            Down_Pose = InputBuffer[io_location.Down_Pose];
            Eqp_Status_Down = InputBuffer[io_location.Eqp_Status_Down];

            HS_EQ_L_REQ = InputBuffer[io_location.HS_EQ_L_REQ];
            HS_EQ_U_REQ = InputBuffer[io_location.HS_EQ_U_REQ];
            HS_EQ_READY = InputBuffer[io_location.HS_EQ_READY];
            HS_EQ_BUSY = InputBuffer[io_location.HS_EQ_BUSY];

            AGVModbusGateway.StoreEQOutpus(new bool[] { HS_EQ_L_REQ, HS_EQ_U_REQ, HS_EQ_READY, HS_EQ_BUSY });

        }

        public void ToEQUp()
        {
            To_EQ_Up = true;
            To_EQ_Low = false;
            _WriteOutputSiganls();
        }

        public void ToEQLow()
        {
            To_EQ_Up = false;
            To_EQ_Low = true;
            _WriteOutputSiganls();
        }
        public void ReserveUp()
        {
            CMD_Reserve_Up = true;
            CMD_Reserve_Low = false;
            _WriteOutputSiganls();
        }
        public void ReserveLow()
        {
            CMD_Reserve_Up = false;
            CMD_Reserve_Low = true;
            _WriteOutputSiganls();
        }
        public void CancelReserve()
        {
            CMD_Reserve_Low = CMD_Reserve_Up = false;
            _WriteOutputSiganls();
        }
        private void _WriteOutputSiganls()
        {
            var io_location = EndPointOptions.IOLocation;
            bool[] outputs = new bool[16];

            outputs[io_location.To_EQ_Up] = To_EQ_Up;
            outputs[io_location.To_EQ_Low] = To_EQ_Low;
            outputs[io_location.CMD_Reserve_Up] = CMD_Reserve_Up;
            outputs[io_location.CMD_Reserve_Low] = CMD_Reserve_Low;

            outputs[io_location.HS_AGV_VALID] = HS_AGV_VALID;
            outputs[io_location.HS_AGV_TR_REQ] = HS_AGV_TR_REQ;
            outputs[io_location.HS_AGV_BUSY] = HS_AGV_BUSY;
            outputs[io_location.HS_AGV_READY] = HS_AGV_READY;
            outputs[io_location.HS_AGV_COMPT] = HS_AGV_COMPT;
            WriteOutputs(0, outputs);
        }

        public void WriteOutputs(ushort start, bool[] value)
        {
            WriteInputsUseModbusTCP(value);
            //master.WriteMultipleCoils(start, value);
        }

        protected override void WriteOutuptsData()
        {
            _WriteOutputSiganls();
        }
    }
}
