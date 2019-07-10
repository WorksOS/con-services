using Microsoft.AspNetCore.Hosting;
using VSS.WebApi.Common;

namespace LandfillService.WebApi.netcore
{
  public class Program
  {
    public static void Main()
    {
      var host = new WebHostBuilder().BuildHostWithReflectionException(builder =>
      {
        return builder.UseKestrel()
          .UseLibuv(opts => { opts.ThreadCount = 32; })
          .BuildKestrelWebHost()
          .UseStartup<Startup>()
          .Build();
      });

      host.Run();
    }
  }
}
