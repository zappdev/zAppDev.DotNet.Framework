using System;

#if NETFRAMEWORK
using Glimpse.Core.Message;
#endif

namespace CLMS.Framework.Profiling
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