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