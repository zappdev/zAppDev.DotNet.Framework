using Glimpse.Core.Extensibility;
using System;

namespace CLMS.Framework.Profiling.Glimpse
{
    public class Logger : IInspector
    {
        private string modelName;
        private AppDevSymbolType type;
        private string symbolName;

        private static Func<RuntimePolicy> _runtime;
        private static Func<IExecutionTimer> _timerStrategy;
        private static IMessageBroker _messageBroker;
        private IExecutionTimer _timer;
        private TimeSpan _startOffset;

        public Logger()
        {

        }

        internal Logger(string modelName, AppDevSymbolType type, string symbolName)
        {
            this.modelName = modelName;
            this.type = type;
            this.symbolName = symbolName;
        }

        public void Setup(IInspectorContext context)
        {
            if (context == null) return;
            _runtime = context.RuntimePolicyStrategy;
            _timerStrategy = context.TimerStrategy;
            _messageBroker = context.MessageBroker;
        }

        public static Logger Start(string modelName, AppDevSymbolType type, string symbolName)
        {
            var logger = new Logger(modelName, type, symbolName);

            if (_timerStrategy != null)
            {
                logger.Begin();
            }

            return logger;
        }

        private void Begin()
        {
            _timer = _timerStrategy.Invoke();
            if (_timer == null) return;
            _startOffset = _timer.Start();
        }

        public void Stop()
        {
            if (_timer == null || _messageBroker == null || _runtime?.Invoke() == RuntimePolicy.Off)
            {
                return;
            }

            var point = _timer.Stop(_startOffset);

            var msg = new LogStatistic
            {
                ModelName = modelName,
                SymbolType = type,
                SymbolName = symbolName,
                Time = point.Duration.Milliseconds
            };

            var pointTimelineMessage = new AppDevTimelineMessage
            {
                Duration = point.Duration,
                Offset = point.Offset,
                StartTime = point.StartTime,
                EventName = $"{type} {modelName}.{symbolName}",
                EventSubText = msg.Id.ToString()
            };
            _messageBroker.Publish(pointTimelineMessage);

            _messageBroker.Publish(msg);
        }
    }
}
