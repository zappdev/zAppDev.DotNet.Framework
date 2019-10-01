// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
namespace zAppDev.DotNet.Framework.Mvc
{
    public interface ICacheKeyHasher
    {
        string ApiName { get; set; }
        string Operation { get; set; }
        string OriginalKey { get; set; }
        string UserName { get; set; }

        string GetHashedKey();
        string GetHashedKey(string apiName, string operationName, string originalKey, string userName);
        ICacheKeyHasher SplitToObject(string hashedKey, string deliminator = "|", bool throwOnException = false);
    }
}