using System;
using System.Net;
using System.Threading.Tasks;
using KafkaConsumer.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.GenericConfiguration;
using VSS.Productivity3D.MasterDataProxies;
using VSS.Productivity3D.MasterDataProxies.Interfaces;
using VSS.Productivity3D.ProjectWebApiCommon.Internal;
using VSS.Productivity3D.ProjectWebApi.Filters;
using VSS.Productivity3D.ProjectWebApiCommon.Executors;
using VSS.Productivity3D.ProjectWebApiCommon.Models;
using VSS.Productivity3D.ProjectWebApiCommon.ResultsHandling;
using VSS.Productivity3D.Repo;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.ProjectWebApi.Controllers
{
  public class ProjectSettingsV4Controller : Controller
  {
    private readonly ProjectRepository projectRepo;
    private readonly IRaptorProxy raptorProxy;
    private readonly IConfigurationStore configStore;
    private readonly ILoggerFactory logger;
    private readonly ILogger log;
    private readonly IServiceExceptionHandler serviceExceptionHandler;
    private readonly IKafka producer;
    private readonly string kafkaTopicName;

    public ProjectSettingsV4Controller(IRepository<IProjectEvent> projectRepo, IRaptorProxy raptorProxy,
      IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler, IKafka producer)
    {
      this.projectRepo = projectRepo as ProjectRepository;
      this.raptorProxy = raptorProxy;
      this.configStore = configStore;
      this.logger = logger;
      log = logger.CreateLogger<ProjectSettingsV4Controller>(); 
      this.serviceExceptionHandler = serviceExceptionHandler;
      this.producer = producer;
      kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" + configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
    }


    /// <summary>
    /// Gets the project settings for a project.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns></returns>
    [Route("api/v4/projectsettings/{projectUid}")]
    [HttpGet]
    public async Task<ProjectSettingsResult> GetProjectSettings(string projectUid)
    {
      if (string.IsNullOrEmpty(projectUid))
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 68);
      LogCustomerDetails("UpsertProjectSettings", projectUid);

      var executor = RequestExecutorContainer.Build<GetProjectSettingsExecutor>(projectRepo, configStore, logger, serviceExceptionHandler, producer);
      var result = await executor.ProcessAsync(projectUid);

      log.LogResult(this.ToString(), projectUid, result);
      return result as ProjectSettingsResult;
    }


    /// <summary>
    /// Upserts the project settings for a project.
    /// </summary>
    /// <returns></returns>
    [Route("api/v4/projectsettings")]
    [HttpPut]
    public async Task<ProjectSettingsResult> UpsertProjectSettings([FromBody]ProjectSettingsRequest request)
    {
      if (string.IsNullOrEmpty(request?.projectUid))
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 68);
      LogCustomerDetails("UpsertProjectSettings", request?.projectUid);
      log.LogDebug($"UpsertProjectSettings: {JsonConvert.SerializeObject(request)}");

      await RaptorValidateProjectSettings(request.projectUid, request.settings);

      var executor = RequestExecutorContainer.Build<UpsertProjectSettingsExecutor>(projectRepo, configStore, logger, serviceExceptionHandler, producer);
      var result = await executor.ProcessAsync(request);

      log.LogResult(this.ToString(), JsonConvert.SerializeObject(request), result);
      return result as ProjectSettingsResult;
    }

    private string LogCustomerDetails(string functionName, string projectUid)
    {
      var customerUid = (User as TIDCustomPrincipal).CustomerUid;
      log.LogInformation($"{functionName}: CustomerUID={customerUid} and projectUid={projectUid}");

      return customerUid;
    }

    private async Task RaptorValidateProjectSettings(string projectUid, string settings)
    {
      MasterDataProxies.ResultHandling.ContractExecutionResult result = null;
      try
      {
        result = await raptorProxy
          .ProjectSettingsValidate(settings, Request.Headers.GetCustomHeaders())
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"RaptorValidateProjectSettings: RaptorServices failed with exception. projectUid:{projectUid} settings:{settings}. Exception Thrown: {e.Message}. ");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 70, "raptorProxy.ProjectSettingsValidate", e.Message);
      }

      log.LogDebug(
        $"RaptorValidateProjectSettings: projectUid: {projectUid} settings: {settings}. RaptorServices returned code: {result?.Code ?? -1} Message {result?.Message ?? "result == null"}.");

      if (result != null && result.Code != 0)
      {
        log.LogError($"FRaptorValidateProjectSettings: RaptorServices failed. projectUid:{projectUid} settings:{settings}. Reason: {result?.Code ?? -1} {result?.Message ?? "null"}. ");

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 67, result.Code.ToString(), result.Message);
      }
    }
  }
}
