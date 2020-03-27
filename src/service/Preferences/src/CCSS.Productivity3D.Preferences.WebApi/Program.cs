using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Extensions.Logging;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;

namespace CCSS.Productivity3D.Preferences.WebApi
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var config = new ConfigurationBuilder()
                   .AddCommandLine(args)
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                   .AddJsonFile("kestrelsettings.json", optional: true, reloadOnChange: false)
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
                            SerilogExtensions.Configure("CCSS.Productivity3D.Preferences.WebAPI.log", config, p.GetService<IHttpContextAccessor>())));
                      })
                      .Build();
      }).Run();
    }
  }
}
