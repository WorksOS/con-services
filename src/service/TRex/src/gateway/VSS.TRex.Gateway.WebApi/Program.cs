using Microsoft.AspNetCore.Hosting;
using VSS.TRex.Gateway.Common.Converters;
using VSS.WebApi.Common;

namespace VSS.TRex.Gateway.WebApi
{
  public class Program
  {
    public static void Main()
    {
      BuildWebHost().Run();
    }

    public static IWebHost BuildWebHost()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();

      return new WebHostBuilder().BuildHostWithReflectionException(builder =>
      {
        return builder.UseKestrel()
          .UseLibuv(opts => { opts.ThreadCount = 32; })
          .BuildKestrelWebHost()
          .UseStartup<Startup>()
          .Build();
      });
    }
  }
}
