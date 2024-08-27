using System;
using System.Collections.Generic;

namespace EquipmentManagment.Device.Options
{
    public class clsRackOptions : clsEndPointOptions
    {
        public int Columns { get; set; } = 3;
        public int Rows { get; set; } = 3;
        public override EQ_TYPE EqType { get; set; } = EQ_TYPE.STK;
        public clsLayoutInfo LayoutInfo { get; set; } = new clsLayoutInfo();

        public List<clsRackPortProperty> PortsOptions { get; set; } = new List<clsRackPortProperty>();
        [Obsolete]
        public new clsRackIOLocation IOLocation { get; set; } = new clsRackIOLocation();

        /// <summary>
        /// 將EQ當作WIP
        /// </summary>
        public bool MaterialInfoFromEquipment { get; set; } = false;
        /// <summary>
        /// key: colunm index , value: tag陣列(因為有可能有貼雙Tag)
        /// </summary>
        public Dictionary<int, int[]> ColumnTagMap = new Dictionary<int, int[]>();
        public class clsLayoutInfo
        {
            public int Width { get; set; } = 400;
            public int Height { get; set; } = 400;
        }
    }



}
