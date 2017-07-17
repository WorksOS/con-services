using Newtonsoft.Json;

namespace VSS.KafkaConsumer.Interfaces
{
    public interface IMessageTypeResolver
    {
        JsonConverter GetConverter<T>();
    }
}