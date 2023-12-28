using EquipmentManagment.Connection;
using EquipmentManagment.MainEquipment;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EquipmentManagment.Device
{
    public enum EQ_TYPE
    {
        /// <summary>
        /// 機台設備，如烤箱、轉框機...
        /// </summary>
        EQ,
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

    public class clsEndPointOptions
    {
        public ConnectOptions ConnOptions { get; set; } = new ConnectOptions();
        public string Name { get; set; } = "";
        public int TagID { get; set; }

        public EQ_TYPE EqType { get; set; }

        public EQLDULD_TYPE LdULdType { get; set; } = EQLDULD_TYPE.LDULD;
        public string Region { get; set; } = "";

        public List<string> ValidDownStreamEndPointNames { get; set; }

        public clsEQIOLocation IOLocation { get; set; } = new clsEQIOLocation();

        public string PLCOptionJsonFile { get; set; }

        public string InstalledCargoID { get; set; } = "";

        public int Height { get; set; } = 0;

        internal bool IsEmulation = false;
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

        #endregion
    }

    public class clsRackOptions : clsEndPointOptions
    {
        public int Columns { get; set; } = 3;
        public int Rows { get; set; } = 3;

        public new clsRackIOLocation IOLocation { get; set; } = new clsRackIOLocation();

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
