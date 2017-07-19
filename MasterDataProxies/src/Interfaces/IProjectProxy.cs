using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace VSS.MasterData.Proxies.Interfaces
{
 
  public interface IProjectProxy
  {
    Task<List<ProjectData>> GetProjectsV4(string customerUid, IDictionary<string, string> customHeaders = null);
    Task<List<FileData>> GetFiles(string projectUid, IDictionary<string, string> customHeaders = null);
    Task<string> GetProjectSettings(string projectUid, IDictionary<string, string> customHeaders = null);
  }
  
}
