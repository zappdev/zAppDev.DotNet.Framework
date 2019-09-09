// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
namespace zAppDev.DotNet.Framework.Services
{
    public interface ICacheKeyHasher
    {
        string ApiName { get; set; }
        string Operation { get; set; }
        string OriginalKey { get; set; }
        string UserName { get; set; }

        string GetHashedKey();
        ICacheKeyHasher SplitToObject(string hashedKey, string deliminator = "|", bool throwOnException = false);
    }
}