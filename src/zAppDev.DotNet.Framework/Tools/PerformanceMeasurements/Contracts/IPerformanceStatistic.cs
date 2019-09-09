// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
namespace zAppDev.DotNet.Framework.Tools.PerformanceMeasurements.Contracts
{
    public interface IPerformanceStatistic<IPerformanceConfiguration>
    {
        bool IsInteresting(IPerformanceConfiguration configuration);
    }
}
#endif