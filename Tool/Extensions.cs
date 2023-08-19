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
    }
}
