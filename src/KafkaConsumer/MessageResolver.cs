using KafkaConsumer;
using KafkaConsumer.JsonConverters;
using Newtonsoft.Json;
using VSS.UnifiedProductivity.Service.Utils.JsonConverters;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace MasterDataConsumer
{
    public class MessageResolver : IMessageTypeResolver
    {
        public JsonConverter GetConverter<T>()
        {
            if (typeof(T) == typeof(IAssetEvent))
                return new AssetEventConverter();
            if (typeof(T) == typeof(ISubscriptionEvent))
                return new SubscriptionEventConverter();
            if (typeof(T) == typeof(ICustomerEvent))
                return new CustomerEventConverter();
            return null;
        }
    }
}