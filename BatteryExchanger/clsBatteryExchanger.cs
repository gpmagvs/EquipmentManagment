using EquipmentManagment.Device;
using EquipmentManagment.Device.Options;
using EquipmentManagment.PLC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentManagment.BatteryExchanger
{
    public class clsBatteryExchanger : EndPointDeviceAbstract
    {
        public clsBatteryExchanger(clsEndPointOptions options) : base(options)
        {
        }

        public override PortStatusAbstract PortStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override void InputsHandler()
        {
            //TODO EQPLCMemoryTb.ReadBit();
        }
        public new async Task<bool> Connect(bool use_for_conn_test = false)
        {
            if (File.Exists(EndPointOptions.PLCOptionJsonFile))
            {
                clsPLCMemOption option = LoadPLCMemOption(EndPointOptions.PLCOptionJsonFile);
                EQPLCMemoryTb = new CIMComponent.MemoryTable(option.EQP_Bit_Size, option.IsEQP_Bit_Hex, option.EQP_Word_Size, option.IsEQP_Word_Hex, 20);
                AGVSMemoryTb = new CIMComponent.MemoryTable(option.AGVS_Bit_Size, option.IsAGVS_Bit_Hex, option.AGVS_Word_Size, option.IsAGVS_Word_Hex, 20);
                EQPLCMemoryTb.SetMemoryStart(option.EQPBitAreaName,option.EQPBitStartAddressName,option.EQPWordAreaName,option.EQPWordStartAddressName);
                AGVSMemoryTb.SetMemoryStart(option.AGVSBitAreaName,option.AGVSBitStartAddressName,option.AGVSWordAreaName,option.AGVSWordStartAddressName);
                this.PLCMemOption = option;
            }
            else
            {
                throw new FileNotFoundException();
            }
            return await base.Connect();
        }
        private clsPLCMemOption LoadPLCMemOption(string json_file_path)
        {
            string json = File.ReadAllText(json_file_path);
            return JsonConvert.DeserializeObject<clsPLCMemOption>(json);
        }

        protected override void WriteOutuptsData()
        {
        }
    }
}
