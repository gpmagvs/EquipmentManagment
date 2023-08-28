using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentManagment.Tool
{
    public static class Extensions
    {
        /// <summary>
        /// [0]:low [1]:high
        /// </summary>
        /// <param name="intVal"></param>
        /// <returns></returns>
        public static byte[] GetHighLowBytes(this int intVal)
        {
            byte lowbyte = (byte)(intVal & 0xFF); // 取得低位元組
            byte highbyte = (byte)((intVal >> 8) & 0xFF); // 取得高位元組
            return new byte[2] { lowbyte, highbyte };
        }
        public static short GetInt(this byte[] bytes)
        {
            return BitConverter.ToInt16(bytes, 0);
        }

        public static bool[] GetBoolArray(this ushort InputValue)
        {
            bool[] OutputData = new bool[16];
            bool[] OriginBoolArray = new bool[16];
            int BitInt = 0;
            while (InputValue > 0)
            {
                OriginBoolArray[BitInt] = InputValue % 2 == 1;
                BitInt++;
                InputValue /= 2;
            }
            Array.Copy(OriginBoolArray, 8, OutputData, 0, 8);
            Array.Copy(OriginBoolArray, 0, OutputData, 8, 8);
            return OutputData;
        }
        public static ushort GetUshort(this bool[] BoolArray)
        {
            bool[] NewSwitchArray = new bool[16];
            Array.Copy(BoolArray, 8, NewSwitchArray, 0, 8);
            Array.Copy(BoolArray, 0, NewSwitchArray, 8, 8);
            ushort ReturnData = 0;
            for (int i = 0; i < NewSwitchArray.Length; i++)
            {
                ReturnData += (ushort)(Convert.ToUInt16(Math.Pow(2, i)) * Convert.ToUInt16(NewSwitchArray[i]));
            }
            return ReturnData;

        }
    }
}
