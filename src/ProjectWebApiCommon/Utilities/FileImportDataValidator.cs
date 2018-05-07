using System;
using System.IO;
using System.Net;
using VSS.Common.Exceptions;
using VSS.FlowJSHandler;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  /// <summary>
  /// Validates all file import data
  /// </summary>
  public class FileImportDataValidator
  {
    private const int MaxFileNameLength = 256;
    protected static ProjectErrorCodesProvider projectErrorCodesProvider = new ProjectErrorCodesProvider();


    /// <summary>
    /// Validate the Create request e.g that the file has been uploaded and parameters are as expected.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="projectUid"></param>
    /// <param name="importedFileType"></param>
    /// /// <param name="dxfUnitsType"></param>
    /// <param name="importedBy"></param>
    /// <param name="surveyedUtc"></param>
    /// <param name="fileCreatedUtc"></param>
    /// <param name="fileUpdatedUtc"></param>
    public static void ValidateUpsertImportedFileRequest(FlowFile file, Guid projectUid,
      ImportedFileType importedFileType, DxfUnitsType dxfUnitsType, 
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc,
      string importedBy, DateTime? surveyedUtc)
    {
      // by the time we are here, the file has been uploaded and location is in file. Some validation:
      if (file == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(27),
            projectErrorCodesProvider.FirstNameWithOffset(27)));
      }

      if (file.flowFilename.Length > MaxFileNameLength || string.IsNullOrEmpty(file.flowFilename) ||
          file.flowFilename.IndexOfAny(Path.GetInvalidPathChars()) > 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(28),
            projectErrorCodesProvider.FirstNameWithOffset(28)));
      }

      if (string.IsNullOrEmpty(file.path) ||
          file.path.IndexOfAny(Path.GetInvalidPathChars()) > 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(29),
            projectErrorCodesProvider.FirstNameWithOffset(29)));
      }

      if (projectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(5),
            projectErrorCodesProvider.FirstNameWithOffset(5)));
      }

      if (!Enum.IsDefined(typeof(ImportedFileType), importedFileType))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(30),
            projectErrorCodesProvider.FirstNameWithOffset(30)));
      }

      if (!(importedFileType >= ImportedFileType.Linework && importedFileType <= ImportedFileType.Alignment))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(30),
            projectErrorCodesProvider.FirstNameWithOffset(31)));
      }

      var fileExtension = Path.GetExtension(file.flowFilename).ToLower();
      if (!(
        (importedFileType == ImportedFileType.Linework && fileExtension == ".dxf") ||
        (importedFileType == ImportedFileType.DesignSurface && fileExtension == ".ttm") ||
        (importedFileType == ImportedFileType.SurveyedSurface && fileExtension == ".ttm") ||
        (importedFileType == ImportedFileType.Alignment && fileExtension == ".svl")
      ))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(32),
            projectErrorCodesProvider.FirstNameWithOffset(32)));
      }

      if (!Enum.IsDefined(typeof(DxfUnitsType), dxfUnitsType))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(75),
            projectErrorCodesProvider.FirstNameWithOffset(75)));
      }

      if (importedFileType == ImportedFileType.Linework && (dxfUnitsType < DxfUnitsType.Meters || dxfUnitsType > DxfUnitsType.UsSurveyFeet))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(75),
            projectErrorCodesProvider.FirstNameWithOffset(76)));
      }


      if (fileCreatedUtc < DateTime.UtcNow.AddYears(-30) || fileCreatedUtc > DateTime.UtcNow.AddDays(2))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(33),
            projectErrorCodesProvider.FirstNameWithOffset(33)));
      }

      if (fileUpdatedUtc < DateTime.UtcNow.AddYears(-30) || fileUpdatedUtc > DateTime.UtcNow.AddDays(2))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(34),
            projectErrorCodesProvider.FirstNameWithOffset(34)));
      }

      if (string.IsNullOrEmpty(importedBy))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(35),
            projectErrorCodesProvider.FirstNameWithOffset(35)));
      }

      if (importedFileType == ImportedFileType.SurveyedSurface && surveyedUtc == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(36),
            projectErrorCodesProvider.FirstNameWithOffset(36)));
      }
    }
  }
}
