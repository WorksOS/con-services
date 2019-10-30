using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TagFiles;
using TagFiles.Interface;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.Serilog.Extensions;

namespace MegalodonSvc
{
  class Program
  {
    public static async Task Main(string[] args)
    {
      Console.WriteLine("Megalodon Service");
      await CreateHostBuilder(args).Build().RunAsync();
    }


    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureAppConfiguration((context, config) =>
            {
              config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
              // Configure the app here.
            })
            .ConfigureServices((hostContext, services) =>
            {
              services.AddHostedService<MegalodonService>();
              services.AddSingleton<IConfigurationStore, GenericConfiguration>();
              services.AddSingleton<ISocketManager, SocketManager>();
            })
              .ConfigureLogging((hostContext, loggingBuilder) =>
              {

                var generalConfig = new ConfigurationBuilder()
                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                  .Build();

                loggingBuilder.AddProvider(
                  p => new SerilogLoggerProvider(
                    SerilogExtensions.Configure(config: generalConfig, httpContextAccessor: p.GetService<IHttpContextAccessor>())));
              });

  }
}
