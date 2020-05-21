using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Project.Repository;
using VSS.Serilog.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace VSS.MasterData.ProjectTests
{
  public class UnitTestsDIFixture<T> : IDisposable
  {
    public IServiceProvider ServiceProvider;
    protected IServiceExceptionHandler ServiceExceptionHandler;
    public ILogger Log;
    protected IServiceCollection ServiceCollection;

    public UnitTestsDIFixture()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();

      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Project.UnitTests.log"));
      ServiceCollection = new ServiceCollection();

      ServiceCollection.AddLogging();
      ServiceCollection.AddSingleton(loggerFactory);
      ServiceCollection
        .AddTransient<IProjectRepository, ProjectRepository>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IProductivity3dV1ProxyCoord, Productivity3dV1ProxyCoord>()
        .AddTransient<IProductivity3dV2ProxyNotification, Productivity3dV2ProxyNotification>()
        .AddTransient<IProductivity3dV2ProxyCompaction, Productivity3dV2ProxyCompaction>()
        .AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>();

      ServiceProvider = ServiceCollection.BuildServiceProvider();
      ServiceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      Log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<T>();
    }

    public void Dispose()
    { }
  }
}
