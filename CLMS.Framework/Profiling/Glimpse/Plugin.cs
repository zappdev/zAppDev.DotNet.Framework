using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Tab.Assist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assist = Glimpse.Core.Tab.Assist;

namespace CLMS.Framework.Profiling.Glimpse
{
    public class Plugin : ITab, ITabSetup, IDocumentation
    {
        public string DocumentationUri
        {
            get { return "https://zappdev.com"; }
        }

        public string Name
        {
            get { return "zAppDev"; }
        }

        public RuntimeEvent ExecuteOn
        {
            get { return RuntimeEvent.EndRequest; }
        }

        public System.Type RequestContextType
        {
            get { return null; }
        }

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
                    g.FirstOrDefault().ModelName,
                    g.FirstOrDefault().SymbolType,
                    g.FirstOrDefault().SymbolName,
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
}
