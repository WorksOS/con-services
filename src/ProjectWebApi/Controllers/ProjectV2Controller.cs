using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project controller v2
  /// This is used by BusinessCenter. 
  ///     The signature must be retained.
  ///     BC is now compatible with jwt/TID etc.   
  /// </summary>
  public class ProjectV2Controller : ProjectBaseController
  {
    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="projectRepo"></param>
    /// <param name="subscriptionsRepo"></param>
    /// <param name="store"></param>
    /// <param name="subscriptionProxy"></param>
    /// <param name="geofenceProxy"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="fileRepo"></param>
    /// <param name="logger"></param>
    /// <param name="serviceExceptionHandler">The ServiceException handler.</param>
    public ProjectV2Controller(IKafka producer, IProjectRepository projectRepo,
      ISubscriptionRepository subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subscriptionProxy,
      IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, IFileRepository fileRepo,
      ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler)
      : base(producer, projectRepo, subscriptionsRepo, store, subscriptionProxy, geofenceProxy, raptorProxy, fileRepo,
        logger, serviceExceptionHandler, logger.CreateLogger<ProjectV2Controller>())
    {
      this.logger = logger;
    }

    #region projects

    // POST: api/v2/projects
    /// <summary>
    /// TBC CreateProject. Footprint must remain the same as CGen: 
    ///     POST /t/trimble.com/vss-projectmonitoring/1.0/api/v2/projects HTTP/1.1
    ///     Body: {"CoordinateSystem":{"FileSpaceID":"u927f3be6-7987-4944-898f-42a088da94f2","Path":"/BC Data/Sites/Svevia Vargarda","Name":"Svevia Vargarda.dc","CreatedUTC":"0001-01-01T00:00:00Z"},"ProjectType":2,"StartDate":"2018-04-11T00:00:00Z","EndDate":"2018-05-11T00:00:00Z","ProjectName":"Svevia Vargarda","TimeZoneName":"Romance Standard Time","BoundaryLL":[{"Latitude":58.021890362243404,"Longitude":12.778613775843427},{"Latitude":58.033751276149488,"Longitude":12.783760539866186},{"Latitude":58.035972399195963,"Longitude":12.812762795456051},{"Latitude":58.032604039701752,"Longitude":12.841590546413993},{"Latitude":58.024515931878035,"Longitude":12.842137844178708},{"Latitude":58.016620613589389,"Longitude":12.831491715508857},{"Latitude":58.0128142214101,"Longitude":12.793567555971942},{"Latitude":58.021890362243404,"Longitude":12.778613775843427}],"CustomerUid":"323e4a34-56aa-11e5-a400-0050569757e0","CustomerName":"MERINO CONSTRUCTION"}
    ///     Result: {"id":6964} 
    /// 
    /// </summary>
    /// <param name="projectRequest">CreateProjectV2Request model</param>
    /// <remarks>Updates existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v2/projects")]
    [HttpPost]
    public async Task<ContractExecutionResult> CreateProjectV2([FromBody] CreateProjectV2Request projectRequest)
    {
      if (projectRequest == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 81);
      }
      log.LogInformation("CreateProjectV2. projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));

      var createProjectEvent = MapV2Models.MapCreateProjectV2RequestToEvent(projectRequest, customerUid);

      ProjectDataValidator.Validate(createProjectEvent, projectRepo);
      if (createProjectEvent.ProjectType != ProjectType.ProjectMonitoring)
      {
         serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 85);
      }

      // get CoordinateSystem file content from TCC
      projectRequest.CoordinateSystem = ProjectDataValidator.ValidateBusinessCentreFile(projectRequest.CoordinateSystem);

      var projectRequestHelper = new ProjectRequestHelper(
        log, configStore, serviceExceptionHandler,
        GetCustomerUid(), userId, customHeaders,
        producer,
        geofenceProxy, raptorProxy, subscriptionProxy,
        projectRepo, subscriptionRepo, fileRepo);
      createProjectEvent.CoordinateSystemFileContent = await projectRequestHelper.GetCoordinateSystemContent(projectRequest.CoordinateSystem).ConfigureAwait(false);

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<CreateProjectExecutor>(logger, configStore, serviceExceptionHandler,
            customerUid, userId, null, customHeaders,
            producer, kafkaTopicName,
            geofenceProxy, raptorProxy, subscriptionProxy,
            projectRepo, subscriptionRepo, fileRepo)
          .ProcessAsync(createProjectEvent)
      );

      log.LogDebug("CreateProjectV2. completed succesfully");
      return CreateProjectV2Result.CreateAProjectV2Result(createProjectEvent.ProjectID);
    }

    #endregion projects
  }
}