using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EquipmentManagment.ChargeStation
{
    public class clsChargerData
    {
        public bool Connected { get; set; } = false;
        public bool IsUsing { get; private set; } = false;
        public DateTime UpdateTime { get; set; }

        public clsChargeStation.CHARGE_MODE CurrentChargeMode { get; set; } = clsChargeStation.CHARGE_MODE.CCM;

        public bool IsBatteryFull { get; set; } = false;
        public double Vin { get; set; }
        public double Vout { get; set; }
        public double Iout { get; set; }
        public double CC
        {
            get => _CC;
            set
            {
                if (_CC != value)
                {
                    _CC = value;
                    int valToWrite = int.Parse(Math.Round(value * 10) + "");
                    CC_Setting = valToWrite;
                }
            }
        }
        public double CV
        {
            get => _CV;
            set
            {
                if (_CV != value)
                {
                    _CV = value;
                    int valToWrite = int.Parse(Math.Round(value * 10) + "");
                    CV_Setting = valToWrite;
                }
            }
        }
        public double FV
        {
            get => _FV;
            set
            {
                if (_FV != value)
                {
                    _FV = value;
                    int valToWrite = int.Parse(Math.Round(value * 10) + "");
                    FV_Setting = valToWrite;
                }
            }
        }
        public double TC
        {
            get => _TC;
            set
            {
                if (_TC != value)
                {
                    _TC = value;
                    int valToWrite = int.Parse(Math.Round(value * 10) + "");
                    TC_Setting = valToWrite;
                }
            }
        }


        public double Fan_Speed_1 { get; internal set; }
        public double Fan_Speed_2 { get; internal set; }
        public string[] UsableAGVNames { get; set; } = new string[0];
        private double _TC { get; set; } = 6;
        public DateTime Time { get; set; }
        public List<clsChargeStation.ERROR_CODE> ErrorCodes { get; set; } = new List<clsChargeStation.ERROR_CODE>();
        public List<string> ErrorCodesDescrptions
        {
            get
            {
                return ErrorCodes.Select(x => x.ToString()).ToList();
            }
        }
        public double Temperature { get; internal set; }
        public int TagNumber { get; set; } = 0;

        private double _CC { get; set; } = 66;
        private double _CV { get; set; } = 28.8;
        private double _FV { get; set; } = 27.6;
        internal int CC_Setting { get; set; } = 660;
        internal int CV_Setting { get; set; } = 288;
        internal int FV_Setting { get; set; } = 276;
        internal int TC_Setting { get; set; } = 60;
        public string UseVehicleName { get; set; } = "";

        internal void SetAsNotUsing()
        {
            IsBatteryFull = Connected = IsUsing = false;
            Iout = Vin = Vout = Temperature = 0;
            UseVehicleName = "";
            ErrorCodes.Clear();
        }
        internal void SetAsUsing()
        {
            IsUsing = Connected = true;
        }
    }
}
