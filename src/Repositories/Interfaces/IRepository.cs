using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TagFileAuth.Service.Repositories
{
  public interface IRepository<in T>
  {
    Task<int> StoreEvent(T deserializedObject);
    object GetAssetDevice(string radioSerial, string v);
    object StoreEvent(CreateAssetEvent evt);
  }
}