// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using log4net;
using NHibernate;

namespace zAppDev.DotNet.Framework.Logging
{
    public static class NHStatisticsLogger
    {
        public static void PrintGlobalStatistics(this ISession session)
        {
            LogManager.GetLogger(typeof(NHStatisticsLogger)).Info("~~~ NHibernate Statistics ~~~");
            var stats = session.SessionFactory.Statistics;
            stats.LogSummary();
        }

        public static void PrintSessionStatistics(this ISession session)
        {
            var stats = session.Statistics;
        }
    }
}
