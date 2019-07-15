using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
#if NETFRAMEWORK
#else
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
#endif

namespace CLMS.Framework.Mvc
{
    public class RuntimePredicateBuilder
    {
        public static List<string> RequiredAssembliesPaths;

        // FORCE DateTime formatting for now...
        // We use moment.js compatible formats for cell values and datepickers, but our C# backend 
        // is incompatible with them and we need A LOT of effort to implement convertions between them
        // See related comment on clms.data.list.ts
        private static string DefaultDateTimeFormat = "d/M/yyyy";

        private static string CreateFilterExpression(FilterOperator op, ColumnInfo col, string filterValue, string pathToProp)
        {
            var exp = "";
            var convertFromNullable = DataTypeIsNullable(col) ? ".GetValueOrDefault()" : "";

            var safeCasePathToProp = pathToProp;
            var safeCastSegments = new List<string>();
            var pathToPropSegments = pathToProp.Split('.');
            for (var i = 1; i < pathToPropSegments.Length - 1; i++)
            {
                safeCastSegments.Add($@"{string.Join(".", pathToPropSegments.Take(i + 1).ToArray())}");
            }
            if (safeCastSegments.Count > 0)
            {
                safeCastSegments.Add(pathToProp);
                safeCasePathToProp = string.Join(" != null && ", safeCastSegments);
            }
            switch (op)
            {
                case FilterOperator.LIKE:
                    if (col.MambaDataType == "DateTime")
                    {
                        /* Removed the convertFromNullable because it produced a SQL query with coalesce function
						 * like the following:
						 *
						 * 		coalesce([SOME_COLUMN_NAME].[DATETIME_FIELD], '1/1/0001 12:00:00 πμ')
						 * 
						 * 	which results to the following error:
						 *
						 *		Conversion failed when converting date and/or time from character string
						 *
						 *	because the coalesce parameter for the default datetime cannot be implicitly converted
						 */
                        //exp = $"{pathToProp}{convertFromNullable} >= {filterValue} &&  {pathToProp}{convertFromNullable} <= {filterValue}2";
                        exp = $"{safeCasePathToProp} >= {filterValue} &&  {safeCasePathToProp} <= {filterValue}2";
                    }
                    else if (col.MambaDataType == "string")
                    {
                        exp = $"{safeCasePathToProp}{convertFromNullable} != null && {pathToProp}{convertFromNullable}.ToLowerInvariant().Contains({filterValue}.ToString().ToLowerInvariant())";
                    }
                    else
                    {
                        exp = $"{safeCasePathToProp}{convertFromNullable}.ToString().ToLowerInvariant().Contains({filterValue}.ToString().ToLowerInvariant())";
                    }
                    break;

                case FilterOperator.EQUAL_TO:
                    exp = $"{safeCasePathToProp} == {filterValue}";
                    break;

                case FilterOperator.NOT_EQUAL_TO:
                    exp = $"{safeCasePathToProp} != {filterValue}";
                    break;

                case FilterOperator.GREATER_THAN:
                    exp = $"{safeCasePathToProp} > {filterValue}";
                    break;

                case FilterOperator.GREATER_THAN_OR_EQUAL_TO:
                    exp = $"{safeCasePathToProp} >= {filterValue}";
                    break;

                case FilterOperator.HAS_NO_VALUE:
                    exp = $"{safeCasePathToProp} == null";
                    break;

                case FilterOperator.HAS_VALUE:
                    exp = $"{safeCasePathToProp} != null";
                    break;

                case FilterOperator.LESS_THAN:
                    exp = $"{safeCasePathToProp} < {filterValue}";
                    break;

                case FilterOperator.LESS_THAN_OR_EQUAL_TO:
                    exp = $"{safeCasePathToProp} <= {filterValue}";
                    break;

                case FilterOperator.RANGE:
                    exp = $"{safeCasePathToProp} >= {filterValue} &&  {safeCasePathToProp} <= {filterValue}2";
                    break;
            }

            return exp;
        }

        private static Dictionary<RowOperator, string> FilterRowOpToCsOp = new Dictionary<RowOperator, string>()
        {
            {RowOperator.AND, "&&"},
            {RowOperator.OR, "||"},
            {RowOperator.NONE, ""}
        };

        private static object FormatFilterValue(ColumnInfo col, object value)
        {
            var val = "";

            using (var writer = new StringWriter())
            {
#if NETFRAMEWORK
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    if (value is DateTime)
                    {
                        value = value.ToString();
                    }

                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(value), writer, null);
                    val = writer.ToString();
                }
#else
                using (var provider = Microsoft.CSharp.CSharpCodeProvider.CreateProvider("C#"))
                {
                    if (value is DateTime)
                    {
                        value = value.ToString();
                    }

                    provider.GenerateCodeFromExpression(new System.CodeDom.CodePrimitiveExpression(value), writer, null);
                    val = writer.ToString();
                }
#endif

            }

            switch (col.MambaDataType)
            {
                case "Object":
                case "string":
                    return val;

                case "int":
                    return $"Int32.Parse({val})";

                case "bool":
                    return $"bool.Parse({val})";

                case "char":
                    return $"char.Parse({val})";

                case "Guid":
                    return $"Guid.Parse({val})";

                case "double":
                    return $"double.Parse({val})";

                case "decimal":
                    return $"decimal.Parse({val})";

                case "float":
                    return $"float.Parse({val})";

                case "long":
                    return $"long.Parse({val})";

                case "DateTime":
                    return $"DateTime.ParseExact({val }, \"{DefaultDateTimeFormat}\", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal).ToUniversalTime()";

                default:
                    return val;
            }
        }

        private static string MambaDataTypeToCSharp(string dt)
        {
            if (dt == "Object")
            {
                return typeof(object).Name;
            }
            else if (dt == "int")
            {
                return typeof(int).Name + "?";
            }
            else if (dt == "long")
            {
                return typeof(long).Name + "?";
            }
            else if (dt == "float")
            {
                return typeof(float).Name + "?";
            }
            else if (dt == "decimal")
            {
                return typeof(decimal).Name + "?";
            }
            else if (dt == "double")
            {
                return typeof(double).Name + "?";
            }
            else if (dt == "char")
            {
                return typeof(char).Name + "?";
            }
            else if (dt == "DateTime")
            {
                return typeof(DateTime).Name + "?";
            }
            else
            {
                return dt;
            }
        }

        private static bool DataTypeIsNullable(ColumnInfo col)
        {
            return col.MambaDataType != "bool" && col.MambaDataType != "string";
        }

        private static bool ValueCanBeParsedToColumnDatatype(FilterInfo filter)
        {
            var val = filter.Value.ToString();

            switch (filter.Column.MambaDataType)
            {
                case "Object":
                case "string":
                    return true;

                case "int":
                    return int.TryParse(val, out _);

                case "bool":
                    return bool.TryParse(val, out _);

                case "char":
                    return char.TryParse(val, out _);

                case "Guid":
                    return Guid.TryParse(val, out _);

                case "double":
                    return double.TryParse(val, out _);

                case "decimal":
                    return decimal.TryParse(val, out _);

                case "float":
                    return float.TryParse(val, out _);

                case "long":
                    return long.TryParse(val, out _);

                case "DateTime":
                    return DateTime.TryParseExact(val, DefaultDateTimeFormat, null, System.Globalization.DateTimeStyles.None, out _);

                default:
                    return false;
            }
        }

        private static string WrapCodeIntoDummyAssembly(string code)
        {
            return $@"
					using System;
					using System.Linq;
					using System.Linq.Expressions;
					using System.Collections.Generic;

					namespace DummyAssembly
					{{
						public class DummyClass
						{{
							{code}
						}}
					}};";
        }

        private static string TypeName(Type type)
        {
            return type.FullName.Replace("+", ".");
        }

        private static string GenerateFilterCode<T>(List<FilterInfo> filters)
        {
            var type = typeof(T);

            var predicateCode = "";
            var lastRowOperator = RowOperator.NONE;
            var variableDefinitionsCode = "";

            // When creating the predicate we start with filters with row operator OR
            // e.g. Global Filters are set using an OR row operator
            // Quick Filters and Custom Filters are set using an AND operator
            // We first want to filter using OR (records matching at least one condition)
            // then keep records that match ALL other (AND) conditions
            filters = filters.OrderByDescending(f => f.RowOperator == RowOperator.OR).ToList();

            foreach (var filter in filters)
            {
                //We do not encrypt the parameter value in the equal and not equal operators, because NHibernate does that for us by calling NullSafeGet
                //when the query is executed
                if (filter.Column.IsEncrypted && !(filter.Operator == FilterOperator.EQUAL_TO || filter.Operator == FilterOperator.NOT_EQUAL_TO))
                {
                    filter.Value = BaseViewPageBase<object>.EncryptValue(filter.Value);
                }

                // Since filters are ordered with OR preceeding AND, when operator changes,
                // we wrap the predicate created at the moment in parenthesis
                // so that the above "filtering strategy" is honored
                var wrap = filters.First() != filter && lastRowOperator != filter.RowOperator;

                if (wrap)
                {
                    predicateCode = "(" + predicateCode + ")";
                }

                lastRowOperator = filter.RowOperator;

                if (!string.IsNullOrWhiteSpace(predicateCode))
                {
                    predicateCode += " " + FilterRowOpToCsOp[filter.RowOperator] + " ";
                }

                if (ValueCanBeParsedToColumnDatatype(filter) ||
                    filter.Operator == FilterOperator.HAS_VALUE ||
                    filter.Operator == FilterOperator.HAS_NO_VALUE)
                {
                    var pathToProp = "q." + filter.Column.Name;
                    var varName = filter.Column.Name.Replace(".", "_") + "Value" + filters.IndexOf((filter));

                    if (filter.Operator != FilterOperator.HAS_VALUE &&
                        filter.Operator != FilterOperator.HAS_NO_VALUE)
                    {
                        if (filter.Column.MambaDataType == "DateTime")
                        {
                            switch (filter.Operator)
                            {
                                case FilterOperator.LIKE:
                                    variableDefinitionsCode += $"var {varName} = {FormatFilterValue(filter.Column, filter.Value)}.Date;";
                                    variableDefinitionsCode += $"var {varName}2 = {FormatFilterValue(filter.Column, filter.Value)}.Date.AddDays(1).AddTicks(-1);";
                                    break;
                                case FilterOperator.LESS_THAN_OR_EQUAL_TO:
                                    variableDefinitionsCode += $"var {varName} = {FormatFilterValue(filter.Column, filter.Value)}.Date.AddDays(1).AddTicks(-1);";
                                    break;
                                default:
                                    variableDefinitionsCode += $"var {varName} = {FormatFilterValue(filter.Column, filter.Value)};";
                                    break;
                            }
                        }
                        else
                        {
                            variableDefinitionsCode += $"var {varName} = {FormatFilterValue(filter.Column, filter.Value)};";
                        }
                    }

                    if (filter.Operator == FilterOperator.RANGE)
                    {
                        variableDefinitionsCode += $"var {varName}2 = {FormatFilterValue(filter.Column, filter.SecondValue)};";
                    }

                    predicateCode += CreateFilterExpression(filter.Operator, filter.Column, varName, pathToProp);
                }
                else
                {
                    predicateCode += "false";
                }
            }

            return $"public static Expression<Func<{TypeName(type)}, bool>> GetFilterPredicate() {{ {variableDefinitionsCode} return q => {predicateCode}; }}";
        }

        private static string GenerateOrderByCode<T>(List<OrderByInfo> orderBy)
        {
            var type = typeof(T);

            var code = $@"public static List<AppCode.DynamicOrderBy<{TypeName(type)}>> BuildOrderByList() {{
                            var list = new List<AppCode.DynamicOrderBy<{TypeName(type)}>>();";

            foreach (var order in orderBy)
            {
                var ascending = (order.Direction == OrderByDirection.ASC).ToString().ToLower();

                var expressionToUse = "";
                var parts = order.Column.Name.Split('.').ToList();

                if (parts.Count > 1)
                {
                    // Make member null safe so that 
                    // we don't get null pointer exceptions
                    foreach (var part in parts)
                    {
                        if (part != parts.First())
                        {
                            expressionToUse += " || ";
                        }

                        var index = parts.IndexOf(part);
                        var subParts = "";

                        for (var i = 0; i < index; i++)
                        {
                            subParts += $".{parts[i]}";
                        }

                        expressionToUse += $"o{subParts} == null";
                    }

                    var defaultValue = GetColumnDefaultValue(order.Column);
                    expressionToUse += $" ? {defaultValue} : o.{order.Column.Name}";
                }
                else
                {
                    expressionToUse = $"o.{order.Column.Name}";
                }
                code += $"list.Add(new AppCode.DynamicOrderBy<{TypeName(type)}>(\"{order.Column.Name}\", o => {expressionToUse}, {ascending}));";
            }

            code += "return list; }";

            return code;
        }

        private static string GenerateGroupByCode<T>(List<GroupByInfo> groupBy)
        {
            var type = typeof(T);

            var code = $@"public static List<AppCode.DynamicGroupBy<{TypeName(type)}>> BuildGroupByList() {{
                            var list = new List<AppCode.DynamicGroupBy<{TypeName(type)}>>();";

            foreach (var group in groupBy)
            {
                var expanded = (group.State == GroupState.EXPANDED).ToString().ToLower();

                var pathToProp = "o." + group.Column.Name;
                var safecasePathToProp = pathToProp;
                var safecastSegments = new List<string>();
                var pathToPropSegments = pathToProp.Split('.');
                for (var i = 1; i < pathToPropSegments.Length - 1; i++)
                {
                    safecastSegments.Add($@"{string.Join(".", pathToPropSegments.Take(i + 1).ToArray())}");
                }
                if (safecastSegments.Count > 0)
                {
                    safecastSegments.Add(pathToProp);
                    safecasePathToProp = string.Join(" != null && ", safecastSegments);
                    safecasePathToProp += $" != null ? {pathToProp} : null";
                }

                code += $"list.Add(new AppCode.DynamicGroupBy<{TypeName(type)}>(\"{group.Column.Name}\", o => {safecasePathToProp}, {expanded}));";
            }

            code += "return list; }";

            return code;
        }

        private static string GenerateAggregatorsCode<T>(List<AggregatorInfo<T>> aggregators)
        {
            var type = typeof(T);

            var code =
                $"public static List<AppCode.AggregatorInfo<{TypeName(type)}>> BuildAggregatorPredicates(List<AppCode.AggregatorInfo<{TypeName(type)}>> aggregators)  {{";

            for (var i = 0; i < aggregators.Count(); i++)
            {
                if (aggregators[i].Type == AggregatorType.COUNT) continue;

                code += $"aggregators[{i}].Expression = q => (double)(q.{aggregators[i].Column}.GetValueOrDefault(0));";
            }

            code += "return aggregators; }";

            return code;
        }

        private static string GenerateClosedGroupsCode<T>(List<GroupByInfo> groupBy, List<AggregatorInfo<T>> aggregators)
        {
            aggregators = aggregators ?? new List<AggregatorInfo<T>>();
            var type = typeof(T);

            var code = $@"public static AppCode.DynamicClosedGroupsInfo<{TypeName(type)}> BuildClosedGroupsInfo() {{
                            var closedGroupsInfo = new AppCode.DynamicClosedGroupsInfo<{TypeName(type)}>();
                            var groups = new List<CLMS.Framework.LinqRuntimeTypeBuilder.FieldDefinition<{TypeName(type)}>>();";


            foreach (var entry in groupBy)
            {
                if (entry.Inactive) continue;

                if (string.IsNullOrEmpty(entry.Column.Name)) break;

                var dataType = MambaDataTypeToCSharp(entry.Column.MambaDataType);
                var defaultValue = GetColumnDefaultValue(entry.Column);

                code += $@"groups.Add(new CLMS.Framework.LinqRuntimeTypeBuilder.FieldDefinition<{TypeName(type)}>
                           {{
                               Name = ""{entry.Column.Name}"",
                               Type = typeof({dataType}),
                               Selector = o => o.{entry.Column.Name}/* ??  {defaultValue}*/
                           }});";
            }

            code += $@"var selectors = new List<CLMS.Framework.LinqRuntimeTypeBuilder.FieldDefinition<IGrouping<object, {TypeName(type)}>>>();
                       selectors.Add(new CLMS.Framework.LinqRuntimeTypeBuilder.FieldDefinition<IGrouping<object, {TypeName(type)}>>
                       {{
                            Name = ""Key"",
                            Type = closedGroupsInfo.GroupByAnonymousType,
                            Selector = g => g.Key
                       }});";

            foreach (var entry in aggregators)
            {
                if (string.IsNullOrEmpty(entry.Column)) break;

                var selector = "";
                var selectorDt = "";

                switch (entry.Type)
                {
                    case AggregatorType.COUNT:
                        selector = $"o.Count()";
                        selectorDt = "int";
                        break;

                    case AggregatorType.AVERAGE:
                        selector = $"o.Average(i => (double)i.{entry.Column}.GetValueOrDefault(0))";
                        selectorDt = "double";
                        break;

                    case AggregatorType.SUM:
                        selector = $"o.Sum(i => (double)(i.{entry.Column}.GetValueOrDefault(0)))";
                        selectorDt = "double";
                        break;
                }

                code += $@"selectors.Add(new CLMS.Framework.LinqRuntimeTypeBuilder.FieldDefinition<IGrouping<object, {TypeName(type)}>>
                           {{
                               Name = ""{entry.Type}{GroupsHelper.GroupPathSeperator}{entry.Column}"",
                               Type = typeof({selectorDt}),
                               Selector = o => {selector}
                           }});";
            }

            // Always add a count
            if (!aggregators.Any(c => c.Type == AggregatorType.COUNT))
            {
                code += $@"selectors.Add(new CLMS.Framework.LinqRuntimeTypeBuilder.FieldDefinition<IGrouping<object, {TypeName(type)}>>
                           {{
                               Name = ""COUNT{GroupsHelper.GroupPathSeperator}__Count"",
                               Type = typeof(int),
                               Selector = o => o.Count()
                           }});";
            }

            code += $@"Type groupAnonType, selectorAnonType;
                       var expressions = CLMS.Framework.LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.CreateGroupByAndSelectExpressions<{TypeName(type)}>(groups, selectors, out groupAnonType, out selectorAnonType);

                       closedGroupsInfo.GroupByExpression = expressions.Item1;
                       closedGroupsInfo.GroupByAnonymousType = groupAnonType;
                       closedGroupsInfo.CombinedSelectorExpression = expressions.Item2;
                       closedGroupsInfo.SelectorsAnonymousType = selectorAnonType;

                       return closedGroupsInfo; }}";

            return code;
        }
        private static string GetColumnDefaultValue(ColumnInfo col)
        {
            var defaultValue = "";
            switch (col.MambaDataType)
            {
                case "string":
                    defaultValue = "\"\"";
                    break;
                case "DateTime":
                    defaultValue = "DateTime.MinValue";
                    break;
                case "char":
                    defaultValue = "''";
                    break;
                case "int":
                case "long":
                case "float":
                case "decimal":
                case "double":
                    defaultValue = "0";
                    break;
                default:
                    defaultValue = "new Object()";
                    break;
            }
            return defaultValue;
        }

        public static Expression<Func<T, bool>> BuildPredicateForFiltering<T>(List<FilterInfo> filters)
        {
            if (filters == null || !filters.Any()) return (i) => true;

            var key = GetCashedMethodInfosKey(MethodBase.GetCurrentMethod(), typeof(T), filters);

            MethodInfo methodInfo = null;
            if (cashedMethodInfos.ContainsKey(key))
            {
                methodInfo = cashedMethodInfos[key];
            }
            else
            {
                var code = WrapCodeIntoDummyAssembly(GenerateFilterCode<T>(filters));
                var dummyAssembly = BuildAssembly(code);
                var matchClassType = dummyAssembly.GetType("DummyAssembly.DummyClass");
                methodInfo = matchClassType.GetMethods().FirstOrDefault(m => m.Name == "GetFilterPredicate");
                cashedMethodInfos.TryAdd(key, methodInfo);
            }

            return (Expression<Func<T, bool>>)methodInfo.Invoke(null, null);
        }


        static ConcurrentDictionary<string, MethodInfo> cashedMethodInfos = new ConcurrentDictionary<string, MethodInfo>();

        public static string GetCashedMethodInfosKey(MethodBase methodBase, Type type, Object obj1, Object obj2 = null)
        {

            var defaultSerializationSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };


            var serializedObj = JsonConvert.SerializeObject(obj1, Formatting.None, defaultSerializationSettings);
            var stringBuilder = new StringBuilder(256);
            stringBuilder.Append(methodBase.Name)
                .Append("|")
                .Append(type.Name)
                .Append("|")
                .Append(type.FullName)
                .Append("|")
                .Append(serializedObj);

            if (obj2 != null)
            {
                serializedObj = JsonConvert.SerializeObject(obj2, Formatting.None);
                stringBuilder.Append("|").Append(serializedObj);
            }

            return stringBuilder.ToString();
        }

        private static string GenerateDynamicDtoConvertionCode(DynamicDtoInfo dto, string varPath, string objectPath)
        {
            var varName = $"{varPath}_{dto.Name}";
            var code = $"dynamic {varName} = new System.Dynamic.ExpandoObject();";
            if (dto.Props == null) return code;
            foreach (var prop in dto.Props.Where(p => p.IsPrimitive))
            {
                code += $"{varName}.{prop.Name} = {objectPath} == null ? null : {objectPath}.{prop.Name};";
            }
            foreach (var prop in dto.Props.Where(p => p.IsComplex && !p.IsCollection))
            {
                var targetPath = $"{objectPath}.{prop.Name}";
                code += $@"
if ({targetPath} != null) 
{{   
    {GenerateDynamicDtoConvertionCode(prop, varName, targetPath)}
    {varName}.{prop.Name} = {varName}_{prop.Name};   
}}";
            }
            foreach (var prop in dto.Props.Where(p => p.IsCollection))
            {
                var collectionPath = $"{objectPath}.{prop.Name}";
                var itemName = $"{varName}_{prop.Name}_Item";
                var tempCollection = $"{varName}_{prop.Name}_Temp";
                code += $@"
var {tempCollection} = new List<System.Dynamic.ExpandoObject>();
if ({collectionPath} != null) 
{{
    foreach (var {itemName} in {collectionPath}) 
    {{
        {GenerateDynamicDtoConvertionCode(prop, varName, itemName)}
        {tempCollection}.Add({varName}_{prop.Name});
    }}    
}}
{varName}.{prop.Name} = {tempCollection};";
            }
            return code;
        }
        public static List<DynamicOrderBy<T>> BuildPredicateForOrdering<T>(List<OrderByInfo> orderBy)
        {
            if (orderBy == null || !orderBy.Any()) return new List<DynamicOrderBy<T>>();


            var key = GetCashedMethodInfosKey(MethodBase.GetCurrentMethod(), typeof(T), orderBy);

            MethodInfo methodInfo = null;

            if (cashedMethodInfos.ContainsKey(key))
            {
                methodInfo = cashedMethodInfos[key];
            }
            else
            {
                var code = WrapCodeIntoDummyAssembly(GenerateOrderByCode<T>(orderBy));
                var dummyAssembly = BuildAssembly(code);
                var matchClassType = dummyAssembly.GetType("DummyAssembly.DummyClass");
                methodInfo = matchClassType.GetMethods().FirstOrDefault(m => m.Name == "BuildOrderByList");
                cashedMethodInfos.TryAdd(key, methodInfo);
            }

            return (List<DynamicOrderBy<T>>)methodInfo.Invoke(null, null);
        }

        public static List<DynamicGroupBy<T>> BuildDynamicGroupBy<T>(List<GroupByInfo> groupBy)
        {
            if (groupBy == null || !groupBy.Any()) return new List<DynamicGroupBy<T>>();

            var key = GetCashedMethodInfosKey(MethodBase.GetCurrentMethod(), typeof(T), groupBy);

            MethodInfo methodInfo = null;
            if (cashedMethodInfos.ContainsKey(key))
            {
                methodInfo = cashedMethodInfos[key];
            }
            else
            {
                var code = WrapCodeIntoDummyAssembly(GenerateGroupByCode<T>(groupBy));
                var dummyAssembly = BuildAssembly(code);
                var matchClassType = dummyAssembly.GetType("DummyAssembly.DummyClass");
                methodInfo = matchClassType.GetMethods().FirstOrDefault(m => m.Name == "BuildGroupByList");
                cashedMethodInfos.TryAdd(key, methodInfo);
            }

            return (List<DynamicGroupBy<T>>)methodInfo.Invoke(null, null);
        }

        public static List<AggregatorInfo<T>> BuildAggregatorPredicates<T>(List<AggregatorInfo<T>> aggregators)
        {
            if (aggregators == null || !aggregators.Any()) return new List<AggregatorInfo<T>>();

            var key = GetCashedMethodInfosKey(MethodBase.GetCurrentMethod(), typeof(T), aggregators);

            MethodInfo methodInfo = null;
            if (cashedMethodInfos.ContainsKey(key))
            {
                methodInfo = cashedMethodInfos[key];
            }
            else
            {
                var code = WrapCodeIntoDummyAssembly(GenerateAggregatorsCode<T>(aggregators));
                var dummyAssembly = BuildAssembly(code);
                var matchClassType = dummyAssembly.GetType("DummyAssembly.DummyClass");
                methodInfo = matchClassType.GetMethods().FirstOrDefault(m => m.Name == "BuildAggregatorPredicates");
                cashedMethodInfos.TryAdd(key, methodInfo);
            }

            return (List<AggregatorInfo<T>>)methodInfo.Invoke(null, new object[] { aggregators });
        }

        public static DynamicClosedGroupsInfo<T> BuildClosedGroupsInfo<T>(List<GroupByInfo> groupBy, List<AggregatorInfo<T>> aggregators)
        {
            var key = GetCashedMethodInfosKey(MethodBase.GetCurrentMethod(), typeof(T), groupBy, aggregators);

            MethodInfo methodInfo = null;
            if (cashedMethodInfos.ContainsKey(key))
            {
                methodInfo = cashedMethodInfos[key];
            }
            else
            {
                var code = WrapCodeIntoDummyAssembly(GenerateClosedGroupsCode<T>(groupBy, aggregators));
                var dummyAssembly = BuildAssembly(code);
                var matchClassType = dummyAssembly.GetType("DummyAssembly.DummyClass");
                methodInfo = matchClassType.GetMethods().FirstOrDefault(m => m.Name == "BuildClosedGroupsInfo");
                cashedMethodInfos.TryAdd(key, methodInfo);
            }

            return (DynamicClosedGroupsInfo<T>)methodInfo.Invoke(null, null);
        }

        public static MethodInfo BuildDynamicDtoConvertionMethod<T>(DynamicDtoInfo info)
        {
            var key = GetCashedMethodInfosKey(MethodBase.GetCurrentMethod(), typeof(T), info);
            MethodInfo methodInfo = null;
            if (cashedMethodInfos.ContainsKey(key))
            {
                methodInfo = cashedMethodInfos[key];
            }
            else
            {
                var methodName = "BuildDynamicDto";
                var innerCode = GenerateDynamicDtoConvertionCode(info, "", "input");
                var type = typeof(T);
                var methodCode =
$@"public static System.Dynamic.ExpandoObject {methodName}({TypeName(type)} input)
{{
    if (input == null) return null;
    {innerCode}
    return _root;
}}";
                var code = WrapCodeIntoDummyAssembly(methodCode);
                var dummyAssembly = BuildAssembly(code, withCSharp: true);
                var matchClassType = dummyAssembly.GetType("DummyAssembly.DummyClass");
                methodInfo = matchClassType.GetMethods().FirstOrDefault(m => m.Name == methodName);
                cashedMethodInfos.TryAdd(key, methodInfo);
            }
            return methodInfo;
        }
        private static Assembly BuildAssembly(string code, bool withCSharp = false)
        {
#if NETFRAMEWORK
            var provider = new CSharpCodeProvider();
            var compilerparams = new CompilerParameters();
            compilerparams.GenerateExecutable = false;
            compilerparams.GenerateInMemory = true;
            compilerparams.ReferencedAssemblies.Add("System.Core.Dll");
            compilerparams.ReferencedAssemblies.Add("System.Dll");
#else
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var assemblyName = Path.GetRandomFileName();

            var runtimeDirectory = Directory.GetParent(typeof(object).GetTypeInfo().Assembly.Location).FullName;

            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "System.Dll")),
                MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "System.Private.CoreLib.Dll")),
                MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "System.Linq.Dll")),
                MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "System.Linq.Expressions.Dll")),
                MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "netstandard.dll")),
                MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "System.Core.Dll")),
                MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "System.Collections.Dll")),
                MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "System.Runtime.Dll"))
            };
#endif

            if (withCSharp)
            {
#if NETFRAMEWORK
                compilerparams.ReferencedAssemblies.Add("Microsoft.CSharp.Dll");
#else
                references.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "Microsoft.CSharp.Dll")));
#endif
            }
            foreach (var path in RequiredAssembliesPaths)
            {
#if NETFRAMEWORK
                compilerparams.ReferencedAssemblies.Add(path);
#else
                references.Add(MetadataReference.CreateFromFile(path));
#endif
            }

#if NETFRAMEWORK
            var results = provider.CompileAssemblyFromSource(compilerparams, code);

            if (results.Errors.HasErrors)
            {
                var errors = new StringBuilder("Compiler Errors :\r\n");
                foreach (CompilerError error in results.Errors)
                {
                    errors.AppendFormat("Line {0},{1}\t: {2}\n",
                        error.Line, error.Column, error.ErrorText);
                }

                throw new ApplicationException(errors.ToString());
            }
            else
            {
                return results.CompiledAssembly;
            }
#else
            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                // TODO: First time execution took a lot of time (1-2s)
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    var errors = new StringBuilder("Compiler Errors :\r\n");
                    var failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);
                    foreach (var error in failures)
                    {
                        errors.AppendFormat("Error {0}: {1}\n",
                            error.Id, error.GetMessage());
                    }

                    throw new ApplicationException(errors.ToString());
                }

                ms.Seek(0, SeekOrigin.Begin);
                return AssemblyLoadContext.Default.LoadFromStream(ms);
            }
#endif
        }

#if NETFRAMEWORK
#else
        public static void Configure(List<string> paths)
        {
            RequiredAssembliesPaths = new List<string>();

            var location = Assembly.GetEntryAssembly().Location;
            var directory = Path.GetDirectoryName(location);

            foreach (var assemblyRelativePath in paths)
            {
                RequiredAssembliesPaths.Add(Path.Combine(directory, assemblyRelativePath));
            }
        }
#endif
    }

    public class DynamicOrderBy<T>
    {
        public DynamicOrderBy(string col, Expression<Func<T, object>> exp, bool ascending)
        {
            Column = col;
            Expression = exp;
            Ascending = ascending;
        }

        public string Column;
        public bool Ascending;
        public Expression<Func<T, object>> Expression;
    }

    public class DynamicGroupBy<T>
    {
        public DynamicGroupBy(string col, Expression<Func<T, object>> exp, bool expanded)
        {
            Column = col;
            Expression = exp;
            Expanded = expanded;
        }

        public string Column;
        public bool Expanded;
        public Expression<Func<T, object>> Expression;
    }

    public class DynamicClosedGroupsInfo<T>
    {
        public Type GroupByAnonymousType;
        public Type SelectorsAnonymousType;
        public Expression<Func<T, object>> GroupByExpression;
        public Expression<Func<IGrouping<object, T>, object>> CombinedSelectorExpression;
    }
    public class DynamicDtoInfo
    {
        public string Name { get; set; }
        public bool IsCollection { get; set; }
        public bool IsComplex => Props != null;
        public bool IsPrimitive => !IsComplex && !IsCollection;
        public List<DynamicDtoInfo> Props { get; set; }
        public DynamicDtoInfo FindPropByName(string name, bool deep = false)
        {
            if (Props == null) return null;
            var fixedName = name.Replace("[]", "");
            foreach (var prop in Props)
            {
                if (prop.Name.Equals(fixedName)) return prop;
                if (!deep || prop.IsPrimitive) continue;
                var searched = prop.FindPropByName(fixedName);
                if (searched != null) return searched;
            }
            return null;
        }
        public DynamicDtoInfo CreateProperty(string name)
        {
            if (Props == null)
            {
                Props = new List<DynamicDtoInfo>();
            }
            else
            {
                var existing = FindPropByName(name);
                if (existing != null)
                {
                    return existing;
                }
            }
            var isCollection = name.Contains("[]");
            var fixedName = name.Replace("[]", "");
            var newProp = new DynamicDtoInfo { Name = fixedName, IsCollection = isCollection };
            Props.Add(newProp);
            return newProp;
        }
        public static DynamicDtoInfo CreateFromPropsArray(List<string> propsList, string rootName = "root")
        {
            return CreateFromPropsArray(propsList?.ToArray(), rootName);
        }
        public static DynamicDtoInfo CreateFromPropsArray(string[] propsArray, string rootName = "root")
        {
            try
            {
                var root = new DynamicDtoInfo
                {
                    Name = rootName
                };
                if (propsArray == null || !propsArray.Any()) return root;
                foreach (var entry in propsArray.OrderBy(x => x.Count(c => c == '.')))
                {
                    var parts = entry.Split('.');
                    var owner = root;
                    foreach (var part in parts)
                    {
                        var existing = owner.FindPropByName(part);
                        if (existing == null)
                        {
                            owner = owner.CreateProperty(part);
                        }
                        else
                        {
                            owner = existing;
                        }
                    }
                }
                return root;
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof(DynamicDtoInfo)).Error("Error creating Dynamic DTO structure", e);
                throw;
            }
        }
        public Func<T, object> GetConvertionFunc<T>() where T : class
        {
            MethodInfo convertionMethod = null;
            try
            {
                convertionMethod = RuntimePredicateBuilder.BuildDynamicDtoConvertionMethod<T>(this);
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof(DynamicDtoInfo)).Error("Error generating Dynamic DTO convertion method", e);
                throw;
            }
            return (item) =>
            {
                try
                {
                    return convertionMethod.Invoke(null, new[] { item });
                }
                catch (Exception e)
                {
                    log4net.LogManager.GetLogger(typeof(DynamicDtoInfo)).Error("Error converting item to Dynamic DTO", e);
                    return null;
                }
            };
        }
    }
}