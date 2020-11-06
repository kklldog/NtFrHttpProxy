using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace HttpProxy
{
    public static class Logger
    {
        #region Trace

        public static void Trace(string message)
        {
            Trace(message, null);
        }

        public static void Trace(string message, Type sender)
        {
            Trace(message, sender, null);
        }

        public static void Trace(string message, Type sender, Exception ex)
        {
            WriteLog(message, sender, ex, LogType.Trace);
        }

        #endregion

        #region Debug

        public static void Debug(string message)
        {
            Debug(message, null);
        }

        public static void Debug(string message, Type sender)
        {
            Debug(message, sender, null);
        }

        public static void Debug(string message, Type sender, Exception ex)
        {
            WriteLog(message, sender, ex, LogType.Debug);
        }

        #endregion

        #region Info

        public static void Info(string message)
        {
            Info(message, null);
        }

        public static void Info(string message, Type sender)
        {
            Info(message, sender, null);
        }

        public static void Info(string message, Type sender, Exception ex)
        {
            WriteLog(message, sender, ex, LogType.Info);
        }

        #endregion

        #region Warning

        public static void Warning(string message, Type sender)
        {
            Warning(message, sender, null);
        }

        public static void Warning(string message, Type sender, Exception ex)
        {
            WriteLog(message, sender, ex, LogType.Warning);
        }

        #endregion

        #region Error

        public static void Error(string message, Type sender)
        {
            Error(message, sender, null);
        }

        public static void Error(string message, Type sender, Exception ex)
        {
            WriteLog(message, sender, ex, LogType.Error);
            if (ex.InnerException != null)
            {
                Error(message + " > Inner Ex .", sender, ex.InnerException);
            }
        }

        #endregion

        #region Fatal

        public static void Fatal(string message, Type sender)
        {
            Fatal(message, sender, null);
        }

        public static void Fatal(string message, Type sender, Exception ex)
        {
            WriteLog(message, sender, ex, LogType.Fatal);
        }

        #endregion

        #region Private

        private static void WriteLog(string message, Type sender, Exception exception, LogType type)
        {
            string logMessage = GenerateMessage(message, type, sender, exception);

            LogEventInfo logEvent = new LogEventInfo { LoggerName = sender == null ? "System" : sender.FullName, TimeStamp = DateTime.Now };
            logEvent.Properties.Add("LogMessage", logMessage);
            logEvent.Properties.Add("AppLocation", _appLocation);

            switch (type)
            {
                case LogType.Trace:
                    logEvent.Level = LogLevel.Trace;
                    break;
                case LogType.Debug:
                    logEvent.Level = LogLevel.Debug;
                    break;
                case LogType.Info:
                    logEvent.Level = LogLevel.Info;
                    break;
                case LogType.Warning:
                    logEvent.Level = LogLevel.Warn;
                    break;
                case LogType.Error:
                    logEvent.Level = LogLevel.Error;
                    break;
                case LogType.Fatal:
                    logEvent.Level = LogLevel.Fatal;
                    break;
            }

            _logger.Log(logEvent);
#if DEBUG
            System.Diagnostics.Debug.WriteLine(logMessage);
#endif
        }

        private static string GenerateMessage(string message, LogType type, Type sender, Exception ex)
        {
            return string.Format("{0} {1} {2}\r\n{3}\r\n",
                                 DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.mmm"),
                                 type.ToString().ToUpper(),
                                 sender == null ? "System" : sender.FullName,
                                 ex == null
                                     ? "Message : " + message
                                     : string.Format("Message : {0}\r\nException : {1}\r\nStackTrace :\r\n{2}", message, ex.Message, ex.StackTrace));
        }

        private static void ConfigLog()
        {
            LoggingConfiguration config = new LoggingConfiguration();

            FileTarget fileTarget = new FileTarget
            {
                Encoding = Encoding.UTF8,
                FileName = @"${event-context:item=AppLocation}\\logs\\${shortdate}\\[${level}]${logger}.txt",
                Layout = @"${event-context:item=LogMessage}"
            };
            LoggingRule rule = new LoggingRule("*", LogLevel.Trace, fileTarget);
            config.AddTarget("file", fileTarget);
            config.LoggingRules.Add(rule);

            FileTarget timelineTarget = new FileTarget
            {
                Encoding = Encoding.UTF8,
                FileName = @"${event-context:item=AppLocation}\\logs\\${shortdate}\\Timeline.txt",
                Layout = @"${event-context:item=LogMessage}"
            };
            LoggingRule timelineRule = new LoggingRule("*", LogLevel.Trace, timelineTarget);
            config.AddTarget("timeline", timelineTarget);
            config.LoggingRules.Add(timelineRule);

            LogManager.Configuration = config;
        }

        static Logger()
        {
            _appLocation = AppDomain.CurrentDomain.BaseDirectory;
            ConfigLog();
        }

        private enum LogType { Trace, Debug, Info, Warning, Error, Fatal }
        private static readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly string _appLocation;

        #endregion
    }
}