using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YZ.Utility
{
    public class Asym_RSA : ICrypto
    {
        private string m_RsaKey = "<RSAKeyValue><Modulus>wOq0QHjSnNmS6qAySWCYhWhMfWZHyCz+u2kTdFSboVoRgAH4T+wobLydGXUVdi2XccJwjvZcPHOZ5vZpYY9Hf9fkpJfxpOwaIB2IV+owq0EFyCdhE7vTFHiZm2cfCo+T8m224KHrMEFsoAhd11eQzyhXIU2K7XHiX5Xu2Jtnn1s=</Modulus><Exponent>AQAB</Exponent><P>9g91q1gltBev0vWlfdkElVXcV7TIu99/nHo5DE5wDDQPGO2Fmtfy02rWlc1G9pm67xcdCgPQ8wKbJ1JuYPY99Q==</P><Q>yLWuJ2/R5Levg3h8f2RZ2EhnyN3+ht7t0sFtdKSBOroU8Mgtvsu6FGkYQdihqN3+mbe3nICq/GuROvg3MUVGDw==</Q><DP>XlYTAPwsiF1EdZbkOdmIHlDqx11yUEUhwbZCROuVnbgfyajWvkTovhGJ76jh+g16U8wCwCIya9il7291DguaOQ==</DP><DQ>AfEQAD2qsCW+wuzVd34HCHqa1myfW7qoXlOUtX4p6eGG9lVZa/EYmb3yiCCKX9HV9rK6Sf9MqCh6PTHNhuJ+rQ==</DQ><InverseQ>yye+av62y1KJrvhUGtw5wqY0rBW8aCwywlqAy4+gOU8OsoNpzSW5j1rLNz7vZxCv/smrVDPm2hvJN745Ln3yow==</InverseQ><D>s/xFn8EZ/myfvXcoc31Dz3O3qWc7oW8ZWhB2rhoh+S/nE97CpQ5XyNtQVuf91fxDR0d5bGg9NclE1U8gknzy3prh4WoRsDv9ik44Dge8FvlFotAWuRJeSlla55m3mv9EcoKq9mxxDAMTin1Bnd70yE/HAnzybgSgFQUhNNxh4kE=</D></RSAKeyValue>";

        public string Decrypt(string encryptedBase64ConnectString)
        {
            byte[] rgb = Convert.FromBase64String(encryptedBase64ConnectString);
            CspParameters cspParams = new CspParameters();
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider(cspParams);
            provider.FromXmlString(this.m_RsaKey);
            byte[] bytes = provider.Decrypt(rgb, false);
            return Encoding.Unicode.GetString(bytes);
        }

        public string Encrypt(string plainConnectString)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(plainConnectString);
            CspParameters cspParams = new CspParameters();
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider(cspParams);
            provider.FromXmlString(this.m_RsaKey);
            byte[] inArray = provider.Encrypt(bytes, false);
            return Convert.ToBase64String(inArray, 0, inArray.Length);
        }

        #region for js rsa Encrypt

        private RSAParameters param;

        public Asym_RSA()
        {
            CspParameters cspParams = new CspParameters();
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider(cspParams);
            provider.FromXmlString(this.m_RsaKey);

            param = provider.ExportParameters(false);
        }

        public string GetRSA_E()
        {
            return BytesToHexString(param.Exponent);
        }

        public string GetRSA_M()
        {
            return BytesToHexString(param.Modulus);
        }

        public string EncryptForJSDecrypt(string message)
        {
            CspParameters cspParams = new CspParameters();
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider(cspParams);
            provider.FromXmlString(this.m_RsaKey);

            byte[] tmp1 = provider.Decrypt(HexStringToBytes(message), false);
            string tmp2 = ASCIIBytesToString(tmp1);
            return ASCIIBytesToString(Convert.FromBase64String(tmp2));
        }

        private string BytesToHexString(byte[] input)
        {
            StringBuilder hexString = new StringBuilder(64);

            for (int i = 0; i < input.Length; i++)
            {
                hexString.Append(String.Format("{0:X2}", input[i]));
            }
            return hexString.ToString();
        }

        private byte[] HexStringToBytes(string hex)
        {
            if (hex.Length == 0)
            {
                return new byte[] { 0 };
            }

            if (hex.Length % 2 == 1)
            {
                hex = "0" + hex;
            }

            byte[] result = new byte[hex.Length / 2];

            for (int i = 0; i < hex.Length / 2; i++)
            {
                result[i] = byte.Parse(hex.Substring(2 * i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            }

            return result;
        }

        private string ASCIIBytesToString(byte[] input)
        {
            System.Text.ASCIIEncoding enc = new ASCIIEncoding();
            return enc.GetString(input);
        }

        #endregion

        #region 生成非对称秘钥
        /// <summary>
        /// 获取RSA加密公钥私钥对
        /// </summary>
        /// <returns>Key:PublicKey;Value:PrivateKey</returns>
        public KeyValuePair<string, string> GetKeyPair()
        {
            var rsaProvider = new RSACryptoServiceProvider();
            //将RSA算法的公钥导出到字符串PublicKey中，参数为false表示不导出私钥
            var publicKey = rsaProvider.ToXmlString(false);
            //将RSA算法的私钥导出到字符串PrivateKey中，参数为true表示导出私钥
            var privateKey = rsaProvider.ToXmlString(true);
            return new KeyValuePair<string, string>(publicKey, privateKey);
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="publicKey">公钥(xml)</param>
        /// <param name="plainStr">加密文本</param>
        /// <returns>密文</returns>
        public string Encrypt(string publicKey, string plainStr)
        {
            var rsa = new RSACryptoServiceProvider();
            byte[] data = Encoding.UTF8.GetBytes(plainStr);
            rsa.FromXmlString(publicKey);
            int keySize = rsa.KeySize / 8;
            var bufferSize = keySize - 11;
            var buffer = new byte[bufferSize];
            var msInput = new MemoryStream(data);
            var msOutput = new MemoryStream();
            int readLen = msInput.Read(buffer, 0, bufferSize);
            while (readLen > 0)
            {
                byte[] dataToEnc = new byte[readLen];
                Array.Copy(buffer, 0, dataToEnc, 0, readLen);
                byte[] encData = rsa.Encrypt(dataToEnc, false);
                msOutput.Write(encData, 0, encData.Length);
                readLen = msInput.Read(buffer, 0, bufferSize);
            }

            msInput.Close();
            var result = msOutput.ToArray();
            msOutput.Close();
            rsa.Clear();
            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="privateKey">私钥（xml）</param>
        /// <param name="encryptStr">待解密文本</param>
        /// <returns>解密后字符串</returns>
        public string Decrypt(string privateKey, string encryptStr)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);
            var keySize = rsa.KeySize / 8;
            var buffer = new byte[keySize];
            var msInput = new MemoryStream(Convert.FromBase64String(encryptStr));
            var msOuput = new MemoryStream();
            var readLen = msInput.Read(buffer, 0, keySize);
            while (readLen > 0)
            {
                var dataToDec = new byte[readLen];
                Array.Copy(buffer, 0, dataToDec, 0, readLen);
                byte[] decData = rsa.Decrypt(dataToDec, false);
                msOuput.Write(decData, 0, decData.Length);
                readLen = msInput.Read(buffer, 0, keySize);
            }
            msInput.Close();
            var result = msOuput.ToArray();
            msOuput.Close();
            rsa.Clear();
            return Encoding.UTF8.GetString(result);
        }

        #endregion
    }
}

