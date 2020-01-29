using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Extensions.Logging;
using VSS.Serilog.Extensions;
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
