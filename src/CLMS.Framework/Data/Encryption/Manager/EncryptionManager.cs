using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using log4net;

namespace CLMS.Framework.Data.Encryption.Manager
{
    public class EncryptionManager : IEncryptionManager
    {
        public string StringEncryptionKey;

        private static EncryptionManager _instance;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EncryptionManager));

        public static EncryptionManager Instance => _instance ?? (_instance = new EncryptionManager());

        private EncryptionManager()
        {
            InitializeKey();
        }

        private void InitializeKey()
        {
            StringEncryptionKey = GetEncryptionKey();
        }

        #region Security Keys


        public string GetEncryptionKey()
        {
            throw new NotImplementedException("GetEncryptionKey");
        }
        
        #endregion

        #region String Value

        public static string EncryptString(string stringValue, string encryptionKey)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(encryptionKey);
                aesAlg.IV = Encoding.UTF8.GetBytes(encryptionKey.Substring(16));
                aesAlg.Padding = PaddingMode.Zeros;
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(stringValue);
                        }
                        var bytesEncrypted = msEncrypt.ToArray();
                        var encryptedString = Convert.ToBase64String(bytesEncrypted);
                        return encryptedString;
                    }
                }
            }
        }

        public string EncryptString(string stringValue)
        {
            return EncryptString(stringValue, StringEncryptionKey);
        }

        public static string DecryptString(string encryptedString, string encryptionKey, bool validateKey = true, bool inputIsBASE64 = true)
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
            return DecryptString(bytesEncrypted, encryptionKey, validateKey, inputIsBASE64);
        }

        public static string DecryptString(byte[] bytesEncrypted, string encryptionKey, bool validateKey = true, bool inputIsBASE64 = true)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(encryptionKey);
                aesAlg.IV = Encoding.UTF8.GetBytes(encryptionKey.Substring(16));
                aesAlg.Padding = PaddingMode.Zeros;
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                var validModResults = new System.Collections.Generic.List<int> { 0, 16 }; //these are valid mod results with a 32byte length encryption key
                var hasvalidLength = (bytesEncrypted.Length >= encryptionKey.Length
                                      && validModResults.Contains(bytesEncrypted.Length % encryptionKey.Length))
                                     ||
                                     (bytesEncrypted.Length < encryptionKey.Length
                                      && validModResults.Contains(encryptionKey.Length % bytesEncrypted.Length));
                if (validateKey && !hasvalidLength)
                {
                    Logger.Debug("Key has invalid length!");
                    return string.Empty;
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

        public string DecryptString(string encryptedString)
        {
            return DecryptString(encryptedString, StringEncryptionKey);
        }

        #endregion

        #region Integer Value

        public static string EncryptInteger(string stringValue, string encryptionKey)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(encryptionKey);
                aesAlg.IV = Encoding.UTF8.GetBytes(encryptionKey.Substring(16));
                aesAlg.Padding = PaddingMode.Zeros;
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(stringValue);
                        }
                        var bytesEncrypted = msEncrypt.ToArray();
                        var encryptedString = Convert.ToBase64String(bytesEncrypted);
                        return encryptedString;
                    }
                }
            }
        }

        public string EncryptInteger(string stringValue)
        {
            return EncryptInteger(stringValue, StringEncryptionKey);
        }

        public static int? DecryptInteger(string encryptedString, string encryptionKey, bool validateKey = true, bool inputIsBASE64 = true)
        {
            if (string.IsNullOrEmpty(encryptedString))
            {
                return Convert.ToInt32(encryptedString);
            }
            if (validateKey && inputIsBASE64 && !IsBase64String(encryptedString))
            {
                Logger.Debug($"Key {encryptedString} is not a BASE64 string!");
                return Convert.ToInt32(encryptedString);
            }
            var bytesEncrypted = inputIsBASE64 ? Convert.FromBase64String(encryptedString) : Encoding.UTF8.GetBytes(encryptedString);
            return DecryptInteger(bytesEncrypted, encryptionKey, validateKey, inputIsBASE64);
        }

        public static int? DecryptInteger(byte[] bytesEncrypted, string encryptionKey, bool validateKey = true, bool inputIsBASE64 = true)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(encryptionKey);
                aesAlg.IV = Encoding.UTF8.GetBytes(encryptionKey.Substring(16));
                aesAlg.Padding = PaddingMode.Zeros;
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                var validModResults = new System.Collections.Generic.List<int> { 0, 16 }; //these are valid mod results with a 32byte length encryption key
                var hasvalidLength = (bytesEncrypted.Length >= encryptionKey.Length
                                      && validModResults.Contains(bytesEncrypted.Length % encryptionKey.Length))
                                     ||
                                     (bytesEncrypted.Length < encryptionKey.Length
                                      && validModResults.Contains(encryptionKey.Length % bytesEncrypted.Length));
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
                            return Convert.ToInt32(stringValue);
                        }
                    }
                }
            }
        }

        public int? DecryptInteger(string encryptedString)
        {
            return DecryptInteger(encryptedString, StringEncryptionKey);
        }

        #endregion

        #region Object Value

        public static byte[] EncryptObject(string value, string encryptionKey)
        {
            return new Encryptor(encryptionKey).Encrypt(value);
        }

        public byte[] EncryptObject(string value)
        {
            return new Encryptor(StringEncryptionKey).Encrypt(value);
        }

        public static string DecryptObject(byte[] encryptedString, string encryptionKey, bool validateKey = true, bool inputIsBASE64 = true)
        {
            return new Decryptor(encryptionKey).Decrypt(encryptedString, validateKey, inputIsBASE64);
        }

        public static string DecryptObject(byte[] bytesEncrypted, string encryptionKey, Type targetType, bool validateKey = true, bool inputIsBASE64 = true)
        {
            return new Decryptor(encryptionKey).Decrypt(bytesEncrypted, validateKey, inputIsBASE64);
        }

        public string DecryptObject(byte[] encryptedString)
        {
            return DecryptObject(encryptedString, StringEncryptionKey);
        }

        #endregion

        public static bool IsBase64String(string s)
        {
            s = s.Trim();
            return (s.Length % 4 == 0) && System.Text.RegularExpressions.Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", System.Text.RegularExpressions.RegexOptions.None);
        }
    }
}