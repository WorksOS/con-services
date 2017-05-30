using System.Collections.Generic;
using System.Threading.Tasks;
using MasterDataProxies.Models;

namespace MasterDataProxies.Interfaces
{
  public interface IProjectListProxy
  {
    Task<List<ProjectData>> GetProjectsV4(string customerUid, IDictionary<string, string> customHeaders = null);
  }
}
