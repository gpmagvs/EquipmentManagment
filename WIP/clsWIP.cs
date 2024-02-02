using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EquipmentManagment.Device;
using EquipmentManagment.Device.Options;
using static EquipmentManagment.WIP.clsWIPPort;

namespace EquipmentManagment.WIP
{
    /// <summary>
    /// 表示一個WIP架
    /// </summary>
    public class clsWIP : EndPointDeviceAbstract
    {
        public clsRackOptions RackOption { get; set; }
        /// <summary>
        /// 總儲存格數
        /// </summary>
        public int TotalZones => RackOption.Columns * RackOption.Rows;

        public clsWIPPort[] PortsStatus { get; set; } = new clsWIPPort[0];

        public override PortStatusAbstract PortStatus { get; set; }
        public clsWIP(clsRackOptions options) : base(options)
        {
            RackOption = options;
            Initialize();
        }
        public int EmptyPortNum => PortsStatus.Where(port => port.ExistSensorStates.Values.Any(exist => exist)).Count();
        public int HasCargoPortNum => PortsStatus.Count() - PortsStatus.Where(port => port.ExistSensorStates.Values.Any(exist => exist)).Count();

        protected override void InputsHandler()
        {

            bool TryGetInputFromBuffer(ushort location)
            {
                try
                {
                    return InputBuffer[location];
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + ex.StackTrace);
                    return false;
                }
            }

            foreach (clsWIPPort port in PortsStatus)
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
                   new clsWIPPort(option)
               ).ToArray();
        }

        internal void ModifyPortCargoID(string portID, string newCargoID)
        {
            clsWIPPort _PortFound = PortsStatus.FirstOrDefault(port => port.Properties.ID == portID);
            if (_PortFound != null)
            {
                _PortFound.CarrierID = newCargoID;
            }
        }
    }
}
