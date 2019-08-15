using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
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
    public string KafkaTopicName;
    
    protected IErrorCodesProvider ProjectErrorCodesProvider;
    protected IServiceExceptionHandler ServiceExceptionHandler;
    public ILogger Log;

    public UnitTestsDIFixture()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();

      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Project.UnitTests.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddTransient<IProjectRepository, ProjectRepository>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IProductivity3dProxy, Productivity3dProxy>()
        .AddSingleton<IKafka, RdKafkaDriver>()
        .AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
      KafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                       ServiceProvider.GetRequiredService<IConfigurationStore>().GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      ProjectErrorCodesProvider = ServiceProvider.GetRequiredService<IErrorCodesProvider>();
      ServiceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      
      Log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<T>();
    }

    public void Dispose()
    { }
  }
}
