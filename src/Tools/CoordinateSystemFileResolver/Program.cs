using System;
using CoordinateSystemFileResolver.Interfaces;
using CoordinateSystemFileResolver.Types;
using CoordinateSystemFileResolver.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;

namespace CoordinateSystemFileResolver
{
  internal class Program
  {
    private static void Main()
    {
      var serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);

      var serviceProvider = serviceCollection.BuildServiceProvider();
      var resolver = serviceProvider.GetRequiredService<IResolver>();

      resolver.ResolveCSIB(Guid.Parse("d069f073-993b-497c-8a34-c8ef67b1a32a"), Guid.Parse("87bdf851-44c5-e311-aa77-00505688274d"))
              .GetCoordSysInfoFromCSIB64();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
      var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, false)
                                             .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT_NAME")}.json", true, false)
                                             .Build();

      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("CoordinateSystemFileResolver.log"));

      services.AddLogging()
              .AddSingleton(loggerFactory);

      services.AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>(_ => new GenericConfiguration(loggerFactory, config));
      services.AddScoped<IErrorCodesProvider, ErrorCodesProvider>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddSingleton<IEnvironmentHelper, EnvironmentHelper>();
      services.AddSingleton<IRestClient, RestClient>();
      services.AddSingleton<IResolver, Resolver>();
      services.AddTransient<ITPaasProxy, TPaasProxy>();
      services.AddSingleton<ICSIBAgent, CSIBAgent>();
    }
  }
}
