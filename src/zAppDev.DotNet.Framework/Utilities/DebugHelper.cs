// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using log4net;
using Newtonsoft.Json;

namespace zAppDev.DotNet.Framework.Utilities
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
            var debugMessageType = DebugMessageType.Debug;
            var message = "";

            try
            {
                var numberToBeParsed = parameters.Count > 0 ? parameters[0] : "0";
                int.TryParse(parameters[0], out var intMessageType);
                debugMessageType = (DebugMessageType) intMessageType;

                message = parameters.Count > 1 ? parameters[1] : "";
            }
            catch
            {
                LogManager.GetLogger(Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(), logger)
                    .Error("Error parsing Log Message type to number!");
                //Hush....
            }

            Log(debugMessageType, logger, message);
        }

        public static void Log(DebugMessageType messageType, string logger, object message)
        {
            if (logger == null) return;

            var loggerManager = LogManager.GetLogger(Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(), logger);
            switch (messageType)
            {
                case DebugMessageType.Debug:
                    loggerManager.Debug(message);
                    break;
                case DebugMessageType.Info:
                    loggerManager.Info(message);
                    break;
                case DebugMessageType.Warning:
                    loggerManager.Warn(message);
                    break;
                case DebugMessageType.Error:
                    loggerManager.Error(message);
                    break;
                case DebugMessageType.IDEF0Trace:
                    Trace.WriteLine(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
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