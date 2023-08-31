using System;
using System.Collections.Generic;
using System.Text;
using EquipmentManagment.Device;

namespace EquipmentManagment.MainEquipment
{
    public enum EQLDULD_TYPE
    {
        LD,
        ULD,
        LDULD
    }
    public class clsEQ : EndPointDeviceAbstract
    {
        #region EQ->AGVS
        private bool _Load_Reuest = false;
        private bool _Unload_Request = false;
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

        #endregion

        #region AGVS->EQ

        public bool To_EQ_Up { get; set; }
        public bool To_EQ_Low { get; set; }
        public bool CMD_Reserve_Up { get; set; }
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
        public clsEQ(clsEndPointOptions options) : base(options)
        {
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

            WriteInputsUseModbusTCP(outputs);
        }

        public void WriteOutputs(ushort start, bool[] value)
        {
            master.WriteMultipleCoils(start, value);
        }
    }
}
