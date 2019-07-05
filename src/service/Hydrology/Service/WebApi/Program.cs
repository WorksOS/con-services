using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Extensions.Logging;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;

namespace VSS.Hydrology.WebApi
{
  /// <summary>
  /// 
  /// </summary>
  public class Program
  {

    /// <summary>
    /// Default program entry point.
    /// </summary>
    public static void Main(string[] args)
    {
      var config = new ConfigurationBuilder()
        .AddCommandLine(args)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddJsonFile("kestrelsettings.json", optional: true, reloadOnChange: false)
        .AddJsonFile("serilog.json", optional: true, reloadOnChange: true)
        .Build();
      
      new WebHostBuilder().BuildHostWithReflectionException(builder =>
      {
        return builder.UseConfiguration(config)
          .UseKestrel()
          .UseLibuv(opts =>
          {
            opts.ThreadCount = 32;
          })
          .UseStartup<Startup>()
          .ConfigureLogging((hostContext, loggingBuilder) =>
          {
            loggingBuilder.AddProvider(
              p => new SerilogLoggerProvider(
                SerilogExtensions.Configure("VSS.Hydrology.WebAPI.log", config, p.GetService<IHttpContextAccessor>())));
          })
          .Build();
      }).Run();
    }
  }
}
