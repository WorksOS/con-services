using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Reflection;
using Utilities.Logging.Models;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Utilities.Logging
{
	public class LoggingService : ILoggingService
    {
        private readonly LogRequestContext _logContext;
        private readonly string _applicationName;
        private readonly string _applicationVersion;
		private static ILoggerFactory _loggerFactory;
		private ILogger _logger;
		private string _assemblyDetails;

		static LoggingService()
		{
			_loggerFactory = new LoggerFactory();
			//var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
			var configuration = new ConfigurationBuilder().AddJsonFile("log.json").Build();
			var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configuration);
			_loggerFactory.AddProvider(new SerilogLoggerProvider(loggerConfiguration.CreateLogger()));
			//_loggerFactory.AddProvider(new SerilogLoggerProvider(loggerConfiguration.));
		}

		public LoggingService(LogRequestContext logContext)
        {
			_logContext = logContext;
			this._assemblyDetails = Assembly.GetExecutingAssembly().GetName().FullName;
		}

		public void CreateLogger(Type type)
        {
            this._logger = _loggerFactory.CreateLogger(type);
        }

        private string GetLogMessage(string message, string classMethod, string stackTrace = null)
        {
            return JsonConvert.SerializeObject(new LoggerContext(this._applicationName, this._applicationVersion)
            {
                CorrelationId = this._logContext.CorrelationId,
                TraceId = this._logContext.TraceId,
                Message = message,
                ClassMethod = classMethod,
                AssemblyDetails = _assemblyDetails,
                StackTrace = stackTrace
            }, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }


        public void Debug(string message, string classMethod)
        {
            if (this._logger.IsEnabled(LogLevel.Debug))
            {
                this._logger.LogDebug(this.GetLogMessage(message, classMethod));
            }
        }

        public void Error(string message, string classMethod,Exception ex)
        {
            if (this._logger.IsEnabled(LogLevel.Error))
			{
                this._logger.LogError(this.GetLogMessage(message, classMethod , string.Format("{0} : {1}",ex.Message, ex.StackTrace)));
            }
        }

		public void Error<T>(string message, string classMethod, Exception ex, T errors)
		{
			if (this._logger.IsEnabled(LogLevel.Error))
			{
				this._logger.LogError(this.GetLogMessage(message, classMethod, string.Format("{0}\r\n\r\n{1} : {2}", JsonConvert.SerializeObject(errors), ex.Message, ex.StackTrace)));
			}
		}

		public void Fatal(string message, string classMethod)
        {
            if (this._logger.IsEnabled(LogLevel.Critical))
			{
                this._logger.LogCritical(this.GetLogMessage(message, classMethod));
            }
        }

        public void Info(string message, string classMethod)
        {
            if (this._logger.IsEnabled(LogLevel.Information))
			{
                this._logger.LogInformation(this.GetLogMessage(message, classMethod));
            }
        }

        public void Warn(string message, string classMethod)
        {
            if (this._logger.IsEnabled(LogLevel.Warning))
			{
                this._logger.LogWarning(this.GetLogMessage(message, classMethod));
            }
        }

		public void CreateLogger<T>()
		{
			this._logger = _loggerFactory.CreateLogger<T>();
		}
	}
}
