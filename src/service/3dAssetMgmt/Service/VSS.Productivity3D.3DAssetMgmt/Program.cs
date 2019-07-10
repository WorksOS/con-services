using Microsoft.AspNetCore.Hosting;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.AssetMgmt3D
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
