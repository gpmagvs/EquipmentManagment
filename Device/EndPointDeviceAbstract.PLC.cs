using System;
using System.Collections.Generic;
using System.Text;
using CIMComponent;
using EquipmentManagment.PLC;

namespace EquipmentManagment.Device
{
    public partial class EndPointDeviceAbstract
    {
        public MemoryTable EQPLCMemoryTb;
        public MemoryTable EQPLCMemoryTb_Write;
        public MemoryTable AGVSMemoryTb;
        public clsPLCMemOption PLCMemOption;

    }
}
