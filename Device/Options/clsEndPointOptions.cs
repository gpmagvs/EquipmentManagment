using EquipmentManagment.Connection;
using EquipmentManagment.MainEquipment;
using Modbus.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace EquipmentManagment.Device.Options
{
    public enum EQ_TYPE
    {
        /// <summary>
        /// 機台設備，如烤箱、轉框機...
        /// </summary>
        EQ,
        EQ_OVEN,
        /// <summary>
        /// STOCKER、WIP 
        /// </summary>
        STK,
        /// <summary>
        /// 充電站
        /// </summary>
        CHARGE,
        /// <summary>
        /// 換電站
        /// </summary>
        BATTERY_EXCHANGER
    }

    public enum VEHICLE_TYPE
    {
        ALL,
        FORK,
        SUBMERGED_SHIELD,
    }

    public class clsEndPointOptions
    {
        public Dictionary<string, string> Notes { get; set; } = new Dictionary<string, string>()
        {
            { "Accept_AGV_Type","允許進行任務之車輛種類(0:所有車種,1:叉車AGV, 2:潛盾AGV)" },
            { "EQAcceeptCargoType","設備可移載的貨物種類(0:不限, 200:子母框, 201:Tray)" },
            { "CheckRackContentStateIOSignal","空框/實框IO訊號檢查(目前僅 UMTC S1/5F專案須將此設為true, false:不檢查,true:檢查)" },
        };
        public bool Enable { get; set; } = true;
        public ConnectOptions ConnOptions { get; set; } = new ConnectOptions();
        public string Name { get; set; } = "";
        /// <summary>
        /// 設備唯一ID，可供MCS系統識別使用
        /// </summary>
        public string DeviceID { get; set; } = "SYS2341G23";
        public int TagID { get; set; }

        public virtual EQ_TYPE EqType { get; set; } = EQ_TYPE.EQ;

        public EQLDULD_TYPE LdULdType { get; set; } = EQLDULD_TYPE.LDULD;
        public EQ_PICKUP_CARGO_MODE LoadUnloadCargoMode { get; set; } = EQ_PICKUP_CARGO_MODE.AGV_PICK_AND_PLACE;

        public EQ_ACCEPT_CARGO_TYPE EQAcceeptCargoType { get; set; } = EQ_ACCEPT_CARGO_TYPE.None;

        /// <summary>
        /// 允許取放貨之車輛類型
        /// </summary>
        public VEHICLE_TYPE Accept_AGV_Type { get; set; } = VEHICLE_TYPE.ALL;
        public string Region { get; set; } = "";

        public List<string> ValidDownStreamEndPointNames { get; set; }

        public clsEQIOLocation IOLocation { get; set; } = new clsEQIOLocation();

        public string PLCOptionJsonFile { get; set; }

        public string InstalledCargoID { get; set; } = "";

        /// <summary>
        /// Zero-Base. 0=>第一層, 1=>第二層 ...以此類推。
        /// </summary>
        public int Height { get; set; } = 0;

        public bool CheckRackContentStateIOSignal { get; set; } = false;

        /// <summary>
        /// 空/實框移出狀態使用虛擬Input(Like S1 5F OVEN.)
        /// </summary>
        public bool IsFullEmptyUnloadAsVirtualInput { get; set; } = false;

        public bool IsEmulation { get; set; } = true; // 是否使用GLAC內部模擬器, 要改用EmulationMode控制
        public int EmulationMode { get; set; } = 0; //[0]:不模擬連外部設備(外部模擬器), [1]:內建模擬器, [2]:GlacEQSimulator用

        internal bool IsProdution_EQ => EqType == EQ_TYPE.EQ || EqType == EQ_TYPE.EQ_OVEN;

        public List<int> AcceptTransferTag { get; set; } = new List<int>();

        public bool IsNeedStorageMonitor { get; set; } = false; // True:會被納入水位監控, False:不會

        public int StorageMonitorPriority { get; set; } = 0; // 水位監控順位等級，數字愈大愈先被檢查
        /// <summary>
        /// 是否具有貨物轉向機構(例如平對平設備)
        /// </summary>
        public bool HasCstSteeringMechanism { get; set; } = false;
        /// <summary>
        /// 是否為雙Port設備的其中一個Port
        /// </summary>
        public bool IsOneOfDualPorts { get; set; } = false;

        public int AllowUnloadPortTypeNumber { get; set; } = 0;

        public int AnotherPortTagNumber { get; set; } = 0;

        public bool HasLDULDMechanism { get; set; } = false;


    }

    public class clsEQIOLocation
    {
        /// <summary>
        /// 設備狀態IO定義SPEC列舉項目
        /// </summary>
        public enum STATUS_IO_DEFINED_VERSION
        {
            /// <summary>
            /// 舊版狀態IO定義(有EQP_STATUS_RUN/DOWN/IDLE的那一份SPEC)
            /// </summary>
            V1 = 1,
            /// <summary>
            /// 新版狀態IO定義(有EQP_STATUS_DOWN的那一份SPEC)
            /// </summary>
            V2
        }
        [JsonConverter(typeof(StringEnumConverter))]
        public STATUS_IO_DEFINED_VERSION STATUS_IO_SPEC_VERSION { get; set; } = STATUS_IO_DEFINED_VERSION.V1;
        #region X-Input
        public ushort Load_Request { get; set; } = 0;
        public ushort Unload_Request { get; set; } = 1;
        public ushort Port_Exist { get; set; } = 2;
        public ushort Eqp_Status_Down { get; set; } = 3;
        public ushort Eqp_Status_Run { get; set; } = 4;
        public ushort Eqp_Status_Idle { get; set; } = 5;
        public ushort HS_EQ_L_REQ { get; set; } = 6;
        public ushort HS_EQ_U_REQ { get; set; } = 7;
        public ushort HS_EQ_READY { get; set; } = 8;
        public ushort HS_EQ_UP_READY { get; set; } = 9;
        public ushort HS_EQ_LOW_READY { get; set; } = 10;
        public ushort Up_Pose { get; set; } = 11;
        public ushort Down_Pose { get; set; } = 12;
        public ushort TB_Down_Pose { get; set; } = 12;
        public ushort HS_EQ_BUSY { get; set; } = 13;

        public ushort Empty_CST { get; set; } = 14;
        public ushort Full_CST { get; set; } = 15;

        public ushort Eqp_Maintaining { get; set; } = 16;
        public ushort Eqp_PartsReplacing { get; set; } = 17;

        #endregion

        #region Y-Output

        public ushort To_EQ_Up { get; set; } = 0;
        public ushort To_EQ_Low { get; set; } = 1;

        public ushort HS_AGV_VALID { get; set; } = 2;
        public ushort HS_AGV_TR_REQ { get; set; } = 3;
        public ushort HS_AGV_BUSY { get; set; } = 4;
        public ushort HS_AGV_COMPT { get; set; } = 5;

        public ushort CMD_Reserve_Up { get; set; } = 6;
        public ushort CMD_Reserve_Low { get; set; } = 7;
        public ushort HS_AGV_READY { get; set; } = 8;
        public ushort To_EQ_Empty_CST { get; set; } = 9;
        public ushort To_EQ_Full_CST { get; set; } = 10;

        #endregion

        public ClsHoldingRegist HoldingRegists { get; set; } = new ClsHoldingRegist();

        public class ClsHoldingRegist
        {
            public ushort PortTypeStatus { get; set; } = 6;
        }

    }
    public class clsRackIOLocation
    {

        #region X-Input
        public ushort Tray_Sensor1 { get; set; } = 0;
        public ushort Tray_Sensor2 { get; set; } = 1;
        public ushort Box_Sensor1 { get; set; } = 2;
        public ushort Box_Sensor2 { get; set; } = 3;
        #endregion
    }


}
