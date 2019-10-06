using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer;
using VSS.KafkaConsumer.Interfaces;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Project.Repository;
using VSS.Serilog.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using ILogger = Microsoft.Extensions.Logging.ILogger;

#if NET_4_7
using Topshelf;
#endif

namespace VSS.Productivity3D.MasterDataConsumer
{
  public class Program
  {
    public static void Main(string[] args)
    {
#if NET_4_7
      HostFactory.Run(x =>
      {
        x.Service<ConsumerContainer>(s =>
        {
          s.ConstructUsing(name => new ConsumerContainer());
          s.WhenStarted(tc => tc.Initialize());
          s.WhenStopped(tc => tc.StopAndDispose());
        });
        x.RunAsLocalSystem();

        x.SetDescription("Kafka messaging consumer, consumer whatever is configured. NET 4.7 port.");
        x.SetDisplayName("MessagesConsumerNet47");
        x.SetServiceName("MessagesConsumerNet47");
        x.EnableServiceRecovery(c =>
        {
          c.RestartService(1);
          c.OnCrashOnly();
        });
      });

#else
      var consumer = new ConsumerContainer();
      consumer.Initialize();
#endif
    }
  }

  public class ConsumerContainer
  {
    ILogger _log;

    private readonly List<Task> tasks;
    private readonly CancellationTokenSource token;
    private readonly List<IAbstractKafkaConsumer> consumers;

    public ConsumerContainer()
    {
      tasks = new List<Task>();

      token = new CancellationTokenSource();
      consumers = new List<IAbstractKafkaConsumer>();
    }

    public void StopAndDispose()
    {
      _log.LogInformation($"{nameof(StopAndDispose)}: Stopping all consumers.");
      consumers.ForEach(c => c.StopProcessing());
      _log.LogInformation($"{nameof(StopAndDispose)}: Cancelling all consumers.");
      token.Cancel();
    }

    public void Initialize()
    {
      var serviceConverter = new Dictionary<string, Type>()
      {
        {"IAssetEvent", typeof(IKafkaConsumer<IAssetEvent>)},
        {"ICustomerEvent", typeof(IKafkaConsumer<ICustomerEvent>)},
        {"IDeviceEvent", typeof(IKafkaConsumer<IDeviceEvent>)},
        {"IProjectEvent", typeof(IKafkaConsumer<IProjectEvent>)},
        {"ISubscriptionEvent", typeof(IKafkaConsumer<ISubscriptionEvent>)},
        {"IGeofenceEvent", typeof(IKafkaConsumer<IGeofenceEvent>)},
        {"IFilterEvent", typeof(IKafkaConsumer<IFilterEvent>)},
      };
      
      var serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure()))
        .AddTransient<IKafka, RdKafkaDriver>()
        .AddTransient<IMessageTypeResolver, MessageResolver>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IRepositoryFactory, RepositoryFactory>()

        .AddTransient<IKafkaConsumer<IAssetEvent>, KafkaConsumer<IAssetEvent>>()
        .AddTransient<IKafkaConsumer<ICustomerEvent>, KafkaConsumer<ICustomerEvent>>()
        .AddTransient<IKafkaConsumer<IDeviceEvent>, KafkaConsumer<IDeviceEvent>>()
        .AddTransient<IKafkaConsumer<IGeofenceEvent>, KafkaConsumer<IGeofenceEvent>>()
        .AddTransient<IKafkaConsumer<IProjectEvent>, KafkaConsumer<IProjectEvent>>()
        .AddTransient<IKafkaConsumer<ISubscriptionEvent>, KafkaConsumer<ISubscriptionEvent>>()
        .AddTransient<IKafkaConsumer<IFilterEvent>, KafkaConsumer<IFilterEvent>>()

        .AddTransient<IRepository<IAssetEvent>, AssetRepository>()
        .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
        .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
        .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
        .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>()
        .AddTransient<IRepository<IFilterEvent>, FilterRepository>()
        .BuildServiceProvider();

      _log = serviceProvider.GetService<ILoggerFactory>().CreateLogger(GetType());
      var configStore = serviceProvider.GetService<IConfigurationStore>();
      var kafkaTopics = configStore
        .GetValueString("KAFKA_TOPICS")
        .Split(new[] { "," }, StringSplitOptions.None);

      _log.LogDebug($"{nameof(Initialize)}: consumers are starting....");

      foreach (var kafkaTopic in kafkaTopics)
      {
        _log.LogInformation($"{nameof(Initialize)}: Starting consumer topic: {kafkaTopic}");
        if (serviceConverter.Any(s => kafkaTopic.Contains(s.Key)))
        {
          var consumer =
            serviceProvider.GetService(serviceConverter.First(s => kafkaTopic.Contains(s.Key)).Value) as
              IAbstractKafkaConsumer;
          consumer.SetTopic(kafkaTopic);
          consumers.Add(consumer);
          tasks.Add(consumer.StartProcessingAsync(token));
        }
        else
        {
          _log.LogDebug($"{nameof(Initialize)}: Consumer topic not recognized: {kafkaTopic}");
          continue;
        }

        _log.LogDebug($"{nameof(Initialize)}: Consumer to be started: {kafkaTopic}");
      }

#if !NET_4_7
      if (tasks.Count > 0)
        Task.WaitAll(tasks.ToArray());
      else
        _log.LogCritical($"{nameof(Initialize)}: No consumers started.");
#endif
    }
  }
}
