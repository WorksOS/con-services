using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterDataProxies.Interfaces
{
  public interface IFileListProxy
  {
    Task<List<FileData>> GetFiles(string projectUid, IDictionary<string, string> customHeaders = null);
  }
}
