using System;
using System.Linq;

#if NETFRAMEWORK
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Tab.Assist;
using Assist = Glimpse.Core.Tab.Assist;
#endif

namespace CLMS.Framework.Profiling.Glimpse
{
#if NETFRAMEWORK
    public class Plugin : ITab, ITabSetup, IDocumentation
    {
        public string DocumentationUri => "https://zappdev.com";

        public string Name => "zAppDev";

        public RuntimeEvent ExecuteOn => RuntimeEvent.EndRequest;

        public Type RequestContextType => null;

        public object GetData(ITabContext context)
        {
            if (context == null) return string.Empty;
            var messages = context.GetMessages<LogStatistic>();
            if (messages == null) return string.Empty;


            var data = Assist.Plugin.Create("Model Name", "Symbol Type", "Symbol Name", "Invocations", "Time");

            var items = messages
                .GroupBy(m => new { m.ModelName, m.SymbolName, m.SymbolType })
                .Select(g => new
                {
                    g.FirstOrDefault()?.ModelName,
                    g.FirstOrDefault()?.SymbolType,
                    g.FirstOrDefault()?.SymbolName,
                    Times = g.Count(),
                    Time = g.Sum(m => m.Time)
                })
                .OrderByDescending(o => o.Time);

            foreach (var msg in items)
            {
                data.AddRow()
                         .Column(msg.ModelName).Strong()
                         .Column(msg.SymbolType.ToString())
                         .Column(msg.SymbolName)
                         .Column(msg.Times)
                         .Column(msg.Time + "ms");
            }

            return data;
        }

        public void Setup(ITabSetupContext context)
        {
            context.PersistMessages<LogStatistic>();
        }
    }
#endif
}