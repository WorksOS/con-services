using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.MasterDataProxies.Models;

namespace VSS.Productivity3D.MasterDataProxies.Interfaces
{
  public interface IProjectListProxy
  {
    Task<List<ProjectData>> GetProjectsV4(string customerUid, IDictionary<string, string> customHeaders = null);
  }
}
