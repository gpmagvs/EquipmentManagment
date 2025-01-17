﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EquipmentManagment.Device;
using EquipmentManagment.Device.Options;
using EquipmentManagment.Manager;
using Newtonsoft.Json;
using static EquipmentManagment.WIP.clsPortOfRack;

namespace EquipmentManagment.WIP
{
    /// <summary>
    /// 表示一個WIP架
    /// </summary>
    public class clsRack : EndPointDeviceAbstract
    {
        public clsRackOptions RackOption { get; set; }
        /// <summary>
        /// 總儲存格數
        /// </summary>
        public int TotalZones => RackOption.Columns * RackOption.Rows;

        public clsPortOfRack[] PortsStatus { get; set; } = new clsPortOfRack[0];

        public override PortStatusAbstract PortStatus { get; set; }

        public static event EventHandler<(string portID, string carrierID)> OnRackPortCarrierIDChanged;


        public clsRack(clsRackOptions options) : base(options)
        {
            RackOption = options;
            Initialize();
        }
        public int EmptyPortNum => PortsStatus.Where(port => port.MaterialExistSensorStates.Values.Any(exist => exist == SENSOR_STATUS.ON)).Count();
        public int HasCargoPortNum => PortsStatus.Count() - PortsStatus.Where(port => port.CargoExist).Count();

        public override bool IsMaintaining { get { return false; } }
        public override Task StartSyncData()
        {
            if (RackOption.MaterialInfoFromEquipment)
                return Task.CompletedTask;
            return base.StartSyncData();
        }
        protected override void InputsHandler()
        {
            foreach (clsPortOfRack port in PortsStatus)
            {
                port.UpdateIO(ref InputBuffer);
            }
        }

        protected override void WriteOutuptsData()
        {
        }
        private void Initialize()
        {
            PortsStatus = RackOption.PortsOptions.Select(option =>
                   new clsPortOfRack(option, this)
               ).ToArray();
        }

        internal void ModifyPortCargoID(string portID, string newCargoID, bool triggerByEqCarrierIDChanged)
        {
            clsPortOfRack _PortFound = PortsStatus.FirstOrDefault(port => port.Properties.ID == portID);
            if (_PortFound != null)
            {
                _PortFound.CarrierID = newCargoID;
                if (!triggerByEqCarrierIDChanged)
                    OnRackPortCarrierIDChanged?.Invoke(this, (portID, newCargoID));
            }
        }

        public PortOfRackViewModel[] GetPortStatusWithEqInfo()
        {
            if (RackOption.MaterialInfoFromEquipment)
            {
                return PortsStatus.Select(port =>
                {
                    int tagOfColumn = this.RackOption.ColumnTagMap[port.Properties.Column].First();
                    MainEquipment.clsEQ eq = StaEQPManagager.GetEQByTag(tagOfColumn);
                    PortOfRackViewModel portClone = JsonConvert.DeserializeObject<PortOfRackViewModel>(JsonConvert.SerializeObject(port));
                    portClone.NickName = eq.EndPointOptions.Name;
                    portClone.RackContentState = eq.RackContentState;
                    portClone.CarrierExist = eq.Port_Exist;
                    portClone.CarrierID = eq.PortStatus.CarrierID;
                    portClone.TagNumbers = port.TagNumbers;
                    return portClone;
                }).ToArray();
            }
            else
                return PortsStatus.Select(port =>
              {
                  PortOfRackViewModel portClone = JsonConvert.DeserializeObject<PortOfRackViewModel>(JsonConvert.SerializeObject(port));
                  portClone.TagNumbers = port.TagNumbers;
                  return portClone;
              }).ToArray(); ;
        }

        public clsPortOfRack GetPortByKeyWithRackName(string key)
        {
            var result = PortsStatus.FirstOrDefault(port => $"{RackOption.Name}_{port.Properties.ID}" == key);
            return result;
        }

        public override void UpdateCarrierInfo(int tagNumber, string carrierID, int height)
        {
            clsPortOfRack Port = PortsStatus.FirstOrDefault(port => port.TagNumbers.Contains(tagNumber) && port.Layer == height);
            if (Port != null)
            {
                Port.CarrierID = carrierID;
                if (this.EndPointOptions.IsEmulation)
                {
                }
            }
        }
    }
}
