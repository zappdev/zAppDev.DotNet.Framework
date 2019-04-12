#if NETFRAMEWORK
using CLMS.Framework.Tools.PerformanceMeasurements.Configuration;
using CLMS.Framework.Tools.PerformanceMeasurements.Contracts;
using log4net;
using NHibernate.Stat;
using System;
using System.Linq;

namespace CLMS.Framework.Tools.PerformanceMeasurements.Helpers
{
    public class NHSessionStatisticsHelper
    {
        public enum UnionType
        {
            Union, 
            Previous, 
            Current, 
            OnlyPrevious, 
            OnlyCurrent, 
            Ignore
        }

        public bool ContinueOnError { get; set; }

        private readonly UnionType _unionType;

        private readonly DatabaseConfiguration _configuration;

        private readonly ILog _genericLog;

        private string _nameOfThis => nameof(NHSessionStatisticsHelper);
        private Type _typeOfThis => typeof(NHSessionStatisticsHelper);

        private NHSessionStatistics _previousStatistics { get; set; }
        private NHSessionStatistics _currentStatistics { get; set; }

        public NHSessionStatisticsHelper(DatabaseConfiguration configuration, UnionType unionType = UnionType.Union, ILog log = null)
        {
            ContinueOnError = true;

            _configuration = configuration;
            _unionType = unionType;

            _genericLog = LogManager.GetLogger(_typeOfThis);
        }//end ctor()

        public void Capture(IStatistics statistics, MonitorStatus status)
        {
            switch (status)
            {
                case MonitorStatus.None:
                    break;
                case MonitorStatus.Stopped:
                    _currentStatistics = Get(statistics);
                    break;
                case MonitorStatus.Running:
                    _previousStatistics = Get(statistics);
                    break;
                default:
                    break;
            }
        }

        private NHSessionStatistics Get(IStatistics statistics)
        {
            try
            {
                if (statistics == null) return null;

                var result = new NHSessionStatistics();

                result.CloseStatementCount = statistics.CloseStatementCount;
                result.CollectionFetchCount = statistics.CollectionFetchCount;
                result.CollectionLoadCount = statistics.CollectionLoadCount;
                result.CollectionRecreateCount = statistics.CollectionRecreateCount;
                result.CollectionRemoveCount = statistics.CollectionRemoveCount;

                result.CollectionRoleNames = statistics.CollectionRoleNames;
                result.EntityNames = statistics.EntityNames;
                result.Queries = statistics.Queries;
                result.SecondLevelCacheRegionNames = statistics.SecondLevelCacheRegionNames;
                result.QueryExecutionMaxTimeQueryString = statistics.QueryExecutionMaxTimeQueryString;

                result.CollectionUpdateCount = statistics.CollectionUpdateCount;
                result.ConnectCount = statistics.ConnectCount;
                result.EntityDeleteCount = statistics.EntityDeleteCount;
                result.EntityFetchCount = statistics.EntityFetchCount;
                result.EntityInsertCount = statistics.EntityInsertCount;
                result.EntityLoadCount = statistics.EntityLoadCount;
                result.EntityUpdateCount = statistics.EntityUpdateCount;
                result.FlushCount = statistics.FlushCount;
                result.OperationThreshold = statistics.OperationThreshold;
                result.OptimisticFailureCount = statistics.OptimisticFailureCount;
                result.PrepareStatementCount = statistics.PrepareStatementCount;
                result.QueryCacheHitCount = statistics.QueryCacheHitCount;
                result.QueryCacheMissCount = statistics.QueryCacheMissCount;
                result.QueryCachePutCount = statistics.QueryCachePutCount;
                result.QueryExecutionCount = statistics.QueryExecutionCount;
                result.QueryExecutionMaxTime = statistics.QueryExecutionMaxTime;
                result.SecondLevelCacheHitCount = statistics.SecondLevelCacheHitCount;
                result.SecondLevelCacheMissCount = statistics.SecondLevelCacheMissCount;
                result.SecondLevelCachePutCount = statistics.SecondLevelCachePutCount;
                result.SessionCloseCount = statistics.SessionCloseCount;
                result.SessionOpenCount = statistics.SessionOpenCount;
                result.SuccessfulTransactionCount = statistics.SuccessfulTransactionCount;
                result.TransactionCount = statistics.TransactionCount;

                return result;
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Exception while Getting Statistics: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Getting Statistics: {e.StackTrace}");
                if (!ContinueOnError) throw;
                return null;
            }
        }

        public NHSessionStatistics Get()
        {
            try
            {
                if(_previousStatistics == null || _currentStatistics == null)
                {
                    return null; 
                }

                var result = new NHSessionStatistics();

                result.CloseStatementCount = _currentStatistics?.CloseStatementCount - _previousStatistics?.CloseStatementCount;
                result.CollectionFetchCount = _currentStatistics?.CollectionFetchCount - _previousStatistics?.CollectionFetchCount;
                result.CollectionLoadCount = _currentStatistics?.CollectionLoadCount - _previousStatistics?.CollectionLoadCount;
                result.CollectionRecreateCount = _currentStatistics?.CollectionRecreateCount - _previousStatistics?.CollectionRecreateCount;
                result.CollectionRemoveCount = _currentStatistics?.CollectionRemoveCount - _previousStatistics?.CollectionRemoveCount;

                switch (_unionType)
                {
                    case UnionType.Union:
                        result.CollectionRoleNames = _currentStatistics?.CollectionRoleNames.Union(_previousStatistics?.CollectionRoleNames).ToArray();
                        result.EntityNames = _currentStatistics?.EntityNames.Union(_previousStatistics?.EntityNames).ToArray();
                        result.Queries = _currentStatistics?.Queries.Union(_previousStatistics?.Queries).ToArray();
                        result.SecondLevelCacheRegionNames = _currentStatistics?.SecondLevelCacheRegionNames.Union(_previousStatistics?.SecondLevelCacheRegionNames).ToArray();
                        result.QueryExecutionMaxTimeQueryString = _currentStatistics?.QueryExecutionMaxTimeQueryString;
                        break;
                    case UnionType.Current:
                        result.CollectionRoleNames = _currentStatistics?.CollectionRoleNames;
                        result.EntityNames = _currentStatistics?.EntityNames;
                        result.Queries = _currentStatistics?.Queries;
                        result.SecondLevelCacheRegionNames = _currentStatistics?.SecondLevelCacheRegionNames;
                        result.QueryExecutionMaxTimeQueryString = _currentStatistics?.QueryExecutionMaxTimeQueryString;
                        break;
                    case UnionType.Previous:
                        result.CollectionRoleNames = _previousStatistics?.CollectionRoleNames;
                        result.EntityNames = _previousStatistics?.EntityNames;
                        result.Queries = _previousStatistics?.Queries;
                        result.SecondLevelCacheRegionNames = _previousStatistics?.SecondLevelCacheRegionNames;
                        result.QueryExecutionMaxTimeQueryString = _previousStatistics?.QueryExecutionMaxTimeQueryString;
                        break;
                    case UnionType.OnlyCurrent:
                        result.CollectionRoleNames = _currentStatistics?.CollectionRoleNames.Except(_previousStatistics?.CollectionRoleNames).ToArray();
                        result.EntityNames = _currentStatistics?.EntityNames.Except(_previousStatistics?.EntityNames).ToArray();
                        result.Queries = _currentStatistics?.Queries.Except(_previousStatistics?.Queries).ToArray();
                        result.SecondLevelCacheRegionNames = _currentStatistics?.SecondLevelCacheRegionNames.Except(_previousStatistics?.SecondLevelCacheRegionNames).ToArray();
                        result.QueryExecutionMaxTimeQueryString = _currentStatistics?.QueryExecutionMaxTimeQueryString;
                        break;
                    case UnionType.OnlyPrevious:
                        result.CollectionRoleNames = _previousStatistics?.CollectionRoleNames.Except(_currentStatistics?.CollectionRoleNames).ToArray();
                        result.EntityNames = _previousStatistics?.EntityNames.Except(_currentStatistics?.EntityNames).ToArray();
                        result.Queries = _previousStatistics?.Queries.Except(_currentStatistics?.Queries).ToArray();
                        result.SecondLevelCacheRegionNames = _previousStatistics?.SecondLevelCacheRegionNames.Except(_currentStatistics?.SecondLevelCacheRegionNames).ToArray();
                        result.QueryExecutionMaxTimeQueryString = _previousStatistics?.QueryExecutionMaxTimeQueryString;
                        break;
                    case UnionType.Ignore:
                        break;
                    default:
                        break;
                }

                result.CollectionUpdateCount = _currentStatistics?.CollectionUpdateCount - _previousStatistics?.CollectionUpdateCount;
                result.ConnectCount = _currentStatistics?.ConnectCount - _previousStatistics?.ConnectCount;
                result.EntityDeleteCount = _currentStatistics?.EntityDeleteCount - _previousStatistics?.EntityDeleteCount;
                result.EntityFetchCount = _currentStatistics?.EntityFetchCount - _previousStatistics?.EntityFetchCount;
                result.EntityInsertCount = _currentStatistics?.EntityInsertCount - _previousStatistics?.EntityInsertCount;
                result.EntityLoadCount = _currentStatistics?.EntityLoadCount - _previousStatistics?.EntityLoadCount;
                result.EntityUpdateCount = _currentStatistics?.EntityUpdateCount - _previousStatistics?.EntityUpdateCount;
                result.FlushCount = _currentStatistics?.FlushCount - _previousStatistics?.FlushCount;
                result.OperationThreshold = _currentStatistics?.OperationThreshold - _previousStatistics?.OperationThreshold;
                result.OptimisticFailureCount = _currentStatistics?.OptimisticFailureCount - _previousStatistics?.OptimisticFailureCount;
                result.PrepareStatementCount = _currentStatistics?.PrepareStatementCount - _previousStatistics?.PrepareStatementCount;
                result.QueryCacheHitCount = _currentStatistics?.QueryCacheHitCount - _previousStatistics?.QueryCacheHitCount;
                result.QueryCacheMissCount = _currentStatistics?.QueryCacheMissCount - _previousStatistics?.QueryCacheMissCount;
                result.QueryCachePutCount = _currentStatistics?.QueryCachePutCount - _previousStatistics?.QueryCachePutCount;
                result.QueryExecutionCount = _currentStatistics?.QueryExecutionCount - _previousStatistics?.QueryExecutionCount;
                result.QueryExecutionMaxTime = _currentStatistics?.QueryExecutionMaxTime - _previousStatistics?.QueryExecutionMaxTime;
                result.SecondLevelCacheHitCount = _currentStatistics?.SecondLevelCacheHitCount - _previousStatistics?.SecondLevelCacheHitCount;
                result.SecondLevelCacheMissCount = _currentStatistics?.SecondLevelCacheMissCount - _previousStatistics?.SecondLevelCacheMissCount;
                result.SecondLevelCachePutCount = _currentStatistics?.SecondLevelCachePutCount - _previousStatistics?.SecondLevelCachePutCount;
                result.SessionCloseCount = _currentStatistics?.SessionCloseCount - _previousStatistics?.SessionCloseCount;
                result.SessionOpenCount = _currentStatistics?.SessionOpenCount - _previousStatistics?.SessionOpenCount;
                result.SuccessfulTransactionCount = _currentStatistics?.SuccessfulTransactionCount - _previousStatistics?.SuccessfulTransactionCount;
                result.TransactionCount = _currentStatistics?.TransactionCount - _previousStatistics?.TransactionCount;

                return result;
            }
            catch (Exception e)
            {
                _genericLog.Error($"{_nameOfThis} - Exception while Calculating Statistics: {e.Message}");
                _genericLog.Debug($"{_nameOfThis} - Exception while Calculating Statistics: {e.StackTrace}");
                if (!ContinueOnError) throw;
                return null;
            }
        }//end Get()
    }
}
#endif