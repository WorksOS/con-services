using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.MasterDataProxies.Models;

namespace VSS.Productivity3D.MasterDataProxies.Interfaces
{
  public interface IFileListProxy
  {
    Task<List<FileData>> GetFiles(string projectUid, IDictionary<string, string> customHeaders = null);
  }
}
