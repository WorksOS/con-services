using System;
using System.IO;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
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
    protected static ProjectErrorCodesProvider projectErrorCodesProvider = new ProjectErrorCodesProvider();

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
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(5),
            projectErrorCodesProvider.FirstNameWithOffset(5)));
      }

      if (!Enum.IsDefined(typeof(ImportedFileType), importedFile.ImportedFileTypeId))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(30),
            projectErrorCodesProvider.FirstNameWithOffset(30)));
      }

      if (!(importedFile.ImportedFileTypeId >= ImportedFileType.Linework && importedFile.ImportedFileTypeId <= ImportedFileType.Alignment))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(31),
            projectErrorCodesProvider.FirstNameWithOffset(31)));
      }

      ProjectDataValidator.ValidateBusinessCentreFile(importedFile);


      if (importedFile.ImportedFileTypeId == ImportedFileType.Linework &&
           ( importedFile.LineworkFile == null ||
             !Enum.IsDefined(typeof(DxfUnitsType), importedFile.LineworkFile.DxfUnitsTypeId))
         )
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(75),
            projectErrorCodesProvider.FirstNameWithOffset(75)));
      }

      if (importedFile.ImportedFileTypeId == ImportedFileType.SurveyedSurface &&
          ( importedFile.SurfaceFile == null ||
            importedFile.SurfaceFile.SurveyedUtc < DateTime.UtcNow.AddYears(-30) || importedFile.SurfaceFile.SurveyedUtc > DateTime.UtcNow.AddDays(2))
          )
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(33),
            projectErrorCodesProvider.FirstNameWithOffset(33)));
      }

      if (importedFile.ImportedFileTypeId == ImportedFileType.Alignment &&
          importedFile.AlignmentFile == null )
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(95),
            projectErrorCodesProvider.FirstNameWithOffset(95)));
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
          new ContractExecutionResult(projectErrorCodesProvider.GetErrorNumberwithOffset(32),
            projectErrorCodesProvider.FirstNameWithOffset(32)));
      }

      return importedFile;
    }
  }
}
