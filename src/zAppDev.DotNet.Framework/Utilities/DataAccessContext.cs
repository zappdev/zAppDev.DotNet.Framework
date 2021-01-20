// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using zAppDev.DotNet.Framework.Linq;
using zAppDev.DotNet.Framework.Mvc;

namespace zAppDev.DotNet.Framework.Utilities
{
    public class DataAccessContext<T>
    {
        public Expression<Func<T, bool>> Filter { get; set; }
        public Dictionary<Expression<Func<T, object>>, bool> SortByColumnName { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public Dictionary<string, bool> SortByColumns { get; set; }
        public List<FilterInfo> FilterColumns { get; set; }

        public DataAccessContext()
        {
        }

        public DataAccessContext(DatasourceRequest request)
        {
            SortByColumnName = new Dictionary<Expression<Func<T, object>>, bool>();
            SortByColumns = new Dictionary<string, bool>();
            PageIndex = (int)Math.Floor((decimal)(request.StartRow / request.PageSize));
            PageSize = request.PageSize;
            Filter = @this => true;
            Filter = Filter.And(RuntimePredicateBuilder.BuildPredicateForFiltering<T>(request.Filters));
            
            if (request.OrderBy != null && request.OrderBy.Any())
            {
                var _dynamicOrdering = RuntimePredicateBuilder.BuildPredicateForOrdering<T>(request.OrderBy);
                if (_dynamicOrdering != null)
                {
                    foreach (var _dynamicOrderBy in _dynamicOrdering)
                    {
                        SortByColumns.Add(_dynamicOrderBy.Column, _dynamicOrderBy.Ascending);
                        SortByColumnName.Add(_dynamicOrderBy.Expression, _dynamicOrderBy.Ascending);
                    }
                }
            }

            FilterColumns = request.Filters;
        }
    }

    public class DataAccessFilterColumn
    {
        public string Column { get; set; }
        public string Value { get; set; }
        public RowOperator RowOperator { get; set; }
        public FilterOperator FilterOperator { get; set; }
    }
}
