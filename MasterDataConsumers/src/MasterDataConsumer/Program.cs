using System.Threading;
using System.Threading.Tasks;
using KafkaConsumer;
using Microsoft.Extensions.DependencyInjection;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using Microsoft.Extensions.Logging;
using log4netExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.GenericConfiguration;
using KafkaConsumer.Interfaces;
using KafkaConsumer.Kafka;
using Repositories;
#if NET_4_7
using Topshelf;
#endif

namespace MasterDataConsumer
{
  public class Program
  {
    public static void Main(string[] args)
    {
      IServiceProvider serviceProvider;
      ILogger log;
      Dictionary<string, Type> serviceConverter;
      string[] kafkaTopics;

      serviceConverter = new Dictionary<string, Type>()
      {
        {"IAssetEvent", typeof(IKafkaConsumer<IAssetEvent>)},
        {"ICustomerEvent", typeof(IKafkaConsumer<ICustomerEvent>)},
        {"IDeviceEvent", typeof(IKafkaConsumer<IDeviceEvent>)},
        {"IProjectEvent", typeof(IKafkaConsumer<IProjectEvent>)},
        {"ISubscriptionEvent", typeof(IKafkaConsumer<ISubscriptionEvent>)},
        {"IGeofenceEvent", typeof(IKafkaConsumer<IGeofenceEvent>)},
      };


      var serviceCollection = new ServiceCollection()
        .AddTransient<IKafka, RdKafkaDriver>()
        .AddTransient<IMessageTypeResolver, MessageResolver>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddLogging()
        .AddTransient<IRepositoryFactory, RepositoryFactory>()

        .AddTransient<IKafkaConsumer<IAssetEvent>, KafkaConsumer<IAssetEvent>>()
        .AddTransient<IKafkaConsumer<ICustomerEvent>, KafkaConsumer<ICustomerEvent>>()
        .AddTransient<IKafkaConsumer<IDeviceEvent>, KafkaConsumer<IDeviceEvent>>()
        .AddTransient<IKafkaConsumer<IGeofenceEvent>, KafkaConsumer<IGeofenceEvent>>()
        .AddTransient<IKafkaConsumer<IProjectEvent>, KafkaConsumer<IProjectEvent>>()
        .AddTransient<IKafkaConsumer<ISubscriptionEvent>, KafkaConsumer<ISubscriptionEvent>>()

        .AddTransient<IRepository<IAssetEvent>, AssetRepository>()
        .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
        .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
        .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
        .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
        .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>();

      // catch-22 here. I want to use the GenericConfig to get the kafkaTopics
      //     I can't use it until it is in DI (as it needs a logger)
      serviceProvider = serviceCollection
        .BuildServiceProvider();

      kafkaTopics =
        serviceProvider.GetService<IConfigurationStore>()
          .GetValueString("KAFKA_TOPICS")
          .Split(new[] {","}, StringSplitOptions.None);
      string loggerRepoName = "MDC " + kafkaTopics[0].Split('.').Last();

      var logPath = System.IO.Directory.GetCurrentDirectory();
      Console.WriteLine("Log path:" + logPath);
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4net.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceProvider = serviceCollection.BuildServiceProvider();

      log = loggerFactory.CreateLogger(loggerRepoName);

#if NET_4_7
      HostFactory.Run(x =>
      {
        x.Service<ConsumerContainer>(s =>
        {
          s.ConstructUsing(name => new ConsumerContainer(serviceConverter, log, kafkaTopics, serviceProvider));
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
            var consumer = new ConsumerContainer(serviceConverter, log, kafkaTopics, serviceProvider);
            consumer.Initialize();
#endif
    }
  }

  public class ConsumerContainer
  {

    ILogger log;
    Dictionary<string, Type> serviceConverter;
    string[] kafkaTopics;
    private IServiceProvider serviceProvider;

    private List<Task> tasks;
    private CancellationTokenSource token;
    private List<IAbstractKafkaConsumer> consumers;

    public ConsumerContainer(Dictionary<string, Type> converter, ILogger logger, string[] topics,
      IServiceProvider services)
    {
      log = logger;
      serviceConverter = converter;
      kafkaTopics = topics;
      serviceProvider = services;
      tasks = new List<Task>();

      token = new CancellationTokenSource();
      consumers = new List<IAbstractKafkaConsumer>();
      Initialize();
    }

    public void StopAndDispose()
    {
      log.LogInformation("MasterDataConsumer: Stopping all consumers.");
      consumers.ForEach(c => c.StopProcessing());
      log.LogInformation("MasterDataConsumer: Cancelling all consumers.");
      token.Cancel();
    }

    public void Initialize()
    {
      log.LogDebug("MasterDataConsumer is starting....");

      foreach (var kafkaTopic in kafkaTopics)
      {
        log.LogInformation("MasterDataConsumer topic: " + kafkaTopic);
        if (serviceConverter.Any(s => kafkaTopic.Contains(s.Key)))
        {
          var consumer =
            serviceProvider.GetService(serviceConverter.First(s => kafkaTopic.Contains(s.Key)).Value) as
              IAbstractKafkaConsumer;
          consumer.SetTopic(kafkaTopic);
          consumers.Add(consumer);
#if NET_4_7
          var taskThread = new Thread(() =>
          {
            consumer.StartProcessingAsync(token);
          });
          taskThread.Start();
#else
          tasks.Add(consumer.StartProcessingAsync(token));
#endif
        }
        else
        {
          log.LogDebug("MasterDataConsumer: Kafka topic consumer not recognized: {0}", kafkaTopic);
          continue;
        }
        log.LogDebug("MasterDataConsumer: Kafka topic consumer to be started: {0}", kafkaTopic);
      }

#if !NET_4_7
      if (tasks.Count > 0)
                Task.WaitAll(tasks.ToArray());
            else
                log.LogCritical("MasterDataConsumer: No consumers started.");
#endif
    }
  }
}
