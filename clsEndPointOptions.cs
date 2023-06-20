using EquipmentManagment.Connection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment
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
        CHARGE
    }

    public class clsEndPointOptions
    {
        public ConnectOptions ConnOptions { get; set; } = new ConnectOptions();
        public string Name { get; set; } = "";
        public int TagID { get; set; }

        public EQ_TYPE EqType { get; set; }

        public string Region { get; set; } = "";

        public List<string> ValidDownStreamEndPointNames{ get; set; }
    }

    public class clsRackOptions : clsEndPointOptions
    {
        public int Columns { get; set; } = 3;
        public int Rows { get; set; } = 3;


    }
}
