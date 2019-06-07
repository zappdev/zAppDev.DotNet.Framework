using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CLMS.Framework.Data.Encryption.Manager
{
    public class Encryptor
    {
        private readonly string _encryptionKey;

        public Encryptor(string encryptionKey)
        {
            _encryptionKey = encryptionKey;
        }

        public byte[] Encrypt(string value)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(_encryptionKey);
                aesAlg.IV = Encoding.UTF8.GetBytes(_encryptionKey.Substring(16));
                aesAlg.Padding = PaddingMode.Zeros;
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(value);
                        }
                        return msEncrypt.ToArray();
                    }
                }
            }
        }
    }
}
