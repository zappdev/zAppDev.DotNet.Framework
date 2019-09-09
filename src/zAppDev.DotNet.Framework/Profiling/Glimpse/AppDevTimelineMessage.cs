// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
using Glimpse.Core.Message;
#endif

using System;

namespace zAppDev.DotNet.Framework.Profiling.Glimpse
{
#if NETFRAMEWORK
    public class AppDevTimelineMessage : ITimelineMessage
    {
        public Guid Id { get; } = Guid.NewGuid();

        public TimeSpan Offset { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime StartTime { get; set; }

        public string EventName { get; set; }

        public string EventSubText { get; set; }

        public TimelineCategoryItem EventCategory
        {
            get => new TimelineCategoryItem("zAppDev", "#405064", "#dce2ea");
            set { }
        }
    }
#endif
}