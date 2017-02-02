using System.Threading;
using System.Threading.Tasks;
using KafkaConsumer;
using Microsoft.Extensions.DependencyInjection;
using VSS.Customer.Data;
using VSS.Project.Data;
using VSS.Project.Service.Interfaces;
using VSS.Project.Service.Repositories;
using VSS.Project.Service.Utils;
using VSS.Project.Service.Utils.Kafka;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.Geofence.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using log4netExtensions;
using System;
using System.Collections.Generic;

namespace MasterDataConsumer
{
  public class Program
  {

    public static void Main(string[] args)
    {
      string loggerRepoName = "MasterDataConsumer";
      var logPath = System.IO.Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4net.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      //setup our DI
      var serviceProvider = new ServiceCollection()
          .AddTransient<IKafka, RdKafkaDriver>()
          .AddTransient<IKafkaConsumer<ISubscriptionEvent>, KafkaConsumer<ISubscriptionEvent>>()
          .AddTransient<IKafkaConsumer<IProjectEvent>, KafkaConsumer<IProjectEvent>>()
          .AddTransient<IKafkaConsumer<ICustomerEvent>, KafkaConsumer<ICustomerEvent>>()
          .AddTransient<IKafkaConsumer<IGeofenceEvent>, KafkaConsumer<IGeofenceEvent>>()
          .AddTransient<IMessageTypeResolver, MessageResolver>()
          .AddTransient<IRepositoryFactory, RepositoryFactory>()
          .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>()
          .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
          .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
          .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          .AddLogging()
          .AddSingleton<ILoggerFactory>(loggerFactory)
          .BuildServiceProvider();

      var log = loggerFactory.CreateLogger("MasterDataConsumer");

      var kafkaTopics = serviceProvider.GetService<IConfigurationStore>().GetValueString("KAFKA_TOPICS").Split(new[] { "," }, StringSplitOptions.None);
      var tasks = new List<Task>();

      log.LogDebug("MasterDataConsumer is starting....");

      foreach (var kafkaTopic in kafkaTopics)
      {
        if (kafkaTopic.Contains("ICustomerEvent"))
        {
          var consumer = serviceProvider.GetService<IKafkaConsumer<ICustomerEvent>>();
          consumer.SetTopic(kafkaTopic);
          tasks.Add(consumer.StartProcessingAsync(new CancellationTokenSource()));
        }
        else if (kafkaTopic.Contains("IProjectEvent"))
        {
          var consumer = serviceProvider.GetService<IKafkaConsumer<IProjectEvent>>();
          consumer.SetTopic("VSS.Interfaces.Events.MasterData.IProjectEvent");
          tasks.Add(consumer.StartProcessingAsync(new CancellationTokenSource()));
        }
        else if(kafkaTopic.Contains("ISubscriptionEvent"))
        {
          var consumer = serviceProvider.GetService<IKafkaConsumer<ISubscriptionEvent>>();
          consumer.SetTopic("VSS.Interfaces.Events.MasterData.ISubscriptionEvent");
          tasks.Add(consumer.StartProcessingAsync(new CancellationTokenSource()));
        }
        else if (kafkaTopic.Contains("IGeofenceEvent"))
        {
          var consumer = serviceProvider.GetService<IKafkaConsumer<IGeofenceEvent>>();
          consumer.SetTopic("VSS.Interfaces.Events.MasterData.IGeofenceEvent");
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
        log.LogCritical("MasterDataConsumer: No consumers started. Kafka topics: {0} do not exist", kafkaTopics);
    }
  }
}
