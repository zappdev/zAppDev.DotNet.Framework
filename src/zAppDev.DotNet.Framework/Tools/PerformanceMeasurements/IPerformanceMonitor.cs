// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts
{
    public enum MonitorStatus
    {
        None,
        Stopped,
        Running
    }

    public interface IPerformanceMonitor<IPerformanceStatistic>
    {
        MonitorStatus MonitorStatus { get; }
        bool ContinueOnError { get; set; }

        void Start();
        void Stop();
        IPerformanceStatistic Get();
    }

}