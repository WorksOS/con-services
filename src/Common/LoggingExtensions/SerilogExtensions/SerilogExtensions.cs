using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using VSS.Serilog.Extensions.Enrichers;

namespace VSS.Serilog.Extensions
{
  public class SerilogExtensions
  {
    /// <summary>
    /// Create and setup Serilog configuration, including custom enrichers and filters.
    /// </summary>
    public static Logger Configure(IConfigurationRoot config, string logFilename)
    {
      // TODO Why is Level:u5 not uppercasing the string?
      const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{ThreadId}] {Level:u5} [{SourceContext}] {Message:j}{NewLine}{EscapedException}";

      var logger = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .MinimumLevel.Override("System", LogEventLevel.Information)
                   .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                   .Enrich.FromLogContext()
                   .Enrich.WithThreadId()
                   .Enrich.With<ExceptionEnricher>()
                   .WriteTo.Console(
                     outputTemplate: outputTemplate,
                     theme: AnsiConsoleTheme.Code)
                   .ReadFrom.Configuration(config);

      // If we start deploying Release configurations then the following options could be compiled out during development.
      logger.WriteTo.File(
        $"./logs/{logFilename}",
        outputTemplate: outputTemplate);

      return logger.CreateLogger();
    }
  }
}
