using System;
using System.Collections.Generic;
using System.Linq;
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

namespace MasterDataConsumer
{
  public class Program
  {
    public static void Main(string[] args)
    {
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
          .BuildServiceProvider();

      var bar1 = serviceProvider.GetService<IKafkaConsumer<ICustomerEvent>>();
      bar1.SetTopic("VSS.Interfaces.Events.MasterData.ICustomerEvent");
      var t1 = bar1.StartProcessingAsync(new CancellationTokenSource());

      var bar2 = serviceProvider.GetService<IKafkaConsumer<IProjectEvent>>();
      bar2.SetTopic("VSS.Interfaces.Events.MasterData.IProjectEvent");
      var t2 = bar2.StartProcessingAsync(new CancellationTokenSource());

      var bar3 = serviceProvider.GetService<IKafkaConsumer<ISubscriptionEvent>>();
      bar3.SetTopic("VSS.Interfaces.Events.MasterData.ISubscriptionEvent");
      var t3 = bar3.StartProcessingAsync(new CancellationTokenSource());

      var bar4 = serviceProvider.GetService<IKafkaConsumer<IGeofenceEvent>>();
      bar4.SetTopic("VSS.Interfaces.Events.MasterData.IGeofenceEvent");
      var t4 = bar4.StartProcessingAsync(new CancellationTokenSource());

      Task.WaitAll(t1, t2, t3, t4);
    }
  }
}
