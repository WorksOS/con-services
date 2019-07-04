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
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Project.Repository;
using VSS.Serilog.Extensions;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class ExecutorBaseTests : IDisposable
  {
    public IServiceProvider ServiceProvider;
    public string KafkaTopicName;
    protected IErrorCodesProvider ProjectErrorCodesProvider;
    protected IServiceExceptionHandler ServiceExceptionHandler;

    public ExecutorBaseTests()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();

      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Project.WebApi.log", null));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddTransient<IProjectRepository, ProjectRepository>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IRaptorProxy, RaptorProxy>()
        .AddSingleton<IKafka, RdKafkaDriver>()
        .AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
      KafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                       ServiceProvider.GetRequiredService<IConfigurationStore>().GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
      ProjectErrorCodesProvider = ServiceProvider.GetRequiredService<IErrorCodesProvider>();
      ServiceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
    }

    public void Dispose()
    { }
  }
}
