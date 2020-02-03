using Microsoft.AspNetCore.Hosting;
using VSS.WebApi.Common;

namespace CCSS.TagFileSplitter.WebAPI
{
  /// <summary>  </summary>
  public class Program
  {
    /// <summary>
    /// Application entry point.
    /// </summary>
    public static void Main(string[] args)
    {
      new WebHostBuilder().BuildHostWithReflectionException(builder =>
      {
        return builder.UseKestrel()
          .UseLibuv(opts => { opts.ThreadCount = 32; })
          .BuildKestrelWebHost()
          .UseStartup<Startup>()
          .Build();
      }).Run();
    }
  }
}
