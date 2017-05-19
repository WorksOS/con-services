
using System.Net;
using Microsoft.Extensions.Logging;
using TCCFileAccess;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Notification.Models;
using WebApiModels.Interfaces;
using WebApiModels.Notification.Helpers;

namespace VSS.Raptor.Service.WebApiModels.Notification.Executors
{
  /// <summary>
  /// Processes the request to delete a file.
  /// Action taken depends on the file type.
  /// </summary>
  public class DeleteFileExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="raptorClient"></param>
    /// <param name="fileRepository"></param>
    /// <param name="tileGenerator"></param>
    public DeleteFileExecutor(ILoggerFactory logger, IASNodeClient raptorClient, IFileRepository fileRepository, ITileGenerator tileGenerator) : base(logger, raptorClient, null, null, fileRepository, tileGenerator)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DeleteFileExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        ProjectFileDescriptor request = item as ProjectFileDescriptor;
        ImportedFileTypeEnum fileType = FileUtils.GetFileType(request.File.fileName);

        //Only alignment files at present
        if (fileType != ImportedFileTypeEnum.Alignment)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
                "Unsupported file type"));
        }

        if (fileType == ImportedFileTypeEnum.DesignSurface ||
            fileType == ImportedFileTypeEnum.Alignment ||
            fileType == ImportedFileTypeEnum.Linework)
        {
          var suffix = FileUtils.GeneratedFileSuffix(fileType);
          //Delete generated files
          bool success = DeleteGeneratedFile(request.File, suffix, FileUtils.PROJECTION_FILE_EXTENSION) &&
                         DeleteGeneratedFile(request.File, suffix, FileUtils.HORIZONTAL_ADJUSTMENT_FILE_EXTENSION);
          if (fileType != ImportedFileTypeEnum.Linework)
          {
            success = success && DeleteGeneratedFile(request.File, suffix, FileUtils.DXF_FILE_EXTENSION);
          }
          if (!success)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                "Failed to delete generated files"));
          }
          //Delete tiles 
          tileGenerator.DeleteDxfTiles(request.projectId.Value, request.File, suffix);
        }


        //If surveyed surface, delete it in Raptor
        if (fileType == ImportedFileTypeEnum.SurveyedSurface)
        {
          log.LogDebug("Discarding ground surface file in Raptor");
          if (!raptorClient.DiscardGroundSurfaceFileDetails(request.projectId.Value, request.FileId))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                "Failed to discard ground surface file"));
          }
        }

        return new ContractExecutionResult(ContractExecutionStatesEnum.ExecutedSuccessfully, "Delete file notification successful");        
      }
      finally
      {
      }

    }

    /// <summary>
    /// Delete a generated file associated with the specified file
    /// </summary>
    /// <param name="fileDescr">The original file</param>
    /// <param name="suffix">The suffix applied to the file name to get the generated file name</param>
    /// <param name="extension">The file extension of the generated file</param>
    /// <returns>True if the file is successfully deleted, false otherwise</returns>
    private bool DeleteGeneratedFile(FileDescriptor fileDescr, string suffix, string extension)
    {
      string generatedName = FileUtils.GeneratedFileName(fileDescr.fileName, suffix, extension);
      log.LogDebug("Deleting generated file {0}", generatedName);
      string fullName = fileDescr.path + "/" + generatedName;
      if (fileRepo.FileExists(fileDescr.filespaceId, fullName).Result)
      {
        return fileRepo.DeleteFile(fileDescr.filespaceId, fullName).Result;
      }
      return true;//TODO: Is this what we want if file not there?
    }

 
  }
}
