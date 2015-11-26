using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Utilization.Common.Models;

namespace VSS.VisionLink.Utilization.Common.Interfaces
{
  public interface IProjectRepository
  {
    Project GetProject(string projectUid);
    Task<int> StoreProject(IProjectEvent evt);
  }
}