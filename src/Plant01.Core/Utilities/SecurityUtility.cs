using System.Security.Cryptography;
using System.Text;

namespace Plant01.Core.Utilities
{
    public static class SecurityUtility
    {
        #region HMAC
        public static class HMACs
        {
            #region 计算哈希
            /// <summary>
            /// 计算哈希
            /// </summary>
            /// <typeparam name="T">继承HAMC</typeparam>
            /// <param name="hmac">算法实例</param>
            /// <param name="text">待加密文本</param>
            /// <param name="key">密钥</param>
            /// <param name="encoding">编码</param>
            /// <returns></returns>
            private static string ComputeHash<T>(T hmac, string text, string key, string encoding) where T : HMAC
            {
                string retStr;
                using (hmac)
                {
                    hmac.Key = Encoding.GetEncoding(encoding).GetBytes(key);
                    var inputBye = Encoding.GetEncoding(encoding).GetBytes(text);
                    var outputBye = hmac.ComputeHash(inputBye);
                    retStr = BitConverter.ToString(outputBye);
                    hmac.Clear();
                }
                return retStr.Replace("-", "").ToLower();
            }
            #endregion

            #region MD5
            /// <summary>
            /// MD5
            /// </summary>
            /// <param name="text">待加密文本</param>
            /// <param name="key">密钥</param>
            /// <param name="encoding">编码</param>
            /// <returns></returns>
            public static string MD5(string text, string key = "iB!y5n235fjSrZvP", string encoding = "utf-8") => ComputeHash(new HMACMD5(), text, key, encoding);
            #endregion

            #region SHA1
            /// <summary>
            /// SHA1
            /// </summary>
            /// <param name="text">待加密文本</param>
            /// <param name="key">密钥</param>
            /// <param name="encoding">编码</param>
            /// <returns></returns>
            public static string SHA1(string text, string key = "6^6@2F1IS9QVM&lT", string encoding = "utf-8") => ComputeHash(new HMACSHA1(), text, key, encoding);
            #endregion

            #region SHA256
            /// <summary>
            /// SHA256
            /// </summary>
            /// <param name="text">待加密文本</param>
            /// <param name="key">密钥</param>
            /// <param name="encoding">编码</param>
            /// <returns></returns>
            public static string SHA256(string text, string key = "r$*g&#oLa#TbhMPS", string encoding = "utf-8") => ComputeHash(new HMACSHA256(), text, key, encoding);
            #endregion

            #region SHA384
            /// <summary>
            /// SHA384
            /// </summary>
            /// <param name="text">待加密文本</param>
            /// <param name="key">密钥</param>
            /// <param name="encoding">编码</param>
            /// <returns></returns>
            public static string SHA384(string text, string key = "W!e&jE%%mey3IB3&", string encoding = "utf-8") => ComputeHash(new HMACSHA384(), text, key, encoding);
            #endregion

            #region SHA512
            /// <summary>
            /// SHA512
            /// </summary>
            /// <param name="text">待加密文本</param>
            /// <param name="key">密钥</param>
            /// <param name="encoding">编码</param>
            /// <returns></returns>
            public static string SHA512(string text, string key = "!ow4A7u0CMYZxF&q", string encoding = "utf-8") => ComputeHash(new HMACSHA512(), text, key, encoding);
            #endregion
        }
        #endregion

        #region RSA
        public static class RSAs
        {
            #region 获取哈希算法
            /// <summary>
            /// 获取哈希算法
            /// </summary>
            /// <param name="hashAlgorithmName"></param>
            /// <returns></returns>
            private static HashAlgorithm GetHashAlgorithm(string hashAlgorithmName)
            {
                return hashAlgorithmName switch
                {
                    "MD5" => MD5.Create(),
                    "SHA1" => SHA1.Create(),
                    "SHA256" => SHA256.Create(),
                    "SHA384" => SHA384.Create(),
                    "SHA512" => SHA512.Create(),
                    _ => SHA256.Create(),
                };
            }
            #endregion

            #region 签名
            /// <summary>
            /// 签名
            /// </summary>
            /// <param name="signData">要签名的数据字节</param>
            /// <param name="privateKey">XML类型的密钥</param>
            /// <param name="hashAlgorithmName">哈希算法</param>
            /// <param name="urlEncode">结果是否需要编码</param>
            /// <returns></returns>
            public static string SignData(byte[] signData, string privateKey, string hashAlgorithmName = "SHA256")
            {
                byte[] signatureBytes;
                using (var provider = new RSACryptoServiceProvider())
                {
                    provider.FromXmlString(privateKey);
                    using (var halg = GetHashAlgorithm(hashAlgorithmName))
                    {
                        signatureBytes = provider.SignData(signData, halg);
                    }
                }
                return Convert.ToBase64String(signatureBytes);
            }
            #endregion

            #region 验签
            /// <summary>
            /// 验签
            /// </summary>
            /// <param name="waitSignatureBytes">等待签名验证的数据</param>
            /// <param name="signatureBytes">已经签名验证的数据</param>
            /// <param name="publicKey">XML格式的公钥</param>
            /// <param name="hashAlgorithmName">哈希算法</param>
            /// <returns></returns>
            public static bool VerifyData(byte[] waitSignatureBytes, byte[] signatureBytes, string publicKey, string hashAlgorithmName = "SHA256")
            {
                bool isVerify;
                using (var provider = new RSACryptoServiceProvider())
                {
                    provider.FromXmlString(publicKey);
                    using (var halg = GetHashAlgorithm(hashAlgorithmName))
                    {
                        isVerify = provider.VerifyData(waitSignatureBytes, halg, signatureBytes);
                    }
                }
                return isVerify;
            }
            #endregion
        }
        #endregion

        #region CRCs
        /// <summary>
        /// CRCs
        /// </summary>
        public static class CRCs
        {
            #region CRC-16
            /// <summary>
            /// CRC-16
            /// </summary>
            /// <param name="data">待计算校验和的数据</param>
            /// <param name="reverse">高低位是否颠倒</param>
            /// <returns>默认高位在前</returns>
            public static byte[] CRC16(byte[] data, bool reverse = true)
            {
                ushort crc = 0x0;
                foreach (byte d in data)
                {
                    crc ^= d;
                    for (var j = 0; j < 8; j++)
                    {
                        if ((crc & 0x01) == 0x01)
                        {
                            crc >>= 1;
                            crc ^= 0xA001; // POLY 0x8005 二进制颠倒
                        }
                        else
                        {
                            crc >>= 1;
                        }
                    }
                }

                byte hi = (byte)((crc & 0xFF00) >> 8);
                byte lo = (byte)(crc & 0xFF);

                if (reverse)
                {
                    return [hi, lo];
                }
                return [lo, hi];
            }
            #endregion

            #region CRC-16/MODBUS
            /// <summary>
            /// CRC-16/MODBUS
            /// </summary>
            /// <param name="data">待计算校验和的数据</param>
            /// <param name="reverse">高低位是否颠倒</param>
            /// <returns>默认高位在前</returns>
            public static byte[] CRC16Modbus(byte[] data, bool reverse = true)
            {
                ushort crc = 0xFFFF;
                foreach (byte d in data)
                {
                    crc ^= d;
                    for (var j = 0; j < 8; j++)
                    {
                        if ((crc & 0x01) == 0x01)
                        {
                            crc >>= 1;
                            crc ^= 0xA001; // POLY 0x8005 二进制颠倒
                        }
                        else
                        {
                            crc >>= 1;
                        }
                    }
                }

                byte hi = (byte)((crc & 0xFF00) >> 8);
                byte lo = (byte)(crc & 0xFF);

                if (reverse)
                {
                    return [hi, lo];
                }
                return [lo, hi];
            }
            #endregion

            #region CRC-16/CCITT
            /// <summary>
            /// CRC-16/CCITT
            /// </summary>
            /// <param name="data">待计算校验和的数据</param>
            /// <param name="reverse">高低位是否颠倒</param>
            /// <returns>默认高位在前</returns>
            public static byte[] CRC16CCITT(byte[] data, bool reverse = true)
            {
                ushort crc = 0x0000;
                foreach (byte d in data)
                {
                    crc ^= d;
                    for (var j = 0; j < 8; j++)
                    {
                        if ((crc & 0x01) == 0x01)
                        {
                            crc >>= 1;
                            crc ^= 0x8408; // POLY 0x1021 二进制颠倒
                        }
                        else
                        {
                            crc >>= 1;
                        }
                    }
                }

                byte hi = (byte)((crc & 0xFF00) >> 8);
                byte lo = (byte)(crc & 0xFF);

                if (reverse)
                {
                    return [hi, lo];
                }
                return [lo, hi];
            }
            #endregion
        }
        #endregion
    }
}