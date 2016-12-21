using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KafkaConsumer;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using VSS.UnifiedProductivity.Service.Interfaces;
using VSS.UnifiedProductivity.Service.Utils;
using VSS.UnifiedProductivity.Service.Utils.JsonConverters;
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
                .AddTransient<IKafkaConsumer<IAssetEvent>, KafkaConsumer<IAssetEvent>>()
                .AddTransient<IMessageTypeResolver, MessageResolver>()
                .AddTransient<IRepositoryFactory,RepositoryFactory>()
                .AddSingleton<IConfigurationStore, KafkaConsumerConfiguration>()
                .BuildServiceProvider();

            var bar = serviceProvider.GetService<IKafkaConsumer<IAssetEvent>>();
            bar.SetTopic("VSS.Interfaces.Events.MasterData.IAssetEvent");
            bar.StartProcessingAsync(new CancellationTokenSource()).Wait();
            

        }
    }

    public class RepositoryFactory : IRepositoryFactory
    {
        public IRepository<T> GetRepository<T>()
        {
            if (typeof(T) == typeof(IAssetEvent))
                return new AssetRepository() as IRepository<T>;
            return null;
        }
    }

    public class AssetRepository : IRepository<IAssetEvent>
    {
        public void StoreEvent(IAssetEvent deserializedObject)
        {

        }
    }

    public class MessageResolver : IMessageTypeResolver
    {
        public JsonConverter GetConverter<T>()
        {
            if (typeof(T) == typeof(IAssetEvent))
                return new AssetEventConverter();
            return null;
        }
    }
}
