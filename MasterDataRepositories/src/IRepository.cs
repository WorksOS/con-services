using System.Threading.Tasks;

namespace Repositories
{
    public interface IRepository<in T>
    {
        Task<int> StoreEvent(T deserializedObject);
    }
}