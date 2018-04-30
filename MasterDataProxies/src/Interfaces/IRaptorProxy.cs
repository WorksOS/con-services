using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IRaptorProxy
  {
    Task<CoordinateSystemSettingsResult> CoordinateSystemValidate(byte[] coordinateSystemFileContent,
      string coordinateSystemFilename,
      IDictionary<string, string> customHeaders = null);

    Task<CoordinateSystemSettingsResult> CoordinateSystemPost(long legacyProjectId, byte[] coordinateSystemFileContent,
      string coordinateSystemFilename,
      IDictionary<string, string> customHeaders = null);

    Task<AddFileResult> AddFile(Guid projectUid, ImportedFileType fileType, Guid fileUid, string fileDescriptor,
      long fileId, DxfUnitsType dxfUnitsType, IDictionary<string, string> customHeaders = null);

    Task<BaseDataResult> DeleteFile(Guid projectUid, ImportedFileType fileType, Guid fileUid, string fileDescriptor,
      long fileId, long? legacyFileId, IDictionary<string, string> customHeaders = null);

    Task<ProjectStatisticsResult> GetProjectStatistics(Guid projectUid,
      IDictionary<string, string> customHeaders = null);

    Task<BaseDataResult> UpdateFiles(Guid projectUid, IEnumerable<Guid> fileUids,
      IDictionary<string, string> customHeaders = null);

    Task<BaseDataResult> NotifyImportedFileChange(Guid projectUid, Guid fileUid,
      IDictionary<string, string> customHeaders = null);

    Task<BaseDataResult> ValidateProjectSettings(Guid projectUid, string projectSettings,
      IDictionary<string, string> customHeaders = null);

    Task<BaseDataResult> ValidateProjectSettings(Guid projectUid, string projectSettings,
      ProjectSettingsType settingsType, IDictionary<string, string> customHeaders = null);

    Task<BaseDataResult> ValidateProjectSettings(ProjectSettingsRequest request, 
	  IDictionary<string, string> customHeaders = null);
    Task<BaseDataResult> NotifyFilterChange(Guid filterUid, Guid projectUid,
      IDictionary<string, string> customHeaders = null);

    Task<BaseDataResult> UploadTagFile(string filename, byte[] data, string orgId = null,
      IDictionary<string, string> customHeaders = null);
  }
}