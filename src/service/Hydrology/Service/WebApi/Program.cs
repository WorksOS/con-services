using Microsoft.AspNetCore.Hosting;
using VSS.WebApi.Common;

namespace VSS.Hydrology.WebApi
{
  /// <summary>
  /// VSS.Hydrology program.
  /// </summary>
  public class Program
  {

    /// <summary>
    /// Application entry point.
    /// </summary>
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
