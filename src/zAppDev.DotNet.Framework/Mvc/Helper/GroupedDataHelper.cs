using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace zAppDev.DotNet.Framework.Mvc
{
    public class GroupsHelper
    {
        // WARNING! These must be in sync with List Control script (clms.list.control.js) corresponding vars
        public static string RootGroupString = "ROOT";
        public static string NullString = "(empty)";
        public static string GroupPathClientSeperator = " "; // Group path is used in jQuery "~ type" selectors requiring " ". Do not change! 
        public static string GroupPathSeperator = "___"; // Group path is used in jQuery "~ type" selectors requiring " ". Do not change! 
        public static string GroupValueSeperator = "/---/";
        public static string GroupValueSpace = "/___/";

        public static string ObjectToString(object obj)
        {
            if (obj == null) return NullString;

            try
            {
                Type type = obj.GetType();

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = Nullable.GetUnderlyingType(type);
                }

                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return ((int)obj).ToString();

                    case TypeCode.Decimal:
                        return ((decimal)obj).ToString("G29").Replace(",", "."); // Ommit last zeros and force dot seperator                          
                    case TypeCode.Double:
                        return ((double)obj).ToString("G29").Replace(",", ".");  // Ommit last zeros and force dot seperator                                                 
                    case TypeCode.Single:
                        return ((Single)obj).ToString("G29").Replace(",", ".");  // Ommit last zeros and force dot seperator                         
                    case TypeCode.DateTime:
                        return ((DateTime)obj).ToString();

                    default:
                        return obj.ToString();
                }
            }
            catch
            {
                return obj.ToString();
            }
        }

        public static GroupTree<T> ParseGroups<T>(IQueryable<T> filtered, List<DynamicOrderBy<T>> orderBy, List<DynamicGroupBy<T>> groupBy, Func<T, object> uniqueKeyExpression, int startRow, int pageSize)
        {
            #region Retrieve data ordered by Groups (initially)

            // Ordering for Grouping
            foreach (var group in groupBy)
            {
                var columnInOrderBy = orderBy.FirstOrDefault(o => o.Column == group.Column);

                var ascending = columnInOrderBy == null
                        ? true
                        : columnInOrderBy.Ascending;

                filtered = ascending ? filtered.OrderBy(group.Expression) : filtered.OrderByDescending(group.Expression);
            }

            // Ordering for Sorting
            foreach (var order in orderBy)
            {
                if (groupBy.Any(g => g.Column == order.Column)) continue; // Already Sorted

                filtered = order.Ascending
                                ? filtered.OrderBy(order.Expression)
                                : filtered.OrderByDescending(order.Expression);
            }

            var paged = filtered.Skip(startRow).Take(pageSize).ToList();
            #endregion

            return ParseGroups(paged, groupBy, uniqueKeyExpression);
        }

        public static GroupTree<T> ParseGroups<T>(IList<T> paged, List<DynamicGroupBy<T>> groupBy, Func<T, object> uniqueKeyExpression)
        {
            var firstLevelGroups = paged.GroupBy(groupBy[0].Expression.Compile());

            var groupsRoot = new GroupTree<T>(RootGroupString, GroupState.EXPANDED);

            foreach (var group in firstLevelGroups)
            {
                var groupItems = group.ToList();
                var subGroup = new GroupTree<T>(group.Key,
                                                groupBy[0].Column,
                                                groupBy[0].Expanded ? GroupState.EXPANDED : GroupState.COLLAPSED);

                if (groupBy.Count() > 1)
                {
                    var nestedGroups = ParseNestedGroups(subGroup, groupItems, groupBy, 1, uniqueKeyExpression);
                    groupsRoot.SubGroups.Add(nestedGroups);
                }
                else
                {
                    subGroup.Parent = groupsRoot;
                    subGroup.Items.AddRange(groupItems);
                    subGroup.UniqueItemKeys = subGroup.Items.Select(uniqueKeyExpression).ToList();
                    groupsRoot.SubGroups.Add(subGroup);
                }
            }

            return groupsRoot;
        }

        public static GroupTree<T> ParseNestedGroups<T>(GroupTree<T> root, List<T> items, List<DynamicGroupBy<T>> groupBy, int counter, Func<T, object> uniqueKeyExpression)
        {
            if (counter == groupBy.Count())
            {
                root.Items.AddRange(items);
                root.UniqueItemKeys = root.Items.Select(uniqueKeyExpression).ToList();
                return root;
            }

            var groups = items.GroupBy(groupBy[counter].Expression.Compile());

            foreach (var group in groups)
            {
                var groupItems = group.ToList();
                var nextCounter = counter + 1;
                var subGroup = new GroupTree<T>(group.Key,
                                                groupBy[counter].Column,
                                                groupBy[counter].Expanded ? GroupState.EXPANDED : GroupState.COLLAPSED);
                subGroup.Parent = root;

                root.SubGroups.Add(ParseNestedGroups(subGroup, groupItems, groupBy, nextCounter, uniqueKeyExpression));
            }

            return root;
        }

        public static GroupTree<T> GetAllGroupsClosed<T>(IQueryable<T> filtered, DynamicClosedGroupsInfo<T> closedGroupsInfo, Dictionary<string, string> ColumnsToFormattings, bool fetchAllBeforeGrouping = false)
        {
            var groupByAnonType = closedGroupsInfo.GroupByAnonymousType;
            var groupByExpression = closedGroupsInfo.GroupByExpression;
            var selectorsAnonType = closedGroupsInfo.SelectorsAnonymousType;
            var combinedSelector = closedGroupsInfo.CombinedSelectorExpression;

            var groupedData = fetchAllBeforeGrouping ? filtered.ToList().AsQueryable().GroupBy(groupByExpression).Select(combinedSelector)
                : filtered.GroupBy(groupByExpression).Select(combinedSelector);

            var groupedFields = groupByAnonType.GetFields();
            var selectedFields = selectorsAnonType.GetFields();

            var leafGroups = new List<LeafGroup>();

            // At this point we have the "Leaf" Groups data.
            // We need to recreate full tree hierarchy, using this data.
            // We transform this data to LeafGroup classes which are have more human readable properties.            
            foreach (var group in groupedData)
            {
                // Dictionary used for storing the group column path (e.g. Name -> Age -> City)
                // and the group values path (e.g. Jim -> 40 -> Athens)
                var groupDic = new Dictionary<string, object>();

                var aggregatorsList = new List<AggregatorInfo<object>>();

                // Get Group Values
                foreach (var fieldInfo in selectedFields)
                {
                    if (fieldInfo.FieldType == groupByAnonType) // Group By Columns
                    {
                        foreach (var grpFieldInfo in groupedFields)
                        {
                            var column = grpFieldInfo.Name;
                            var value = grpFieldInfo.GetValue(fieldInfo.GetValue(group));

                            groupDic.Add(column, value == null ? NullString : value);
                        }
                    }
                    else // Aggregators etc
                    {
                        var parts = fieldInfo.Name.Split(new string[] { GroupPathSeperator }, StringSplitOptions.RemoveEmptyEntries);
                        var aggregatorType = parts[0];
                        var column = parts[1];
                        double value = 0;
                        double.TryParse(fieldInfo.GetValue(group).ToString(), out value);

                        aggregatorsList.Add(new AggregatorInfo<object>(column, aggregatorType, value));
                    }
                }

                leafGroups.Add(new LeafGroup(groupDic, aggregatorsList));
            }

            // Recreate Structure
            var groupsStructure = RecreateHierarchicalGroupStructure<T>(leafGroups, ColumnsToFormattings);

            return groupsStructure;
        }

        private static GroupTree<T> RecreateHierarchicalGroupStructure<T>(List<LeafGroup> data, Dictionary<string, string> ColumnsToFormattings)
        {
            // Our root
            var groupsRoot = new GroupTree<T>(RootGroupString, GroupState.EXPANDED);

            // Maximum Nesting Level
            var maxLevel = data.Count > 0 ? data[0].Level : 0;

            // Start from level one and create tree level by level
            for (var level = 1; level <= maxLevel; level++)
            {
                // Find the distinct groups for this level
                var thisGroups = data.GroupBy(d => d.PathAtLevel(level));

                // Iterate through these Level Groups
                foreach (var group in thisGroups.ToList())
                {
                    // Get the first Leaf Group in each Level Group
                    // This leaf group always exists and is enough for this task
                    var first = group.ToList()[0];

                    // Groupped by Column
                    var column = first.ColumnsToKeys.ElementAt(level - 1).Key;

                    // Column Value
                    var value = first.ColumnsToKeys.ElementAt(level - 1).Value;

                    // Column Formatting
                    var formatting = ColumnsToFormattings.ContainsKey(column)
                                        ? ColumnsToFormattings[column]
                                        : null;

                    // Group's Initial State
                    // Last Level Groups must be Collapsed as there are no records
                    var state = level < maxLevel && maxLevel > 1
                            ? GroupState.EXPANDED
                            : GroupState.COLLAPSED;

                    // Create the actual Group
                    var groupTree = new GroupTree<T>(value, column, state);

                    // For the last level Groups, include the aggregators we have retrieved
                    if (level == maxLevel)
                    {
                        var aggList = new List<AggregatorInfo<T>>();

                        foreach (var agg in first.Aggregators)
                        {
                            aggList.Add(new AggregatorInfo<T>(agg.Column, agg.Type, agg.Value, formatting));
                        }

                        groupTree.Aggregates = aggList;
                    }

                    // We now need to find were this Group belongs in the tree
                    // If we are at first level, the Group belongs to the tree's root
                    // For higher levels, we search for the parent Group
                    var owner = level > 1
                        ? groupsRoot.FindSubGroup(first.PathAtLevel(level - 1))
                        : groupsRoot;

                    // Add Group to Owner
                    groupTree.Parent = owner;
                    owner.SubGroups.Add(groupTree);
                }
            }

            // Calculate Aggregates and Count for all Groups based on the last level Groups
            groupsRoot.AggregateSubGroupAggregators();

            // Special handle for the Average Aggregates which require Group Count to be available
            groupsRoot.AggregateSubGroupAggregators(true);

            return groupsRoot;
        }

        public static void FormatGroupedAggregators<T>(GroupTree<T> group, Dictionary<string, string> formattings)
        {
            foreach (var aggregatorInfo in group.Aggregates)
            {
                var formatting = formattings.ContainsKey(aggregatorInfo.Column) ? formattings[aggregatorInfo.Column] : null;
                aggregatorInfo.FormatValue(formatting);
                foreach (var subGroup in group.SubGroups)
                {
                    FormatGroupedAggregators(subGroup, formattings);
                }
            }
        }
    }

    public class LeafGroup
    {
        public LeafGroup(Dictionary<string, object> columnsToKeys, List<AggregatorInfo<object>> aggregators = null)
        {
            ColumnsToKeys = columnsToKeys;
            Aggregators = aggregators == null ? new List<AggregatorInfo<object>>() : aggregators;
        }

        public Dictionary<string, object> ColumnsToKeys;

        public string PathAtLevel(int level)
        {
            var path = "";

            for (var i = 0; i < level; i++)
            {
                if (ColumnsToKeys.Count < i + 1) return path;

                var entry = ColumnsToKeys.ElementAt(i);

                if (!string.IsNullOrEmpty(path)) path += GroupsHelper.GroupPathSeperator;

                path += entry.Key + GroupsHelper.GroupValueSeperator + GroupsHelper.ObjectToString(entry.Value).Replace(" ", GroupsHelper.GroupValueSpace);
            }

            return path;
        }

        public string ParentPath
        {
            get
            {
                return PathAtLevel(Level - 1);
            }
        }

        public string Path
        {
            get
            {
                return PathAtLevel(Level);
            }
        }

        public int Level
        {
            get
            {
                return ColumnsToKeys.Count;
            }

        }

        public List<AggregatorInfo<object>> Aggregators;
    }

    public class GroupTree<T>
    {
        public GroupTree()
        {
            Items = new List<T>();
            SubGroups = new List<GroupTree<T>>();
        }

        public GroupTree(object key, ColumnInfo column, GroupState state, List<T> items = null)
        {
            Items = items == null ? new List<T>() : items;
            SubGroups = new List<GroupTree<T>>();
            Column = column;
            Key = key;
            Column = column;
            Aggregates = new List<AggregatorInfo<T>>();
            State = state;
        }

        public GroupTree(object key, string columnName, GroupState state, List<T> items = null)
        : this(key, new ColumnInfo(columnName, "object"), state, items)
        {
            // shortcut constructor        
        }

        public GroupTree(object key, GroupState state)
        : this(key, key.ToString(), state)
        {
            // shortcut constructor        
        }

        public List<T> GetAllItems()
        {
            if (this.SubGroups == null || !this.SubGroups.Any()) return this.Items;

            var items = new List<T>();

            foreach (var subGroup in this.SubGroups)
            {
                items.AddRange(subGroup.GetAllItems());
            }

            return items;
        }

        public GroupTree<T> FindSubGroup(string path)
        {
            var groupAtThisDepth = this.SubGroups.FirstOrDefault(s => s.Identifier == path);

            if (groupAtThisDepth != null) return groupAtThisDepth;

            foreach (var subGroup in this.SubGroups)
            {
                var subGroupCheck = subGroup.FindSubGroup(path);

                if (subGroupCheck != null) return subGroupCheck;
            }

            return null;
        }

        public void AggregateSubGroupAggregators(bool forAvg = false)
        {
            foreach (var s in SubGroups)
            {
                s.AggregateSubGroupAggregators(forAvg);
                s.FillParentAggregators(forAvg);
            }
        }

        public void FillParentAggregators(bool forAvg = false)
        {
            if (Parent == null) return;

            foreach (var aggregate in Aggregates.Where(agg => forAvg && agg.Type == AggregatorType.AVERAGE || !forAvg && agg.Type != AggregatorType.AVERAGE))
            {
                var aggregatorOfParent =
                    Parent.Aggregates.FirstOrDefault(a => a.Type == aggregate.Type && a.Column == aggregate.Column);

                var valueToAdd = forAvg
                                ? aggregate.Value * (GetCount() / Parent.GetCount()) // for Average the child aggregator value is weighted 
                                : aggregate.Value; // for other aggregators it is just added

                if (aggregatorOfParent != null)
                {
                    aggregatorOfParent.Value += valueToAdd;
                    aggregatorOfParent.FormatValue();
                }
                else
                {
                    Parent.Aggregates.Add(
                        new AggregatorInfo<T>(aggregate.Column, aggregate.Type, valueToAdd, aggregate.Formatting));
                }
            }
        }

        public string KeyFormatted
        {
            get
            {
                if (Key == null) return GroupsHelper.NullString;

                if (string.IsNullOrEmpty(Column.Formatting?.BackEndFormatting)) return Key.ToString();

                try
                {
                    Type type = Key.GetType();

                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        type = Nullable.GetUnderlyingType(type);
                    }

                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return ((int)Key).ToString(Column.Formatting.BackEndFormatting);

                        case TypeCode.Decimal:
                            return ((decimal)Key).ToString(Column.Formatting.BackEndFormatting);

                        case TypeCode.Double:
                            return ((double)Key).ToString(Column.Formatting.BackEndFormatting);

                        case TypeCode.Single:
                            return ((Single)Key).ToString(Column.Formatting.BackEndFormatting);

                        case TypeCode.DateTime:
                            return ((DateTime)Key).ToString(Column.Formatting.BackEndFormatting);

                        default:
                            return Key.ToString();
                    }
                }
                catch
                {
                    return Key.ToString();
                }
            }
        }

        public double GetCount()
        {
            if (!Aggregates.Any(agg => agg.Type == AggregatorType.COUNT)) throw new ApplicationException("Cannot get Count for Group");

            return Aggregates.FirstOrDefault(agg => agg.Type == AggregatorType.COUNT).Value;
        }

        public string Identifier
        {
            get
            {
                if (Key?.Equals(GroupsHelper.RootGroupString) == true) return "";

                var thisPartOfPath = Column.Name + GroupsHelper.GroupValueSeperator + GroupsHelper.ObjectToString(Key).Replace(" ", GroupsHelper.GroupValueSpace);

                var parentPath = Parent == null ? "" : Parent.Identifier;

                return string.IsNullOrEmpty(parentPath)
                        ? thisPartOfPath
                        : Parent.Identifier + GroupsHelper.GroupPathSeperator + thisPartOfPath;
            }
        }

        [JsonIgnore]
        public List<T> Items;

        [JsonIgnore]
        public GroupTree<T> Parent;

        public List<AggregatorInfo<T>> Aggregates;

        public List<object> UniqueItemKeys;
        public List<GroupTree<T>> SubGroups;
        public object Key;
        public ColumnInfo Column;
        public GroupState State;
    }

    public class GroupInfo<T>
    {
        public GroupInfo(string identifier, List<FilterInfo> filters, List<AggregatorInfo<T>> aggregators = null, Expression<Func<T, bool>> groupPredicate = null)
        {
            Identifier = identifier;
            Filters = filters;
            Aggregators = aggregators;
            GroupPredicate = groupPredicate;
        }

        public string Identifier;

        [JsonIgnore]
        public List<FilterInfo> Filters;

        [JsonIgnore]
        public Expression<Func<T, bool>> GroupPredicate;

        public List<AggregatorInfo<T>> Aggregators;

        public void CalculateAggregators(IQueryable<T> filteredQuery)
        {
            foreach (var agg in Aggregators)
            {
                agg.Calculate(filteredQuery.Where(GroupPredicate));
            }
        }
    }
}