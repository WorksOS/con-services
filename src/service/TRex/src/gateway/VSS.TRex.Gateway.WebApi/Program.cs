using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;
using VSS.TRex.Gateway.Common.Converters;
using VSS.WebApi.Common;

namespace VSS.TRex.Gateway.WebApi
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var webHost = BuildWebHost(args);

      webHost.Run();
    }

    public static IWebHost BuildWebHost(string[] args)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();

      return new WebHostBuilder().BuildHostWithReflectionException(builder =>
      {
        return builder.UseKestrel()
          .UseLibuv(opts => { opts.ThreadCount = 32; })
          .BuildKestrelWebHost(Startup.LoggerRepoName)
          .UseStartup<Startup>()
          .Build();
      });

    }
  }
}

