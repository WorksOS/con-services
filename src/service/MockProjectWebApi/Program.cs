using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using VSS.WebApi.Common;

namespace MockProjectWebApi
{
  public static class Program
  {
    public static void Main(string[] args)
    {
      CreateWebHostBuilder(args)
        .Build()
        .Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
      .UseLibuv(opts => opts.ThreadCount = 32)
      .BuildKestrelWebHost()
      .UseStartup<Startup>()
      .UseUrls("http://0.0.0.0:5001");
  }
}
