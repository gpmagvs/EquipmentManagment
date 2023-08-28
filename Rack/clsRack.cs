﻿using System;
using System.Collections.Generic;
using System.Text;
using EquipmentManagment.Device;

namespace EquipmentManagment.Rack
{
    public class clsRack : EndPointDeviceAbstract
    {
        public clsRackOptions RackOptions { get; set; }
        /// <summary>
        /// 總儲存格數
        /// </summary>
        public int TotalZones => RackOptions.Columns * RackOptions.Rows;
        public clsRack(clsRackOptions options) : base(options)
        {
            RackOptions = options;
        }

        public PortStatusAbstract[] PortsStatus { get; set; } = new clsRackPort[0];

        public override PortStatusAbstract PortStatus { get; set; }

        protected override void DefineInputData()
        {
        }
    }
}