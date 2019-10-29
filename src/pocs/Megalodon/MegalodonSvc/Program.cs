using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using TagFiles.Interface;

namespace MegalodonSvc
{
  class Program
  {
    public static async Task Main(string[] args)
    {
      Console.WriteLine("Hello World!");
      await CreateHostBuilder(args).Build().RunAsync();
    }



    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureAppConfiguration((context, config) =>
            {
              // Configure the app here.
            })
            .ConfigureServices((hostContext, services) =>
            {
              services.AddHostedService<MegalodonService>();
            })
              .ConfigureLogging((hostContext, loggingBuilder) =>
              {
                loggingBuilder.AddProvider(
                  p => new SerilogLoggerProvider(
                    SerilogExtensions.Configure(config: kestrelConfig, httpContextAccessor: p.GetService<IHttpContextAccessor>())));
              });

            // Only required if the service responds to requests.
        //    .ConfigureWebHostDefaults(webBuilder =>
          //  {
          //    webBuilder.UseStartup<Startup>();
          //  });
            
  }
}
