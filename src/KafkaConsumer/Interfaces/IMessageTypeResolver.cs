using Newtonsoft.Json;

namespace KafkaConsumer
{
    public interface IMessageTypeResolver
    {
        JsonConverter GetConverter<T>();
    }
}