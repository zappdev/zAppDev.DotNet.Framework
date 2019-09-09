// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
namespace zAppDev.DotNet.Framework.Tests.Data.POCO
{
    public class User
    {
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual byte[] VersionTimestamp { get; set; }
    }
}
