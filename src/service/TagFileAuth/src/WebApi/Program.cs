using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Extensions.Logging;

namespace VSS.Productivity3D.TagFileAuth.WebAPI
{
  /// <summary>
  /// 
  /// </summary>
  public class Program
  {
    /// <summary>
    /// Application entry point.
    /// </summary>
    public static void Main(string[] args)
    {
      var config = new ConfigurationBuilder()
                   .AddCommandLine(args)
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                   .AddJsonFile("kestrelsettings.json", optional: true, reloadOnChange: false)
                   .AddJsonFile("serilog.json", optional: true, reloadOnChange: true)
                   .Build();

      var host = new WebHostBuilder().BuildHostWithReflectionException(builder =>
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
                        loggingBuilder.AddProvider(p =>
                          new SerilogLoggerProvider(
                            SerilogExtensions.Configure(config, "VSS.TagFileAuth.WebAPI.log", p.GetService<IHttpContextAccessor>())));
                      })
                      .Build();
      });

      host.Run();
    }
  }
}
