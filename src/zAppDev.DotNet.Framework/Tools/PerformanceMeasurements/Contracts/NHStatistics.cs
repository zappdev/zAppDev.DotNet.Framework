#if NETFRAMEWORK
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Configuration;
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts
{
    public class NHStatistics: IPerformanceStatistic<DatabaseConfiguration>
    {
        public List<NHEntityStatistics> NHEntityStatistics {get;set;}
        public NHSessionStatistics NHSessionStatistics { get; set; }

        public NHStatistics()
        {
            NHEntityStatistics = new List<NHEntityStatistics>();
            NHSessionStatistics = new NHSessionStatistics();
        }

        public bool IsInteresting(DatabaseConfiguration configuration = null)
        {
            if (configuration == null) return true;
            if (!configuration.Enabled) return false;

            var entitiesInteresting = false;

            if(NHEntityStatistics != null)
            {
                var empties = new List<NHEntityStatistics>();
                foreach (var entityStat in NHEntityStatistics)
                {
                    if (entityStat.IsInteresting(configuration.Entities))
                    {
                        entitiesInteresting = true;
                    }
                    else
                    {
                        empties.Add(entityStat);
                    }
                }
                NHEntityStatistics.RemoveAll(x => empties.Contains(x));
            }


            if (!entitiesInteresting) NHEntityStatistics = null;

            var sessionInteresting = NHSessionStatistics?.IsInteresting(configuration.Session) == true;
            if (!sessionInteresting) NHSessionStatistics = null;

            return (entitiesInteresting || sessionInteresting);
        }//end IsInteresting()
    }
}

#endif