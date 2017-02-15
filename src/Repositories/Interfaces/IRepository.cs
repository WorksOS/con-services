using System.Threading.Tasks;

namespace VSS.TagFileAuth.Service.Interfaces
{
    public interface IRepository<in T>
    {
        Task<int> StoreEvent(T deserializedObject);
    }
}