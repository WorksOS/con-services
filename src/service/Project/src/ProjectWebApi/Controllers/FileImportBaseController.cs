using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// FileImporter controller
  /// </summary>
  public class FileImportBaseController : BaseController
  {
    protected ITransferProxy persistantTransferProxy;

    protected IFilterServiceProxy filterServiceProxy;

    protected ITRexImportFileProxy tRexImportFileProxy;

    protected string FileSpaceId;
    protected string DataOceanRootFolder;
    protected bool UseTrexGatewayDesignImport;
    protected bool UseRaptorGatewayDesignImport;

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
    /// <param name="persistantTransferProxy"></param>
    /// <param name="filterServiceProxy"></param>
    /// <param name="tRexImportFileProxy"></param>
    /// <param name="logger">The logger.</param>
    /// <param name="subscriptionRepo"></param>
    /// <param name="fileRepo">For TCC file transfer</param>
    /// <param name="serviceExceptionHandler">For correctly throwing ServiceException errors</param>
    /// <param name="requestFactory"></param>
    /// <param name="log"></param>
    /// <param name="dataOceanClient"></param>
    /// <param name="tileServiceProxy"></param>
    /// <param name="authn"></param>
    public FileImportBaseController(IKafka producer,
      IConfigurationStore configStore, ILoggerFactory logger, ILogger log, IServiceExceptionHandler serviceExceptionHandler,
      IRaptorProxy raptorProxy, Func<TransferProxyType, ITransferProxy> persistantTransferProxy,
      IFilterServiceProxy filterServiceProxy, ITRexImportFileProxy tRexImportFileProxy,
      IProjectRepository projectRepo, ISubscriptionRepository subscriptionRepo,
      IFileRepository fileRepo, IRequestFactory requestFactory, IDataOceanClient dataOceanClient, 
      ITileServiceProxy tileServiceProxy, ITPaaSApplicationAuthentication authn)
      : base(log, configStore, serviceExceptionHandler, producer, raptorProxy, projectRepo, 
        subscriptionRepo, fileRepo, dataOceanClient, tileServiceProxy, authn)
    {
      this.logger = logger;
      this.requestFactory = requestFactory;

      this.persistantTransferProxy = persistantTransferProxy(TransferProxyType.DesignImport);
      this.filterServiceProxy = filterServiceProxy;
      this.tRexImportFileProxy = tRexImportFileProxy;

      FileSpaceId = configStore.GetValueString("TCCFILESPACEID");
      DataOceanRootFolder = configStore.GetValueString("DATA_OCEAN_ROOT_FOLDER");
    UseTrexGatewayDesignImport = false;
    UseRaptorGatewayDesignImport = true;
    bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT"),
        out UseTrexGatewayDesignImport);
      bool.TryParse(configStore.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT"),
        out UseRaptorGatewayDesignImport);

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
    /// Notify raptor of an updated import file (used for activations, not file import).
    /// </summary>
    protected async Task NotifyRaptorUpdateFile(Guid projectUid, IEnumerable<Guid> updatedFileUids)
    {
      var notificationResult = await raptorProxy.UpdateFiles(projectUid, updatedFileUids, Request.Headers.GetCustomHeaders());

      log.LogDebug(
        $"FileImport UpdateFiles in RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult != null && notificationResult.Code != 0)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 108, notificationResult.Code.ToString(), notificationResult.Message);
      }
    }

    /// <summary>
    /// Sets activated state for imported files.
    /// </summary>
    protected async Task<IEnumerable<Guid>> SetFileActivatedState(string projectUid, Dictionary<Guid, bool> fileUids)
    {
      log.LogDebug($"SetFileActivatedState: projectUid={projectUid}, {fileUids.Keys.Count} files with changed state");

      var deactivatedFileList = await ImportedFileRequestDatabaseHelper.GetImportedFileProjectSettings(projectUid, userId, projectRepo).ConfigureAwait(false) ?? new List<ActivatedFileDescriptor>();
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
            raptorProxy, null, null, null, null,
            projectRepo)
          .ProcessAsync(projectSettingsRequest)
      ) as ProjectSettingsResult;

      var changedUids = fileUids.Keys.Except(missingUids);
      log.LogDebug($"SetFileActivatedState: {changedUids.Count()} changedUids");
      return changedUids;
    }

    protected bool IsDesignFileType(ImportedFileType importedFileType)
    {
      return importedFileType == ImportedFileType.DesignSurface ||
             importedFileType == ImportedFileType.SurveyedSurface;
    }

  }
}