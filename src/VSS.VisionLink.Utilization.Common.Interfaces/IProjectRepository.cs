using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Landfill.Common.Models;


namespace VSS.VisionLink.Landfill.Common.Interfaces
{
  public interface IProjectRepository
  {
    Project GetProject(string projectUid);
    Task<int> StoreProject(IProjectEvent evt);
    IEnumerable<Project> GetProjects();
  }
}