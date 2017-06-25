using ProjectWebApi.Filters;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using KafkaConsumer.Kafka;
using MasterDataProxies;
using MasterDataProxies.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectWebApiCommon.Models;
using Repositories;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using ProjectWebApiCommon.ResultsHandling;
using Repositories.DBModels;
using TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using ProjectWebApiCommon.Utilities;

namespace Controllers
{
  /// <summary>
  /// FileImporter controller
  /// </summary>
  public class FileImportBaseController : Controller
  {
    protected readonly IKafka producer;
    protected readonly ILogger log;
    protected readonly IRaptorProxy raptorProxy;
    protected readonly IFileRepository fileRepo;

    protected readonly ProjectRepository projectService;
    protected readonly IConfigurationStore store;
    protected readonly string kafkaTopicName;
    protected string fileSpaceId;
    protected ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();


    /// <summary>
    /// Initializes a new instance of the <see cref="FileImportBaseController"/> class.
    /// </summary>
    /// <param name="producer">The producer.</param>
    /// <param name="projectRepo">The project repo.</param>
    /// <param name="store">The store.</param>
    /// <param name="raptorProxy">The raptorServices proxy.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="fileRepo">For TCC file transfer</param>
    public FileImportBaseController(IKafka producer, IRepository<IProjectEvent> projectRepo,
      IConfigurationStore store, IRaptorProxy raptorProxy,
      IFileRepository fileRepo, ILoggerFactory logger)
    {
      log = logger.CreateLogger<FileImportBaseController>();
      this.producer = producer;
      if (!this.producer.IsInitializedProducer)
        this.producer.InitProducer(store);
      //TODO change this pattern, make it safer
      projectService = projectRepo as ProjectRepository;
      this.raptorProxy = raptorProxy;
      this.fileRepo = fileRepo;
      this.store = store;

      kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                       store.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
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
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(1),
            contractExecutionStatesEnum.FirstNameWithOffset(1)));
      }

      log.LogInformation($"Project {JsonConvert.SerializeObject(project)} retrieved");
      return project;
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
    protected async Task<CreateImportedFileEvent> CreateImportedFile(Guid customerUid, Guid projectUid,
      ImportedFileType importedFileType, string filename, DateTime? surveyedUtc,
      string fileDescriptor, DateTime fileCreatedUtc, DateTime fileUpdatedUtc, string importedBy)
    {
      log.LogDebug($"Creating the ImportedFile {filename} for project {projectUid}.");
      var nowUtc = DateTime.UtcNow;
      var createImportedFileEvent = new CreateImportedFileEvent()
      {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileType = importedFileType,
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
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(49),
            contractExecutionStatesEnum.FirstNameWithOffset(49)));

      log.LogDebug($"Created the ImportedFile in DB. ImportedFile {filename} for project {projectUid}.");

      // plug the legacyID back into the struct to be injected into kafka
      var existing = await projectService.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString())
        .ConfigureAwait(false);
      if (existing != null && existing.ImportedFileId > 0)
        createImportedFileEvent.ImportedFileID = existing.ImportedFileId;
      else
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(50),
            contractExecutionStatesEnum.FirstNameWithOffset(50)));

      log.LogDebug(
        $"CreateImportedFile: Legacy importedFileId {createImportedFileEvent.ImportedFileID} for ImportedFile {filename} for project {projectUid}.");
      return createImportedFileEvent;
    }

    /// <summary>
    /// Update an imported file in the Db.
    /// </summary>
    /// <param name="existing">The existing imported file event from the database</param>
    /// <param name="fileDescriptor"></param>
    /// <param name="surveyedUtc"></param>
    /// <param name="fileCreatedUtc"></param>
    /// <param name="fileUpdatedUtc"></param>
    /// <param name="importedBy"></param>
    /// <returns></returns>
    protected async Task<UpdateImportedFileEvent> UpdateImportedFile(
      ImportedFile existing,
      string fileDescriptor, DateTime? surveyedUtc,
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc, string importedBy)
    {
      var nowUtc = DateTime.UtcNow;
      var updateImportedFileEvent = AutoMapperUtility.Automapper.Map<UpdateImportedFileEvent>(existing);
      updateImportedFileEvent.FileDescriptor = fileDescriptor;
      updateImportedFileEvent.SurveyedUtc = surveyedUtc;
      updateImportedFileEvent.FileCreatedUtc = fileCreatedUtc; // as per Barret 19th June 2017
      updateImportedFileEvent.FileUpdatedUtc = fileUpdatedUtc;
      updateImportedFileEvent.ImportedBy = importedBy;
      updateImportedFileEvent.ActionUTC = nowUtc;
      updateImportedFileEvent.ReceivedUTC = nowUtc;

      if (await projectService.StoreEvent(updateImportedFileEvent).ConfigureAwait(false) == 1)
        return updateImportedFileEvent;

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(52),
          contractExecutionStatesEnum.FirstNameWithOffset(52)));
    }

    /// <summary>
    /// Deletes imported file from the Db.
    /// </summary>
    /// <returns />
    protected async Task<DeleteImportedFileEvent> DeleteImportedFile(Guid projejctUid, Guid importedFileUid, bool deletePermanently = false)
    {
      var nowUtc = DateTime.UtcNow;
      var deleteImportedFileEvent = new DeleteImportedFileEvent()
      {
        ProjectUID = projejctUid,
        ImportedFileUID = importedFileUid,
        DeletePermanently = deletePermanently,
        ActionUTC = nowUtc, // aka importedDate
        ReceivedUTC = nowUtc
      };

      if (await projectService.StoreEvent(deleteImportedFileEvent).ConfigureAwait(false) == 1)
        return deleteImportedFileEvent;

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(51),
          contractExecutionStatesEnum.FirstNameWithOffset(51)));
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

      log.LogInformation(
        $"WriteFileToTCCRepository: fileSpaceId {fileSpaceId} tccPath {tccPath} tccFileName {tccFileName}");
      // check for exists first to avoid an misleading exception in our logs.
      var folderAlreadyExists = await fileRepo.FolderExists(fileSpaceId, tccPath).ConfigureAwait(false);
      if (folderAlreadyExists == false)
        await fileRepo.MakeFolder(fileSpaceId, tccPath).ConfigureAwait(false);

      // this does an upsert
      var ccPutFileResult = await fileRepo.PutFile(fileSpaceId, tccPath, tccFileName, fileStream, fileStream.Length)
        .ConfigureAwait(false);
      if (ccPutFileResult == false)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(53),
            contractExecutionStatesEnum.FirstNameWithOffset(53)));
      }

      log.LogInformation(
        $"WriteFileToTCCRepository: tccFileName {tccFileName} written to TCC. folderAlreadyExists {folderAlreadyExists}");
      return FileDescriptor.CreateFileDescriptor(fileSpaceId, tccPath, tccFileName);
    }

    /// <summary>
    /// Deletes the importedFile from TCC
    /// </summary>
    /// <returns></returns>
    protected async Task DeleteFileFromTCCRepository(FileDescriptor fileDescriptor)
    {
      log.LogInformation($"DeleteFileFromTCCRepository: fileDescriptor {JsonConvert.SerializeObject(fileDescriptor)}");

      var ccFileExistsResult = await fileRepo
        .FileExists(fileDescriptor.filespaceId, fileDescriptor.path + '/' + fileDescriptor.fileName)
        .ConfigureAwait(false);
      if (ccFileExistsResult == true)
      {
        var ccDeleteFileResult = await fileRepo.DeleteFile(fileDescriptor.filespaceId,
            fileDescriptor.path + '/' + fileDescriptor.fileName)
          .ConfigureAwait(false);
        if (ccDeleteFileResult == false)
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(54),
              contractExecutionStatesEnum.FirstNameWithOffset(54)));
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
    protected async Task NotifyRaptorAddFile(long? projectId, Guid projectUid, FileDescriptor fileDescriptor, long importedFileId, Guid importedFileUid, bool isCreate)
    {
      MasterDataProxies.ResultHandling.ContractExecutionResult notificationResult;
      try
      {
        notificationResult = await raptorProxy.AddFile(projectUid, importedFileUid,
            JsonConvert.SerializeObject(fileDescriptor), importedFileId, Request.Headers.GetCustomHeaders())
                  .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError($"FileImport AddFile in RaptorServices failed with exception. projectId:{projectId} projectUid:{projectUid} FileDescriptor:{fileDescriptor}. isCreate: {isCreate}. Exception Thrown: {e.Message}. ");
        if (isCreate)
          await DeleteImportedFile(projectUid, importedFileUid).ConfigureAwait(false);

        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(57),
            string.Format(contractExecutionStatesEnum.FirstNameWithOffset(57), "raptorProxy.AddFile", e.Message)));
      }
      log.LogDebug(
        $"NotifyRaptorAddFile: projectId: {projectId} projectUid: {projectUid}, FileDescriptor: {JsonConvert.SerializeObject(fileDescriptor)}. RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult != null && notificationResult.Code != 0)
      {
        log.LogError($"FileImport AddFile in RaptorServices failed. projectId:{projectId} projectUid:{projectUid} FileDescriptor:{fileDescriptor}. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"} isCreate: {isCreate}. ");
        if (isCreate)
          await DeleteImportedFile(projectUid, importedFileUid).ConfigureAwait(false);

        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(notificationResult.Code, notificationResult.Message));
      }
    }

    /// <summary>
    /// Notify raptor of delete file
    ///  if it doesn't know about it then it do nothing and return success
    /// </summary>
    /// <returns></returns>
    protected async Task NotifyRaptorDeleteFile(Guid projectUid, string fileDescriptor, long importedFileId, Guid importedFileUid)
    {
      var notificationResult = await raptorProxy
        .DeleteFile(projectUid, importedFileUid, fileDescriptor, importedFileId, Request.Headers.GetCustomHeaders()).ConfigureAwait(false);
      log.LogDebug(
        $"FileImport DeleteFile in RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");
      if (notificationResult != null && notificationResult.Code != 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(54),
            string.Format(contractExecutionStatesEnum.FirstNameWithOffset(54), (notificationResult?.Code ?? -1),
              (notificationResult?.Message ?? "null"))
          ));
      }
    }

    /// <summary>
    /// Notify raptor of an updated import file.
    /// </summary>
    protected async Task NotifyRaptorUpdateFile(Guid projectUid, string fileDescriptor, long importedFileId)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Sets activated state for imported files.
    /// </summary>
    protected async Task<UpdateImportedFileEvent> SetFileActivatedState(Guid projectUid, IEnumerable<Guid> fileUids, string fileDescriptor)
    {
      throw new NotImplementedException();
      //  var notificationResult = await raptorProxy.UpdateFiles(projectUid, fileUids, Request.Headers.GetCustomHeaders());

      //  log.LogDebug(
      //    $"FileImport DeleteFile in RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");
      //  if (notificationResult != null && notificationResult.Code != 0)
      //  {
      //    throw new ServiceException(HttpStatusCode.BadRequest,
      //      new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(54),
      //        string.Format(contractExecutionStatesEnum.FirstNameWithOffset(54), (notificationResult?.Code ?? -1),
      //          notificationResult?.Message ?? "null")
      //      ));
      //  }

      //log.LogInformation($"ActivateFile list contains {importedFileUids.Count} importedFiles");

      //  var nowUtc = DateTime.UtcNow;
      //  var updateImportedFileEvent = AutoMapperUtility.Automapper.Map<UpdateImportedFileEvent>(existing);
      //  updateImportedFileEvent.ActionUTC = nowUtc;
      //  updateImportedFileEvent.ReceivedUTC = nowUtc;

      //  var messagePayload = JsonConvert.SerializeObject(new { UpdateImportedFileEvent = updateImportedFileEvent });

      //  producer.Send(kafkaTopicName,
      //    new List<KeyValuePair<string, string>>
      //    {
      //      new KeyValuePair<string, string>(updateImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
      //    });

      //  if (await projectService.StoreEvent(updateImportedFileEvent).ConfigureAwait(false) == 1)
      //  {
      //    return updateImportedFileEvent;
      //  }

      //  throw new ServiceException(HttpStatusCode.BadRequest,
      //    new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(52),
      //      contractExecutionStatesEnum.FirstNameWithOffset(52)));




      //        var fileStream = new FileStream(pathAndFileName, FileMode.Open);
      //  var tccPath = $"/{customerUid}/{projectUid}";
      //  string tccFileName = Path.GetFileName(pathAndFileName);

      //  if (importedFileType == ImportedFileType.SurveyedSurface)
      //    if (surveyedUtc != null) // validation should prevent this
      //      tccFileName = GeneratedFileName(tccFileName, GeneratedSuffix(surveyedUtc.Value),
      //        Path.GetExtension(tccFileName));

      //  log.LogInformation(
      //    $"WriteFileToRepository: fileSpaceId {fileSpaceId} tccPath {tccPath} tccFileName {tccFileName}");
      //  // check for exists first to avoid an misleading exception in our logs.
      //  var folderAlreadyExists = await fileRepo.FolderExists(fileSpaceId, tccPath).ConfigureAwait(false);
      //  if (folderAlreadyExists == false)
      //    await fileRepo.MakeFolder(fileSpaceId, tccPath).ConfigureAwait(false);

      //  // this does an upsert
      //  var ccPutFileResult = await fileRepo.PutFile(fileSpaceId, tccPath, tccFileName, fileStream, fileStream.Length)
      //    .ConfigureAwait(false);
      //  if (ccPutFileResult == false)
      //  {
      //    throw new ServiceException(HttpStatusCode.InternalServerError,
      //      new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(53),
      //        contractExecutionStatesEnum.FirstNameWithOffset(53)));
      //  }

      //  log.LogInformation(
      //    $"WriteFileToRepository: tccFileName {tccFileName} written to TCC. folderAlreadyExists {folderAlreadyExists}");
      //  return FileDescriptor.CreateFileDescriptor(fileSpaceId, tccPath, tccFileName);
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
      // var customerUid = ((User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      log.LogInformation($"{functionName}: CustomerUID={customerUid} and projectUid={projectUid}");

      return customerUid;
    }
    #endregion

  }
}
