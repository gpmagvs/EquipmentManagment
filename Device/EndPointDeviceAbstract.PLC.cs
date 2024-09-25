using System;
using System.Collections.Generic;
using System.Text;
using CIMComponent;
using EquipmentManagment.Manager;
using EquipmentManagment.PLC;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace EquipmentManagment.Device
{
    public partial class EndPointDeviceAbstract
    {
        public MemoryTable EQPLCMemoryTb;
        public MemoryTable EQPLCMemoryTb_Write;
        public MemoryTable AGVSMemoryTb;
        public clsPLCMemOption PLCMemOption;

        public bool TryLoadMCProtocolParamFromFile(out clsPLCMemOption configuration, out string configJsonfilePath)
        {
            configuration = new clsPLCMemOption();
            configJsonfilePath = string.Empty;
            if (!string.IsNullOrEmpty(EndPointOptions.PLCOptionJsonFile) && File.Exists(EndPointOptions.PLCOptionJsonFile))
            {
                configJsonfilePath = EndPointOptions.PLCOptionJsonFile;
                try
                {
                    configuration = JsonConvert.DeserializeObject<clsPLCMemOption>(File.ReadAllText(EndPointOptions.PLCOptionJsonFile));
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                string eqPLCConfigSavefolder = Path.Combine(Path.GetDirectoryName(StaEQPManagager.Configs.EQConfigPath), "PLCConfigurations");//C:\AGVS\Equipement...
                Directory.CreateDirectory(eqPLCConfigSavefolder);
                string JsonFile = Path.Combine(eqPLCConfigSavefolder, EndPointOptions.Name + ".json");
                EndPointOptions.PLCOptionJsonFile = JsonFile;
                File.WriteAllText(JsonFile, JsonConvert.SerializeObject(configuration, Formatting.Indented));
                StaEQPManagager.EQOptions.FirstOrDefault(keypair => keypair.Value == EndPointOptions).Value.PLCOptionJsonFile = JsonFile;
                StaEQPManagager.SaveEqConfigs();

            }
            return configuration != null;
        }

    }
}
