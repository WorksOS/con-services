using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KafkaConsumer;
using Microsoft.Extensions.DependencyInjection;
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
                .AddTransient<IMessageTypeResolver, MessageResolver>()
                .AddTransient<IRepositoryFactory,RepositoryFactory>()
                .AddTransient<IRepository<ISubscriptionEvent>, Subscriptionepository>()
                .AddSingleton<IConfigurationStore, KafkaConsumerConfiguration>()
                .BuildServiceProvider();
            var bar = serviceProvider.GetService<IKafkaConsumer<ISubscriptionEvent>>();
            bar.SetTopic("VSS.Interfaces.Events.MasterData.ISubscriptionEvent");
            bar.StartProcessingAsync(new CancellationTokenSource()).Wait();
        }
    }
}
