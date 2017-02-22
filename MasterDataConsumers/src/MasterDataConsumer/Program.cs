using System.Threading;
using System.Threading.Tasks;
using KafkaConsumer;
using Microsoft.Extensions.DependencyInjection;
using VSS.Customer.Data;
using VSS.Project.Data;
using VSS.Project.Service.Interfaces;
using VSS.Project.Service.Repositories;
using VSS.Project.Service.Utils.Kafka;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.Geofence.Data;
using Microsoft.Extensions.Logging;
using log4netExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.GenericConfiguration;
using VSS.Masterdata;
using VSS.Asset.Data;
using VSS.Device.Data;

namespace MasterDataConsumer
{
  public class Program
  {

    public static void Main(string[] args)
    {



      Dictionary<string, Type> serviceConverter = new Dictionary<string, Type>()
            {
                {"IAssetEvent", typeof(IKafkaConsumer<IAssetEvent>)},
                {"ICustomerEvent", typeof(IKafkaConsumer<ICustomerEvent>)},
                {"IDeviceEvent", typeof(IKafkaConsumer<IDeviceEvent>)},
                {"IProjectEvent", typeof(IKafkaConsumer<IProjectEvent>)},
                {"ISubscriptionEvent", typeof(IKafkaConsumer<ISubscriptionEvent>)},
                {"IGeofenceEvent", typeof(IKafkaConsumer<IGeofenceEvent>)},
            };


      //setup our DI
      IServiceProvider serviceProvider;

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

      var kafkaTopics =
               serviceProvider.GetService<IConfigurationStore>()
                   .GetValueString("KAFKA_TOPICS")
                   .Split(new[] { "," }, StringSplitOptions.None);
      string loggerRepoName = "MDC " + kafkaTopics[0].Split('.').Last();

      var logPath = System.IO.Directory.GetCurrentDirectory();
      Console.WriteLine("Log path:" + logPath);
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4net.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceProvider = serviceCollection.BuildServiceProvider();

      var log = loggerFactory.CreateLogger(loggerRepoName);

      var tasks = new List<Task>();

      log.LogDebug("MasterDataConsumer is starting....");

      foreach (var kafkaTopic in kafkaTopics)
      {
        Console.WriteLine("MasterDataConsumer topic: " + kafkaTopic);
        if (serviceConverter.Any(s => kafkaTopic.Contains(s.Key)))
        {
          var consumer =
              serviceProvider.GetService(serviceConverter.First(s => kafkaTopic.Contains(s.Key)).Value) as
                  IAbstractKafkaConsumer;
          consumer.SetTopic(kafkaTopic);
          tasks.Add(consumer.StartProcessingAsync(new CancellationTokenSource()));
        }
        else
        {
          log.LogDebug("MasterDataConsumer: Kafka topic consumer not recognized: {0}", kafkaTopic);
          continue;
        }
        log.LogDebug("MasterDataConsumer: Kafka topic consumer to be started: {0}", kafkaTopic);
      }

      if (tasks.Count > 0)
        Task.WaitAll(tasks.ToArray());
      else
        log.LogCritical("MasterDataConsumer: No consumers started.");
    }
  }
}
