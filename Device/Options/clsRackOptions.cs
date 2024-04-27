using System;
using System.Collections.Generic;

namespace EquipmentManagment.Device.Options
{
    public class clsRackOptions : clsEndPointOptions
    {
        public int Columns { get; set; } = 3;
        public int Rows { get; set; } = 3;
        public clsLayoutInfo LayoutInfo { get; set; } = new clsLayoutInfo();

        public List<clsRackPortProperty> PortsOptions { get; set; } = new List<clsRackPortProperty>();
        [Obsolete]
        public new clsRackIOLocation IOLocation { get; set; } = new clsRackIOLocation();


        public class clsLayoutInfo
        {
            public int Width { get; set; } = 400;
            public int Height { get; set; } = 400;
        }
    }

    /// <summary>
    /// 每一個儲存格的參數屬性
    /// </summary>
    public class clsRackPortProperty
    {
        public string ID => $"{Row}-{Column}";
        public int Row { get; set; } = 0;
        public int Column { get; set; } = 0;
        public clsPortIOLocation IOLocation { get; set; } = new clsPortIOLocation();

        public class clsPortIOLocation
        {

            #region X-Input
            public ushort Tray_Sensor1 { get; set; } = 0;
            public ushort Tray_Sensor2 { get; set; } = 1;
            public ushort Box_Sensor1 { get; set; } = 2;
            public ushort Box_Sensor2 { get; set; } = 3;
            #endregion
        }
    }



}
