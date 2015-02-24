using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository.Hierarchy;
#pragma warning disable 1591

namespace LandfillService.Common
{
    public static class log4netExtensions
    {
        public static void IfInfo(this ILog log, object message)
        {
            try
            {
                if (log.IsInfoEnabled)
                {
                    log.Info(message);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void IfInfo(this ILog log, object message, Exception ex)
        {
            try
            {
                if (log.IsInfoEnabled)
                {
                    log.Info(message, ex);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void IfInfoFormat(this ILog log, string format, params object[] args)
        {
            try
            {
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat(format, args);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <param name="methodName">Gets calling method name- Automatically set by compiler</param>
        /// <param name="sourceFile">Gets calling source file - Automatically set by compiler</param>
        /// <param name="lineNumber">Gets calling line number - Automatically set by compiler</param>
        public static void IfDebug(this ILog log, object message,
                [CallerMemberName] string methodName = null,
                [CallerFilePath] string sourceFile = null,
                [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format("[{0}] ({1}:{2}) - {3}", methodName, sourceFile, lineNumber, message));
                }
            }
            catch (Exception)
            {
            }
        }

        public static void IfDebug(this ILog log, object message, Exception ex)
        {
            try
            {
                if (log.IsDebugEnabled)
                {
                    StackFrame sf = new StackFrame(1, true);
                    string methodName = sf.GetMethod().Name;
                    string sourceFile = sf.GetFileName();
                    int lineNumber = sf.GetFileLineNumber();
                    log.Debug(string.Format("[{0}] ({1}:{2}) - {3}", methodName, sourceFile, lineNumber, message), ex);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void IfDebugFormat(this ILog log, string format, params object[] args)
        {
            try
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat(format, args);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void IfWarn(this ILog log, object message)
        {
            try
            {
                if (log.IsWarnEnabled)
                {
                    log.Warn(message);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void IfWarn(this ILog log, object message, Exception ex)
        {
            try
            {
                if (log.IsWarnEnabled)
                {
                    log.Warn(message, ex);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void IfWarnFormat(this ILog log, string format, params object[] args)
        {
            try
            {
                if (log.IsWarnEnabled)
                {
                    log.WarnFormat(format, args);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void IfWarnFormat(this ILog log, Exception ex, string format, params object[] args)
        {
            try
            {
                if (log.IsWarnEnabled)
                {
                    log.Warn(string.Format(format, args), ex);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void IfFatal(this ILog log, object message)
        {
            try
            {
                if (log.IsFatalEnabled)
                {
                    log.Fatal(message);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void IfFatal(this ILog log, object message, Exception ex)
        {
            try
            {
                if (log.IsFatalEnabled)
                {
                    log.Fatal(message, ex);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void IfError(this ILog log, object message)
        {
            try
            {
                if (log.IsErrorEnabled)
                {
                    log.Error(message);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void IfErrorFormat(this ILog log, string format, params object[] args)
        {
            try
            {
                if (log.IsErrorEnabled)
                {
                    log.Error(string.Format(format, args));
                }
            }
            catch (Exception)
            {
            }
        }

        public static void IfErrorFormat(this ILog log, Exception ex, string format, params object[] args)
        {
            try
            {
                if (log.IsErrorEnabled)
                {
                    log.Error(string.Format(format, args), ex);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void IfError(this ILog log, object message, Exception ex)
        {
            try
            {
                if (log.IsErrorEnabled)
                {
                    log.Error(message, ex);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void AddMatchingStringAppender(ILogger root, string filePath, string name, string stringToMatch,
                Level level)
        {
            IAppender containsAppender = ((Logger)root).GetAppender(name);

            if (containsAppender == null)
            {
                Logger l = (Logger)root;
                RollingFileAppender fileAppender = new RollingFileAppender();
                fileAppender.RollingStyle = RollingFileAppender.RollingMode.Date;
                fileAppender.DatePattern = "yyyyMMdd";
                fileAppender.Name = name;
                fileAppender.File = string.Format("{0}\\{1}.log", filePath, name);
                fileAppender.AppendToFile = true;
                fileAppender.LockingModel = new FileAppender.MinimalLock();
                fileAppender.ImmediateFlush = true;
                PatternLayout layout = new PatternLayout();
                layout.ConversionPattern = "%d [%t] %-5p %m%n";
                layout.ActivateOptions();
                fileAppender.Layout = layout;
                fileAppender.Threshold = level;
                StringMatchFilter filter = new StringMatchFilter();
                filter.StringToMatch = stringToMatch;
                filter.AcceptOnMatch = true;
                fileAppender.AddFilter(filter);
                DenyAllFilter deny = new DenyAllFilter();
                fileAppender.AddFilter(deny);
                fileAppender.ActivateOptions();
                l.AddAppender(fileAppender);
            }
        }

        public static void RemoveAppender(ILogger rootLogger, string name)
        {
            Logger root = (Logger)rootLogger;
            if (root != null)
            {
                IAppender appender = root.GetAppender(name);
                if (appender != null)
                {
                    appender.Close();
                    root.RemoveAppender(appender);
                }
            }
        }
    }
}