using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KafkaConsumer;
using Microsoft.Extensions.DependencyInjection;
using VSS.Customer.Data;
using VSS.Project.Data;
using VSS.UnifiedProductivity.Service.Interfaces;
using VSS.UnifiedProductivity.Service.Repositories;
using VSS.UnifiedProductivity.Service.Utils;
using VSS.UnifiedProductivity.Service.Utils.Kafka;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace MasterDataConsumer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddTransient<IKafka,RdKafkaDriver>()
                .AddTransient<IKafkaConsumer<ISubscriptionEvent>, KafkaConsumer<ISubscriptionEvent>>()
                .AddTransient<IKafkaConsumer<IProjectEvent>, KafkaConsumer<IProjectEvent>>()
                .AddTransient<IKafkaConsumer<ICustomerEvent>, KafkaConsumer<ICustomerEvent>>()
                .AddTransient<IMessageTypeResolver, MessageResolver>()
                .AddTransient<IRepositoryFactory,RepositoryFactory>()
                .AddTransient<IRepository<ISubscriptionEvent>, Subscriptionepository>()
                .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
                .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
                .AddSingleton<IConfigurationStore, KafkaConsumerConfiguration>()
                .BuildServiceProvider();

            var bar = serviceProvider.GetService<IKafkaConsumer<ISubscriptionEvent>>();
            bar.SetTopic("VSS.Interfaces.Events.MasterData.ISubscriptionEvent");
            var t1=bar.StartProcessingAsync(new CancellationTokenSource());

            var bar1 = serviceProvider.GetService<IKafkaConsumer<ISubscriptionEvent>>();
            bar1.SetTopic("VSS.Interfaces.Events.MasterData.IProjectEvent");
            var t2=bar1.StartProcessingAsync(new CancellationTokenSource());


            var bar2 = serviceProvider.GetService<IKafkaConsumer<ISubscriptionEvent>>();
            bar2.SetTopic("VSS.Interfaces.Events.MasterData.ICustomerEvent");
            var t3=bar2.StartProcessingAsync(new CancellationTokenSource());

            Task.WaitAll(t1,t2,t3);
        }
    }
}
