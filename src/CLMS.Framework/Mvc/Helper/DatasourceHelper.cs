using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
#if NETFRAMEWORK
using System.Runtime.Caching;
#endif
using System.Runtime.Serialization;

namespace CLMS.Framework.Mvc
{
    #if NETFRAMEWORK
    public class DatasourceRetriever
    {
        private static MemoryCache Cache = MemoryCache.Default;
        
        private static void AddToCache(DatasourceRequest request, object filtered)
        {
            var policy = new CacheItemPolicy();            
            policy.SlidingExpiration.Add(TimeSpan.FromSeconds(30));

            var cacheItem = new CacheItem(request.GetHashCode().ToString(), filtered);
            Cache.Add(cacheItem, policy);
        }

        private static object GetFromCache(DatasourceRequest request)
        {
            return Cache.Get(request.GetHashCode().ToString());
        }

        public static IQueryable<T> ApplyDynamicFilterToQueryable<T>(DatasourceRequest request, IQueryable<T> filteredWithPredefinedFilters)
        {
            // Try getting filtered items from cache
            var filtered = GetFromCache(request) as IQueryable<T>;

            // Cache hit!
            if (filtered != null) return filtered;

            // If not found in cache, use the Runtime Predicate Builder
            // to dynamically get the predicates for filter and ordering            
            Expression<Func<T, bool>> dynamicFilter = null;            
            dynamicFilter = RuntimePredicateBuilder.BuildPredicateForFiltering<T>(request.Filters);                
            filtered = filteredWithPredefinedFilters.Where(dynamicFilter);

            // Add to cache
            AddToCache(request, filtered);

            return filtered;
        }

        public static IQueryable<T> Retrieve<T>(DatasourceRequest request, IQueryable<T> filteredWithPredefinedFilters)
        {
            // Apply Dynamic Filter
            var filtered = ApplyDynamicFilterToQueryable(request, filteredWithPredefinedFilters);
            
            // Apply Dynamic Order By
            var dynamicOrderBy = RuntimePredicateBuilder.BuildPredicateForOrdering<T>(request.OrderBy);
           
            var ordered = filtered;

            foreach (var entry in dynamicOrderBy)
            {
                ordered = entry.Ascending == true
                            ? ordered.OrderBy(entry.Expression)
                            : ordered.OrderByDescending(entry.Expression);
            }

            return ordered.Skip(request.StartRow).Take(request.PageSize);
        }

        public static  int GetTotalGroups<T>(GroupTree<T> group)
        {
            var totalGroups = group.SubGroups.Count;
            foreach (var subGroup in @group.SubGroups)
            {
                totalGroups += GetTotalGroups(subGroup);
            }

            return totalGroups;
        }

        public static GroupTree<T> RetrieveGrouped<T>(DatasourceRequest request, IQueryable<T> filteredWithPredefinedFilters, Func<T, object> uniqueKeyExpression, Newtonsoft.Json.Linq.JObject postedData, bool forceClosedGroups = false)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            };

            var aggregators = postedData["aggregatorsRequest"] == null
                    ? new List<AggregatorInfo<T>>()
                    : Utilities.Deserialize<List<AggregatorInfo<T>>>(postedData["aggregatorsRequest"].ToString(), serializerSettings);

            return RetrieveGrouped<T>(request, filteredWithPredefinedFilters, uniqueKeyExpression, aggregators, forceClosedGroups);
        }

        public static GroupTree<T> RetrieveGrouped<T>(DatasourceRequest request, IQueryable<T> filteredWithPredefinedFilters, Func<T, object> uniqueKeyExpression, List<AggregatorInfo<T>> aggregators, bool forceClosedGroups = false)
        {
            // Apply Dynamic Filter
            var filtered = ApplyDynamicFilterToQueryable(request, filteredWithPredefinedFilters);
           
            // Groups only (Closed)
            if (forceClosedGroups || request.GroupBy.First().GetGroupsClosed)
            {                                                
                var closedGroupsInfo = RuntimePredicateBuilder.BuildClosedGroupsInfo<T>(request.GroupBy, aggregators);
                return GroupsHelper.GetAllGroupsClosed<T>(filtered, closedGroupsInfo, new Dictionary<string, string>());                
            }
            // Groups with data
            else
            {
                // Variables initialization
                var dynamicOrderBy = RuntimePredicateBuilder.BuildPredicateForOrdering<T>(request.OrderBy);
                var dynamicGroupBy = RuntimePredicateBuilder.BuildDynamicGroupBy<T>(request.GroupBy);

                return GroupsHelper.ParseGroups<T>(filtered, dynamicOrderBy, dynamicGroupBy, uniqueKeyExpression, request.StartRow, request.PageSize);
            }            
        }
    }
#endif
    public class DatasourceRequest
	{
		public DatasourceRequest(int startRow = 0, int pageSize = 9999, List<FilterInfo> filters = null, List<OrderByInfo> orderBy = null, List<object> excludeKeys = null, int[] indexes = null, List<GroupByInfo> grouping = null)
		{            
			StartRow = startRow;
			PageSize = pageSize;
            Indexes = indexes;
			Filters = filters == null ? new List<FilterInfo>() : filters;
			OrderBy = orderBy == null ? new List<OrderByInfo>() : orderBy;
            GroupBy = grouping == null ? new List<GroupByInfo>() : grouping;
            ExcludeKeys = excludeKeys == null ? new List<object>() : excludeKeys;
        }

		public int StartRow;
		public int PageSize;
		public int[] Indexes;
		public List<FilterInfo> Filters;
		public List<OrderByInfo> OrderBy;
        public List<GroupByInfo> GroupBy;
        public List<object> ExcludeKeys;		
        public List<string> DtoProperties;
	}

	public class DatasourceResponse
	{
		public object Data;
        public object Groups;
		public int TotalRows;		
	}

    public class ListResponse : DatasourceResponse
    {
        public List<ListRuleEvaluation> RuleEvaluations;
    }

    public class ColumnInfo
	{
		public ColumnInfo(string name, string mambaDT, bool isEncrypted = false)
		{
			Name = name;
			MambaDataType = mambaDT;            
			IsEncrypted = isEncrypted;
		}
       
        public string Name;        
        public string MambaDataType;
        public ValueFormat Formatting;  
		public bool IsEncrypted;    		
    }

    public class ListColumnInfo : ColumnInfo
    {
        public ListColumnInfo(string name, string mambaDT, string style, string cssClasses,
                            bool searchable, bool importable, bool groupable, bool orderabale, bool aggregators, bool editable,
                            ColumnItemType type, int? length = null, bool isEncrypted = false) : base(name, mambaDT, isEncrypted)
        {            
            Style = style;
            CssClasses = cssClasses;
            Searchable = searchable;
            Groupable = groupable;
            Orderable = orderabale;
            Editable = editable;
            ItemType = type;
            SupportsAggregators = aggregators;
            Length = length;
        }
                
        public string Style;
        public string CssClasses;
        public bool Searchable;
        public bool Groupable;
        public bool Orderable;
        public bool Importable;
        public bool Editable;
        public bool SupportsAggregators;
        public int? Length;
		
		[JsonConverter(typeof(StringEnumConverter))]
        public ColumnItemType ItemType;
    }
	
    public class ValueFormat
    {        
        public int? Decimals { get; set; }
        public bool? Groups { get; set; }
        public bool? Signed { get; set; }
        public bool? ShowOnlyDecimalPart { get; set; }
        public string Prefix { get; set; }
        public string Postfix { get; set; }
        public string DateTimeFormat { get; set; }
        public string BackEndFormatting { get; set; }
    }

	public class FilterInfo
	{
		public FilterInfo(ColumnInfo col, object val, RowOperator rowOp, FilterOperator op)
		{
			Column = col;
			Value = val;        
			RowOperator = rowOp;
			Operator = op;
		}

		public ColumnInfo Column;
		public object Value;
        public object SecondValue;

        [JsonConverter(typeof(StringEnumConverter))]
        public RowOperator RowOperator;

        [JsonConverter(typeof(StringEnumConverter))]
        public FilterOperator Operator;
	}

	public class OrderByInfo
	{
        public OrderByInfo(ColumnInfo col, OrderByDirection dir = OrderByDirection.ASC)
		{
			Column = col;
            Direction = dir;        
		}

		public ColumnInfo Column;

        [JsonConverter(typeof(StringEnumConverter))]
        public OrderByDirection Direction;
	}

    public class GroupByInfo
    {
        public GroupByInfo(ColumnInfo col, GroupState state)
        {
            Column = col;
            State = state;
        }

        public ColumnInfo Column;
        public bool Inactive;
        public bool GetGroupsClosed;

        [JsonConverter(typeof(StringEnumConverter))]
        public GroupState State;
    }

    public class AggregatorInfo<T>
    {
        public AggregatorInfo()
        {

        }

        public AggregatorInfo(string col, AggregatorType type)
        {
            Column = col;
            Type = type;
        }
        
        public AggregatorInfo(string col, AggregatorType type, double value, string formatting = null)
        {
            Column = col;
            Value = value;
            Formatting = formatting;
            Type = type;

            FormatValue();
        }

        public AggregatorInfo(string col, string type, double value, string formatting = null) 
            : this(col, (AggregatorType)Enum.Parse(typeof(AggregatorType), type, true), value, formatting)
        {
            
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public AggregatorType Type;
		public string Column;        
        public double Value;
        public string ValueFormatted;

        [JsonIgnore]
        public string Formatting;

        [JsonIgnore]
        public Func<T, double> Expression;

        public void Calculate(IQueryable<T> queryable, string formattingOverride = null)
        {            
            try
            {
                switch (Type)
                {
                    case AggregatorType.SUM:
                        Value = queryable.Sum(this.Expression);
                        break;
                    case AggregatorType.AVERAGE:
                        Value = queryable.Average(this.Expression);
                        break;
                    case AggregatorType.COUNT:
                        Value = queryable.Count();
                        break;
                }
            }
            catch (Exception e)
            {
                Value = 0;
                log4net.LogManager.GetLogger(GetType()).Error($"Could not calculate aggregator Column: '{Column}', Type: '{Type}'", e);
            }

            FormatValue(formattingOverride);
        }

        public void FormatValue(string formattingOverride = null)
        {
            var formattingToUse = formattingOverride ?? Formatting;

            ValueFormatted = Type == AggregatorType.COUNT || string.IsNullOrEmpty(formattingToUse)
                              ? Value.ToString()
                              : Value.ToString(formattingToUse);
        }

        public void Calculate(List<T> items, string formatting)
        {
            try
            {
                switch (Type)
                {
                    case AggregatorType.SUM:
                        Value = items.Sum(this.Expression);
                        break;
                    case AggregatorType.AVERAGE:
                        Value = items.Average(this.Expression);
                        break;
                    case AggregatorType.COUNT:
                        Value = items.Count();
                        break;
                }
            }
            catch
            {
                Value = 0;
            }

            ValueFormatted = Type == AggregatorType.COUNT
                                ? Value.ToString()
                                : Value.ToString(formatting);
        }
    }

    public class ListRuleEvaluation : RuleEvaluation
    {
        public object Key;
        public string RuleName;
        public List<string> ColumnNames = new List<string>(); 
        public bool ApplyToRow;
        public bool ApplyToColumn
        {
            get
            {
                return ColumnNames.Any();
            }
        }
    }

    public class ManualValuesData
    {
        public ManualValuesData(string key, string formName, string controlName, bool isPickList)
        {
            _key = key;
            Value = BaseViewPageBase<object>.GetResourceValue(formName, $"RES_{(isPickList ? "PICKLIST" : "LIST")}_{controlName}_VALUE_{key}");
        }

        public string _key { get; }
        public string Value { get; }
    }

	public class ListImportResult
	{
		public int NumberImported;
		public int NumberFailled;
		public List<ImportError> Errors = new List<ImportError>();

		public class ImportError
		{
			public int RowNumber;
			public string ErrorMessage;
			public string ErrorDescription;
		}
	}

    public enum ColumnItemType
    {
        HYPERLINK = 0,
        DOWNLOADLINK,
        CHECKBOX,
        IMAGEBOX,
        TEXTBOX
    }

	public enum FilterOperator
	{     
		NONE = 0,
        EQUAL_TO,
		NOT_EQUAL_TO,
		LESS_THAN,
		GREATER_THAN,
		LESS_THAN_OR_EQUAL_TO,
		GREATER_THAN_OR_EQUAL_TO,
		LIKE,
		RANGE,
		HAS_VALUE,
		HAS_NO_VALUE		
	}

	public enum RowOperator
	{
		NONE = 0,
        AND,
		OR
	}

    public enum OrderByDirection
    {
        ASC = 0,
        DESC
    }

    public enum AggregatorType
    {
        [EnumMember(Value = "0")]
        SUM,
        [EnumMember(Value = "1")]
        AVERAGE,
        [EnumMember(Value = "2")]
        COUNT
    }

    public enum GroupState
    {
        EXPANDED,
        COLLAPSED
    }

}
