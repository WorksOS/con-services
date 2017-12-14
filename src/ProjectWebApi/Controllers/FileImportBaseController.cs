using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Project.WebAPI.Filters;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// FileImporter controller
  /// </summary>
  public class FileImportBaseController : Controller
  {
    /// <summary>
    /// Local log provider.
    /// </summary>
    protected readonly ILogger log;

    /// <summary>
    /// The ServiceException handler.
    /// </summary>
    protected IServiceExceptionHandler ServiceExceptionHandler;
    /// <summary>
    /// The fileSpaceId.
    /// </summary>
    protected string fileSpaceId;

    private readonly IConfigurationStore store;
    private readonly IRaptorProxy raptorProxy;
    private readonly IFileRepository fileRepo;
    protected readonly ProjectRepository projectService;

    /// <summary>
    /// 
    /// </summary>
    protected readonly IKafka producer;

    /// <summary>
    /// 
    /// </summary>
    protected readonly string kafkaTopicName;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileImportBaseController"/> class.
    /// </summary>
    /// <param name="producer">The producer.</param>
    /// <param name="projectRepo">The project repo.</param>
    /// <param name="store">The configStore.</param>
    /// <param name="raptorProxy">The raptorServices proxy.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="fileRepo">For TCC file transfer</param>
    /// <param name="serviceExceptionHandler">For correctly throwing ServiceException errors</param>
    public FileImportBaseController(IKafka producer, IRepository<IProjectEvent> projectRepo,
      IConfigurationStore store, IRaptorProxy raptorProxy,
      IFileRepository fileRepo, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler)
    {
      log = logger.CreateLogger<FileImportBaseController>();
      this.producer = producer;
      if (!this.producer.IsInitializedProducer)
        this.producer.InitProducer(store);
      projectService = projectRepo as ProjectRepository;
      this.raptorProxy = raptorProxy;
      this.fileRepo = fileRepo;
      this.store = store;

      ServiceExceptionHandler = serviceExceptionHandler;
      kafkaTopicName = (store.GetValueString("PROJECTSERVICE_KAFKA_TOPIC_NAME") +
                        store.GetValueString("KAFKA_TOPIC_NAME_SUFFIX")).Trim();
    }

    /// <summary>
    /// Gets the project.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns></returns>
    protected async Task<Repositories.DBModels.Project> GetProject(string projectUid)
    {
      var customerUid = LogCustomerDetails("GetProject", projectUid);
      var project =
        (await projectService.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(
          p => string.Equals(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));
      if (project == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
      }

      log.LogInformation($"Project {JsonConvert.SerializeObject(project)} retrieved");
      return project;
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
        (await projectService.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(
          p => string.Equals(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));
      if (project == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
      }

      log.LogInformation($"Project {JsonConvert.SerializeObject(project)} retrieved");
    }

    /// <summary>
    /// Gets the imported file list for a project
    /// </summary>
    /// <returns></returns>
    protected async Task<ImmutableList<ImportedFile>> GetImportedFiles(string projectUid)
    {
      LogCustomerDetails("GetImportedFiles", projectUid);

      var importedFiles = (await projectService.GetImportedFiles(projectUid).ConfigureAwait(false))
        .ToImmutableList();

      log.LogInformation($"ImportedFile list contains {importedFiles.Count()} importedFiles");
      return importedFiles;
    }

    /// <summary>
    /// Gets the imported file list for a project in Response
    /// </summary>
    /// <returns></returns>
    protected async Task<ImmutableList<ImportedFileDescriptor>> GetImportedFileList(string projectUid)
    {
      LogCustomerDetails("GetImportedFileList", projectUid);

      var importedFiles = (await projectService.GetImportedFiles(projectUid).ConfigureAwait(false))
        .ToImmutableList();

      log.LogInformation($"ImportedFile list contains {importedFiles.Count()} importedFiles");

      var importedFileList = importedFiles.Select(importedFile =>
          AutoMapperUtility.Automapper.Map<ImportedFileDescriptor>(importedFile))
        .ToImmutableList();

      return importedFileList;
    }


    /// <summary>
    /// Creates an imported file in Db.
    /// </summary>
    /// <returns />
    protected async Task<CreateImportedFileEvent> CreateImportedFileinDb(Guid customerUid, Guid projectUid,
      ImportedFileType importedFileType, DxfUnitsType dxfUnitsType, string filename, DateTime? surveyedUtc,
      string fileDescriptor, DateTime fileCreatedUtc, DateTime fileUpdatedUtc, string importedBy)
    {
      log.LogDebug($"Creating the ImportedFile {filename} for project {projectUid}.");
      var nowUtc = DateTime.UtcNow;
      var createImportedFileEvent = new CreateImportedFileEvent
      {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileType = importedFileType,
        DxfUnitsType = dxfUnitsType,
        Name = filename,
        FileDescriptor = fileDescriptor,
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        ImportedBy = importedBy,
        SurveyedUTC = surveyedUtc,
        ActionUTC = nowUtc, // aka importedUtc
        ReceivedUTC = nowUtc
      };

      var isCreated = await projectService.StoreEvent(createImportedFileEvent).ConfigureAwait(false);
      if (isCreated == 0)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 49);
      }

      log.LogDebug($"Created the ImportedFile in DB. ImportedFile {filename} for project {projectUid}.");

      // plug the legacyID back into the struct to be injected into kafka
      var existing = await projectService.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString())
        .ConfigureAwait(false);
      if (existing != null && existing.ImportedFileId > 0)
        createImportedFileEvent.ImportedFileID = existing.ImportedFileId;
      else
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 50);
      }

      log.LogDebug(
        $"CreateImportedFileinDb: Legacy importedFileId {createImportedFileEvent.ImportedFileID} for ImportedFile {filename} for project {projectUid}.");
      return createImportedFileEvent;
    }

    /// <summary>
    /// Update an imported file in the Db.
    /// </summary>
    /// <param name="existing">The existing imported file event from the database</param>
    /// <param name="fileDescriptor"></param>
    /// <param name="surveyedUtc"></param>
    /// <param name="minZoom"></param>
    /// <param name="maxZoom"></param>
    /// <param name="fileCreatedUtc"></param>
    /// <param name="fileUpdatedUtc"></param>
    /// <param name="importedBy"></param>
    /// <returns></returns>
    protected async Task<UpdateImportedFileEvent> UpdateImportedFileInDb(
      ImportedFile existing,
      string fileDescriptor, DateTime? surveyedUtc, int minZoom, int maxZoom,
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc, string importedBy)
    {
      var nowUtc = DateTime.UtcNow;
      var updateImportedFileEvent = AutoMapperUtility.Automapper.Map<UpdateImportedFileEvent>(existing);
      updateImportedFileEvent.FileDescriptor = fileDescriptor;
      updateImportedFileEvent.SurveyedUtc = surveyedUtc;
      updateImportedFileEvent.MinZoomLevel = minZoom;
      updateImportedFileEvent.MaxZoomLevel = maxZoom;
      updateImportedFileEvent.FileCreatedUtc = fileCreatedUtc; // as per Barret 19th June 2017
      updateImportedFileEvent.FileUpdatedUtc = fileUpdatedUtc;
      updateImportedFileEvent.ImportedBy = importedBy;
      updateImportedFileEvent.ActionUTC = nowUtc;
      updateImportedFileEvent.ReceivedUTC = nowUtc;

      if (await projectService.StoreEvent(updateImportedFileEvent).ConfigureAwait(false) == 1)
        return updateImportedFileEvent;

      ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 52);
      return updateImportedFileEvent;
    }

    /// <summary>
    /// Deletes imported file from the Db.
    /// </summary>
    /// <returns />
    protected async Task<DeleteImportedFileEvent> DeleteImportedFile(Guid projectUid, Guid importedFileUid, bool deletePermanently = false)
    {
      var nowUtc = DateTime.UtcNow;
      var deleteImportedFileEvent = new DeleteImportedFileEvent
      {
        ProjectUID = projectUid,
        ImportedFileUID = importedFileUid,
        DeletePermanently = deletePermanently,
        ActionUTC = nowUtc, // aka importedDate
        ReceivedUTC = nowUtc
      };

      if (await projectService.StoreEvent(deleteImportedFileEvent).ConfigureAwait(false) == 1)
        return deleteImportedFileEvent;

      ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 51);
      return deleteImportedFileEvent;
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

      if (await projectService.StoreEvent(undeleteImportedFileEvent).ConfigureAwait(false) == 1)
        return;

      ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 51);
    }

    /// <summary>
    /// Writes the importedFile to TCC
    ///   returns filespaceID; path and filename which identifies it uniquely in TCC
    ///   this may be a create or update, so ok if it already exists in our DB
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
          tccFileName = GeneratedFileName(tccFileName, GeneratedSuffix(surveyedUtc.Value),
            Path.GetExtension(tccFileName));

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
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "fileRepo.PutFile",
          e.Message);
      }
      finally
      {
        fileStream.Dispose();
      }


      if (ccPutFileResult == false)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 53);
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
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "fileRepo.FileExists",
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
          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "fileRepo.FileExists",
            e.Message);
        }
        if (ccDeleteFileResult == false)
        {
          log.LogError(
            $"DeleteFileFromTCCRepository DeleteFile failed to delete importedFileUid:{importedFileUid}.");

          await UndeleteImportedFile(projectUid, importedFileUid).ConfigureAwait(false);
          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 54);
        }
      }
      else
        log.LogInformation(
          $"Unable to put delete fileDescriptor {JsonConvert.SerializeObject(fileDescriptor)} from TCC, as it doesn't exist");
    }


    /// <summary>
    /// Notify raptor of new file
    ///     if it already knows about it, it will just update and re-notify raptor and return success.
    /// </summary>
    /// <returns></returns>
    protected async Task<AddFileResult> NotifyRaptorAddFile(long? projectId, Guid projectUid, ImportedFileType importedFileType, DxfUnitsType dxfUnitsType, FileDescriptor fileDescriptor, long importedFileId, Guid importedFileUid, bool isCreate)
    {
      AddFileResult notificationResult = null;
      try
      {
        notificationResult = await raptorProxy
          .AddFile(projectUid, importedFileType, importedFileUid,
            JsonConvert.SerializeObject(fileDescriptor), importedFileId, dxfUnitsType, Request.Headers.GetCustomHeaders())
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"FileImport AddFile in RaptorServices failed with exception. projectId:{projectId} projectUid:{projectUid} FileDescriptor:{fileDescriptor}. isCreate: {isCreate}. Exception Thrown: {e.Message}. ");
        if (isCreate)
          await DeleteImportedFile(projectUid, importedFileUid, true).ConfigureAwait(false);
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "raptorProxy.AddFile", e.Message);
      }
      log.LogDebug(
        $"NotifyRaptorAddFile: projectId: {projectId} projectUid: {projectUid}, FileDescriptor: {JsonConvert.SerializeObject(fileDescriptor)}. RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult != null && notificationResult.Code != 0)
      {
        log.LogError($"FileImport AddFile in RaptorServices failed. projectId:{projectId} projectUid:{projectUid} FileDescriptor:{fileDescriptor}. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"} isCreate: {isCreate}. ");
        if (isCreate)
          await DeleteImportedFile(projectUid, importedFileUid, true).ConfigureAwait(false);

        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 67, notificationResult.Code.ToString(), notificationResult.Message);
      }
      return notificationResult;
    }

    /// <summary>
    /// Notify raptor of delete file
    ///  if it doesn't know about it then it do nothing and return success
    /// </summary>
    /// <returns></returns>
    protected async Task NotifyRaptorDeleteFile(Guid projectUid, ImportedFileType importedFileType, string fileDescriptor, long importedFileId, Guid importedFileUid)
    {
      BaseDataResult notificationResult = null;
      try
      {
        notificationResult = await raptorProxy
          .DeleteFile(projectUid, importedFileType, importedFileUid, fileDescriptor, importedFileId, Request.Headers.GetCustomHeaders())
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"FileImport DeleteFile in RaptorServices failed with exception. projectUid:{projectUid} FileDescriptor:{fileDescriptor}. Exception Thrown: {e.Message}.");

        await UndeleteImportedFile(projectUid, importedFileUid).ConfigureAwait(false);
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "raptorProxy.DeleteFile", e.Message);
      }

      log.LogDebug(
        $"FileImport DeleteFile in RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");
      if (notificationResult != null && notificationResult.Code != 0)
      {
        log.LogError($"FileImport DeleteFile in RaptorServices failed. projectUid:{projectUid} FileDescriptor:{fileDescriptor}. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"}");

        await UndeleteImportedFile(projectUid, importedFileUid).ConfigureAwait(false);
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 54, notificationResult.Code.ToString(), notificationResult.Message);
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
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 54, notificationResult.Code.ToString(), notificationResult.Message);
      }
    }

    /// <summary>
    /// Sets activated state for imported files.
    /// </summary>
    protected async Task<IEnumerable<UpdateImportedFileEvent>> SetFileActivatedState(Guid projectUid, Dictionary<Guid, bool> fileUids)
    {
      var result = new List<UpdateImportedFileEvent>();

      foreach (var uid in fileUids)
      {
        var file = await projectService.GetImportedFile(uid.Key.ToString());
        if (file == null)
        {
          continue;
        }

        file.IsActivated = uid.Value;

        var nowUtc = DateTime.UtcNow;
        var updateImportedFileEvent = AutoMapperUtility.Automapper.Map<UpdateImportedFileEvent>(file);
        updateImportedFileEvent.ActionUTC = nowUtc;
        updateImportedFileEvent.ReceivedUTC = nowUtc;

        var messagePayload = JsonConvert.SerializeObject(new { UpdateImportedFileEvent = updateImportedFileEvent });
        producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>
          {
            new KeyValuePair<string, string>(updateImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
          });

        if (await projectService.StoreEvent(updateImportedFileEvent) == 1)
        {
          result.Add(updateImportedFileEvent);
        }
        else
        {
          log.LogInformation($"SetFileActivatedState: Failed to set activation state to {updateImportedFileEvent.IsActivated} on ImportFile '{updateImportedFileEvent.ImportedFileUID}'.");
        }
      }

      return result;
    }


    #region private

    private static string GeneratedFileName(string fileName, string suffix, string extension)
    {
      return Path.GetFileNameWithoutExtension(fileName) + suffix + extension;
    }

    private static string GeneratedSuffix(DateTime surveyedUtc)
    {
      //Note: ':' is an invalid character for filenames in Windows so get rid of them
      return "_" + surveyedUtc.ToIso8601DateTimeString().Replace(":", string.Empty);
    }

    private string LogCustomerDetails(string functionName, string projectUid)
    {
      var customerUid = (User as TIDCustomPrincipal).CustomerUid;
      log.LogInformation($"{functionName}: CustomerUID={customerUid} and projectUid={projectUid}");

      return customerUid;
    }
    #endregion
  }
}