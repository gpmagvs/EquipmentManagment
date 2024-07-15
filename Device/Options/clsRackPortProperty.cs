using System.Collections.Generic;
using static EquipmentManagment.WIP.clsPortOfRack;

namespace EquipmentManagment.Device.Options
{
    /// <summary>
    /// 每一個儲存格的參數屬性
    /// </summary>
    public class clsRackPortProperty
    {
        public Dictionary<string, string> Notes { get; set; } = new Dictionary<string, string>()
        {
            {"ProductionQualityStore" ,"該PORT可存放OK/NG產品設定(0:僅可放OK產品 | 1: NG:僅可放NG產品)" },
            {"CargoTypeStore" ,"該PORT可存放的貨物種類(0:Tray | 1: Rack | 2:Mixed)" }
        };
        public string ID => $"{Row}-{Column}";
        public int Row { get; set; } = 0;
        public int Column { get; set; } = 0;
        /// <summary>
        /// 該PORT可存放OK/NG產品設定
        /// </summary>
        public PRUDUCTION_QUALITY ProductionQualityStore { get; set; } = PRUDUCTION_QUALITY.OK;
        /// <summary>
        /// 該PORT可以存放的貨物類型(Tray、Rack、 Mixed (Tray/Rack都可以))
        /// </summary>
        public CARGO_TYPE CargoTypeStore { get; set; } = CARGO_TYPE.MIXED;

        public Port_Enable PortEnable = Port_Enable.Enable;
        public clsPortIOLocation IOLocation { get; set; } = new clsPortIOLocation();
        public clsPortUseToEQProperty EQInstall { get; set; } = new clsPortUseToEQProperty();

        public int StoragePriority {  get; set; } = 0;

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
