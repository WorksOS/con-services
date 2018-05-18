using log4net;
using log4net.Core;
using log4net.Repository;
using log4net.Util;
using System;
using Apache.Ignite.Core.Log;

namespace VSS.TRex.Logging
{
    /// <summary>
    /// Provides an ILogger implementation that wraps a dependency injected ILog interface to allow the Ignite layer
    /// to log into the standard logging location from the Java and .Net layers.
    /// </summary>
    public class TRexIgniteLogger : Apache.Ignite.Core.Log.ILogger
    {
        /// Wrapped log4net log.
        private readonly ILog _log;

        /// <summary>
        /// Initializes a new instance of the TRexIgniteLogger class.
        /// </summary>
        public TRexIgniteLogger() : this(LogManager.GetLogger(typeof(TRexIgniteLogger)))
        { }

        /// <summary>
        /// Initialises a new instance of the TRexIgniteLogger class with the provided ILog interface
        /// </summary>
        /// <param name="log"></param>
        public TRexIgniteLogger(ILog log)
        {
            this._log = log;
        }

        /// <summary>Logs the specified message.</summary>
        /// <param name="level">The level.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments to format <paramref name="message" />.
        /// Can be null (formatting will not occur).</param>
        /// <param name="formatProvider">The format provider. Can be null if <paramref name="args" /> is null.</param>
        /// <param name="category">The logging category name.</param>
        /// <param name="nativeErrorInfo">The native error information.</param>
        /// <param name="ex">The exception. Can be null.</param>
        public void Log(LogLevel level, string message, object[] args, IFormatProvider formatProvider, string category,
            string nativeErrorInfo, Exception ex)
        {
            Level level1 = TRexIgniteLogger.ConvertLogLevel(level);
            ILoggerRepository repository = _log.Logger.Repository;

            object obj;
            if (args == null)
                obj = message;
            else
                obj = new SystemStringFormat(formatProvider, message, args);

            LoggingEvent logEvent = new LoggingEvent(GetType(), repository, category, level1, obj, ex);

            if (nativeErrorInfo != null)
                logEvent.Properties[nameof(nativeErrorInfo)] = nativeErrorInfo;

            _log.Logger.Log(logEvent);
        }

        /// <summary>
        /// Determines whether the specified log level is enabled.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>
        /// Value indicating whether the specified log level is enabled
        /// </returns>
        public bool IsEnabled(Apache.Ignite.Core.Log.LogLevel level)
        {
            return this._log.Logger.IsEnabledFor(TRexIgniteLogger.ConvertLogLevel(level));
        }

        /// <summary>
        /// Converts the Ignite LogLevel to the log4net log level.
        /// </summary>
        /// <param name="level">The Ignite log level.</param>
        /// <returns>Corresponding log4net log level.</returns>
        public static Level ConvertLogLevel(Apache.Ignite.Core.Log.LogLevel level)
        {
            switch (level)
            {
                case Apache.Ignite.Core.Log.LogLevel.Trace:
                    return Level.Trace;
                case Apache.Ignite.Core.Log.LogLevel.Debug:
                    return Level.Debug;
                case Apache.Ignite.Core.Log.LogLevel.Info:
                    return Level.Info;
                case Apache.Ignite.Core.Log.LogLevel.Warn:
                    return Level.Warn;
                case Apache.Ignite.Core.Log.LogLevel.Error:
                    return Level.Error;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), (object)level, (string)null);
            }
        }
    }
}

