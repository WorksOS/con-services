using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using VSS.SeriLog.Extensions;
using VSS.WebApi.Common;

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
                      .ConfigureLogging(x => SerilogExtensions.Configure(config, "VSS.TagFileAuth.WebAPI.log"))
                      .UseSerilog()
                      .Build();
      });

      host.Run();
    }
  }
}
