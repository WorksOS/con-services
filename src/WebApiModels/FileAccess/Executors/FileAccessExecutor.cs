using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using TCCFileAccess;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.FileAccess.Models;
using VSS.Raptor.Service.WebApiModels.FileAccess.ResultHandling;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.WebApiModels.FileAccess.Helpers;


namespace VSS.Raptor.Service.WebApiModels.FileAccess.Executors
{
  /// <summary>
  /// The executor which gets the requested file from TCC and stores a copy in the requested location.
  /// </summary>
  public class FileAccessExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="raptorClient"></param>
    /// <param name="tagProcessor"></param>
    /// <param name="configStore"></param>
    /// <param name="fileAccess"></param>
    /// 
    public FileAccessExecutor(ILoggerFactory logger, IASNodeClient raptorClient, ITagProcessor tagProcessor, IConfigurationStore configStore, IFileRepository fileAccess) 
      : base(logger, raptorClient, tagProcessor, configStore, fileAccess)
    {
      // ...
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public FileAccessExecutor()
    {
      // ...
    }

    /// <summary>
    /// Processes the file access request by getting the file from TCC ,storing it in the required location and returning the result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a FileAccessResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      bool success = false;

      FileAccessRequest request = item as FileAccessRequest;
      log.LogInformation("FileAccessExecutor: {0}: {1}\\{2} --> {3}", request.file.filespaceId, request.file.path, request.file.fileName, request.localPath);

      if (File.Exists(request.localPath))
      {
        // Requested file is already present - nothing more to do
        log.LogInformation("FileAccessExecutor: File {0}: {1}\\{2} already present in local path {3}",
            request.file.filespaceId, request.file.path, request.file.fileName, request.localPath);
        success = true;
      }
      else
      {
        try
        {
          if (fileAccess != null)
          {
            FileAccessHelper.DownloadFile(fileAccess, request);

            success = File.Exists(request.localPath);

            if (success)
            {
              log.LogInformation("FileAccessExecutor: Succeeded in reading {0}: {1}\\{2} to local path {3}",
                  request.file.filespaceId, request.localPath, request.file.path, request.file.fileName);
            }
            else
            {
              log.LogInformation("FileAccessExecutor: Failed to read {0}: {1}\\{2} to local path {3} (local file does not exist)",
                      request.file.filespaceId, request.localPath, request.file.path, request.file.fileName);
            }
          }
          else
          {
            log.LogInformation("Unable to log into TCC as FileAccessExecutor user.");
          }
        }
        catch (Exception ex)
        {
          log.LogError(null, ex, "***ERROR*** FileAccessExecutor: Failed on getting {0} file from TCC!",
              request.file.fileName);
        }
      }

      if (success)
      {
        return FileAccessResult.CreateFileAccessResult();
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to download file from TCC"));
      }
    }

    protected override void ProcessErrorCodes()
    {
      //Nothing to do
    }
  }
}
