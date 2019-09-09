// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace zAppDev.DotNet.Framework.Utilities
{
    public class DataAccessContext<T>
    {
        public Expression<Func<T, bool>> Filter { get; set; }
        public Dictionary<Expression<Func<T, object>>, bool> SortByColumnName { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
