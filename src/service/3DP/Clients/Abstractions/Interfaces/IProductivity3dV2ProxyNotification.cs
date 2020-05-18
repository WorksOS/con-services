using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.Notification.ResultHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Productivity3D.Abstractions.Interfaces
{
  public interface IProductivity3dV2ProxyNotification : IProductivity3dV2Proxy
  {
    Task<AddFileResult> AddFile(Guid projectUid, ImportedFileType fileType, Guid fileUid, string fileDescriptor,
      long fileId, DxfUnitsType dxfUnitsType, IHeaderDictionary customHeaders = null);

    Task<BaseMasterDataResult> DeleteFile(Guid projectUid, ImportedFileType fileType, Guid fileUid, string fileDescriptor,
      long fileId, long? legacyFileId, IHeaderDictionary customHeaders = null);

    Task<BaseMasterDataResult> UpdateFiles(Guid projectUid, IEnumerable<Guid> fileUids,
      IHeaderDictionary customHeaders = null);

    Task<BaseMasterDataResult> InvalidateCache(string projectUid,
      IHeaderDictionary customHeaders = null);

    Task<BaseMasterDataResult> NotifyImportedFileChange(Guid projectUid, Guid fileUid,
      IHeaderDictionary customHeaders = null);

    Task<BaseMasterDataResult> NotifyFilterChange(Guid filterUid, Guid projectUid,
      IHeaderDictionary customHeaders = null);
  }
}
