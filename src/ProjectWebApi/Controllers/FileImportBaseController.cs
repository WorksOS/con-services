using System.Collections.Generic;
using System.Collections.Immutable;
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
using TCCFileAccess;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

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
    protected virtual async Task<int> CreateImportedFile(CreateImportedFileEvent importFile)
    {
      var messagePayload = JsonConvert.SerializeObject(new { CreateImportedFileEvent = importFile });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(importFile.ImportedFileUID.ToString(), messagePayload)
        });

      return await projectService.StoreEvent(importFile).ConfigureAwait(false);
    }

  }
}
