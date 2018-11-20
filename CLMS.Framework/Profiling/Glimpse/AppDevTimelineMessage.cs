using Glimpse.Core.Message;
using System;

namespace CLMS.Framework.Profiling.Glimpse
{
    public class AppDevTimelineMessage : ITimelineMessage
    {
        private readonly Guid _id = Guid.NewGuid();

        public Guid Id
        {
            get { return _id; }
        }

        public TimeSpan Offset { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime StartTime { get; set; }

        public string EventName { get; set; }

        public string EventSubText { get; set; }

        public TimelineCategoryItem EventCategory
        {
            get
            {
                return new TimelineCategoryItem("zAppDev", "#405064", "#dce2ea");
            }
            set { }
        }
    }
}
