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
using System.Linq;
using System.Reflection;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.Extensions.DependencyModel;
using VSS.GenericConfiguration;
using VSS.Masterdata;

namespace MasterDataConsumer
{
    public class Program
    {

        public static void Main(string[] args)
        {
            string loggerRepoName = "MasterDataConsumer";
            var logPath = System.IO.Directory.GetCurrentDirectory();
            Console.WriteLine("Log path:" + logPath);
            Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4net.xml", loggerRepoName);

            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddDebug();
            loggerFactory.AddLog4Net(loggerRepoName);

            Dictionary<string, Type> serviceConverter = new Dictionary<string, Type>()
            {
                {"ICustomerEvent", typeof(IKafkaConsumer<ICustomerEvent>)},
                {"IProjectEvent", typeof(IKafkaConsumer<IProjectEvent>)},
                {"ISubscriptionEvent", typeof(IKafkaConsumer<ISubscriptionEvent>)},
                {"IGeofenceEvent", typeof(IKafkaConsumer<IGeofenceEvent>)},
            };


            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddTransient<IKafka, RdKafkaDriver>()
                .AddTransient<IKafkaConsumer<ISubscriptionEvent>, KafkaConsumer<ISubscriptionEvent>>()
                .AddTransient<IKafkaConsumer<IProjectEvent>, KafkaConsumer<IProjectEvent>>()
                .AddTransient<IKafkaConsumer<ICustomerEvent>, KafkaConsumer<ICustomerEvent>>()
                .AddTransient<IKafkaConsumer<IGeofenceEvent>, KafkaConsumer<IGeofenceEvent>>()
                .AddTransient<IMessageTypeResolver, MessageResolver>()
                .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>()
                .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
                .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
                .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
                .AddSingleton<IConfigurationStore, GenericConfiguration>()
                .AddLogging()
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .AddTransient<IRepositoryFactory, RepositoryFactory>()
                .BuildServiceProvider();

            var log = loggerFactory.CreateLogger("MasterDataConsumer");

            var kafkaTopics =
                serviceProvider.GetService<IConfigurationStore>()
                    .GetValueString("KAFKA_TOPICS")
                    .Split(new[] {","}, StringSplitOptions.None);
            var tasks = new List<Task>();

            log.LogDebug("MasterDataConsumer is starting....");

            foreach (var kafkaTopic in kafkaTopics)
            {
                Console.WriteLine("MasterDataConsumer topic: " + kafkaTopic);
                if (serviceConverter.Any(s => s.Key.Contains(kafkaTopic)))
                {
                    var consumer =
                        serviceProvider.GetService(serviceConverter.First(s => s.Key.Contains(kafkaTopic)).Value) as
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
