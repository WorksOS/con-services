using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IFileListProxy : ICacheProxy
  {
    Task<List<FileData>> GetFiles(string projectUid, IDictionary<string, string> customHeaders = null);
  }
}
