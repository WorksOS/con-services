using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IImportedFileProxy
  {
    Task<FileDataSingleResult> CreateImportedFile(
      string fullFileName, string utf8filename, Guid projectUid, ImportedFileType importedFileType,
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc, DxfUnitsType? dxfUnitsType,
      DateTime? surveyedUtc, IDictionary<string, string> customHeaders = null);

    Task<FileDataSingleResult> UpdateImportedFile(
      string fullFileName, string utf8filename, Guid projectUid, ImportedFileType importedFileType,
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc, DxfUnitsType? dxfUnitsType,
      DateTime? surveyedUtc, IDictionary<string, string> customHeaders = null);

    Task<BaseDataResult> DeleteImportedFile(Guid projectUid, Guid importedFileUid, IDictionary<string, string> customHeaders = null);
  }
}
