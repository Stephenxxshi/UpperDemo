using System.Collections;

namespace Plant01.Core.Extensions
{
    public static class ConverterExtension
    {
        #region ToHex
        /// <summary>
        /// ByteArrayToHex
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ByteArrayToHex(this IEnumerable<byte> value)
        {
            return BitConverter.ToString([.. value]).Replace("-", " ", StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region FromHex
        /// <summary>
        /// HexToByteArray
        /// </summary>
        /// <param name="value">十六进制字符串</param>
        /// <returns>byte数组</returns>
        public static byte[] HexToByteArray(this string value)
        {
            return [.. Enumerable.Range(0, value.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(value.Substring(x, 2), 16))];
        }
        #endregion

        #region Int16ToBooleanArray
        /// <summary>
        /// Int16ToBooleanArray
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool[] Int16ToBooleanArray(this short integer, int resultSize = 16)
        {
            bool[] result = new bool[resultSize];
            byte[] Array = BitConverter.GetBytes(integer);
            BitArray bitArray = new(Array);
            bitArray.CopyTo(result, 0);
            return result;
        }
        #endregion

        #region BooleanArrayToByteArray
        /// <summary>
        /// BooleanArrayToByteArray
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] BooleanArrayToByteArray(this bool[] value)
        {
            int counter = (value.Length % 8 == 0) ? (value.Length / 8) : (value.Length / 8 + 1);
            byte[] bytes = new byte[counter];

            BitArray bitArray = new(value);
            bitArray.CopyTo(bytes, 0);

            return bytes;
        }
        #endregion
    }
}
