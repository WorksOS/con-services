using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Filters;
using VSS.MasterData.Repositories;
using VSS.MasterDataProxies;
using VSS.MasterDataProxies.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project Settings controller, version 4.
  /// </summary>
  public class ProjectSettingsV4Controller : ProjectSettingsBaseController
  {
    private readonly ILogger log;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="projectRepo">The MasterData ProjectRepository persistent storage interface</param>
    /// <param name="raptorProxy">The Raptor Proxy refernce</param>
    /// <param name="configStore">ConfigurationStore for module meta data</param>
    /// <param name="logger">The ILoggerFactory logging implementation</param>
    /// <param name="serviceExceptionHandler">The service exception handler</param>
    /// <param name="producer">The Kafka consumer</param>
    public ProjectSettingsV4Controller(IRepository<IProjectEvent> projectRepo, IRaptorProxy raptorProxy,
      IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler, IKafka producer)
      : base(projectRepo, raptorProxy, configStore, logger, serviceExceptionHandler, producer)
    {
      log = logger.CreateLogger<ProjectSettingsV4Controller>(); 
      kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                                        configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
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

      await ProjectSettingsValidation.ValidateProjectId(projectRepo, log, serviceExceptionHandler, (User as TIDCustomPrincipal).CustomerUid, projectUid);

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

      await ProjectSettingsValidation.ValidateProjectId(projectRepo, log, serviceExceptionHandler, (User as TIDCustomPrincipal).CustomerUid, request.projectUid);
      await ProjectSettingsValidation.RaptorValidateProjectSettings(raptorProxy, log, serviceExceptionHandler, request, Request.Headers.GetCustomHeaders());

      var executor = RequestExecutorContainer.Build<UpsertProjectSettingsExecutor>(projectRepo, configStore, logger, serviceExceptionHandler, producer, kafkaTopicName);
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
  }
}
