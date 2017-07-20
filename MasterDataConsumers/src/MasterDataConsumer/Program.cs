using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer;
using VSS.KafkaConsumer.Interfaces;
using VSS.KafkaConsumer.Kafka;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

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

    ILogger log;

    private List<Task> tasks;
    private CancellationTokenSource token;
    private List<IAbstractKafkaConsumer> consumers;

    public ConsumerContainer()
    {
      tasks = new List<Task>();

      token = new CancellationTokenSource();
      consumers = new List<IAbstractKafkaConsumer>();
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
      var serviceConverter = new Dictionary<string, Type>()
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
        .AddSingleton<IConfigurationStore, ConfigurationStore.GenericConfiguration>()
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
      var serviceProvider = serviceCollection
        .BuildServiceProvider();

      var kafkaTopics = serviceProvider.GetService<IConfigurationStore>()
        .GetValueString("KAFKA_TOPICS")
        .Split(new[] { "," }, StringSplitOptions.None);

      string loggerRepoName = "MDC " + kafkaTopics[0].Split('.').Last();
      string logPath = "";

      if (File.Exists(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "log4net.xml")))
      {
        logPath = System.IO.Directory.GetCurrentDirectory();
        Console.WriteLine($"Setting GetCurrentDirectory path for the config file {logPath}");
      }
      else if (File.Exists(Path.Combine(System.AppContext.BaseDirectory, "log4net.xml")))
      {
        logPath = Path.Combine(System.AppContext.BaseDirectory);
        Console.WriteLine($"Setting BaseDirectory path for the config file {logPath}");
      }
      else
      {
        var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
        logPath = Path.GetDirectoryName(pathToExe);
        Console.WriteLine($"Setting alternative path for the config file {logPath}");
      }

      Console.WriteLine("Log path:" + logPath);
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4net.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceProvider = serviceCollection.BuildServiceProvider();
      log = loggerFactory.CreateLogger(loggerRepoName);
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
          tasks.Add(consumer.StartProcessingAsync(token));
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
