using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.Hydrology.WebApi.Abstractions.ResultsHandling;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Serilog.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace VSS.Hydrology.Tests
{
  public class UnitTestsDIFixture<T> : IDisposable
  {
    public IServiceProvider ServiceProvider;

    protected IErrorCodesProvider HydroErrorCodesProvider;
    protected IServiceExceptionHandler ServiceExceptionHandler;
    public ILogger Log;

    public UnitTestsDIFixture()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Project.UnitTests.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, HydroErrorCodesProvider>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
      HydroErrorCodesProvider = ServiceProvider.GetRequiredService<IErrorCodesProvider>();
      ServiceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      Log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<T>();
    }

    public void Dispose()
    {
    }
  }
}

