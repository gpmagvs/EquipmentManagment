using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public static double LinearToDouble(this IEnumerable<byte> data)
        {
            int value = BitConverter.ToInt16(data.ToArray(), 0);
            return (double)value;
        }
        public static byte[] DoubleToLinear(this double value)
        {
            return BitConverter.GetBytes((short)value);
        }
        public static float Linear16ToDouble(this IEnumerable<byte> raw_data, int N)
        {
            // 擴展陣列到8個字節
            byte[] extendedArray = new byte[8];
            Array.Copy(raw_data.ToArray(), extendedArray, raw_data.Count());
            // 轉換為十進制
            long decimalValue = BitConverter.ToInt64(extendedArray, 0);
            return (float)Math.Pow(2, N) * decimalValue;
        }

        public static byte[] DoubleToLinear16(this double value, int N)
        {
            int exponent = N; // 計算指數
            int mantissa = (int)(value / Math.Pow(2, exponent));
            mantissa = mantissa >> 1;
            var bytes = BitConverter.GetBytes(mantissa);
            return new byte[] { bytes[1], bytes[0] };
        }
        public static double Linear11ToDouble(this IEnumerable<byte> data, int N)
        {
            //byte[] _data = data.ToArray();
            //int raw = (_data[1] << 8) | _data[0];
            //int mantissa = raw & 0x7FF;
            //return mantissa * Math.Pow(2, N);
            // 擴展陣列到8個字節
            byte[] extendedArray = new byte[8];
            var heightByte = data.ToArray()[1];
            var lowByte = data.ToArray()[0];
            byte heighByteMasked = (byte)(heightByte & 0x07);
            byte[] newBytes = new byte[] { lowByte, heighByteMasked };
            Array.Copy(newBytes, extendedArray, newBytes.Count());
            // 轉換為十進制
            long decimalValue = BitConverter.ToInt64(extendedArray, 0);
            return Math.Pow(2, N) * decimalValue;
        }

        public static byte[] DoubleToLinear11(this double value, int N = -2)
        {
            // 這裡需要實現將 value 轉換為 Linear11 格式
            // 這需要一些數學計算來確定指數和尾數
            // 這只是一個示例
            int exponent = N; // 計算指數
            int mantissa = (int)(value / Math.Pow(2, exponent));
            int raw = (mantissa & 0x7FF) | ((exponent & 0x1F) << 11);
            return new byte[] { (byte)(raw & 0xFF), (byte)((raw >> 8) & 0xFF) };
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
            try
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
            catch (Exception ex)
            {
                return 0;
            }

        }

        public static int[] ToBitArray(this byte byteData)
        {
            int[] bitArray = new int[8];

            for (int i = 0; i < 8; i++)
            {
                // 使用位操作來提取每個位元的值
                bitArray[7 - i] = (byteData >> i) & 1;
            }
            bitArray = bitArray.Reverse().ToArray();
            return bitArray;
        }
    }
}
