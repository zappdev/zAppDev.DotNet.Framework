// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Security.Cryptography;
using System.Text;

namespace zAppDev.DotNet.Framework.Services
{
    public class CacheKeyHasher : ICacheKeyHasher
    {
        public string ApiName { get; set; }
        public string UserName { get; set; }
        public string Operation { get; set; }
        public string OriginalKey { get; set; }

        private readonly string _deliminator;

        public CacheKeyHasher(string deliminator = "|")
        {
            _deliminator = deliminator;
        }

        public string GetHashedKey()
        {
            string hash = "";
            using (MD5 md5Hash = MD5.Create())
            {
                hash = GetMd5Hash(md5Hash, OriginalKey);
            }

            return $"{ApiName}{_deliminator}{UserName}{_deliminator}{Operation}{_deliminator}{hash}";
        }

        public ICacheKeyHasher SplitToObject(string hashedKey, string deliminator = "|", bool throwOnException = false)
        {
            var result = new CacheKeyHasher();
            try
            {
                var values = hashedKey.Split(new[] { deliminator }, StringSplitOptions.None);

                result.ApiName = values[0];
                result.UserName = values[1];
                result.Operation = values[2];
                result.OriginalKey = values[3];
            }
            catch (Exception)
            {
                if (throwOnException) throw;
            }

            return result;
        }

        private string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }

}