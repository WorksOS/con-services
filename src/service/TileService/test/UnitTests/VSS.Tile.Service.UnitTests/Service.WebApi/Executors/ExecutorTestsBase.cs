using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;

namespace VSS.Tile.Service.UnitTests.Service.WebApi.Executors
{
  public class ExecutorTestsBase
  {
    public static IServiceProvider ServiceProvider;

    protected IHeaderDictionary customHeaders = new HeaderDictionary();
    protected IConfigurationStore Config;
    protected ILoggerFactory Logger;

    public ExecutorTestsBase()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Project.UnitTests.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>()
        .AddSingleton<ITPaaSApplicationAuthentication, TPaaSApplicationAuthentication>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
      Config = ServiceProvider.GetRequiredService<IConfigurationStore>();
      Logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
    }
  }
}
