using MasterDataModels.Models;
using MasterDataModels.ResultHandling;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.MasterDataProxies.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.MasterDataProxies.Interfaces
{
  public interface IRaptorProxy
  {
    Task<CoordinateSystemSettingsResult> CoordinateSystemValidate(byte[] coordinateSystemFileContent, string coordinateSystemFilename,
        IDictionary<string, string> customHeaders = null);

    Task<CoordinateSystemSettingsResult> CoordinateSystemPost(long legacyProjectId, byte[] coordinateSystemFileContent, string coordinateSystemFilename,
            IDictionary<string, string> customHeaders = null);

    Task<BaseDataResult> AddFile(Guid projectUid, ImportedFileType fileType, Guid fileUid, string fileDescriptor, long fileId, IDictionary<string, string> customHeaders = null);

    Task<BaseDataResult> DeleteFile(Guid projectUid, ImportedFileType fileType, Guid fileUid, string fileDescriptor, long fileId, IDictionary<string, string> customHeaders = null);

    Task<BaseDataResult> UpdateFiles(Guid projectUid, IEnumerable<Guid> fileUids, IDictionary<string, string> customHeaders = null);

    Task<ContractExecutionResult> ProjectSettingsValidate(Guid projectUid, string settings, IDictionary<string, string> customHeaders = null);
  }
}