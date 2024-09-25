using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.PLC
{
    public class clsPLCMemOption
    {

        #region AGVS


        public string AGVS_Bit_Start_Address { get; set; } = "B0000";
        public int AGVS_Bit_Size { get; set; } = 128;
        public bool IsAGVS_Bit_Hex { get; set; } = true;
        public string AGVS_Word_Start_Address { get; set; } = "W0000";
        public int AGVS_Word_Size { get; set; } = 128;
        public bool IsAGVS_Word_Hex { get; set; } = true;


        #endregion
        #region EQP

        public string EQP_Bit_Start_Address { get; set; } = "B0080";
        public int EQP_Bit_Size { get; set; } = 128;
        public bool IsEQP_Bit_Hex { get; set; } = true;
        public string EQP_Word_Start_Address { get; set; } = "W0080";
        public int EQP_Word_Size { get; set; } = 384;
        public bool IsEQP_Word_Hex { get; set; } = true;
        #endregion


        internal string AGVSBitAreaName => AGVS_Bit_Start_Address.Substring(0, 1);
        internal string AGVSBitStartAddressName => AGVS_Bit_Start_Address.Replace(AGVSBitAreaName, "");
        internal string AGVSWordAreaName => AGVS_Word_Start_Address.Substring(0, 1);
        internal string AGVSWordStartAddressName => AGVS_Word_Start_Address.Replace(AGVSWordAreaName, "");

        internal string EQPBitAreaName => EQP_Bit_Start_Address.Substring(0, 1);
        internal string EQPBitStartAddressName => EQP_Bit_Start_Address.Replace(EQPBitAreaName, "");
        internal string EQPWordAreaName => EQP_Word_Start_Address.Substring(0, 1);
        internal string EQPWordStartAddressName => EQP_Word_Start_Address.Replace(EQPWordAreaName, "");


    }
}
