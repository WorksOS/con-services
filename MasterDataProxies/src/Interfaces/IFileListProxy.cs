using System.Collections.Generic;
using System.Threading.Tasks;
using MasterDataModels.Models;

namespace MasterDataProxies.Interfaces
{
  public interface IFileListProxy
  {
    Task<List<FileData>> GetFiles(string projectUid, IDictionary<string, string> customHeaders = null);
  }
}
