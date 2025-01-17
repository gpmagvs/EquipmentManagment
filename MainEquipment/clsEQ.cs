﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EquipmentManagment.Device;
using EquipmentManagment.Device.Options;
using EquipmentManagment.Manager;
using EquipmentManagment.Tool;
using Newtonsoft.Json.Linq;

namespace EquipmentManagment.MainEquipment
{
    public partial class clsEQ : EndPointDeviceAbstract
    {

        public static event EventHandler<string> OnPortCarrierIDChanged;

        protected override bool _IsConnected { get; set; } = true;

        public static bool WirteOuputEnabled { get; set; } = true;

        public clsEQ(clsEndPointOptions options) : base(options)
        {
            Task.Run(() =>
            {
                AGVModbusGateway = new clsStatusIOModbusGateway();
                AGVModbusGateway.StartGateway(options.ConnOptions.AGVModbusGatewayPort);
                AGVModbusGateway.OnAGVOutputsChanged += AGVModbusGateway_OnAGVOutputsChanged;
                Console.WriteLine($"{EQName} IO Modbus Gateway start(Port:{options.ConnOptions.AGVModbusGatewayPort})");
            });
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
        private bool _TB_Down_Pose = false;
        private bool _HS_EQ_L_REQ = false;
        private bool _HS_EQ_U_REQ = false;
        private bool _HS_EQ_READY = false;
        private bool _HS_EQ_UP_READY = false;
        private bool _HS_EQ_LOW_READY = false;
        private bool _HS_EQ_BUSY = false;
        private bool _Empty_CST;
        private bool _Full_CST;
        private string _CSTIDReadValue;
        private Debouncer _CargoExistStateDebouncer = new Debouncer();

        public bool EmptyRackUnloadVirtualInput = false;
        public bool FullRackUnloadVirtualInput = false;

        public DateTime LoadRequestRaiseTime { get; private set; } = DateTime.MinValue;
        public DateTime UnloadRequestRaiseTime { get; private set; } = DateTime.MinValue;
        public clsStatusIOModbusGateway AGVModbusGateway { get; set; } = new clsStatusIOModbusGateway();
        public static event EventHandler<clsEQ> OnEqUnloadRequesting;
        public static event EventHandler<IOChangedEventArgs> OnIOStateChanged;
        public static event EventHandler<clsEQ> OnEQPortCargoChangedToExist;
        public static event EventHandler<clsEQ> OnEQPortCargoChangedToDisappear;
        public static event EventHandler<(clsEQ, bool)> OnUnloadRequestChanged;
        public static event EventHandler<(clsEQ, string newValue, string oldValue)> OnCSTReaderIDChanged;
        public delegate bool MyEventHandler(clsEQ eq, EventArgs e);
        public static event MyEventHandler OnCheckEQPortBelongTwoLayersEQOrNot;


        public bool IS_EQ_STATUS_DOWN
        {
            get
            {
                return EndPointOptions.IOLocation.STATUS_IO_SPEC_VERSION == clsEQIOLocation.STATUS_IO_DEFINED_VERSION.V2 ? !Eqp_Status_Down : Eqp_Status_Down;
            }
        }

        public bool IS_EQ_STATUS_NORMAL_IDLE
        {
            get
            {
                return EndPointOptions.IOLocation.STATUS_IO_SPEC_VERSION == clsEQIOLocation.STATUS_IO_DEFINED_VERSION.V2 ?
                                                                            Eqp_Status_Down : Eqp_Status_Idle && !Eqp_Status_Run && !Eqp_Status_Down;
            }
        }

        public bool Load_Request
        {
            get => _Load_Reuest;
            set
            {
                if (_Load_Reuest != value)
                {
                    _Load_Reuest = value;
                    LoadRequestRaiseTime = _Load_Reuest ? DateTime.Now : DateTime.MinValue;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "Load_Request", value));
                }
            }
        }
        public bool Unload_Request
        {
            //get => EndPointOptions.IsOneOfDualPorts ? _Unload_Request && EndPointOptions.AllowUnloadPortTypeNumber == this.PortTypeNumber && _Unload_Request : _Unload_Request;
            get => _Unload_Request;
            set
            {
                if (_Unload_Request != value)
                {
                    OnUnloadRequestChanged?.Invoke(this, (this, value));
                    _Unload_Request = value;
                    UnloadRequestRaiseTime = _Unload_Request ? DateTime.Now : DateTime.MinValue;
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
                    InvokeEventWithDebounce(_Port_Exist);
                    async Task InvokeEventWithDebounce(bool newState)
                    {
                        _CargoExistStateDebouncer.Debounce(() =>
                        {
                            if (_Port_Exist)
                                OnEQPortCargoChangedToExist(this, this);
                            else
                                OnEQPortCargoChangedToDisappear(this, this);
                        }, 1000);
                    }
                }
            }
        }

        public string CSTIDReadValue
        {
            get => _CSTIDReadValue;
            set
            {
                if (_CSTIDReadValue != value)
                {
                    OnCSTReaderIDChanged?.Invoke(this, (this, value, _CSTIDReadValue));
                    PortStatus.CarrierID = _CSTIDReadValue = value;
                }
            }
        }

        public bool IsCSTIDReadFail => !string.IsNullOrEmpty(CSTIDReadValue) && CSTIDReadValue.Contains("NG");
        public bool IsCSTIDReadMismatch => !string.IsNullOrEmpty(CSTIDReadValue) && !string.IsNullOrEmpty(AGVAssignCarrierID) && CSTIDReadValue != AGVAssignCarrierID;
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
        /// <summary>
        /// 貨物轉向機構位置(false=> 位於低位; true=>位於高位 當機構位置並非位於低位時,禁止AGV侵入)
        /// </summary>
        public bool TB_Down_Pose
        {
            get => _TB_Down_Pose;
            set
            {
                if (_TB_Down_Pose != value)
                {
                    _TB_Down_Pose = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "TB_Down_Pose", value));
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
                    if (value && EndPointOptions.IsFullEmptyUnloadAsVirtualInput) //若機台為運轉模式 表示有在烘烤=>實框
                    {
                        To_EQ_Empty_CST = false;
                        To_EQ_Full_CST = true;
                        //Full_RACK_To_LDULD = true;
                    }
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

                if (!EndPointOptions.CheckRackContentStateIOSignal)
                    return RACK_CONTENT_STATE.FULL;

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
                    _WriteOutputSiganls().GetAwaiter().GetResult();

                    EQHandshakeEmulation("AGV_VALID", value);
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
                    _WriteOutputSiganls().GetAwaiter().GetResult();

                    EQHandshakeEmulation("AGV_TR_REQ", value);
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
                    _WriteOutputSiganls().GetAwaiter().GetResult();
                    EQHandshakeEmulation("AGV_BUSY", value);
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
                    _WriteOutputSiganls().GetAwaiter().GetResult();
                    EQHandshakeEmulation("AGV_READY", value);
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
                    _WriteOutputSiganls().GetAwaiter().GetResult();
                    EQHandshakeEmulation("AGV_COMPT", value);
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
                    EmptyRackUnloadVirtualInput = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "To_EQ_Empty_CST", value));
                    Console.WriteLine($"To_EQ_Empty_CST Changed to :{value}");
                    _WriteOutputSiganls().GetAwaiter().GetResult();
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
                    FullRackUnloadVirtualInput = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "To_EQ_Full_CST", value));
                    Console.WriteLine($"To_EQ_Full_CST Changed to :{value}");
                    _WriteOutputSiganls().GetAwaiter().GetResult();
                }
            }
        }

        public override bool IsMaintaining
        {
            get => base.IsMaintaining;
            set
            {
                if (base.IsMaintaining != value)
                {
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "IsMaintaining", value));
                }
                base.IsMaintaining = value;
            }
        }

        public override bool IsPartsReplacing
        {
            get => base.IsPartsReplacing;
            set
            {
                if (base.IsPartsReplacing != value)
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "IsPartsReplacing", value));
                base.IsPartsReplacing = value;
            }
        }

        private int _PortTypeNumber = -1;
        public int PortTypeNumber
        {
            get => _PortTypeNumber;
            set
            {
                if (_PortTypeNumber != value)
                {
                    _PortTypeNumber = value;
                    Console.WriteLine($"{EndPointOptions.Name} PortTypeNumber Chaged to {_PortTypeNumber}");
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
                    _WriteOutputSiganls().GetAwaiter().GetResult();
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
                    _WriteOutputSiganls().GetAwaiter().GetResult();
                }
            }
        }


        public bool _CMD_Reserve_Up = false;
        public bool _CMD_Reserve_Low = false;


        public bool CMD_Reserve_Up
        {
            get => _CMD_Reserve_Up;
            set
            {
                if (_CMD_Reserve_Up != value)
                {
                    _CMD_Reserve_Up = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "CMD_Reserve_Up ", value));
                    Console.WriteLine($"CMD_Reserve_Up Changed to :{value}");
                    _WriteOutputSiganls().GetAwaiter().GetResult();
                }
            }
        }
        public bool CMD_Reserve_Low
        {
            get => _CMD_Reserve_Low;
            set
            {
                if (_CMD_Reserve_Low != value)
                {
                    _CMD_Reserve_Low = value;
                    OnIOStateChanged?.Invoke(this, new IOChangedEventArgs(this, "CMD_Reserve_Low ", value));
                    Console.WriteLine($"CMD_Reserve_Low Changed to :{value}");
                    _WriteOutputSiganls().GetAwaiter().GetResult();
                }
            }
        }
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
        public string AGVAssignCarrierID { get; private set; } = string.Empty;

        protected override async void InputsHandler()
        {
            bool isFullEmptyRackAsVirtualInput = EndPointOptions.IsFullEmptyUnloadAsVirtualInput;
            var io_location = EndPointOptions.IOLocation;
            try
            {
                Load_Request = InputBuffer[io_location.Load_Request];
                Unload_Request = InputBuffer[io_location.Unload_Request];
                Port_Exist = InputBuffer[io_location.Port_Exist];
                Up_Pose = InputBuffer[io_location.Up_Pose];
                Down_Pose = InputBuffer[io_location.Down_Pose];
                TB_Down_Pose = InputBuffer[io_location.TB_Down_Pose];
                Eqp_Status_Down = InputBuffer[io_location.Eqp_Status_Down];
                Eqp_Status_Run = InputBuffer[io_location.Eqp_Status_Run];
                Eqp_Status_Idle = InputBuffer[io_location.Eqp_Status_Idle];
                Full_RACK_To_LDULD = isFullEmptyRackAsVirtualInput ? FullRackUnloadVirtualInput : InputBuffer[io_location.Full_CST];
                Empty_RACK_To_LDULD = isFullEmptyRackAsVirtualInput ? EmptyRackUnloadVirtualInput : InputBuffer[io_location.Empty_CST];
                HS_EQ_L_REQ = InputBuffer[io_location.HS_EQ_L_REQ];
                HS_EQ_U_REQ = InputBuffer[io_location.HS_EQ_U_REQ];
                HS_EQ_READY = InputBuffer[io_location.HS_EQ_READY];
                HS_EQ_UP_READY = InputBuffer[io_location.HS_EQ_UP_READY];
                HS_EQ_LOW_READY = InputBuffer[io_location.HS_EQ_LOW_READY];
                HS_EQ_BUSY = InputBuffer[io_location.HS_EQ_BUSY];
                IsMaintaining = InputBuffer[io_location.Eqp_Maintaining];
                IsPartsReplacing = InputBuffer[io_location.Eqp_PartsReplacing];
            }
            catch (Exception ex)
            {
                throw new IndexOutOfRangeException(ex.Message, ex);
            }
            finally
            {
            }
            AGVModbusGateway.StoreEQOutpus(new bool[] { HS_EQ_L_REQ, HS_EQ_U_REQ, HS_EQ_READY, HS_EQ_BUSY });


            if (DataBuffer.Any())
            {
                try
                {
                    PortTypeNumber = DataBuffer[EndPointOptions.IOLocation.HoldingRegists.PortTypeStatus];
                }
                catch (Exception ex)
                {
                }
            }
            try
            {
                if (EndPointOptions.IsCSTIDReportable)
                {
                    CSTIDReadValue = ToASCII(BCRIDHoldingRegistStore);
                }

            }
            catch (Exception ex)
            {
                throw new IndexOutOfRangeException(ex.Message, ex);
            }

        }

        public string ToASCII(IEnumerable<ushort> words)
        {
            if (words == null)
                return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (ushort value in words)
            {
                //0x4154 取得 high byte 與 low byte 0x41, 0x54 
                if (value == 0)
                    break;
                // 取得高位元組 (0x41)
                byte highByte = (byte)(value >> 8);    // 右移 8 位
                // 取得低位元組 (0x54)
                byte lowByte = (byte)(value & 0xFF);   // 使用 AND 運算取得最後 8 位
                sb.Append(Encoding.ASCII.GetString(new byte[2] { lowByte, highByte }));
            }
            return sb.ToString().TrimEnd();
        }
        public void ToEQ()
        {
            bool isTwoLayerEQ = OnCheckEQPortBelongTwoLayersEQOrNot(this, EventArgs.Empty);
            if (!isTwoLayerEQ)
            {
                ToEQUp();
                return;
            }
            if (EndPointOptions.Height > 0)
                ToEQUp();
            else
                ToEQLow();
        }
        private void ToEQUp()
        {
            To_EQ_Up = true;
            To_EQ_Low = false;
        }

        private void ToEQLow()
        {
            To_EQ_Up = false;
            To_EQ_Low = true;
        }

        public void Empty_RACK_To_EQ()
        {
            Console.WriteLine($"向{EQName} 載入空框訊號ON");
            To_EQ_Empty_CST = true;
            To_EQ_Full_CST = false;
        }
        public void Full_RACK_To_EQ()
        {
            Console.WriteLine($"向{EQName} 載入滿框訊號ON");
            To_EQ_Empty_CST = false;
            To_EQ_Full_CST = true;
        }

        public async Task CancelToEQUpAndLow()
        {
            To_EQ_Up = false;
            To_EQ_Low = false;
        }

        public void Reserve(string carrierID)
        {
            AGVAssignCarrierID = carrierID;
            Reserve();
        }
        public void Reserve()
        {
            bool isTwoLayerEQ = OnCheckEQPortBelongTwoLayersEQOrNot(this, EventArgs.Empty);
            if (!isTwoLayerEQ)
            {
                ReserveUp();
                return;
            }
            if (EndPointOptions.Height > 0)
                ReserveUp();
            else
                ReserveLow();
        }

        private void ReserveUp()
        {
            CMD_Reserve_Up = true;
            CMD_Reserve_Low = false;
        }
        private void ReserveLow()
        {
            CMD_Reserve_Up = false;
            CMD_Reserve_Low = true;
        }
        public async Task CancelReserve()
        {
            CMD_Reserve_Low = CMD_Reserve_Up = false;
        }
        private SemaphoreSlim _WriteOuputSignalsSemaphore = new SemaphoreSlim(1, 1);
        private SemaphoreSlim _SyncInputSignalsSemaphore = new SemaphoreSlim(1, 1);
        private async Task _WriteOutputSiganls()
        {
            try
            {
                var io_location = EndPointOptions.IOLocation;
                var outputStart = EndPointOptions.ConnOptions.Output_Start_Address;
                bool[] outputs = new bool[64];

                outputs[io_location.To_EQ_Up] = _To_EQ_UP;
                outputs[io_location.To_EQ_Low] = _To_EQ_LOW;
                outputs[io_location.CMD_Reserve_Up] = _CMD_Reserve_Up;
                outputs[io_location.CMD_Reserve_Low] = _CMD_Reserve_Low;
                outputs[io_location.HS_AGV_VALID] = _HS_AGV_VALID;
                outputs[io_location.HS_AGV_TR_REQ] = _HS_AGV_TR_REQ;
                outputs[io_location.HS_AGV_BUSY] = _HS_AGV_BUSY;
                outputs[io_location.HS_AGV_COMPT] = _HS_AGV_COMPT;
                outputs[io_location.HS_AGV_READY] = _HS_AGV_READY;
                outputs[io_location.To_EQ_Empty_CST] = _To_EQ_Empty_CST;
                outputs[io_location.To_EQ_Full_CST] = _To_EQ_Full_CST;
                WriteOutputs(outputStart, outputs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + ex.StackTrace);
            }
            finally
            {
            }
        }

        public void WriteOutputs(ushort start, bool[] value)
        {
            if (!clsEQ.WirteOuputEnabled)
                return;
            switch (this.EndPointOptions.ConnOptions.ConnMethod)
            {
                case Connection.CONN_METHODS.MODBUS_TCP:
                    WriteInputsUseModbusTCP(start, value);
                    break;
                case Connection.CONN_METHODS.MODBUS_RTU:
                    break;
                case Connection.CONN_METHODS.TCPIP:
                    break;
                case Connection.CONN_METHODS.SERIAL_PORT:
                    break;
                case Connection.CONN_METHODS.MX:
                    break;
                case Connection.CONN_METHODS.MC:
                    WriteOutputsUseMCProtocol(value);
                    break;
                default:
                    break;
            }


            //master.WriteMultipleCoils(start, value);
        }

        protected override void WriteOutuptsData()
        {
            _WriteOutputSiganls();
        }
        protected override bool[] RollBackModbusOutputs()
        {
            var io_location = EndPointOptions.IOLocation;

            bool[] outputsWritten = base.RollBackModbusOutputs();
            To_EQ_Up = outputsWritten[io_location.To_EQ_Up];
            To_EQ_Low = outputsWritten[io_location.To_EQ_Low];
            CMD_Reserve_Up = outputsWritten[io_location.CMD_Reserve_Up];
            CMD_Reserve_Low = outputsWritten[io_location.CMD_Reserve_Low];
            HS_AGV_VALID = outputsWritten[io_location.HS_AGV_VALID];
            HS_AGV_TR_REQ = outputsWritten[io_location.HS_AGV_TR_REQ];
            HS_AGV_BUSY = outputsWritten[io_location.HS_AGV_BUSY];
            HS_AGV_COMPT = outputsWritten[io_location.HS_AGV_COMPT];
            HS_AGV_READY = outputsWritten[io_location.HS_AGV_READY];
            To_EQ_Empty_CST = outputsWritten[io_location.To_EQ_Empty_CST];
            To_EQ_Full_CST = outputsWritten[io_location.To_EQ_Full_CST];
            return outputsWritten;
        }
        internal EQStatusDIDto GetEQStatusDTO()
        {
            EQStatusDIDto dto = new EQStatusDIDto(this.EndPointOptions.EqType);
            dto.IsConnected = IsConnected;
            dto.EQName = EQName;
            dto.MainStatus = _GetMainStatus();
            dto.Load_Request = Load_Request;
            dto.Unload_Request = Unload_Request;
            dto.Port_Exist = Port_Exist;
            dto.Up_Pose = Up_Pose;
            dto.Down_Pose = Down_Pose;
            dto.TB_Down_Pose = TB_Down_Pose;
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
            dto.IsMaintaining = IsMaintaining;
            dto.IsPartsReplacing = IsPartsReplacing;
            dto.CarrierID = PortStatus.CarrierID;
            return dto;
        }

        private EQStatusDIDto.EQ_MAIN_STATUS _GetMainStatus()
        {
            if (EndPointOptions.IOLocation.STATUS_IO_SPEC_VERSION == clsEQIOLocation.STATUS_IO_DEFINED_VERSION.V1)
            {
                if (Eqp_Status_Down)
                    return EQStatusDIDto.EQ_MAIN_STATUS.Down;
                else if (Eqp_Status_Run)
                    return EQStatusDIDto.EQ_MAIN_STATUS.BUSY;
                else if (Eqp_Status_Idle)
                    return EQStatusDIDto.EQ_MAIN_STATUS.Idle;
                else
                    return EQStatusDIDto.EQ_MAIN_STATUS.Unknown;
            }
            else
            {
                if (Eqp_Status_Down)
                    return EQStatusDIDto.EQ_MAIN_STATUS.Idle;
                else
                    return EQStatusDIDto.EQ_MAIN_STATUS.Down;
            }

        }

        public string GetStatusDescription()
        {
            string desc = $"Unload_Request  ={Unload_Request}\r\n" +
                          $"Load_Request    ={Load_Request}\r\n" +
                          $"Port Exist      ={Port_Exist}\r\n" +
                          $"Eqp_Status_Down ={Eqp_Status_Down}\r\n" +
                          $"Up_Pose         ={Up_Pose}\r\n" +
                          $"Down_Pose       ={Down_Pose}\r\n" +
                          $"Maintaining     ={IsMaintaining}\r\n" +
                          $"Parts_Replacing ={IsPartsReplacing}";
            return desc;
        }

        public override void UpdateCarrierInfo(int tagNumber, string carrierID, int height)
        {
            PortStatus.CarrierID = carrierID;
            OnPortCarrierIDChanged?.Invoke(this, PortStatus.CarrierID);
        }

        public bool IsCreateUnloadTaskAble()
        {
            bool IsRackContextTypePointOut = !EndPointOptions.CheckRackContentStateIOSignal ? true : this.RackContentState != RACK_CONTENT_STATE.UNKNOWN;
            bool IsLDULDMechanismPoseCorrect = !EndPointOptions.HasLDULDMechanism ? true : Up_Pose;
            bool IsCstSteeringMechanismPoseCorrect = !EndPointOptions.HasCstSteeringMechanism ? true : TB_Down_Pose;
            return Unload_Request && !IS_EQ_STATUS_DOWN && Port_Exist && !CMD_Reserve_Up && IsLDULDMechanismPoseCorrect && IsCstSteeringMechanismPoseCorrect && IsRackContextTypePointOut;
        }

        public bool IsCreateLoadTaskAble()
        {
            bool IsLDULDMechanismPoseCorrect = !EndPointOptions.HasLDULDMechanism ? true : Down_Pose;
            bool IsCstSteeringMechanismPoseCorrect = !EndPointOptions.HasCstSteeringMechanism ? true : TB_Down_Pose;
            return Load_Request && !IS_EQ_STATUS_DOWN && !Port_Exist && !CMD_Reserve_Up && IsLDULDMechanismPoseCorrect && IsCstSteeringMechanismPoseCorrect;
        }

        public void SetAGVAssignedCarrierID(string carrierID)
        {
            AGVAssignCarrierID = carrierID;
        }
        private async Task EQHandshakeEmulation(string signalName, bool value)
        {
            if (!EndPointOptions.IsEmulation)
                return;

            var emulator = StaEQPEmulatorsManagager.GetEQEmuByName(this.EQName);

            if (emulator == null)
                return;
            if (signalName == "AGV_VALID")
            {
                if (Load_Request && value)
                {
                    emulator.SetHS_L_REQ(true);
                }
                else if (Unload_Request && value)
                {
                    emulator.SetHS_U_REQ(true);
                }
            }

            if (signalName == "AGV_TR_REQ" && value)
            {
                emulator.SetHS_READY(true);
            }

            if (signalName == "AGV_READY")
            {
                if (value)
                {
                    emulator.SetHS_BUSY(true);
                    _ = Task.Factory.StartNew(async () =>
                    {
                        await Task.Delay(2000);
                        emulator.SetHS_BUSY(false);
                    });
                }
            }

            if (signalName == "AGV_COMPT" && value)
            {

                emulator.SetHS_U_REQ(false);
                emulator.SetHS_L_REQ(false);
                emulator.SetHS_READY(false);
                emulator.SetHS_BUSY(false);
            }
        }
    }
}
