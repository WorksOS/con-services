using System;
using System.Net;
using ProjectWebApiCommon.ResultsHandling;
using System.IO;
using FlowUploadFilter;
using Repositories.DBModels;

namespace ProjectWebApiCommon.Models
{
  /// <summary>
  /// Validates all file import data
  /// </summary>
  public class FileImportDataValidator
  {
    private const int MaxFileNameLength = 256;

    /// <summary>
    /// Validate the request e.g that the file has been uploaded and project is as expected.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="projectUid"></param>
    /// <param name="importedFileType"></param>
    /// <param name="surveyedSurfaceUtc"></param>
    public static void ValidateImportedFileRequest(FlowFile file, Guid projectUid,
      ImportedFileType importedFileType, DateTime? surveyedSurfaceUtc)
    {
      // by the time we are here, the file has been uploaded and location is in file. Some validation:
      if (file == null)
      {
        var error = "CreateImportedFileV4.The file was not imported successfully";
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, error));
      }

      if (file.flowFilename.Length > MaxFileNameLength || string.IsNullOrEmpty(file.flowFilename) ||
          file.flowFilename.IndexOfAny(Path.GetInvalidPathChars()) > 0)
      {
        var error = "CreateImportedFileV4.Supplied filename is not valid. Either exceeds the length limit of 256 is empty or contains illegal characters.";
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, error));
      }

      if (string.IsNullOrEmpty(file.path) ||
          file.path.IndexOfAny(Path.GetInvalidPathChars()) > 0)
      {
        var error = string.Format(
          $"CreateImportedFileV4.Supplied path {0} is not valid.Either is empty or contains illegal characters.",
          file.path);

        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, error));
      }

      if (projectUid == Guid.Empty)
      {
        var error = string.Format($"The projectUid is invalid {0}.", projectUid);
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, error));
      }

      if (!Enum.IsDefined(typeof(ImportedFileType), importedFileType))
      {
        var error = string.Format(
          "CreateImportedFileV4. ImportedFileType: {0}, is an unrecognized type.",
          importedFileType.ToString());

        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, error));
      }

      if (importedFileType != ImportedFileType.Alignment)
      {
        var error = string.Format(
          "CreateImportedFileV4. ImportedFileType: {0}, is invalid. Only Alignment file types are supported at present",
          importedFileType.ToString());

        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, error));
      }


      var fileExtension = Path.GetExtension(file.flowFilename);
      if (!(
        (importedFileType == ImportedFileType.Linework && fileExtension == ".dxf") ||
        (importedFileType == ImportedFileType.DesignSurface && fileExtension == ".ttm") ||
        (importedFileType == ImportedFileType.SurveyedSurface && fileExtension == ".ttm") ||
        (importedFileType == ImportedFileType.Alignment && fileExtension == ".svl")
      ))
      {
        var error = string.Format(
          "CreateImportedFileV4. ImportedFileType {0} does not match the file extension received {1}.",
          importedFileType.ToString(), Path.GetExtension(file.flowFilename));

        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, error));
      }

      if (importedFileType == ImportedFileType.SurveyedSurface && surveyedSurfaceUtc == null)
      {
        var error = string.Format("The SurveyedSurfaceUtc {0} is not available.", (surveyedSurfaceUtc == null ? "n/A" : surveyedSurfaceUtc.ToString()));

        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, error));
      }
    }
  }
}
