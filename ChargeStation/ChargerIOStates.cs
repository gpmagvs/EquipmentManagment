using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EquipmentManagment.ChargeStation
{
    public class ChargerIOStates
    {

        internal event EventHandler OnEMO;
        internal event EventHandler OnSmokeDetected;
        internal event EventHandler OnAirError;
        internal event EventHandler OnTemperatureError;

        private bool _EMO = true;
        private bool _SMOKE_DECTECTED = false;
        private bool _AIR_ERROR = false;
        private bool _TEMPERATURE_MODULE_ABN = false;

        public bool IO_St_EMO { get; private set; } = false;
        public bool IO_St_SMOKE_DECTECTED { get; private set; } = false;
        public bool IO_St_AIR_ERROR { get; private set; } = false;
        public bool IO_St_TEMPERATURE_MODULE_ABN { get; private set; } = false;
        public bool IO_St_CYLINDER_FORWARD { get; private set; } = false;
        public bool IO_St_CYLINDER_BACKWARD { get; private set; } = false;


        public void UpdateIOAcutalState(bool _emo, bool _smoke, bool _air, bool _temperature, bool _cylinder_fw, bool _cylinder_bw)
        {
            IO_St_EMO = _emo;
            IO_St_SMOKE_DECTECTED = _smoke;
            IO_St_AIR_ERROR = _air;
            IO_St_TEMPERATURE_MODULE_ABN = _temperature;
            IO_St_CYLINDER_FORWARD = _cylinder_fw;
            IO_St_CYLINDER_BACKWARD = _cylinder_bw;
        }


        /// <summary>
        /// (IO B接點)
        /// </summary>
        public bool EMO
        {
            get => _EMO;
            set
            {
                if (_EMO != value)
                {
                    _EMO = value;
                    if (value)
                        OnEMO?.Invoke(this, EventArgs.Empty);
                }
            }
        }


        public bool SMOKE_DECTECTED
        {
            get => _SMOKE_DECTECTED;
            set
            {
                if (_SMOKE_DECTECTED != value)
                {
                    _SMOKE_DECTECTED = value;
                    if (value)
                        OnSmokeDetected?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool AIR_ERROR
        {
            get => _AIR_ERROR;
            set
            {
                if (_AIR_ERROR != value)
                {
                    _AIR_ERROR = value;
                    if (value)
                        OnAirError?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool CYLINDER_FORWARD { get; set; } = false;

        public bool CYLINDER_BACKWARD { get; set; } = false;

        public bool TEMPERATURE_MODULE_ABN
        {
            get => _TEMPERATURE_MODULE_ABN;
            set
            {
                if (_TEMPERATURE_MODULE_ABN != value)
                {
                    _TEMPERATURE_MODULE_ABN = value;
                    if (value)
                        OnTemperatureError?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void Reset()
        {
            _EMO = _AIR_ERROR = true;
            _SMOKE_DECTECTED = CYLINDER_BACKWARD = CYLINDER_BACKWARD = TEMPERATURE_MODULE_ABN = false;
        }
    }
}
