namespace CLMS.Framework.Data.Encryption.Manager
{
    public interface IEncryptionManager
    {
        int? DecryptInteger(string encryptedString);
        string DecryptObject(byte[] encryptedString);
        string DecryptString(string encryptedString);
        string EncryptInteger(string stringValue);
        byte[] EncryptObject(string value);
        string EncryptString(string stringValue);
        string GetEncryptionKey();
    }
}