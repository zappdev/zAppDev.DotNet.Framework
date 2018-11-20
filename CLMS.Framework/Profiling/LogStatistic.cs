using Glimpse.Core.Message;
using System;

namespace CLMS.Framework.Profiling
{
    public class LogStatistic : IMessage
    {
        private readonly Guid _id = Guid.NewGuid();

        internal LogStatistic()
        {

        }

        public string ModelName { get; set; }
        public AppDevSymbolType SymbolType { get; set; }
        public string SymbolName { get; set; }

        public Guid Id
        {
            get { return _id; }
        }

        public int Time { get; internal set; }
    }
}
