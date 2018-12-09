using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using log4net;
using Newtonsoft.Json;

namespace CLMS.Framework.Utilities
{
    public enum DebugMessageType
    {
        Debug = 0,
        Info,
        Warning,
        Error,
        IDEF0Trace
    }

    public static class DebugHelper
    {
        public delegate void RaiseDebugEventCallBack(string type, string message);

        public static void Log(DebugMessageType messageType, RaiseDebugEventCallBack raiseEvent, object message,
            bool showInDebugConsole = false)
        {
            Log(messageType, null, raiseEvent, message, showInDebugConsole);
        }

        public static void Log(List<string> parameters, string logger)
        {
            var intMessageType = 0;
            var debugMessageType = DebugMessageType.Debug;
            var message = "";

            try
            {
                var numberToBeParsed = parameters.Count > 0 ? parameters[0] : "0";
                int.TryParse(parameters[0], out intMessageType);
                debugMessageType = (DebugMessageType) intMessageType;

                message = parameters.Count > 1 ? parameters[1] : "";
            }
            catch
            {
                LogManager.GetLogger(Assembly.GetEntryAssembly(), logger)
                    .Error("Error parsing Log Message type to number!");
                //Hush....
            }

            Log(debugMessageType, logger, message);
        }

        public static void Log(DebugMessageType messageType, string logger, object message)
        {
            if (logger != null)
            {
                var _logger = LogManager.GetLogger(Assembly.GetEntryAssembly(), logger);
                switch (messageType)
                {
                    case DebugMessageType.Debug:
                        _logger.Debug(message);
                        break;
                    case DebugMessageType.Info:
                        _logger.Info(message);
                        break;
                    case DebugMessageType.Warning:
                        _logger.Warn(message);
                        break;
                    case DebugMessageType.Error:
                        _logger.Error(message);
                        break;
                    case DebugMessageType.IDEF0Trace:
                        Trace.WriteLine(message);
                        break;
                }
            }
        }

        public static void Log(DebugMessageType messageType, string logger, RaiseDebugEventCallBack raiseEvent,
            object message, bool showInDebugConsole = false)
        {
            if (logger == null && !showInDebugConsole) return;

            var messageTypeStr = messageType.ToString();
            var messageDataString =
                JsonConvert.SerializeObject(message, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });

            Log(messageType, logger, messageDataString);

            if (showInDebugConsole) raiseEvent(messageTypeStr, messageDataString);
        }
    }
}