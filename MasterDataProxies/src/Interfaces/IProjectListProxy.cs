using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IProjectListProxy : ICacheProxy
  {
    Task<List<ProjectData>> GetProjectsV4(string customerUid, IDictionary<string, string> customHeaders = null);

    Task<ProjectData> GetProjectForCustomer(string customerUid, string projectUid,
      IDictionary<string, string> customHeaders = null);

    //To support 3dpm v1 end points which use legacy project id
    Task<ProjectData> GetProjectForCustomer(string customerUid, long projectId,
      IDictionary<string, string> customHeaders = null);
  }
}
