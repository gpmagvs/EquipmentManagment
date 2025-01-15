using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.MainEquipment
{
    public partial class clsEQ
    {
        public enum EQLDULD_TYPE
        {
            LD,
            ULD,
            LDULD,
            Charge
        }

        public enum RACK_CONTENT_STATE
        {
            EMPTY, FULL, UNKNOWN
        }
        /// <summary>
        /// 設備從AGV取放貨物的方式
        /// </summary>
        public enum EQ_PICKUP_CARGO_MODE
        {
            /// <summary>
            /// 設備無取放機構，僅透過AGV(如FORK AGV)取放貨物
            /// </summary>
            AGV_PICK_AND_PLACE,
            /// <summary>
            /// 設備有取放機構，透過撈爪升降從AGV車上取放貨物
            /// </summary>
            EQ_PICK_AND_PLACE
        }
        /// <summary>
        /// 設備可接受貨物類型
        /// ref: AGVSystemCommonNet6.AGVDispatch.Messages->CST_TYPE
        /// </summary>
        public enum EQ_ACCEPT_CARGO_TYPE
        {
            None = 0 /*用在不限orCST_TYPE未指派*/, KUAN = 201, TRAY = 200
        }
    }
}
