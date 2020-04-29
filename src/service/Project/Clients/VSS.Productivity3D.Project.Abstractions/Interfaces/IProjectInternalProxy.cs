using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IProjectInternalProxy : ICacheProxy
  {
    Task<List<ProjectData>> GetProjects(string customerUid, IDictionary<string, string> customHeaders = null);

    Task<ProjectData> GetProject(string projectUid, IDictionary<string, string> customHeaders = null);
    
    Task<ProjectData> GetProject(long shortRaptorProjectId, IDictionary<string, string> customHeaders = null);
    
    Task<List<ProjectData>> GetIntersectingProjects(string customerUid,
        double latitude, double longitude, string projectUid = null, IDictionary<string, string> customHeaders = null);

  }
}
