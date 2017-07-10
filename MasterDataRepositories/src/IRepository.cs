using System.Threading.Tasks;

namespace VSS.Productivity3D.Repo
{
    public interface IRepository<in T>
    {
        Task<int> StoreEvent(T deserializedObject);
    }
}