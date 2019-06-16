using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace VSS.Serilog.Extensions
{
  public class SerilogExtensions
  {
    public static void Configure(IConfigurationRoot config, string logFilename)
    {
      // https://github.com/serilog/serilog/wiki/Configuration-Basics
      const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss,fff} [{ThreadId}] {Level:u3} [{SourceContext}] {Message:lj}{NewLine}{Exception}";

      Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .MinimumLevel.Override("System", LogEventLevel.Information)
                   .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                   .Enrich.FromLogContext()
                   .Enrich.WithThreadId()
                   .WriteTo.Console(
                     outputTemplate: outputTemplate,
                     theme: AnsiConsoleTheme.Code)
                   .WriteTo.File(
                     $"./logs/{logFilename}",
                     outputTemplate: outputTemplate)
                   .ReadFrom.Configuration(config) // Provide an option to override from serilog.json, if present.
                   .CreateLogger();
    }
  }
}
