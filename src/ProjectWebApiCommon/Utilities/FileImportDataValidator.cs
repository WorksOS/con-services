using System;
using System.Net;
using ProjectWebApiCommon.ResultsHandling;
using System.IO;
using FlowUploadFilter;
using Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ProjectWebApiCommon.Models
{
  /// <summary>
  /// Validates all file import data
  /// </summary>
  public class FileImportDataValidator
  {
    private const int MaxFileNameLength = 256;
    protected static ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();


    /// <summary>
    /// Validate the Create request e.g that the file has been uploaded and parameters are as expected.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="projectUid"></param>
    /// <param name="importedFileType"></param>
    /// <param name="importedBy"></param>
    /// <param name="surveyedUtc"></param>
    /// <param name="fileCreatedUtc"></param>
    /// <param name="fileUpdatedUtc"></param>
    public static void ValidateUpsertImportedFileRequest(FlowFile file, Guid projectUid,
      ImportedFileType importedFileType, DateTime fileCreatedUtc, DateTime fileUpdatedUtc,
      string importedBy, DateTime? surveyedUtc)
    {
      // by the time we are here, the file has been uploaded and location is in file. Some validation:
      if (file == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(27),
            contractExecutionStatesEnum.FirstNameWithOffset(27)));
      }

      if (file.flowFilename.Length > MaxFileNameLength || string.IsNullOrEmpty(file.flowFilename) ||
          file.flowFilename.IndexOfAny(Path.GetInvalidPathChars()) > 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(28),
            contractExecutionStatesEnum.FirstNameWithOffset(28)));
      }

      if (string.IsNullOrEmpty(file.path) ||
          file.path.IndexOfAny(Path.GetInvalidPathChars()) > 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(29),
            contractExecutionStatesEnum.FirstNameWithOffset(29)));
      }

      if (projectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(5),
            contractExecutionStatesEnum.FirstNameWithOffset(5)));
      }

      if (!Enum.IsDefined(typeof(ImportedFileType), importedFileType))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(30),
            contractExecutionStatesEnum.FirstNameWithOffset(30)));
      }

      if (!(importedFileType >= ImportedFileType.Linework && importedFileType <= ImportedFileType.Alignment))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(30),
            contractExecutionStatesEnum.FirstNameWithOffset(31)));
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
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(32),
            contractExecutionStatesEnum.FirstNameWithOffset(32)));
      }

      if (fileCreatedUtc < DateTime.UtcNow.AddYears(-30) || fileCreatedUtc > DateTime.UtcNow.AddDays(2))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(33),
            contractExecutionStatesEnum.FirstNameWithOffset(33)));
      }

      if (fileUpdatedUtc < DateTime.UtcNow.AddYears(-30) || fileUpdatedUtc > DateTime.UtcNow.AddDays(2))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(34),
            contractExecutionStatesEnum.FirstNameWithOffset(34)));
      }

      if (string.IsNullOrEmpty(importedBy))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(35),
            contractExecutionStatesEnum.FirstNameWithOffset(35)));
      }

      if (importedFileType == ImportedFileType.SurveyedSurface && surveyedUtc == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(36),
            contractExecutionStatesEnum.FirstNameWithOffset(36)));
      }
    }
  }
}
