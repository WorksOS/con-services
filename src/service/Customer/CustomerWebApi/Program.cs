using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using VSS.Serilog.Extensions;
using System.IO;

namespace CustomerWebApi
{
  public class Program
  {
    public static void Main()
    {
      var host = new WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseLibuv(opts => opts.ThreadCount = 32)
        .UseStartup<Startup>()
        .ConfigureLogging((hostContext, loggingBuilder) =>
        {
          loggingBuilder.AddProvider(
            p => new SerilogLoggerProvider(
              SerilogExtensions.Configure("CCSS.Customer.log", httpContextAccessor: p.GetService<IHttpContextAccessor>())));
        })
        .Build();

      host.Run();
    }
  }
}
