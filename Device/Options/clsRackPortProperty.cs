namespace EquipmentManagment.Device.Options
{
    /// <summary>
    /// 每一個儲存格的參數屬性
    /// </summary>
    public class clsRackPortProperty
    {
        public string ID => $"{Row}-{Column}";
        public int Row { get; set; } = 0;
        public int Column { get; set; } = 0;
        public clsPortIOLocation IOLocation { get; set; } = new clsPortIOLocation();
        public clsPortUseToEQProperty EQInstall { get; set; } = new clsPortUseToEQProperty();

        public class clsPortUseToEQProperty
        {
            /// <summary>
            /// Just like QX Converter or GPM LawDrop(?)
            /// </summary>
            public bool IsUseForEQ { get; set; } = false;
            public string BindingEQName { get; set; } = "";
        }

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
