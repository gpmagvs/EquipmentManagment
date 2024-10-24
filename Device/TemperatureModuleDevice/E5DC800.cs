using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentManagment.Device.TemperatureModuleDevice
{
    public class E5DC800 : TemperatureModuleAbstract
    {


        struct VAR_LOC
        {
            public const byte TEMPERATURE_H = 0x02;
            public const byte TEMPERATURE_L = 0x03;
        }

        /// <summary>
        /// 讀取
        /// </summary>
        byte[] MODBUS_RTU_CMD_READ_STATUS = new byte[] { 0x01, 0x03, 0x00, 0x00, 0x00, 0x20, 0x44, 0x12 };

        public E5DC800(TemperatureModuleSetupOptions setupOptions) : base(setupOptions)
        {
        }

        protected override async Task<double> GetTemperature()
        {
            try
            {

                byte[] response = await SendDataToDevice(MODBUS_RTU_CMD_READ_STATUS);
                byte[] data = await GetStatusDataByte();
                return ParseTemperature(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                throw new Exception("GetTemperature Error");
            }
        }

        public double ParseTemperature(byte[] data)
        {
            byte temperatureHB = data[VAR_LOC.TEMPERATURE_H];
            byte temperatureLB = data[VAR_LOC.TEMPERATURE_L];
            double temperature = (temperatureHB * 256 + temperatureLB);
            return temperature;
        }

        private async Task<byte[]> GetStatusDataByte()
        {//01 03 40 00 00 00 1F 02 00 40 00 00 00 00 1B 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 2E C1
            byte[] response = await base.SendDataToDevice(MODBUS_RTU_CMD_READ_STATUS);
            if (response.Any() && response.Length == 69) // 69: 頭三個(id,fc,len) + 64個資料 + CRC
            {
                return response.Skip(3).Take(64).ToArray();
            }
            else
                return new byte[64];
        }
    }
}
