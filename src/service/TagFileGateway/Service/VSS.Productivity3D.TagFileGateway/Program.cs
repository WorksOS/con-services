using Microsoft.AspNetCore.Hosting;
using VSS.Productivity3D.TagFileForwarder;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.TagFileGateway
{
  /// <summary>
  /// VSS.Productivity3D.TagFileForwarder program.
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
