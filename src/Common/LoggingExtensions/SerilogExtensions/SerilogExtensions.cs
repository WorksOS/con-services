using System;
using System.IO;
using Microsoft.AspNetCore.Http;
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
    /// <param name="logFilename">The target log filename.</param>
    /// <param name="config">Optional configuration overrides.</param>
    /// <param name="httpContextAccessor">Used for the <see cref="HttpContextEnricher"/> to log the interservice RequestID.</param>
    /// <returns>Returns the Serilog.Core.Logger instance.</returns>
    public static Logger Configure(string logFilename = null, IConfigurationRoot config = null, IHttpContextAccessor httpContextAccessor = null)
    {
      const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{ThreadId}] {Level:u3} [{SourceContext}]{RequestID} {Message} {EscapedException}{NewLine}";

      var logger = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .MinimumLevel.Override("System", LogEventLevel.Information)
                   .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                   .MinimumLevel.Override("OpenTracing", LogEventLevel.Warning)
                   .Enrich.FromLogContext()
                   .Enrich.WithThreadId()
                   .Enrich.With<ExceptionEnricher>()
                   .Enrich.With(new HttpContextEnricher(httpContextAccessor))
                   .WriteTo.Console(
                     LogEventLevel.Debug,
                     outputTemplate: outputTemplate,
                     theme: AnsiConsoleTheme.Code);

      // If we start deploying Release configurations then the following options could be compiled out during development.
      logger.WriteTo.File(
        Path.Combine(Directory.GetCurrentDirectory(), $"logs/{logFilename}"),
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{ThreadId}] {Level:u3} [{SourceContext}]{RequestID} {Message}{NewLine}{Exception}",
        shared: true);

      if (config == null)
      {
        const string configFilename = "appsettings.json";

        try
        {
          string basePath;

          if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), configFilename)))
          {
            basePath = Directory.GetCurrentDirectory();
          }
          else if (File.Exists(Path.Combine(AppContext.BaseDirectory, configFilename)))
          {
            // Likely if the application assembly is executed via a script from a different location.
            basePath = Path.Combine(AppContext.BaseDirectory);
          }
          else
          {
            throw new FileNotFoundException();
          }

          config = new ConfigurationBuilder()
                   .SetBasePath(basePath)
                   .AddJsonFile(path: configFilename)
                   .Build();
        }
        catch (FileNotFoundException)
        {
          if (string.IsNullOrEmpty(logFilename))
          {
            throw new Exception($"Unable to resolve {configFilename} location; must provide either a valid logFilename or appsettings.json configuration file.");
          }
        }
      }

      if (config != null)
      {
        logger.ReadFrom.Configuration(config);
      }

      return logger.CreateLogger();
    }
  }
}
