// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
namespace zAppDev.DotNet.Framework.Data.Encryption.Manager
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