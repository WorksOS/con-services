using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IProjectListProxy : ICacheProxy
  {
    Task<List<ProjectData>> GetProjectsV4(string customerUid, IDictionary<string, string> customHeaders = null);
  }
}
