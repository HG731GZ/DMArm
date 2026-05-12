using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delta9DOFDLL
{
    public class CRC8
    {
        private const byte Poly = 0x07; // CRC-8多项式

        /// <summary>
        /// 计算CRC-8校验值
        /// </summary>
        /// <param name="data">数据包（前26字节为数据，第27字节为CRC）</param>
        /// <returns>计算得到的CRC值</returns>
        public static byte CalculateCRC(byte[] data)
        {
            if (data.Length != 27)
            {
                throw new ArgumentException("数据包长度必须为27字节");
            }

            byte crc = 0x00;

            // 只计算前26字节
            for (int i = 0; i < 26; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x80) != 0)
                    {
                        crc = (byte)((crc << 1) ^ Poly);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }

            return crc;
        }

        /// <summary>
        /// 验证数据包CRC是否正确
        /// </summary>
        public static bool VerifyCRC(byte[] data)
        {
            if (data.Length != 27)
                return false;

            byte calculatedCRC = CalculateCRC(data);
            return calculatedCRC == data[26];
        }

        /// <summary>
        /// 为数据包添加CRC校验位（前26字节数据 -> 27字节带CRC的数据包）
        /// </summary>
        public static byte[] AddCRC(byte[] dataWithoutCRC)
        {
            if (dataWithoutCRC.Length != 26)
            {
                throw new ArgumentException("数据长度必须为26字节");
            }

            byte[] fullData = new byte[27];
            Array.Copy(dataWithoutCRC, fullData, 26);
            fullData[26] = CalculateCRC(fullData);

            return fullData;
        }
    }
}
