#if NETFRAMEWORK
using CLMS.Framework.Tools.PerformanceMeasurements.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CLMS.Framework.Tools.PerformanceMeasurements.Contracts
{
    public class NHSessionStatistics: IPerformanceStatistic<SessionConfiguration>
    {
        public long? EntityDeleteCount { get; set; }
                   
        public long? EntityInsertCount { get; set; }
                   
        public long? EntityLoadCount { get; set; }
                   
        public long? EntityFetchCount { get; set; }
                   
        public long? EntityUpdateCount { get; set; }
                   
        public long? QueryExecutionCount { get; set; }

        public TimeSpan? QueryExecutionMaxTime { get; set; }

        public string QueryExecutionMaxTimeQueryString { get; set; }

        public long? QueryCacheHitCount { get; set; }

        public long? QueryCacheMissCount { get; set; }

        public long? QueryCachePutCount { get; set; }

        public long? FlushCount { get; set; }

        public long? ConnectCount { get; set; }

        public long? SecondLevelCacheHitCount { get; set; }

        public long? SecondLevelCacheMissCount { get; set; }

        public long? SecondLevelCachePutCount { get; set; }

        public long? SessionCloseCount { get; set; }

        public long? SessionOpenCount { get; set; }

        public long? CollectionLoadCount { get; set; }

        public long? CollectionFetchCount { get; set; }

        public long? CollectionUpdateCount { get; set; }

        public long? CollectionRemoveCount { get; set; }

        public long? CollectionRecreateCount { get; set; }

        public DateTime? StartTime { get; set; }

        public bool? IsStatisticsEnabled { get; set; }

        public string[] Queries { get; set; }

        public string[] EntityNames { get; set; }

        public string[] CollectionRoleNames { get; set; }

        public string[] SecondLevelCacheRegionNames { get; set; }

        public long? SuccessfulTransactionCount { get; set; }

        public long? TransactionCount { get; set; }

        public long? PrepareStatementCount { get; set; }

        public long? CloseStatementCount { get; set; }

        public long? OptimisticFailureCount { get; set; }

        public TimeSpan? OperationThreshold { get; set; }

        private Object GetValue(SessionConfiguration configuration, Object value, string name, List<string> monitoredStatistics, bool filterStats)
        {
            if(value is long?)
            {
                if ((filterStats && !monitoredStatistics.Contains(name)) || (configuration.IgnoreNulls && (value as long?).GetValueOrDefault(0) == 0))
                {
                    return null;
                }
            }

            if(value is TimeSpan?)
            {
                if ((filterStats && !monitoredStatistics.Contains(name)) || (configuration.IgnoreNulls && (value as TimeSpan?) == null))
                {
                    return null;
                }
            }

            if(value is string)
            {
                if ((filterStats && !monitoredStatistics.Contains(name)) || (configuration.IgnoreNulls && string.IsNullOrWhiteSpace(value as string)))
                {
                    return null;
                }
            }

            if (value is DateTime?)
            {
                if ((filterStats && !monitoredStatistics.Contains(name)) || (configuration.IgnoreNulls && (value as DateTime?) == null))
                {
                    return null;
                }
            }

            if (value is string[])
            {
                if ((filterStats && !monitoredStatistics.Contains(name)) || (configuration.IgnoreNulls && (value == null || (value as string[])?.Any() != true)))
                {
                    return null;
                }
            }

            return value;
        }

        public bool IsInteresting(SessionConfiguration configuration = null)
        {
            if (configuration == null) return true;
            if (!configuration.Enabled) return false;

            var monitoredStatistics = configuration.MonitoredStatistics;
            var filterStats = (monitoredStatistics?.Count).GetValueOrDefault(0) > 0;

            EntityDeleteCount = GetValue(configuration, EntityDeleteCount, "EntityDeleteCount", monitoredStatistics, filterStats) as long?;
            EntityInsertCount = GetValue(configuration, EntityInsertCount, "EntityInsertCount", monitoredStatistics, filterStats) as long?;
            EntityLoadCount = GetValue(configuration, EntityLoadCount, "EntityLoadCount", monitoredStatistics, filterStats) as long?;
            EntityFetchCount = GetValue(configuration, EntityFetchCount, "EntityFetchCount", monitoredStatistics, filterStats) as long?;
            EntityUpdateCount = GetValue(configuration, EntityUpdateCount, "EntityUpdateCount", monitoredStatistics, filterStats) as long?;
            QueryExecutionCount = GetValue(configuration, QueryExecutionCount, "QueryExecutionCount", monitoredStatistics, filterStats) as long?;
            QueryExecutionMaxTime = GetValue(configuration, QueryExecutionMaxTime, "QueryExecutionMaxTime", monitoredStatistics, filterStats) as TimeSpan?;
            QueryExecutionMaxTimeQueryString = GetValue(configuration, QueryExecutionMaxTimeQueryString, "QueryExecutionMaxTimeQueryString", monitoredStatistics, filterStats) as string;

            QueryCacheHitCount = GetValue(configuration, QueryCacheHitCount, "QueryCacheHitCount", monitoredStatistics, filterStats) as long?;
            QueryCacheMissCount = GetValue(configuration, QueryCacheMissCount, "QueryCacheMissCount", monitoredStatistics, filterStats) as long?;
            QueryCachePutCount = GetValue(configuration, QueryCachePutCount, "QueryCachePutCount", monitoredStatistics, filterStats) as long?;
            FlushCount = GetValue(configuration, FlushCount, "FlushCount", monitoredStatistics, filterStats) as long?;
            ConnectCount = GetValue(configuration, ConnectCount, "ConnectCount", monitoredStatistics, filterStats) as long?;
            SecondLevelCacheHitCount = GetValue(configuration, SecondLevelCacheHitCount, "SecondLevelCacheHitCount", monitoredStatistics, filterStats) as long?;
            SecondLevelCacheMissCount = GetValue(configuration, SecondLevelCacheMissCount, "SecondLevelCacheMissCount", monitoredStatistics, filterStats) as long?;
            SecondLevelCachePutCount = GetValue(configuration, SecondLevelCachePutCount, "SecondLevelCachePutCount", monitoredStatistics, filterStats) as long?;
            SessionCloseCount = GetValue(configuration, SessionCloseCount, "SessionCloseCount", monitoredStatistics, filterStats) as long?;
            SessionOpenCount = GetValue(configuration, SessionOpenCount, "SessionOpenCount", monitoredStatistics, filterStats) as long?;
            CollectionLoadCount = GetValue(configuration, CollectionLoadCount, "CollectionLoadCount", monitoredStatistics, filterStats) as long?;
            CollectionFetchCount = GetValue(configuration, CollectionFetchCount, "CollectionFetchCount", monitoredStatistics, filterStats) as long?;
            CollectionUpdateCount = GetValue(configuration, CollectionUpdateCount, "CollectionUpdateCount", monitoredStatistics, filterStats) as long?;
            CollectionRemoveCount = GetValue(configuration, CollectionRemoveCount, "CollectionRemoveCount", monitoredStatistics, filterStats) as long?;
            CollectionRecreateCount = GetValue(configuration, CollectionRecreateCount, "CollectionRecreateCount", monitoredStatistics, filterStats) as long?;

            StartTime = GetValue(configuration, StartTime, "StartTime", monitoredStatistics, filterStats) as DateTime?;

            Queries = GetValue(configuration, Queries, "Queries", monitoredStatistics, filterStats) as string[];
            EntityNames = GetValue(configuration, EntityNames, "EntityNames", monitoredStatistics, filterStats) as string[];
            CollectionRoleNames = GetValue(configuration, CollectionRoleNames, "CollectionRoleNames", monitoredStatistics, filterStats) as string[];
            SecondLevelCacheRegionNames = GetValue(configuration, SecondLevelCacheRegionNames, "SecondLevelCacheRegionNames", monitoredStatistics, filterStats) as string[];

            SuccessfulTransactionCount = GetValue(configuration, SuccessfulTransactionCount, "SuccessfulTransactionCount", monitoredStatistics, filterStats) as long?;
            TransactionCount = GetValue(configuration, TransactionCount, "TransactionCount", monitoredStatistics, filterStats) as long?;
            PrepareStatementCount = GetValue(configuration, PrepareStatementCount, "PrepareStatementCount", monitoredStatistics, filterStats) as long?;
            CloseStatementCount = GetValue(configuration, CloseStatementCount, "CloseStatementCount", monitoredStatistics, filterStats) as long?;
            OptimisticFailureCount = GetValue(configuration, OptimisticFailureCount, "OptimisticFailureCount", monitoredStatistics, filterStats) as long?;

            OperationThreshold = GetValue(configuration, OperationThreshold, "OperationThreshold", monitoredStatistics, filterStats) as TimeSpan?;

            var isInteresting = (
                EntityDeleteCount.HasValue ||
                EntityInsertCount.HasValue ||
                EntityLoadCount.HasValue ||            
                EntityFetchCount.HasValue ||
                EntityUpdateCount.HasValue ||
                QueryExecutionCount.HasValue ||
                QueryExecutionMaxTime.HasValue ||
                !string.IsNullOrWhiteSpace(QueryExecutionMaxTimeQueryString) ||
                QueryCacheHitCount.HasValue ||
                QueryCacheMissCount.HasValue ||
                QueryCachePutCount.HasValue ||
                FlushCount.HasValue ||
                ConnectCount.HasValue ||
                SecondLevelCacheHitCount.HasValue ||
                SecondLevelCacheMissCount.HasValue ||
                SecondLevelCachePutCount.HasValue ||
                SessionCloseCount.HasValue ||
                SessionOpenCount.HasValue ||
                CollectionLoadCount.HasValue ||
                CollectionFetchCount.HasValue ||
                CollectionUpdateCount.HasValue ||
                CollectionRemoveCount.HasValue ||
                CollectionRecreateCount.HasValue ||
                StartTime.HasValue ||
                Queries?.Any() == true ||
                EntityNames?.Any() == true ||
                CollectionRoleNames?.Any() == true ||
                SecondLevelCacheRegionNames?.Any() == true ||
                SuccessfulTransactionCount.HasValue ||
                TransactionCount.HasValue ||
                PrepareStatementCount.HasValue ||
                CloseStatementCount.HasValue ||
                OptimisticFailureCount.HasValue ||
                OperationThreshold.HasValue
            );

            return isInteresting;
        }
    }
}
#endif