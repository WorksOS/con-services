using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Localization.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  /// <summary>
  /// Validates all file import data
  /// </summary>
  public class FileImportV2DataValidator
  {
    protected static ContractExecutionStatesEnum ContractExecutionStatesEnum = new ContractExecutionStatesEnum();

    /// <summary>
    /// Validate the Upsert request i.e. that the parameters are as expected.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="importedFile"></param>
    public static ImportedFileTbc ValidateUpsertImportedFileRequest(long projectId,
      ImportedFileTbc importedFile)
    {
      if (projectId <= 0 )
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.GetErrorNumberwithOffset(5),
            ContractExecutionStatesEnum.FirstNameWithOffset(5 /* todo */)));
      }

      if (!Enum.IsDefined(typeof(ImportedFileType), importedFile.ImportedFileTypeId))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.GetErrorNumberwithOffset(30),
            ContractExecutionStatesEnum.FirstNameWithOffset(30)));
      }

      if (!(importedFile.ImportedFileTypeId >= ImportedFileType.Linework && importedFile.ImportedFileTypeId <= ImportedFileType.Alignment))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.GetErrorNumberwithOffset(30),
            ContractExecutionStatesEnum.FirstNameWithOffset(31)));
      }

      var fileExtension = Path.GetExtension(importedFile.Name).ToLower();
      if (!(
        (importedFile.ImportedFileTypeId == ImportedFileType.Linework && fileExtension == ".dxf") ||
        (importedFile.ImportedFileTypeId == ImportedFileType.DesignSurface && fileExtension == ".ttm") ||
        (importedFile.ImportedFileTypeId == ImportedFileType.SurveyedSurface && fileExtension == ".ttm") ||
        (importedFile.ImportedFileTypeId == ImportedFileType.Alignment && fileExtension == ".svl")
      ))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.GetErrorNumberwithOffset(32),
            ContractExecutionStatesEnum.FirstNameWithOffset(32)));
      }

      if (importedFile.ImportedFileTypeId == ImportedFileType.Linework &&
           ( importedFile.LineworkFile == null ||
             !Enum.IsDefined(typeof(DxfUnitsType), importedFile.LineworkFile.DxfUnitsTypeId))
         )
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.GetErrorNumberwithOffset(75),
            ContractExecutionStatesEnum.FirstNameWithOffset(75)));
      }

      if (importedFile.ImportedFileTypeId == ImportedFileType.SurveyedSurface &&
          ( importedFile.SurfaceFile == null ||
            importedFile.SurfaceFile.SurveyedUtc < DateTime.UtcNow.AddYears(-30) || importedFile.SurfaceFile.SurveyedUtc > DateTime.UtcNow.AddDays(2))
          )
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.GetErrorNumberwithOffset(33),
            ContractExecutionStatesEnum.FirstNameWithOffset(33)));
      }

      if (importedFile.ImportedFileTypeId == ImportedFileType.Alignment &&
          importedFile.AlignmentFile == null )
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.GetErrorNumberwithOffset(33),
            ContractExecutionStatesEnum.FirstNameWithOffset(33 /* todo */)));
      }

      ProjectDataValidator.ValidateBusinessCentreFile(importedFile as BusinessCenterFile);
      return importedFile;
    }
  }
}
