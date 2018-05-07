using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// FileImporter controller
  /// </summary>
  public class FileImportBaseController : BaseController
  {
    /// <summary>
    /// The fileSpaceId.
    /// </summary>
    protected string fileSpaceId;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;
    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IRequestFactory requestFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileImportBaseController"/> class.
    /// </summary>
    /// <param name="producer">The producer.</param>
    /// <param name="projectRepo">The project repo.</param>
    /// <param name="configStore">The configStore.</param>
    /// <param name="raptorProxy">The raptorServices proxy.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="subscriptionRepo"></param>
    /// <param name="fileRepo">For TCC file transfer</param>
    /// <param name="serviceExceptionHandler">For correctly throwing ServiceException errors</param>
    /// <param name="requestFactory"></param>
    /// <param name="log"></param>
    public FileImportBaseController(IKafka producer,
      IConfigurationStore configStore, ILoggerFactory logger, ILogger log, IServiceExceptionHandler serviceExceptionHandler,
      IRaptorProxy raptorProxy,
      IProjectRepository projectRepo, ISubscriptionRepository subscriptionRepo,
      IFileRepository fileRepo, IRequestFactory requestFactory )
      : base(log, configStore, serviceExceptionHandler, producer,
        raptorProxy, projectRepo, subscriptionRepo, fileRepo)
    {
      this.logger = logger;
      this.requestFactory = requestFactory;
    }

    /// <summary>
    /// Validates a project identifier.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns></returns>
    protected async Task ValidateProjectId(string projectUid)
    {
      var customerUid = LogCustomerDetails("GetProject", projectUid);
      var project =
        (await projectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(
          p => string.Equals(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));
      if (project == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
      }

      log.LogInformation($"Project {JsonConvert.SerializeObject(project)} retrieved");
    }

   
    /// <summary>
    /// un-deletes imported file from the Db using the Respositories library.
    /// Used solely for rollback and is never inserted in the kafka que.
    /// </summary>
    /// <returns />
    protected async Task UndeleteImportedFile(Guid projectUid, Guid importedFileUid)
    {
      var nowUtc = DateTime.UtcNow;
      var undeleteImportedFileEvent = new UndeleteImportedFileEvent
      {
        ProjectUID = projectUid,
        ImportedFileUID = importedFileUid,
        ActionUTC = nowUtc,
        ReceivedUTC = nowUtc
      };

      if (await projectRepo.StoreEvent(undeleteImportedFileEvent).ConfigureAwait(false) == 1)
        return;

      serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 51);
    }

    /// <summary>
    /// Writes the importedFile to TCC
    ///   returns filespaceID; path and filename which identifies it uniquely in TCC
    ///   this may be a create or update, so ok if it already exists already
    /// </summary>
    /// <returns></returns>
    protected async Task<FileDescriptor> WriteFileToTCCRepository(string customerUid, string projectUid,
      string pathAndFileName, ImportedFileType importedFileType, DateTime? surveyedUtc)
    {
      var fileStream = new FileStream(pathAndFileName, FileMode.Open);
      var tccPath = $"/{customerUid}/{projectUid}";
      string tccFileName = Path.GetFileName(pathAndFileName);

      if (importedFileType == ImportedFileType.SurveyedSurface)
        if (surveyedUtc != null) // validation should prevent this
          tccFileName = ImportedFileUtils.IncludeSurveyedUtcInName(tccFileName, surveyedUtc.Value);

      bool ccPutFileResult = false;
      bool folderAlreadyExists = false;
      try
      {
        log.LogInformation(
          $"WriteFileToTCCRepository: fileSpaceId {fileSpaceId} tccPath {tccPath} tccFileName {tccFileName}");
        // check for exists first to avoid an misleading exception in our logs.
        folderAlreadyExists = await fileRepo.FolderExists(fileSpaceId, tccPath).ConfigureAwait(false);
        if (folderAlreadyExists == false)
          await fileRepo.MakeFolder(fileSpaceId, tccPath).ConfigureAwait(false);

        // this does an upsert
        ccPutFileResult = await fileRepo.PutFile(fileSpaceId, tccPath, tccFileName, fileStream, fileStream.Length)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "fileRepo.PutFile",
          e.Message);
      }
      finally
      {
        fileStream.Dispose();
      }


      if (ccPutFileResult == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 53);
      }

      log.LogInformation(
        $"WriteFileToTCCRepository: tccFileName {tccFileName} written to TCC. folderAlreadyExists {folderAlreadyExists}");
      return FileDescriptor.CreateFileDescriptor(fileSpaceId, tccPath, tccFileName);
    }


    /// <summary>
    /// Deletes the importedFile from TCC
    /// </summary>
    /// <returns></returns>
    protected async Task DeleteFileFromTCCRepository(FileDescriptor fileDescriptor, Guid projectUid, Guid importedFileUid)
    {
      log.LogInformation($"DeleteFileFromTCCRepository: fileDescriptor {JsonConvert.SerializeObject(fileDescriptor)}");
      bool ccFileExistsResult = false;

      try
      {
        ccFileExistsResult = await fileRepo
          .FileExists(fileDescriptor.filespaceId, fileDescriptor.path + '/' + fileDescriptor.fileName)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"DeleteFileFromTCCRepository FileExists failed with exception. importedFileUid:{importedFileUid}. Exception Thrown: {e.Message}.");

        await UndeleteImportedFile(projectUid, importedFileUid).ConfigureAwait(false);
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "fileRepo.FileExists",
          e.Message);
      }

      if (ccFileExistsResult == true)
      {
        bool ccDeleteFileResult = false;

        try
        {
          ccDeleteFileResult = await fileRepo.DeleteFile(fileDescriptor.filespaceId,
              fileDescriptor.path + '/' + fileDescriptor.fileName)
            .ConfigureAwait(false);
        }
        catch (Exception e)
        {
          log.LogError(
            $"DeleteFileFromTCCRepository FileExists failed with exception. importedFileUid:{importedFileUid}. Exception Thrown: {e.Message}.");

          await UndeleteImportedFile(projectUid, importedFileUid).ConfigureAwait(false);
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "fileRepo.FileExists",
            e.Message);
        }
        if (ccDeleteFileResult == false)
        {
          log.LogError(
            $"DeleteFileFromTCCRepository DeleteFile failed to delete importedFileUid:{importedFileUid}.");

          await UndeleteImportedFile(projectUid, importedFileUid).ConfigureAwait(false);
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 54);
        }
      }
      else
        log.LogInformation(
          $"Unable to put delete fileDescriptor {JsonConvert.SerializeObject(fileDescriptor)} from TCC, as it doesn't exist");
    }


    /// <summary>
    /// Notify raptor of delete file
    ///  if it doesn't know about it then it do nothing and return success
    /// </summary>
    /// <returns></returns>
    protected async Task NotifyRaptorDeleteFile(Guid projectUid, ImportedFileType importedFileType, Guid importedFileUid, string fileDescriptor, long importedFileId, long? legacyImportedFileId)
    {
      BaseDataResult notificationResult = null;
      try
      {
        notificationResult = await raptorProxy
          .DeleteFile(projectUid, importedFileType, importedFileUid, fileDescriptor, importedFileId, legacyImportedFileId, Request.Headers.GetCustomHeaders())
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"FileImport DeleteFile in RaptorServices failed with exception. projectUid:{projectUid} FileDescriptor:{fileDescriptor}. Exception Thrown: {e.Message}.");

        await UndeleteImportedFile(projectUid, importedFileUid).ConfigureAwait(false);
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "raptorProxy.DeleteFile", e.Message);
      }

      log.LogDebug(
        $"FileImport DeleteFile in RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");
      if (notificationResult != null && notificationResult.Code != 0)
      {
        log.LogError($"FileImport DeleteFile in RaptorServices failed. projectUid:{projectUid} FileDescriptor:{fileDescriptor}. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"}");

        await UndeleteImportedFile(projectUid, importedFileUid).ConfigureAwait(false);
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 54, notificationResult.Code.ToString(), notificationResult.Message);
      }
    }

    /// <summary>
    /// Notify raptor of an updated import file.
    /// </summary>
    protected async Task NotifyRaptorUpdateFile(Guid projectUid, IEnumerable<Guid> updatedFileUids)
    {
      var notificationResult = await raptorProxy.UpdateFiles(projectUid, updatedFileUids, Request.Headers.GetCustomHeaders());

      log.LogDebug(
        $"FileImport UpdateFiles in RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult != null && notificationResult.Code != 0)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 54, notificationResult.Code.ToString(), notificationResult.Message);
      }
    }

    /// <summary>
    /// Sets activated state for imported files.
    /// </summary>
    protected async Task<IEnumerable<Guid>> SetFileActivatedState(string projectUid, Dictionary<Guid, bool> fileUids)
    {
      log.LogDebug($"SetFileActivatedState: projectUid={projectUid}, {fileUids.Keys.Count} files with changed state");

      var deactivatedFileList = await ImportedFileRequestHelper.GetImportedFileProjectSettings(projectUid, userId, projectRepo).ConfigureAwait(false) ?? new List<ActivatedFileDescriptor>();
      log.LogDebug($"SetFileActivatedState: originally {deactivatedFileList.Count} deactivated files");

      var missingUids = new List<Guid>();
      foreach (var key in fileUids.Keys)
      {
        //fileUids contains only uids of files whose state has changed.
        //In the project settings we store only deactivated files.
        //Therefore if the value is true remove from the list else add to the list
        if (fileUids[key])
        {
          var item = deactivatedFileList.SingleOrDefault(d => d.ImportedFileUid == key.ToString());
          if (item != null)
          {
            deactivatedFileList.Remove(item);
          }
          else
          {
            missingUids.Add(key);
            log.LogInformation($"SetFileActivatedState: ImportFile '{key}' not found in project settings.");
          }
        }
        else
        {
          deactivatedFileList.Add(new ActivatedFileDescriptor{ImportedFileUid = key.ToString(), IsActivated = false});
        }
      }
      log.LogDebug($"SetFileActivatedState: now {deactivatedFileList.Count} deactivated files, {missingUids.Count} missingUids");

      ProjectSettingsRequest projectSettingsRequest = 
        requestFactory.Create<ProjectSettingsRequestHelper>(r => r
          .CustomerUid(customerUid))
        .CreateProjectSettingsRequest(projectUid, JsonConvert.SerializeObject(deactivatedFileList), ProjectSettingsType.ImportedFiles);
      projectSettingsRequest.Validate();

      var result = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpsertProjectSettingsExecutor>(logger, configStore, serviceExceptionHandler,
            customerUid, userId, null, customHeaders,
            producer, kafkaTopicName,
            null, raptorProxy, null,
            projectRepo)
          .ProcessAsync(projectSettingsRequest)
      ) as ProjectSettingsResult;

      var changedUids = fileUids.Keys.Except(missingUids);
      log.LogDebug($"SetFileActivatedState: {changedUids.Count()} changedUids");
      return changedUids;
    }

  }
}