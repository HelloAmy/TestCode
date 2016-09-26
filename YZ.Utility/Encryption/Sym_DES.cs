using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YZ.Utility
{
    public class Sym_DES : ICrypto
    {
        private static byte[] s_DesIV = new byte[] { 0x1d, 0x87, 0x34, 9, 0x41, 3, 0x61, 0x62 };
        private static byte[] s_DesKey = new byte[] { 1, 0x4d, 0x54, 0x22, 0x45, 90, 0x17, 0x2c };

        public string Decrypt(string encryptedBase64ConnectString)
        {
            MemoryStream stream = new MemoryStream(200);
            stream.SetLength(0L);
            byte[] buffer = Convert.FromBase64String(encryptedBase64ConnectString);
            DES des = new DESCryptoServiceProvider();
            des.KeySize = 0x40;
            CryptoStream stream2 = new CryptoStream(stream, des.CreateDecryptor(s_DesKey, s_DesIV), CryptoStreamMode.Write);
            stream2.Write(buffer, 0, buffer.Length);
            stream2.FlushFinalBlock();
            stream.Flush();
            stream.Seek(0L, SeekOrigin.Begin);
            byte[] buffer2 = new byte[stream.Length];
            stream.Read(buffer2, 0, buffer2.Length);
            stream2.Close();
            stream.Close();
            return Encoding.Unicode.GetString(buffer2);
        }

        public string Encrypt(string plainConnectString)
        {
            MemoryStream stream = new MemoryStream(200);
            stream.SetLength(0L);
            byte[] bytes = Encoding.Unicode.GetBytes(plainConnectString);
            DES des = new DESCryptoServiceProvider();
            CryptoStream stream2 = new CryptoStream(stream, des.CreateEncryptor(s_DesKey, s_DesIV), CryptoStreamMode.Write);
            stream2.Write(bytes, 0, bytes.Length);
            stream2.FlushFinalBlock();
            stream.Flush();
            stream.Seek(0L, SeekOrigin.Begin);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream2.Close();
            stream.Close();
            return Convert.ToBase64String(buffer, 0, buffer.Length);
        }

        #region simple version
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="plainString">需要加密字符串</param>
        /// <param name="key">加密密钥（最大长度为8）</param>
        /// <returns></returns>
        public string Encrypt(string plainString, string key)
        {
            key += "12345678";
            var stream = new MemoryStream(200);
            stream.SetLength(0L);
            var bytes = Encoding.UTF8.GetBytes(plainString);
            var keyByte = Encoding.UTF8.GetBytes(key.Substring(0, 8));
            DES des = new DESCryptoServiceProvider();
            var stream2 = new CryptoStream(stream, des.CreateEncryptor(keyByte, s_DesIV), CryptoStreamMode.Write);
            stream2.Write(bytes, 0, bytes.Length);
            stream2.FlushFinalBlock();
            stream.Flush();
            stream.Seek(0L, SeekOrigin.Begin);
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream2.Close();
            stream.Close();
            return Convert.ToBase64String(buffer, 0, buffer.Length);
        }


        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="encryptedString">需要解密字符串</param>
        /// <param name="key">解密密钥（最大长度为8）</param>
        /// <returns></returns>
        public string Decrypt(string encryptedString, string key)
        {
            key += "12345678";
            var stream = new MemoryStream(200);
            stream.SetLength(0L);
            var buffer = Convert.FromBase64String(encryptedString);
            var keyByte = Encoding.UTF8.GetBytes(key.Substring(0, 8));
            DES des = new DESCryptoServiceProvider();
            des.KeySize = 0x40;
            var stream2 = new CryptoStream(stream, des.CreateDecryptor(keyByte, s_DesIV), CryptoStreamMode.Write);
            stream2.Write(buffer, 0, buffer.Length);
            stream2.FlushFinalBlock();
            stream.Flush();
            stream.Seek(0L, SeekOrigin.Begin);
            var buffer2 = new byte[stream.Length];
            stream.Read(buffer2, 0, buffer2.Length);
            stream2.Close();
            stream.Close();
            return Encoding.UTF8.GetString(buffer2);
        }
        #endregion
    }
}

