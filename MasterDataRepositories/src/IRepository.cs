using System.Threading.Tasks;

namespace VSS.Masterdata
{
    public interface IRepository<in T>
    {
        Task<int> StoreEvent(T deserializedObject);
    }
}