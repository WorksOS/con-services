using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IFileListProxy : ICacheProxy
  {
    Task<List<FileData>> GetFiles(string projectUid, string userId, IDictionary<string, string> customHeaders);

    Task<FileData> GetFileForProject(string projectUid, string userId, string importedFileUid,
      IDictionary<string, string> customHeaders = null);
  }
}
