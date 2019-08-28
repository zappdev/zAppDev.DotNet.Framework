using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using log4net;

namespace zAppDev.DotNet.Framework.Data.Encryption.Manager
{
    public class Decryptor
    {
        private readonly string _encryptionKey;
        private readonly ILog Logger = LogManager.GetLogger(typeof(Decryptor));

        public Decryptor(string encryptionKey)
        {
            _encryptionKey = encryptionKey;
        }

        public string Decrypt(string encryptedString, bool validateKey = true, bool inputIsBASE64 = true)
        {
            if (string.IsNullOrEmpty(encryptedString))
            {
                return encryptedString;
            }
            if (validateKey && inputIsBASE64 && !IsBase64String(encryptedString))
            {
                Logger.Debug($"Key {encryptedString} is not a BASE64 string!");
                return encryptedString;
            }
            var bytesEncrypted = inputIsBASE64 ? Convert.FromBase64String(encryptedString) : Encoding.UTF8.GetBytes(encryptedString);
            return Decrypt(bytesEncrypted, validateKey, inputIsBASE64);
        }

        public string Decrypt(byte[] bytesEncrypted, bool validateKey = true, bool inputIsBASE64 = true)
        {
            try
            {
                using (var aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(_encryptionKey);
                    aesAlg.IV = Encoding.UTF8.GetBytes(_encryptionKey.Substring(16));
                    aesAlg.Padding = PaddingMode.Zeros;
                    var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    var validModResults = new System.Collections.Generic.List<int> { 0, 16 }; //these are valid mod results with a 32byte length encryption key
                    var hasvalidLength = (bytesEncrypted.Length >= _encryptionKey.Length
                                          && validModResults.Contains(bytesEncrypted.Length % _encryptionKey.Length))
                                         ||
                                         (bytesEncrypted.Length < _encryptionKey.Length
                                          && validModResults.Contains(_encryptionKey.Length % bytesEncrypted.Length));
                    if (validateKey && !hasvalidLength)
                    {
                        Logger.Debug("Key has invalid length!");
                        return null;
                    }
                    using (var msDecrypt = new MemoryStream(bytesEncrypted))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                var stringValue = srDecrypt.ReadToEnd();
                                stringValue = stringValue.Trim().TrimEnd(new[] { (char)0 });
                                return stringValue;
                            }
                        }
                    }
                }
            }
            catch (CryptographicException ce)
            {
                string hint = $"\r\n(Message: {ce.Message})";
                if (ce.Message.Contains("complete block"))
                {
                    hint = $"\r\n(Hint: Check that the data being decrypted are correct (valid) in your DataBase. E.g. a correctly encrypted Byte Array or a String, depending on the type of your Data)";
                }
                Logger.Error($"A CryptographicException was caught and handled while Decrypting Data.\r\nHandling the exception by retuning a NULL value.{hint}");
                return null;
            }
            catch (Exception e)
            {
                Logger.Error($"A [{e.GetType().FullName}] Exception was caught while Decrypting Data: {e.Message}. A NULL value will be returned. \r\n\r\n(StackTrace: {e.StackTrace})");
                return null;
            }
        }


        public bool IsBase64String(string s)
        {
            s = s.Trim();
            return (s.Length % 4 == 0) && System.Text.RegularExpressions.Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", System.Text.RegularExpressions.RegexOptions.None);
        }
    }
}
