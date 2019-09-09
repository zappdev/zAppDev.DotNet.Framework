// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;

#if NETFRAMEWORK
using Glimpse.Core.Message;
#endif

namespace zAppDev.DotNet.Framework.Profiling
{
#if NETFRAMEWORK
    public class LogStatistic : IMessage
    {
        internal LogStatistic()
        {

        }

        public string ModelName { get; set; }
        public AppDevSymbolType SymbolType { get; set; }
        public string SymbolName { get; set; }

        public Guid Id { get; } = Guid.NewGuid();

        public int Time { get; internal set; }
    }
#endif
}