using System.Threading.Tasks;

namespace VSS.MasterData.Repositories
{
    public interface IRepository<in T>
    {
        Task<int> StoreEvent(T deserializedObject);
    }
}