using System.Collections.Generic;
using System.Threading.Tasks;
using src.Models;

namespace src.Interfaces
{
  public interface IFileListProxy
  {
    Task<List<FileData>> GetFiles(string customerUid, IDictionary<string, string> customHeaders = null);
  }
}
