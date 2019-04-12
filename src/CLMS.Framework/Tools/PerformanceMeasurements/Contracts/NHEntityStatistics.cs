#if NETFRAMEWORK
using CLMS.Framework.Tools.PerformanceMeasurements.Configuration;

namespace CLMS.Framework.Tools.PerformanceMeasurements.Contracts
{

    public class NHEntityStatistics : IPerformanceStatistic<EntitiesConfiguration>
    {
        public string EntityName { get; set; }                  //Entity & Collection
        public long? LoadCount { get; set; }                    //Entity & Collection
        public long? UpdateCount { get; set; }                  //Entity & Collection
        public long? InsertCount { get; set; }                  //Entity
        public long? DeleteCount { get; set; }                  //Entity & Collection
        public long? FetchCount { get; set; }                   //Entity & Collection
        public long? OptimisticFailureCount { get; set; }       //Entity
        public long? RecreateCount { get; set; }                //Collection
        public string EntityType { get; set; }

        public bool IsInteresting(EntitiesConfiguration configuration = null)
        {
            if (configuration == null) return true;

            if (!configuration.Enabled) return false;

            if (!configuration.IgnoreNulls) return true;

            var monitoredStatistics = configuration.MonitoredStatistics;
            var filterStats = (monitoredStatistics?.Count).GetValueOrDefault(0) > 0;


            if ( (filterStats && !monitoredStatistics.Contains("LoadCount")) || (configuration.IgnoreNulls && LoadCount.GetValueOrDefault(0) == 0))
            {
                LoadCount = null;
            }

            if ((filterStats && !monitoredStatistics.Contains("UpdateCount")) || (configuration.IgnoreNulls && UpdateCount.GetValueOrDefault(0) == 0))
            {
                UpdateCount = null;
            }

            if ((filterStats && !monitoredStatistics.Contains("DeleteCount")) || (configuration.IgnoreNulls && DeleteCount.GetValueOrDefault(0) == 0))
            {
                DeleteCount = null;
            }

            if ((filterStats && !monitoredStatistics.Contains("FetchCount")) || (configuration.IgnoreNulls && FetchCount.GetValueOrDefault(0) == 0))
            {
                FetchCount = null;
            }

            if ((EntityType != "Entity") || (filterStats && !monitoredStatistics.Contains("InsertCount")) || (configuration.IgnoreNulls && InsertCount.GetValueOrDefault(0) == 0))
            {
                InsertCount = null;
            }

            if ((EntityType != "Entity") || (filterStats && !monitoredStatistics.Contains("OptimisticFailureCount")) || (configuration.IgnoreNulls && OptimisticFailureCount.GetValueOrDefault(0) == 0))
            {
                OptimisticFailureCount = null;
            }

            if ((EntityType != "Collection") || (filterStats && !monitoredStatistics.Contains("RecreateCount")) || (configuration.IgnoreNulls && RecreateCount.GetValueOrDefault(0) == 0))
            {
                RecreateCount = null;
            }

            var isInteresting = (
                (
                    LoadCount.HasValue|| 
                    UpdateCount.HasValue|| 
                    InsertCount.HasValue|| 
                    DeleteCount.HasValue ||
                    FetchCount.HasValue ||
                    OptimisticFailureCount.HasValue ||
                    RecreateCount.HasValue
                )
            );

            if (!isInteresting)
            {
                EntityName = null;
                EntityType = null;
            }

            return isInteresting;
        }//end IsInteresting()
    }
}
#endif