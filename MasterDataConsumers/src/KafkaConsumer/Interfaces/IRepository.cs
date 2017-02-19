using System.Threading.Tasks;

namespace KafkaConsumer
{
    public interface IRepository<in T>
    {
        Task<int> StoreEvent(T deserializedObject);
    }
}