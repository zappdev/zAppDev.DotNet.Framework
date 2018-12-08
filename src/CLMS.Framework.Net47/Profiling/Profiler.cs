#if NETFRAMEWORK
using CLMS.Framework.Profiling.Glimpse;
using System;

namespace CLMS.Framework.Profiling
{
    public class Profiler : IDisposable
    {
        private Logger _logger;

        public Profiler(string modelName, AppDevSymbolType type, string symbolName)
        {
            _logger = Logger.Start(modelName, type, symbolName);
        }

        public void Dispose()
        {
            _logger.Stop();
        }
    }
}
#endif