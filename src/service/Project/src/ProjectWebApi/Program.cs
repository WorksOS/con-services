using Microsoft.AspNetCore.Hosting;
using VSS.WebApi.Common;


namespace VSS.MasterData.Project.WebAPI
{
  /// <summary>
  /// Applicaiton entry point.
  /// </summary>
  public class Program
  {
    /// <summary>
    /// VSS.Productivity3D.Filter main
    /// </summary>
    public static void Main(string[] args)
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
