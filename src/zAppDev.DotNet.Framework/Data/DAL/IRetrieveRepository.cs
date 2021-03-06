﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace zAppDev.DotNet.Framework.Data.DAL
{
    public interface IRetrieveRepository
    {
        T GetById<T>(object id, bool throwIfNotFound = true, bool checkAccess = true) where T : class;
        List<T> Get<T>(Expression<Func<T, bool>> predicate, bool cacheQuery = true);
        List<T> Get<T>(Expression<Func<T, bool>> predicate,
                       int startRowIndex,
                       int pageSize,
                       Dictionary<Expression<Func<T, object>>, bool> orderBy,
                       out int totalRecords, bool cacheQuery = true);
        List<T> GetAll<T>(bool cacheQuery = true);
        List<T> GetAll<T>(int startRowIndex, int pageSize, out int totalRecords, bool cacheQuery = true);
        IQueryable<T> GetAsQueryable<T>(Expression<Func<T, bool>> predicate = null, bool cacheQuery = true);
        IQueryable<T> GetMainQuery<T>();
    }
}
