using MasterDataModels.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VSS.Productivity3D.MasterDataProxies.Interfaces
{
  public interface IFileListProxy
  {
    Task<List<FileData>> GetFiles(string projectUid, IDictionary<string, string> customHeaders = null);
  }
}
