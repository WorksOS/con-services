using System;
using System.Collections.Generic;
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
      const string outputTemplateConsole = "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{ThreadId}] {Level:u3} [{SourceContext}]{RequestID} {Message} {EscapedException}{NewLine}";

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
                     outputTemplate: outputTemplateConsole,
                     theme: AnsiConsoleTheme.Code);

      // Setup WriteTo:File, merging global configuration settings with the following default WriteTo values.
      // Microsoft ConfigurationBuilder applies a latest wins strategy.
      const string outputTemplateFile = "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{ThreadId}] {Level:u3} [{SourceContext}]{RequestID} {Message}{NewLine}{Exception}";
      var defaultWriteToFileConfig = new Dictionary<string, string>
      {
        { "Serilog:WriteTo:0:Name", "File" },
        { "Serilog:WriteTo:0:Args:path", $@".\logs\{logFilename}" },
        { "Serilog:WriteTo:0:Args:outputTemplate", outputTemplateFile },
        { "Serilog:WriteTo:0:Args:shared", "true" }
      };

      var defaultConfig = new ConfigurationBuilder()
                          .SetBasePath(Directory.GetCurrentDirectory())
                          .AddInMemoryCollection(initialData: defaultWriteToFileConfig);

      if (config == null)
      {
        TryLoadAppSettingsJson(defaultConfig, logFilename);
      }
      else
      {
        defaultConfig.AddConfiguration(config);
      }

      logger.ReadFrom.Configuration(defaultConfig.Build());

      return logger.CreateLogger();
    }

    /// <summary>
    /// Locates appsettings.json and loads its Serilog settings into the provided IConfigurationBuilder
    /// </summary>
    /// <remarks>
    /// This is a backup mechanism in-case the application hasn't provided it's own ConfigurationBuilder but
    /// there is an appsettings.json present.
    /// </remarks>
    /// <param name="defaultConfig">The log configuration to add appsettings.json settings too.</param>
    private static void TryLoadAppSettingsJson(IConfigurationBuilder defaultConfig, string logFilename)
    {
      const string configFilename = "appsettings.json";

      try
      {
        string basePath;

        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), configFilename)))
        {
          basePath = Directory.GetCurrentDirectory();
        }
        // e.g. if the application assembly is executed via a script from a different location to itself.
        else if (File.Exists(Path.Combine(AppContext.BaseDirectory, configFilename)))
        {
          basePath = Path.Combine(AppContext.BaseDirectory);
        }
        else
        {
          throw new FileNotFoundException();
        }

        defaultConfig.SetBasePath(basePath)
                     .AddJsonFile(path: configFilename);
      }
      catch (FileNotFoundException)
      {
        if (string.IsNullOrEmpty(logFilename))
        {
          throw new Exception($"Unable to resolve {configFilename} location; must provide either a valid logFilename or appsettings.json configuration file.");
        }
      }
    }
  }
}
