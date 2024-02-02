using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EquipmentManagment.Device;
using EquipmentManagment.Device.Options;

namespace EquipmentManagment.MainEquipment
{
    public enum EQLDULD_TYPE
    {
        LD,
        ULD,
        LDULD,
        Charge
    }

    public enum RACK_CONTENT_STATE
    {
        EMPTY, FULL, UNKNOWN
    }
    /// <summary>
    /// 設備從AGV取放貨物的方式
    /// </summary>
    public enum EQ_PICKUP_CARGO_MODE
    {
        /// <summary>
        /// 設備無取放機構，僅透過AGV(如FORK AGV)取放貨物
        /// </summary>
        AGV_PICK_AND_PLACE,
        /// <summary>
        /// 設備有取放機構，透過撈爪升降從AGV車上取放貨物
        /// </summary>
        EQ_PICK_AND_PLACE
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
        private bool _Port_Exist = false;
        private bool _Up_Pose = false;
        private bool _Down_Pose = false;
        private bool _Eqp_Status_Down = false;
        private bool _Eqp_Status_Run = false;
        private bool _Eqp_Status_Idle = false;
        private bool _HS_EQ_L_REQ = false;
        private bool _HS_EQ_U_REQ = false;
        private bool _HS_EQ_READY = false;
        private bool _HS_EQ_UP_READY = false;
        private bool _HS_EQ_LOW_READY = false;
        private bool _HS_EQ_BUSY = false;
        private bool _Empty_CST;
        private bool _Full_CST;
        public clsStatusIOModbusGateway AGVModbusGateway { get; set; } = new clsStatusIOModbusGateway();
        public static event EventHandler<clsEQ> OnEqUnloadRequesting;
        public static event EventHandler<IOChangedEventArgs> OnIOStateChanged;
        public bool Load_Request
        {
            get => _Load_Reuest;
            set
            {
                if (_Load_Reuest != value)
                {
                    _Load_Reuest = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "Load_Request", value));
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
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "Unload_Request", value));
                    if (value)
                        OnEqUnloadRequesting?.Invoke(this, this);
                }
            }
        }
        public bool Port_Exist
        {
            get => _Port_Exist;
            set
            {
                if (_Port_Exist != value)
                {
                    _Port_Exist = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "Port_Exist", value));
                }
            }
        }
        public bool Up_Pose
        {
            get => _Up_Pose;
            set
            {
                if (_Up_Pose != value)
                {
                    _Up_Pose = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "Up_Pose", value));
                }
            }
        }
        public bool Down_Pose
        {
            get => _Down_Pose;
            set
            {
                if (_Down_Pose != value)
                {
                    _Down_Pose = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "Down_Pose", value));
                }
            }
        }
        public bool Eqp_Status_Down
        {
            get => _Eqp_Status_Down;
            set
            {
                if (_Eqp_Status_Down != value)
                {
                    _Eqp_Status_Down = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "Eqp_Status_Down", value));
                }
            }
        }


        public bool Eqp_Status_Run
        {
            get => _Eqp_Status_Run;
            set
            {
                if (_Eqp_Status_Run != value)
                {
                    _Eqp_Status_Run = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "Eqp_Status_Run", value));
                }
            }
        }


        public bool Eqp_Status_Idle
        {
            get => _Eqp_Status_Idle;
            set
            {
                if (_Eqp_Status_Idle != value)
                {
                    _Eqp_Status_Idle = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "Eqp_Status_Idle", value));
                }
            }
        }

        public bool HS_EQ_L_REQ
        {
            get => _HS_EQ_L_REQ;
            set
            {
                if (_HS_EQ_L_REQ != value)
                {
                    _HS_EQ_L_REQ = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "HS_EQ_L_REQ", value));
                }
            }
        }

        public bool HS_EQ_U_REQ
        {
            get => _HS_EQ_U_REQ;
            set
            {
                if (_HS_EQ_U_REQ != value)
                {
                    _HS_EQ_U_REQ = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "HS_EQ_U_REQ", value));
                }
            }
        }

        public bool HS_EQ_READY
        {
            get => _HS_EQ_READY;
            set
            {
                if (_HS_EQ_READY != value)
                {
                    _HS_EQ_READY = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "HS_EQ_READY", value));
                }
            }
        }
        public bool HS_EQ_UP_READY
        {
            get => _HS_EQ_UP_READY;
            set
            {
                if (_HS_EQ_UP_READY != value)
                {
                    _HS_EQ_UP_READY = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "HS_EQ_UP_READY", value));
                }
            }
        }
        public bool HS_EQ_LOW_READY
        {
            get => _HS_EQ_LOW_READY;
            set
            {
                if (_HS_EQ_LOW_READY != value)
                {
                    _HS_EQ_LOW_READY = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "HS_EQ_LOW_READY", value));
                }
            }
        }

        public bool HS_EQ_BUSY
        {
            get => _HS_EQ_BUSY;
            set
            {
                if (_HS_EQ_BUSY != value)
                {
                    _HS_EQ_BUSY = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "HS_EQ_BUSY", value));
                }
            }
        }


        public bool Empty_RACK_To_LDULD
        {
            get => _Empty_CST;
            set
            {
                if (_Empty_CST != value)
                {
                    _Empty_CST = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "Empty_CST", value));
                    Console.WriteLine($"Empty_CST Changed to :{value}");
                    _WriteOutputSiganls();
                }
            }
        }


        public bool Full_RACK_To_LDULD
        {
            get => _Full_CST;
            set
            {
                if (_Full_CST != value)
                {
                    _Full_CST = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "Full_CST", value));
                    Console.WriteLine($"Full_CST Changed to :{value}");
                    _WriteOutputSiganls();
                }
            }
        }



        private bool _To_EQ_UP;
        private bool _To_EQ_LOW;
        private bool _HS_AGV_VALID;
        private bool _HS_AGV_TR_REQ;
        private bool _HS_AGV_BUSY;
        private bool _HS_AGV_READY;
        private bool _HS_AGV_COMPT;

        private bool _To_EQ_Empty_CST;
        private bool _To_EQ_Full_CST;


        public RACK_CONTENT_STATE RackContentState
        {
            get
            {
                if (Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown)
                    return RACK_CONTENT_STATE.UNKNOWN;
                return Empty_RACK_To_LDULD ? RACK_CONTENT_STATE.EMPTY : RACK_CONTENT_STATE.FULL;
            }

        }

        public bool Is_RACK_HAS_TRAY_OR_NOT_TO_LDULD_Unknown
        {
            get
            {
                return (Full_RACK_To_LDULD && Empty_RACK_To_LDULD) || (!Full_RACK_To_LDULD && !Empty_RACK_To_LDULD);
            }
        }

        public bool HS_AGV_VALID
        {
            get => _HS_AGV_VALID;
            set
            {
                if (_HS_AGV_VALID != value)
                {
                    _HS_AGV_VALID = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "AGV_VALID", value));
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
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "AGV_TR_REQ", value));
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
                    _HS_AGV_BUSY = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "AGV_BUSY", value));
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
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "AGV_READY", value));
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
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "AGV_COMPT", value));
                    Console.WriteLine($"AGV_COMPT Changed to :{value}");
                    _WriteOutputSiganls();
                }
            }
        }

        public bool To_EQ_Empty_CST
        {
            get => _To_EQ_Empty_CST;
            set
            {
                if (_To_EQ_Empty_CST != value)
                {
                    _To_EQ_Empty_CST = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "To_EQ_Empty_CST", value));
                    Console.WriteLine($"To_EQ_Empty_CST Changed to :{value}");
                    _WriteOutputSiganls();
                }
            }
        }


        public bool To_EQ_Full_CST
        {
            get => _To_EQ_Full_CST;
            set
            {
                if (_To_EQ_Full_CST != value)
                {
                    _To_EQ_Full_CST = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "To_EQ_Full_CST", value));
                    Console.WriteLine($"To_EQ_Full_CST Changed to :{value}");
                    _WriteOutputSiganls();
                }
            }
        }

        #endregion

        #region AGVS->EQ

        public bool To_EQ_Up
        {
            get => _To_EQ_UP;
            set
            {
                if (_To_EQ_UP != value)
                {
                    _To_EQ_UP = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "To_EQ_Up", value));
                    Console.WriteLine($"To_EQ_Up Changed to :{value}");
                    _WriteOutputSiganls();
                }
            }
        }
        public bool To_EQ_Low
        {
            get => _To_EQ_LOW;
            set
            {
                if (_To_EQ_LOW != value)
                {
                    _To_EQ_LOW = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "To_EQ_Low ", value));
                    Console.WriteLine($"To_EQ_Low Changed to :{value}");
                    _WriteOutputSiganls();
                }
            }
        }
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

        protected override void InputsHandler()
        {

            var io_location = EndPointOptions.IOLocation;
            Load_Request = InputBuffer[io_location.Load_Request];
            Unload_Request = InputBuffer[io_location.Unload_Request];
            Port_Exist = InputBuffer[io_location.Port_Exist];
            Up_Pose = InputBuffer[io_location.Up_Pose];
            Down_Pose = InputBuffer[io_location.Down_Pose];
            Eqp_Status_Down = InputBuffer[io_location.Eqp_Status_Down];
            Eqp_Status_Run = InputBuffer[io_location.Eqp_Status_Run];
            Eqp_Status_Idle = InputBuffer[io_location.Eqp_Status_Idle];
            Full_RACK_To_LDULD = InputBuffer[io_location.Full_CST];
            Empty_RACK_To_LDULD = InputBuffer[io_location.Empty_CST];

            HS_EQ_L_REQ = InputBuffer[io_location.HS_EQ_L_REQ];
            HS_EQ_U_REQ = InputBuffer[io_location.HS_EQ_U_REQ];
            HS_EQ_READY = InputBuffer[io_location.HS_EQ_READY];
            HS_EQ_UP_READY = InputBuffer[io_location.HS_EQ_UP_READY];
            HS_EQ_LOW_READY = InputBuffer[io_location.HS_EQ_LOW_READY];

            HS_EQ_BUSY = InputBuffer[io_location.HS_EQ_BUSY];

            AGVModbusGateway.StoreEQOutpus(new bool[] { HS_EQ_L_REQ, HS_EQ_U_REQ, HS_EQ_READY, HS_EQ_BUSY });

        }

        public void ToEQUp()
        {
            To_EQ_Up = true;
            To_EQ_Low = false;
        }

        public void ToEQLow()
        {
            To_EQ_Up = false;
            To_EQ_Low = true;
        }

        public void Empty_RACK_To_EQ()
        {
            Console.WriteLine($"向{EQName} 載入空框訊號ON");
            _To_EQ_Empty_CST = true;
            _To_EQ_Full_CST = false;
            _WriteOutputSiganls();
        }
        public void Full_RACK_To_EQ()
        {
            Console.WriteLine($"向{EQName} 載入滿框訊號ON");
            _To_EQ_Empty_CST = false;
            _To_EQ_Full_CST = true;
            _WriteOutputSiganls();
        }

        public void CancelToEQUpAndLow()
        {
            _To_EQ_UP = false;
            _To_EQ_LOW = false;
            _To_EQ_Full_CST = false;
            _To_EQ_Empty_CST = false;
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
            outputs[io_location.HS_AGV_COMPT] = HS_AGV_COMPT;
            outputs[io_location.HS_AGV_READY] = HS_AGV_READY;
            outputs[io_location.To_EQ_Empty_CST] = To_EQ_Empty_CST;
            outputs[io_location.To_EQ_Full_CST] = To_EQ_Full_CST;
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

        internal EQStatusDIDto GetEQStatusDTO()
        {
            EQStatusDIDto dto = new EQStatusDIDto(this.EndPointOptions.EqType);
            dto.IsConnected = IsConnected;
            dto.EQName = EQName;
            dto.Load_Request = Load_Request;
            dto.Unload_Request = Unload_Request;
            dto.Port_Exist = Port_Exist;
            dto.Up_Pose = Up_Pose;
            dto.Down_Pose = Down_Pose;
            dto.Eqp_Status_Down = Eqp_Status_Down;
            dto.Eqp_Status_Run = Eqp_Status_Run;
            dto.Eqp_Status_Idle = Eqp_Status_Idle;
            dto.Cmd_Reserve_Up = CMD_Reserve_Up;
            dto.Cmd_Reserve_Low = CMD_Reserve_Low;
            dto.To_EQ_Up = To_EQ_Up;
            dto.To_EQ_Low = To_EQ_Low;
            dto.HS_EQ_BUSY = HS_EQ_BUSY;
            dto.HS_EQ_READY = HS_EQ_READY;
            dto.HS_EQ_UP_READY = HS_EQ_UP_READY;
            dto.HS_EQ_LOW_READY = HS_EQ_LOW_READY;
            dto.HS_EQ_L_REQ = HS_EQ_L_REQ;
            dto.HS_EQ_U_REQ = HS_EQ_U_REQ;
            dto.HS_AGV_VALID = HS_AGV_VALID;
            dto.HS_AGV_TR_REQ = HS_AGV_TR_REQ;
            dto.HS_AGV_BUSY = HS_AGV_BUSY;
            dto.HS_AGV_READY = HS_AGV_READY;
            dto.HS_AGV_COMPT = HS_AGV_COMPT;
            dto.Region = EndPointOptions.Region;
            dto.Tag = EndPointOptions.TagID;
            dto.Full_CST = Full_RACK_To_LDULD;
            dto.Empty_CST = Empty_RACK_To_LDULD;
            dto.To_EQ_Full_CST = To_EQ_Full_CST;
            dto.To_EQ_Empty_CST = To_EQ_Empty_CST;
            return dto;
        }
    }
}
