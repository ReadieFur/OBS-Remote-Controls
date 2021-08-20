using System;
using System.Diagnostics;
using System.Reflection;

namespace OBS_Remote_Controls
{
    internal static class Logger
    {
#if DEBUG
        public static LogLevel logLevel = LogLevel.Trace;
#else
        public static LogLevel logLevel = LogLevel.None;
#endif
        //Disposal of this window should be ok to be handled automatically.
        private static WPF.LoggerWindow windowInstance = new WPF.LoggerWindow(4);

        public static void ShowWindow() { windowInstance.Show(); }
        public static void HideWindow() { windowInstance.Hide(); }
        public static bool IsWindowVisible() { return windowInstance.IsVisible; }

        public static void Debug(object _message) { WriteLog(LogLevel.Debug, _message.ToString()); }
        public static void Debug(Exception _ex) { WriteLog(LogLevel.Debug, _ex.Message); }
        public static void Info(object _message) { WriteLog(LogLevel.Info, _message.ToString()); }
        public static void Info(Exception _ex) { WriteLog(LogLevel.Info, _ex.Message); }
        public static void Warning(object _message) { WriteLog(LogLevel.Warning, _message.ToString()); }
        public static void Warning(Exception _ex) { WriteLog(LogLevel.Warning, _ex.Message); }
        public static void Error(object _message) { WriteLog(LogLevel.Error, _message.ToString()); }
        public static void Error(Exception _ex) { WriteLog(LogLevel.Error, _ex.Message); }
        public static void Critical(object _message) { WriteLog(LogLevel.Critical, _message.ToString()); }
        public static void Critical(Exception _ex) { WriteLog(LogLevel.Critical, _ex.Message); }
        public static void Notice(object _message) { WriteLog(LogLevel.Notice, _message.ToString()); }
        public static void Notice(Exception _ex) { WriteLog(LogLevel.Notice, _ex.Message); }
        public static void Trace(object _message) { WriteLog(LogLevel.Trace, _message.ToString()); }
        public static void Trace(Exception _ex) { WriteLog(LogLevel.Trace, _ex.Message); }

        private static void WriteLog(LogLevel _logLevel, string _message)
        {
            if (_logLevel <= logLevel)
            {
                MethodBase stackTraceMethod = new StackTrace().GetFrame(2).GetMethod();
                System.Diagnostics.Debug.WriteLine($"[{_logLevel} @ {DateTime.Now} | {stackTraceMethod.DeclaringType}/{stackTraceMethod.Name}] {_message}");

                if (windowInstance.IsVisible)
                {
                    switch (_logLevel)
                    {
                        case LogLevel.Debug:
                            windowInstance.Debug(_message);
                            break;
                        case LogLevel.Info:
                            windowInstance.Info(_message);
                            break;
                        case LogLevel.Warning:
                            windowInstance.Warning(_message);
                            break;
                        case LogLevel.Error:
                            windowInstance.Error(_message);
                            break;
                        case LogLevel.Critical:
                            windowInstance.Critical(_message);
                            break;
                        case LogLevel.Notice:
                            windowInstance.Notice(_message);
                            break;
                        case LogLevel.Trace:
                            windowInstance.Trace(_message);
                            break;
                        default: //None
                            break;
                    }
                }
            }
        }

        public enum LogLevel
        {
            None = 0,
            Info = 1,
            Critical = 2,
            Error = 3,
            Warning = 4,
            Notice = 5,
            Trace = 6,
            Debug = 7
        }
    }
}
