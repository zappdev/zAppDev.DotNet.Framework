using System;
using zAppDev.DotNet.Framework.Profiling.Glimpse;

namespace zAppDev.DotNet.Framework.Profiling
{
#if NETFRAMEWORK
    public class Profiler : IDisposable
    {
        private readonly Logger _logger;

        public Profiler(string modelName, AppDevSymbolType type, string symbolName)
        {
            _logger = Logger.Start(modelName, type, symbolName);
        }

        public void Dispose()
        {
            _logger.Stop();
        }
    }
#endif
}