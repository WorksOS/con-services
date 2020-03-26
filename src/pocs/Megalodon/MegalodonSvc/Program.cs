using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TagFiles;
using TagFiles.Interface;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Cache.MemoryCache;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;
using TagFiles.Common;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.TRex.Gateway.Common.Proxy;

/// <summary>
/// Runs as a Windows service converting packet data from TMC software into Trimble tagfiles.
/// </summary>
namespace MegalodonSvc
{
  class Program
  {
    public static async Task Main(string[] args)
    {
      Console.WriteLine($"MegalodonSvc Main. App:{TagConstants.APP_NAME}");
      await CreateHostBuilder(args).Build().RunAsync();
    }


    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureAppConfiguration((context, config) =>
            {
              // Configure the app here.
              config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, services) => { services.AddHostedService<MegalodonService>()
                                                                    .AddHostedService<TagFileDispatchSvc>()
                                                                    .AddSingleton<IConfigurationStore, GenericConfiguration>()
                                                                    .AddSingleton<ISocketManager, SocketManager>()
                                                                    .AddTransient<IProductivity3dV2ProxyCompaction, Productivity3dV2ProxyCompaction>()
                                                                    .AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>()
                                                                    .AddSingleton<ITPaasProxy,TPaasProxy>()
                                                                    .AddSingleton<IWebRequest, GracefulWebRequest>()
                                                                    .AddSingleton<IDataCache, InMemoryDataCache>()
                                                                    .AddSingleton<IMemoryCache,MemoryCache>()
                                                                    .AddSingleton<ITRexTagFileProxy,TRexTagFileV2Proxy>()
                                                                    .AddServiceDiscovery(); 

            })
            .ConfigureLogging((hostContext, loggingBuilder) =>
            {

              var generalConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
              loggingBuilder.ClearProviders();
              loggingBuilder.AddProvider(
                p => new SerilogLoggerProvider(
                  SerilogExtensions.Configure(config: generalConfig, httpContextAccessor: p.GetService<IHttpContextAccessor>())));
            });

  }
}
