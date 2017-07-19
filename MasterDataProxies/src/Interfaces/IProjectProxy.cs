using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
 
  public interface IProjectProxy
  {
    Task<List<ProjectData>> GetProjectsV4(string customerUid, IDictionary<string, string> customHeaders = null);
    Task<List<FileData>> GetFiles(string projectUid, IDictionary<string, string> customHeaders = null);
    Task<ProjectSettingsDataResult> GetProjectSettings(string projectUid, IDictionary<string, string> customHeaders = null);
  }
  
}
