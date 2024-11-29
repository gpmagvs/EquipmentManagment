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
            _EMO=_AIR_ERROR=true;
            _SMOKE_DECTECTED =CYLINDER_BACKWARD=CYLINDER_BACKWARD=TEMPERATURE_MODULE_ABN=false;
        }
    }
}
