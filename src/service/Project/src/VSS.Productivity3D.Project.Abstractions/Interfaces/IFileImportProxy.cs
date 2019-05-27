using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IFileImportProxy : ICacheProxy
  {
    Task<List<FileData>> GetFiles(string projectUid, string userId, IDictionary<string, string> customHeaders);

    Task<FileData> GetFileForProject(string projectUid, string userId, string importedFileUid,
      IDictionary<string, string> customHeaders = null);
  }
}
