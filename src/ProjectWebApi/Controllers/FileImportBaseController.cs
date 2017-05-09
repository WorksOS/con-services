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
using log4net.Appender;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using ProjectWebApiCommon.Models;
using Repositories;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using ProjectWebApiCommon.ResultsHandling;
using Repositories.DBModels;
using TCCFileAccess;
using TCCFileAccess.Models;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using ProjectWebApiCommon.Utilities;

namespace VSP.MasterData.Project.WebAPI.Controllers
{
  public class FileImportBaseController : Controller
  {
    protected readonly IKafka producer;
    protected readonly ILogger log;
    protected readonly IRaptorProxy raptorProxy;
    protected readonly IFileRepository fileRepo;

    protected readonly ProjectRepository projectService;
    protected readonly IConfigurationStore store;
    protected readonly string kafkaTopicName;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileImportBaseController"/> class.
    /// </summary>
    /// <param name="producer">The producer.</param>
    /// <param name="projectRepo">The project repo.</param>
    /// <param name="store">The store.</param>
    /// <param name="subsProxy">The subs proxy.</param>
    /// <param name="raptorProxy">The raptorServices proxy.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="fileRepo">For TCC file transfer</param>
    public FileImportBaseController(IKafka producer, IRepository<IProjectEvent> projectRepo,
      IConfigurationStore store, IRaptorProxy raptorProxy, 
      IFileRepository fileRepo, ILoggerFactory logger)
    {
      log = logger.CreateLogger<ProjectBaseController>();
      this.producer = producer;
      //We probably want to make this thing singleton?
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
      log.LogInformation("CustomerUID=" + customerUid + " and user=" + User);
      var project =
        (await projectService.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(
          p => p.ProjectUID == projectUid);

      if (project == null)
      {
        log.LogWarning($"User doesn't have access to {projectUid}");
        throw new ServiceException(HttpStatusCode.Forbidden,
          new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
            "No access to the project for a customer or project does not exist."));
      }

      log.LogInformation($"Project {projectUid} retrieved");
      return project;
    }


    /// <summary>
    /// Gets the imported file list for a project
    /// </summary>
    /// <returns></returns>
    protected async Task<ImmutableList<ImportedFileDescriptor>> GetImportedFileList(string projectUid)
    {
      var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      log.LogInformation("CustomerUID=" + customerUid + " and user=" + User + " and projectUid=" + projectUid);
      var importedFiles = (await projectService.GetImportedFiles(projectUid).ConfigureAwait(false))
        .ToImmutableList();

      log.LogInformation($"ImportedFile list contains {importedFiles.Count()} importedFiles");

      var importedFileList = importedFiles.Select(importedFile => new ImportedFileDescriptor()
        {
          ProjectUid = importedFile.ProjectUid,
          ImportedFileUid = importedFile.ImportedFileUid,
          CustomerUid = importedFile.CustomerUid,
          ImportedFileType = importedFile.ImportedFileType,
          Name = importedFile.Name,
          FileCreatedUtc = importedFile.FileCreatedUtc,
          FileUpdatedUtc = importedFile.FileUpdatedUtc,
          ImportedBy = importedFile.ImportedBy,
          SurveyedUtc = importedFile.SurveyedUtc
        })
        .ToImmutableList();

      return importedFileList;
    }

    /// <summary>
    /// Creates an imported file. Writes to Db and creates the Kafka event.
    /// </summary>
    /// <param name="importFile">The create imported file event</param>
    /// <returns></returns>
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
          @"CreateImportedFileV4. Unable to store Imported File event to database: {JsonConvert.SerializeObject(importFile)}."));
      return null;
    }

    /// <summary>
    /// Creates an imported file. Writes to Db and creates the Kafka event.
    /// only thing which should change here is a) FileUpdatedUtc/ActionUtc
    ///       file Descriptor???
    /// </summary>
    /// <param name="importedFileDescriptor">The existing imported file event</param>
    /// <param name="fileDescriptor"></param>
    /// <param name="surveyedUtc"></param>
    /// <param name="fileCreatedUtc"></param>
    /// <param name="fileUpdatedUtc"></param>
    /// <param name="importedBy"></param>
    /// <returns></returns>
    protected virtual async Task<UpdateImportedFileEvent> UpdateImportedFile(ImportedFileDescriptor importedFileDescriptor,
      string fileDescriptor, DateTime? surveyedUtc,
      DateTime fileUpdatedUtc, string importedBy)
    {
      var nowUtc = DateTime.UtcNow;
      var updateImportedFileEvent = new UpdateImportedFileEvent()
      {
        ProjectUID = Guid.Parse(importedFileDescriptor.ProjectUid),
        ImportedFileUID = Guid.NewGuid(),
        FileDescriptor = fileDescriptor,
        FileCreatedUtc = importedFileDescriptor.FileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        ImportedBy = importedBy,
        SurveyedUtc = surveyedUtc,
        ActionUTC = nowUtc,
        ReceivedUTC = nowUtc
      };

      var messagePayload = JsonConvert.SerializeObject(new { UpdateImportedFileEvent = updateImportedFileEvent });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(updateImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
        });

      if (await projectService.StoreEvent(updateImportedFileEvent).ConfigureAwait(false) == 1)
        return updateImportedFileEvent;

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          @"CreateImportedFileV4. Unable to store updated Imported File event to database: {JsonConvert.SerializeObject(importFile)}."));
      return null;
    }

    /// <summary>
    /// Writes the importedFile to TCC
    /// </summary>
    /// <returns></returns>
    protected async Task<FileDescriptor> WriteFileToRepository(int actionType, string customerUid, string projectUid, string pathAndFileName, ImportedFileType importedFileType, DateTime? surveyedUtc)
    {
      var fileSpaceId = store.GetValueString("TCCFILESPACEID");   // "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01"

      var fileStream = new FileStream(pathAndFileName, FileMode.Open);
      var tccPath = string.Format("/{0}/{1}", customerUid, projectUid); // trailing slash etc
      string tccFileName = Path.GetFileName(pathAndFileName);

      if (importedFileType == ImportedFileType.SurveyedSurface)
        if (surveyedUtc != null) // validation should prevent this
          tccFileName = GeneratedFileName(tccFileName, GeneratedSuffix(surveyedUtc.Value),
            Path.GetExtension(tccFileName));

      //var fileDetails = await fileRepo.FileExists(fileSpaceId, tccPath, filename).ConfigureAwait(false);
      // if (actionType = 1/* create */ && fileExists != null) throw except
      // if (actionType = 2/* update */ && fileExists == null) throw except
      //var ccPutFileResult = await fileRepo.PutFile(fileSpaceId, tccPath, filename, fileStream, fileStream.Length).ConfigureAwait(false);

      // todo temp until tcc interface is changed use the customer-based putFile
      var superUserOrg = new Organization() {filespaceId = fileSpaceId};
      var org = new Organization() {filespaceId = fileSpaceId};
      var ccPutFileResult = await fileRepo.PutFile(org, tccPath, tccFileName, fileStream, fileStream.Length).ConfigureAwait(false);
      if (ccPutFileResult == null || ccPutFileResult.success == $@"false")
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.TCCConfigurationError,
            "@Unable to put file to TCC"));
      }

      // is ccPutFileResult.path == tccPath?
      return FileDescriptor.CreateFileDescriptor(fileSpaceId, tccPath, tccFileName);
    }


    /// <summary>
    /// Notify raptor of new file
    ///     if it already knows about it, it will just update and re-notify raptor and return success.
    /// </summary>
    /// <returns></returns>
    protected async Task NotifyRaptorAddFile(string projectUid, FileDescriptor fileDescriptor)
    {
      var notificationResult = new ContractExecutionResult();
      // todo = await raptorProxy.AddFile(projectUid, fileDescriptor).ConfigureAwait(false);
      log.LogDebug($"FileImport AddFile in RaptorServices returned code: {0} Message {1}.",
        notificationResult?.Code ?? -1,
        notificationResult?.Message ?? "notificationResult == null");
      if (notificationResult == null || notificationResult.Code != 0)
      {
        log.LogError($"FileImport AddFile in RaptorServices failed. Reason: {0} {1}. ",
          notificationResult?.Code ?? -1,
          notificationResult?.Message ?? "null");
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            string.Format("Unable to complete FileImport AddFile in  RaptorServices. returned code: {0} Message {1}.",
              notificationResult?.Code ?? -1,
              notificationResult?.Message ?? "notificationResult == null"
            )));
      }
    }

    /// <summary>
    /// Notify raptor of delete file
    ///  if it doesn't know about it then it do nothing and return success
    /// </summary>
    /// <returns></returns>
    protected async Task NotifyRaptorDeleteFile(string projectUid, FileDescriptor fileDescriptor)
    {
      var notificationResult = new ContractExecutionResult();
      // todo = await raptorProxy.DeleteFile(projectUid, fileDescriptor).ConfigureAwait(false);}
      log.LogDebug($"FileImport DeleteFile in RaptorServices returned code: {0} Message {1}.",
        notificationResult?.Code ?? -1,
        notificationResult?.Message ?? "notificationResult == null");
      if (notificationResult == null || notificationResult.Code != 0)
      {
        log.LogError($"FileImport DeleteFile in RaptorServices failed. Reason: {0} {1}. ",
          notificationResult?.Code ?? -1,
          notificationResult?.Message ?? "null");
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            string.Format(
              "Unable to complete FileImport DeleteFile in  RaptorServices. returned code: {0} Message {1}.",
              notificationResult?.Code ?? -1,
              notificationResult?.Message ?? "notificationResult == null"
            )));
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
