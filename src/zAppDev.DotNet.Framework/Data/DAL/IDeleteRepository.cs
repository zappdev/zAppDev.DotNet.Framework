// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.Data.DAL
{
    public interface IDeleteRepository
    {
        void Delete<T>(T entity, bool isCascaded = false) where T : class;
    }
}
