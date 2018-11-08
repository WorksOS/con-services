using System;
using System.IO;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{

  /// <summary>
  /// Validates all file import data for FlowJS and non FlowJS file streams.
  /// </summary>
  public class FileImportDataValidator
  {
    protected const int MAX_FILE_NAME_LENGTH = 256;
    protected static ProjectErrorCodesProvider ProjectErrorCodesProvider = new ProjectErrorCodesProvider();

    /// <summary>
    /// Validate the Create request e.g that the file has been uploaded and parameters are as expected.
    /// </summary>
    public static void ValidateUpsertImportedFileRequest(Guid projectUid, ImportedFileType importedFileType, DxfUnitsType dxfUnitsType, DateTime fileCreatedUtc, DateTime fileUpdatedUtc, string importedBy, DateTime? surveyedUtc, string filename)
    {
      if (projectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(5),
            ProjectErrorCodesProvider.FirstNameWithOffset(5)));
      }

      if (!Enum.IsDefined(typeof(ImportedFileType), importedFileType))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(30),
            ProjectErrorCodesProvider.FirstNameWithOffset(30)));
      }

      if (!(importedFileType >= ImportedFileType.Linework && importedFileType <= ImportedFileType.Alignment))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(30),
            ProjectErrorCodesProvider.FirstNameWithOffset(31)));
      }

      var fileExtension = Path.GetExtension(filename).ToLower();
      if (!(importedFileType == ImportedFileType.Linework && fileExtension == ".dxf" ||
            importedFileType == ImportedFileType.DesignSurface && fileExtension == ".ttm" ||
            importedFileType == ImportedFileType.SurveyedSurface && fileExtension == ".ttm" ||
            importedFileType == ImportedFileType.Alignment && fileExtension == ".svl"))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(32),
            ProjectErrorCodesProvider.FirstNameWithOffset(32)));
      }

      if (!Enum.IsDefined(typeof(DxfUnitsType), dxfUnitsType))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(75),
            ProjectErrorCodesProvider.FirstNameWithOffset(75)));
      }

      if (importedFileType == ImportedFileType.Linework && (dxfUnitsType < DxfUnitsType.Meters || dxfUnitsType > DxfUnitsType.UsSurveyFeet))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(75),
            ProjectErrorCodesProvider.FirstNameWithOffset(76)));
      }

      if (fileCreatedUtc < DateTime.UtcNow.AddYears(-30) || fileCreatedUtc > DateTime.UtcNow.AddDays(2))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(33),
            ProjectErrorCodesProvider.FirstNameWithOffset(33)));
      }

      if (fileUpdatedUtc < DateTime.UtcNow.AddYears(-30) || fileUpdatedUtc > DateTime.UtcNow.AddDays(2))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(34),
            ProjectErrorCodesProvider.FirstNameWithOffset(34)));
      }

      if (string.IsNullOrEmpty(importedBy))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(35),
            ProjectErrorCodesProvider.FirstNameWithOffset(35)));
      }

      if (importedFileType == ImportedFileType.SurveyedSurface && surveyedUtc == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ProjectErrorCodesProvider.GetErrorNumberwithOffset(36),
            ProjectErrorCodesProvider.FirstNameWithOffset(36)));
      }
    }
  }
}
