using System;
using System.IO;
using System.Net;
using VSS.Common.Exceptions;
using VSS.FlowJSHandler;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  /// <inheritdoc />
  public class FlowJsFileImportDataValidator : FileImportDataValidator
  {
    /// <summary>
    /// Validate the Create request e.g that the file has been uploaded and parameters are as expected.
    /// </summary>
    public static void ValidateUpsertImportedFileRequest(FlowFile file, Guid projectUid,
      ImportedFileType importedFileType, DxfUnitsType dxfUnitsType, 
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc,
      string importedBy, DateTime? surveyedUtc)
    {
      if (file == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(27),
            ProjectErrorCodesProvider.FirstNameWithOffset(27)));
      }

      if (file.flowFilename.Length > MAX_FILE_NAME_LENGTH || string.IsNullOrEmpty(file.flowFilename) ||
          file.flowFilename.IndexOfAny(Path.GetInvalidPathChars()) > 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(28),
            ProjectErrorCodesProvider.FirstNameWithOffset(28)));
      }

      if (string.IsNullOrEmpty(file.path) || file.path.IndexOfAny(Path.GetInvalidPathChars()) > 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(29),
            ProjectErrorCodesProvider.FirstNameWithOffset(29)));
      }

      ValidateUpsertImportedFileRequest(projectUid, importedFileType, dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, importedBy, surveyedUtc, file.flowFilename);
    }
  }
}
