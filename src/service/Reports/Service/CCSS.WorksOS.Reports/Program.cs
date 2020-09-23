using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using VSS.WebApi.Common;

namespace CCSS.WorksOS.Reports
{
  public static class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args)
        .Build()
        .Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
      return Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
          webBuilder
            //.UseKestrel()
            .UseLibuv(opts => { opts.ThreadCount = 32; })
            .BuildKestrelWebHost()
            .UseStartup<Startup>();
        });
    }
  }
}
