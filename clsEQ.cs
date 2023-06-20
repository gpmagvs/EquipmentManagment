using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment
{
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

        #endregion

        #region AGVS->EQ

        public bool To_EQ_Up { get; set; }
        public bool To_EQ_Low { get; set; }
        public bool CMD_Reserve_Up { get; set; }
        public bool CMD_Reserve_Low { get; set; }

        #endregion

        public clsEQ(clsEndPointOptions options) : base(options)
        {
        }

        public override PortStatusAbstract PortStatus { get; set; } = new clsEQPort();

        protected override void DefineInputData()
        {
            Load_Request = InputBuffer[0];
            Unload_Request = InputBuffer[1];
            Port_Exist = InputBuffer[2];
            Up_Pose = InputBuffer[3];
            Down_Pose = InputBuffer[4];
            Eqp_Status_Down = InputBuffer[5];
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
        private void _WriteOutputSiganls()
        {
            bool[] outputs = new bool[8];
            outputs[0] = To_EQ_Up;
            outputs[1] = To_EQ_Low;
            outputs[2] = CMD_Reserve_Up;
            outputs[3] = CMD_Reserve_Low;

            master.WriteMultipleCoils(1, outputs);
        }

        public void WriteOutputs(ushort start, bool[] value)
        {
            master.WriteMultipleCoils(start, value);
        }
    }
}
