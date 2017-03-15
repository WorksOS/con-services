using Newtonsoft.Json;

namespace KafkaConsumer.Interfaces
{
    public interface IMessageTypeResolver
    {
        JsonConverter GetConverter<T>();
    }
}