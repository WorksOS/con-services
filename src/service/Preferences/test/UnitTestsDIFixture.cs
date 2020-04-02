using System;
using CCSS.Productivity3D.Preferences.Abstractions.Interfaces;
using CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling;
using CCSS.Productivity3D.Preferences.Repository;
using CSS.Productivity3D.Preferences.Common.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Serilog.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace CCSS.Productivity3D.Preferences.Tests
{
  public class UnitTestsDIFixture<T> : IDisposable
  {
    public IServiceProvider ServiceProvider;
    protected IServiceExceptionHandler ServiceExceptionHandler;
    public ILogger Log;
    protected Mock<IPreferenceRepository> mockPrefRepo = new Mock<IPreferenceRepository>();

    public UnitTestsDIFixture()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();

      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("CCSS.Productivity3D.Preferences.UnitTests.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddSingleton(mockPrefRepo.Object)
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, PreferenceErrorCodesProvider>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
      ServiceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      Log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<T>();
    }

    public void Dispose()
    { }
  }
}
