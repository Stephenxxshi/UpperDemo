using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plant01.Core.Data.Converters
{
    public static class BitHelper
    {
        /// <summary>
        /// 将字节数组转换为位数组
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>位数组</returns>
        public static bool[] ToBitArray(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return Array.Empty<bool>();
            var bits = new bool[bytes.Length * 8];
            for (int i = 0; i < bytes.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    bits[i * 8 + j] = (bytes[i] & 1 << j) != 0;
                }
            }
            return bits;
        }


        public static bool[] ConvertIntToIOStatus(int value, bool highToLow = false)
        {
            bool[] ioStatus = new bool[16];
            int maskedValue = value & 0xFFFF; // 确保只处理低16位

            for (int i = 0; i < 16; i++)
            {
                if (highToLow)
                {
                    // 从最高位到最低位依次处理（bit15到bit0）
                    ioStatus[i] = (maskedValue & 1 << 15 - i) != 0;
                }
                else
                {
                    // 从最低位到最高位依次处理（bit0到bit15）
                    ioStatus[i] = (maskedValue & 1 << i) != 0;
                }
            }

            return ioStatus;
        }
    }
}
