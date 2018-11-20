using CLMS.Framework.Tools.PerformanceMeasurements.Configuration;
using CLMS.Framework.Tools.PerformanceMeasurements.Contracts;
using log4net;
using NHibernate.Stat;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CLMS.Framework.Tools.PerformanceMeasurements.Helpers
{
    public class NHEntityStatisticsHelper
    {
        private readonly Func<NHEntityStatistics, bool> _where;
        private readonly ILog _log;

        private readonly DatabaseConfiguration _configuration;
        public bool ContinueOnError { get; set; }

        private List<NHEntityStatistics> _previousStatistics { get; set; }
        private List<NHEntityStatistics> _currentStatistics { get; set; }

        private List<string> _monitoredEntities { get; set; }

        private string _nameOfThis => nameof(NHEntityStatisticsHelper);
        private Type _typeOfThis => typeof(NHEntityStatisticsHelper);

        public NHEntityStatisticsHelper(DatabaseConfiguration configuration, Func<NHEntityStatistics, bool> where = null, ILog log = null)
        {
            ContinueOnError = true;

            _configuration = configuration;

            if(where != null)
            {
                _where = where;
            }
            else
            {
                _where = (x => true);
            }

            _log = log ?? LogManager.GetLogger(_typeOfThis);
        }

        private string GetEntityType(IStatistics statistics, string entityName)
        {
            if(statistics.EntityNames.Any(x => x == entityName))
            {
                return "Entity";
            }

            if(statistics.CollectionRoleNames.Any(x => x == entityName))
            {
                return "Collection";
            }

            return "Unknown";
        }

        public void Capture(IStatistics statistics, MonitorStatus status)
        {
            try
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
            catch (Exception e)
            {
                _log.Error($"{_nameOfThis} - Failed to Capture Statistics for {_nameOfThis}: {e.Message}");
                _log.Debug($"{_nameOfThis} - Failed to Capture Statistics for {_nameOfThis}: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
        }

        private List<NHEntityStatistics> Get(IStatistics statistics)
        {
            var result = new List<NHEntityStatistics>();

            try
            {

                var entityNames = _configuration?.Entities?.MonitoredEntities;
                if (entityNames == null || !entityNames.Any())
                {
                    entityNames =
                        statistics.EntityNames
                        .Union(statistics.CollectionRoleNames).ToList();
                }

                foreach (var entityName in entityNames)
                {
                    result.Add(Get(statistics, entityName));
                }
            }
            catch (Exception e)
            {
                _log.Error($"{_nameOfThis} - Failed to Get Statistics for {_nameOfThis}: {e.Message}");
                _log.Debug($"{_nameOfThis} - Failed to Get Statistics for {_nameOfThis}: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
            return result.Where(_where).ToList();
        }

        private NHEntityStatistics Get(IStatistics statistics, string entityName)
        {
            var result = new NHEntityStatistics();

            try
            {

                var entityType = GetEntityType(statistics, entityName);
                result.EntityType = entityType;
                result.EntityName = entityName;
                switch (entityType)
                {
                    case "Entity":
                        var entityStatistics = statistics.GetEntityStatistics(entityName);
                        result.DeleteCount = entityStatistics.DeleteCount;
                        result.FetchCount = entityStatistics.FetchCount;
                        result.InsertCount = entityStatistics.InsertCount;
                        result.LoadCount = entityStatistics.LoadCount;
                        result.OptimisticFailureCount = entityStatistics.OptimisticFailureCount;
                        result.UpdateCount = entityStatistics.UpdateCount;
                        result.EntityName = entityStatistics.CategoryName;
                        break;
                    case "Collection":
                        var collectionStatistics = statistics.GetCollectionStatistics(entityName);
                        result.DeleteCount = collectionStatistics.RemoveCount;
                        result.FetchCount = collectionStatistics.FetchCount;
                        result.LoadCount = collectionStatistics.LoadCount;
                        result.RecreateCount = collectionStatistics.RecreateCount;
                        result.UpdateCount = collectionStatistics.UpdateCount;
                        result.EntityName = collectionStatistics.CategoryName;
                        break;
                    default:
                        result.EntityType = "Unknown";
                        break;
                }

            }
            catch (Exception e)
            {
                _log.Error($"{_nameOfThis} - Failed to Get Statistics for {entityName}, by {_nameOfThis}: {e.Message}");
                _log.Debug($"{_nameOfThis} - Failed to Get Statistics for {entityName}, by {_nameOfThis}: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }

            return result;
        }

        public List<NHEntityStatistics> Get()
        {
            if (!(_previousStatistics?.Any() == true || _currentStatistics?.Any() == true)) return null;

            if(_currentStatistics?.Any() == true && _previousStatistics?.Any() != true)
            {
                return _currentStatistics;
            }

            if (_previousStatistics?.Any() == true && _currentStatistics?.Any() != true)
            {
                return _previousStatistics;
            }

            var result = new List<NHEntityStatistics>();

            try
            {
                var allEntities = _previousStatistics.Select(x => x.EntityName).Union(_currentStatistics.Select(x => x.EntityName)).Distinct();

                foreach (var ent in allEntities)
                {
                    var record = new NHEntityStatistics();

                    var previous = _previousStatistics?.FirstOrDefault(x => x.EntityName == ent);
                    var current = _currentStatistics?.FirstOrDefault(x => x.EntityName == ent);

                    record.EntityName = ent;
                    record.EntityType = current != null ? current.EntityType : previous.EntityType;
                    record.DeleteCount = current?.DeleteCount - previous?.DeleteCount;
                    record.FetchCount = current?.FetchCount - previous?.FetchCount;
                    record.InsertCount = current?.InsertCount - previous?.InsertCount;
                    record.LoadCount = current?.LoadCount - previous?.LoadCount;
                    record.OptimisticFailureCount = current?.OptimisticFailureCount - previous?.OptimisticFailureCount;
                    record.UpdateCount = current?.UpdateCount - previous?.UpdateCount;
                    record.RecreateCount = current?.RecreateCount - previous?.RecreateCount;

                    result.Add(record);
                }
            }
            catch (Exception e)
            {
                _log.Error($"{_nameOfThis} - Failed to Calculate Statistics for {_nameOfThis}: {e.Message}");
                _log.Debug($"{_nameOfThis} - Failed to Calculate Statistics for {_nameOfThis}: {e.StackTrace}");
                if (!ContinueOnError) throw;
            }
 
            return result;
        }
    }
}
