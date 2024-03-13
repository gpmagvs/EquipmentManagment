using EquipmentManagment.Connection;
using EquipmentManagment.MainEquipment;
using Modbus.Data;
using System;
using System.Collections.Generic;
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
            { "CheckRackContentStateIOSignal","空框/實框IO訊號檢查(目前僅 UMTC S1/5F專案須將此設為true, false:不檢查,true:檢查)" },
        };
        public ConnectOptions ConnOptions { get; set; } = new ConnectOptions();
        public string Name { get; set; } = "";
        /// <summary>
        /// 設備唯一ID，可供MCS系統識別使用
        /// </summary>
        public string DeviceID { get; set; } = "SYS2341G23";
        public int TagID { get; set; }

        public EQ_TYPE EqType { get; set; }

        public EQLDULD_TYPE LdULdType { get; set; } = EQLDULD_TYPE.LDULD;
        public EQ_PICKUP_CARGO_MODE LoadUnloadCargoMode { get; set; } = EQ_PICKUP_CARGO_MODE.AGV_PICK_AND_PLACE;
        /// <summary>
        /// 允許取放貨之車輛類型
        /// </summary>
        public VEHICLE_TYPE Accept_AGV_Type { get; set; } = VEHICLE_TYPE.ALL;
        public string Region { get; set; } = "";

        public List<string> ValidDownStreamEndPointNames { get; set; }

        public clsEQIOLocation IOLocation { get; set; } = new clsEQIOLocation();

        public string PLCOptionJsonFile { get; set; }

        public string InstalledCargoID { get; set; } = "";

        public int Height { get; set; } = 0;

        public bool CheckRackContentStateIOSignal { get; set; } = false;

        internal bool IsEmulation = false;

        internal bool IsProdution_EQ => EqType == EQ_TYPE.EQ || EqType == EQ_TYPE.EQ_OVEN;

    }

    public class clsEQIOLocation
    {
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
        public ushort HS_EQ_BUSY { get; set; } = 13;

        public ushort Empty_CST { get; set; } = 14;
        public ushort Full_CST { get; set; } = 15;

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
