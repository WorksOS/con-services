using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.Notification.ResultHandling;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Productivity3D.Abstractions.Interfaces
{
  public interface IProductivity3dV2ProxyNotification : IProductivity3dV2Proxy
  {
    Task<AddFileResult> AddFile(string projectUid, ImportedFileType fileType, string fileUid, string fileDescriptor,
      long fileId, DxfUnitsType dxfUnitsType, IDictionary<string, string> customHeaders = null);

    Task<BaseMasterDataResult> DeleteFile(string projectUid, ImportedFileType fileType, string fileUid, string fileDescriptor,
      long fileId, long? legacyFileId, IDictionary<string, string> customHeaders = null);


    Task<BaseMasterDataResult> UpdateFiles(string projectUid, IEnumerable<string> fileUids,
      IDictionary<string, string> customHeaders = null);

    Task<BaseMasterDataResult> InvalidateCache(string projectUid,
      IDictionary<string, string> customHeaders = null);

    Task<BaseMasterDataResult> NotifyImportedFileChange(string projectUid, string fileUid,
      IDictionary<string, string> customHeaders = null);

    Task<BaseMasterDataResult> NotifyFilterChange(string filterUid, string projectUid,
      IDictionary<string, string> customHeaders = null);
  }
}
