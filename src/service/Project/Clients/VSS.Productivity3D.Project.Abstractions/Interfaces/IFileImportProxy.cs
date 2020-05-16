using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface IFileImportProxy : ICacheProxy
  {
    Task<List<FileData>> GetFiles(string projectUid, string userId, IHeaderDictionary customHeaders);

    Task<FileData> GetFileForProject(string projectUid, string userId, string importedFileUid,
      IHeaderDictionary customHeaders = null);
  }
}
