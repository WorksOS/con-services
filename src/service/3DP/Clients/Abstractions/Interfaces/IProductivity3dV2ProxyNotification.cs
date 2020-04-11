using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.Notification.ResultHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Productivity3D.Abstractions.Interfaces
{
  public interface IProductivity3dV2ProxyNotification : IProductivity3dV2Proxy
  {
    Task<AddFileResult> AddFile(Guid projectUid, ImportedFileType fileType, Guid fileUid, string fileDescriptor,
      long fileId, DxfUnitsType dxfUnitsType, IDictionary<string, string> customHeaders = null);

    Task<BaseMasterDataResult> DeleteFile(Guid projectUid, ImportedFileType fileType, Guid fileUid, string fileDescriptor,
      long fileId, long? legacyFileId, IDictionary<string, string> customHeaders = null);


    Task<BaseMasterDataResult> UpdateFiles(Guid projectUid, IEnumerable<Guid> fileUids,
      IDictionary<string, string> customHeaders = null);

    Task<BaseMasterDataResult> InvalidateCache(string projectUid,
      IDictionary<string, string> customHeaders = null);

    Task<BaseMasterDataResult> NotifyImportedFileChange(Guid projectUid, Guid fileUid,
      IDictionary<string, string> customHeaders = null);

    Task<BaseMasterDataResult> NotifyFilterChange(Guid filterUid, Guid projectUid,
      IDictionary<string, string> customHeaders = null);
  }
}
