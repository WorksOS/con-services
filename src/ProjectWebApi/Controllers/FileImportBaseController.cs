using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using KafkaConsumer.Kafka;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectWebApiCommon.Models;
using Repositories;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using ProjectWebApiCommon.ResultsHandling;
using Repositories.DBModels;
using TCCFileAccess;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using ProjectWebApiCommon.Utilities;
using VSS.Raptor.Service.Common.Utilities;

namespace VSP.MasterData.Project.WebAPI.Controllers
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
    protected string userEmailAddress;
    protected string fileSpaceId;

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
      var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      log.LogInformation($"GetProject: CustomerUID={customerUid} and projectUid={projectUid}");
      var project =
        (await projectService.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(
          p => string.Equals(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));
      if (project == null)
      {
        var message = $"No access to the project {projectUid} for customer {customerUid} or project does not exist.";
        log.LogError(message);
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData, message));
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
      var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      log.LogInformation($"GetImportedFiles: CustomerUID={customerUid} and projectUid={projectUid}");
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
      var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      log.LogInformation($"GetImportedFileList: CustomerUID={customerUid} and projectUid={projectUid}");
      var importedFiles = (await projectService.GetImportedFiles(projectUid).ConfigureAwait(false))
        .ToImmutableList();

      log.LogInformation($"ImportedFile list contains {importedFiles.Count()} importedFiles");

      var importedFileList = importedFiles.Select(importedFile =>
          AutoMapperUtility.Automapper.Map<ImportedFileDescriptor>(importedFile))
        .ToImmutableList();

      return importedFileList;
    }

    /// <summary>
    /// Creates an imported file. Writes to Db and creates the Kafka event.
    /// </summary>
    /// <returns />
    protected virtual async Task<CreateImportedFileEvent> CreateImportedFile(Guid customerUid, Guid projectUid,
      ImportedFileType importedFileType, string filename, DateTime? surveyedUtc,
      string fileDescriptor, DateTime fileCreatedUtc, DateTime fileUpdatedUtc, string importedBy)
    {
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
        ActionUTC = nowUtc, // aka importedDate
        ReceivedUTC = nowUtc
      };

      var messagePayload = JsonConvert.SerializeObject(new {CreateImportedFileEvent = createImportedFileEvent});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(createImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
        });

      if (await projectService.StoreEvent(createImportedFileEvent).ConfigureAwait(false) == 1)
        return createImportedFileEvent;

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          $"CreateImportedFileV4. Unable to store Imported File event to database: {JsonConvert.SerializeObject(createImportedFileEvent)}."));
    }

    /// <summary>
    /// Creates an imported file. Writes to Db and creates the Kafka event.
    /// </summary>
    /// <returns />
    protected virtual async Task DeleteImportedFile(Guid projejctUid, Guid importedFileUid)
    {
      var nowUtc = DateTime.UtcNow;
      var deleteImportedFileEvent = new DeleteImportedFileEvent()
      {
        ProjectUID = projejctUid,
        ImportedFileUID = importedFileUid,
        ActionUTC = nowUtc, // aka importedDate
        ReceivedUTC = nowUtc
      };

      var messagePayload = JsonConvert.SerializeObject(new {DeleteImportedFileEvent = deleteImportedFileEvent});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(deleteImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
        });

      if (await projectService.StoreEvent(deleteImportedFileEvent).ConfigureAwait(false) == 1)
        return;

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          $"DeleteImportedFileV4. Unable to set Imported File event to deleted: {JsonConvert.SerializeObject(deleteImportedFileEvent)}."));
    }

    /// <summary>
    /// Creates an imported file. Writes to Db and creates the Kafka event.
    /// only thing which should change here is a) FileUpdatedUtc/ActionUtc
    ///       file Descriptor???
    /// </summary>
    /// <param name="importedFileDescriptor">The existing imported file event</param>
    /// <param name="fileDescriptor"></param>
    /// <param name="surveyedUtc"></param>
    /// <param name="fileUpdatedUtc"></param>
    /// <param name="importedBy"></param>
    /// <returns></returns>
    protected virtual async Task<UpdateImportedFileEvent> UpdateImportedFile(
      ImportedFileDescriptor importedFileDescriptor,
      string fileDescriptor, DateTime? surveyedUtc,
      DateTime fileUpdatedUtc, string importedBy)
    {
      var nowUtc = DateTime.UtcNow;
      var updateImportedFileEvent = new UpdateImportedFileEvent()
      {
        ProjectUID = Guid.Parse(importedFileDescriptor.ProjectUid),
        ImportedFileUID = Guid.Parse(importedFileDescriptor.ImportedFileUid),
        FileDescriptor = fileDescriptor,
        FileCreatedUtc = importedFileDescriptor.FileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        ImportedBy = importedBy,
        SurveyedUtc = surveyedUtc,
        ActionUTC = nowUtc,
        ReceivedUTC = nowUtc
      };

      var messagePayload = JsonConvert.SerializeObject(new {UpdateImportedFileEvent = updateImportedFileEvent});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(updateImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
        });

      if (await projectService.StoreEvent(updateImportedFileEvent).ConfigureAwait(false) == 1)
        return updateImportedFileEvent;

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          $"CreateImportedFileV4. Unable to store updated Imported File event to database: {JsonConvert.SerializeObject(updateImportedFileEvent)}."));
    }

    /// <summary>
    /// Writes the importedFile to TCC
    /// </summary>
    /// <returns></returns>
    protected async Task<FileDescriptor> WriteFileToRepository(string customerUid, string projectUid,
      string pathAndFileName, ImportedFileType importedFileType, DateTime? surveyedUtc)
    {
        var fileStream = new FileStream(pathAndFileName, FileMode.Open);
        var tccPath = $"/{customerUid}/{projectUid}";
        string tccFileName = Path.GetFileName(pathAndFileName);

        if (importedFileType == ImportedFileType.SurveyedSurface)
          if (surveyedUtc != null) // validation should prevent this
            tccFileName = GeneratedFileName(tccFileName, GeneratedSuffix(surveyedUtc.Value),
              Path.GetExtension(tccFileName));

        log.LogInformation($"WriteFileToRepository: fileSpaceId {fileSpaceId} tccPath {tccPath} tccFileName {tccFileName}");
        // ignore any failure as dir may already exist
        var ccMakeFolderResult = await fileRepo.MakeFolder(fileSpaceId, tccPath).ConfigureAwait(false);
       
        // this does an upsert
        var ccPutFileResult = await fileRepo.PutFile(fileSpaceId, tccPath, tccFileName, fileStream, fileStream.Length).ConfigureAwait(false);
        if (ccPutFileResult == false)
        {
        var error = $"WriteFileToRepository: Unable to put file to TCC. TCCfileSpaceId { fileSpaceId} tccPath {tccPath} tccFileName {tccFileName}";
        log.LogError(error);
        throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.TCCReturnFailure, error));
        }

        return FileDescriptor.CreateFileDescriptor(fileSpaceId, tccPath, tccFileName);
    }

    /// <summary>
    /// Deletes the importedFile from TCC
    /// </summary>
    /// <returns></returns>
    protected async Task DeleteFileFromRepository(FileDescriptor fileDescriptor)
    {
      log.LogInformation($"DeleteFileFromRepository: fileDescriptor {JsonConvert.SerializeObject(fileDescriptor)}");

      var ccFileExistsResult = await fileRepo.FileExists(fileDescriptor.filespaceId, fileDescriptor.path + '/' + fileDescriptor.fileName).ConfigureAwait(false);
      if (ccFileExistsResult == true)
      {
        var ccDeleteFileResult = await fileRepo.DeleteFile(fileDescriptor.filespaceId,
            fileDescriptor.path + '/' + fileDescriptor.fileName)
          .ConfigureAwait(false);
        if (ccDeleteFileResult == false)
        {
          var error = $"Unable to put delete fileDescriptor {JsonConvert.SerializeObject(fileDescriptor)} from TCC";
          log.LogError(error);
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.TCCReturnFailure, error));
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
    protected async Task NotifyRaptorAddFile(long? projectId, Guid projectUid, FileDescriptor fileDescriptor)
    {
      var notificationResult = await raptorProxy.AddFile(projectId, projectUid, JsonConvert.SerializeObject(fileDescriptor), Request.Headers.GetCustomHeaders()).ConfigureAwait(false);
      log.LogDebug(
        $"NotifyRaptorAddFile: projectId: {projectId} projectUid: {projectUid}, FileDescriptor: {JsonConvert.SerializeObject(fileDescriptor)}. RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");
      if (notificationResult != null && notificationResult.Code != 0)
      {
        var error =
          $"FileImport AddFile in RaptorServices failed. projectId:{projectId} projectUid:{projectUid} FileDescriptor:{fileDescriptor}. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"}. ";
        log.LogError(error);
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(notificationResult.Code, notificationResult.Message));
      }
    }

    /// <summary>
    /// Notify raptor of delete file
    ///  if it doesn't know about it then it do nothing and return success
    /// </summary>
    /// <returns></returns>
    protected async Task NotifyRaptorDeleteFile(Guid projectUid, string fileDescriptor)
    {
      var notificationResult = await raptorProxy.DeleteFile(null, projectUid, fileDescriptor, Request.Headers.GetCustomHeaders()).ConfigureAwait(false);
      log.LogDebug(
        $"FileImport DeleteFile in RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");
      if (notificationResult != null && notificationResult.Code != 0)
      {
        var error =
          $"FileImport DeleteFile in RaptorServices failed. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"}. ";
        log.LogError(error);
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, error));
      }
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

    #endregion

  }
}
