using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.TCCFileAccess;

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
    /// Gets or sets the Customer Repository.
    /// </summary>
    protected readonly ICustomerRepository customerRepo;

    /// <summary>
    /// Gets or sets the httpContextAccessor.
    /// </summary>
    protected readonly IHttpContextAccessor httpContextAccessor;


    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="projectRepo"></param>
    /// <param name="subscriptionRepo"></param>
    /// <param name="fileRepo"></param>
    /// <param name="customerRepo"></param>
    /// <param name="store"></param>
    /// <param name="subscriptionProxy"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="logger"></param>
    /// <param name="serviceExceptionHandler">The ServiceException handler.</param>
    /// <param name="httpContextAccessor"></param>
    public ProjectV2Controller(IKafka producer,
      IProjectRepository projectRepo, ISubscriptionRepository subscriptionRepo,
      IFileRepository fileRepo, ICustomerRepository customerRepo,
      IConfigurationStore store, ISubscriptionProxy subscriptionProxy,
      IRaptorProxy raptorProxy,
      ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      IHttpContextAccessor httpContextAccessor)
      : base(producer, projectRepo, subscriptionRepo, fileRepo, store, subscriptionProxy, raptorProxy,
        logger, serviceExceptionHandler, logger.CreateLogger<ProjectV2Controller>())
    {
      this.logger = logger;
      this.customerRepo = customerRepo;
      this.httpContextAccessor = httpContextAccessor;
    }

    #region projects

    // POST: api/v2/projects
    /// <summary>
    /// TBC CreateProject. Footprint must remain the same as CGen: 
    ///     POST /t/trimble.com/vss-projectmonitoring/1.0/api/v2/projects HTTP/1.1
    ///     Body: {"CoordinateSystem":{"FileSpaceID":"u927f3be6-7987-4944-898f-42a088da94f2","Path":"/BC Data/Sites/Svevia Vargarda","Name":"Svevia Vargarda.dc","CreatedUTC":"0001-01-01T00:00:00Z"},"ProjectType":2,"StartDate":"2018-04-11T00:00:00Z","EndDate":"2018-05-11T00:00:00Z","ProjectName":"Svevia Vargarda","TimeZoneName":"Romance Standard Time","BoundaryLL":[{"Latitude":58.021890362243404,"Longitude":12.778613775843427},{"Latitude":58.033751276149488,"Longitude":12.783760539866186},{"Latitude":58.035972399195963,"Longitude":12.812762795456051},{"Latitude":58.032604039701752,"Longitude":12.841590546413993},{"Latitude":58.024515931878035,"Longitude":12.842137844178708},{"Latitude":58.016620613589389,"Longitude":12.831491715508857},{"Latitude":58.0128142214101,"Longitude":12.793567555971942},{"Latitude":58.021890362243404,"Longitude":12.778613775843427}],"CustomerUid":"323e4a34-56aa-11e5-a400-0050569757e0","CustomerName":"MERINO CONSTRUCTION"}
    ///     Result: HttpStatusCode.Created
    ///            {"id":6964} 
    /// 
    ///   This US only handles happy path. ServiceExceptions will be mapped in a future US.
    /// 
    /// </summary>
    /// <param name="projectRequest">CreateProjectV2Request model</param>
    /// <remarks>Updates existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v2/projects")]
    [HttpPost]
    public async Task<ReturnLongV2Result> CreateProjectV2([FromBody] CreateProjectV2Request projectRequest)
    {
      if (projectRequest == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 81);
      }

      log.LogInformation($"CreateProjectV2. projectRequest: {JsonConvert.SerializeObject(projectRequest)}");

      var createProjectEvent = MapV2Models.MapCreateProjectV2RequestToEvent(projectRequest, customerUid);

      ProjectDataValidator.Validate(createProjectEvent, projectRepo, serviceExceptionHandler);
    
      projectRequest.CoordinateSystem =
        ProjectDataValidator.ValidateBusinessCentreFile(projectRequest.CoordinateSystem);

      // Read CoordSystem file from TCC as byte[]. 
      //    Filename and content are used: 
      //      validated via raptorproxy
      //      created in Raptor via raptorProxy
      //      stored in CreateKafkaEvent
      //    Only Filename is stored in the VL database 
      createProjectEvent.CoordinateSystemFileContent = 
        await TccHelper
        .GetFileContentFromTcc(projectRequest.CoordinateSystem,
          log, serviceExceptionHandler, fileRepo).ConfigureAwait(false);

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<CreateProjectExecutor>(logger, configStore, serviceExceptionHandler,
            customerUid, userId, null, customHeaders,
            producer, kafkaTopicName,
            raptorProxy, subscriptionProxy, null, null, null,
            projectRepo, subscriptionRepo, fileRepo, null, httpContextAccessor)
          .ProcessAsync(createProjectEvent)
      );

      log.LogDebug("CreateProjectV2. completed succesfully");
      return ReturnLongV2Result.CreateLongV2Result(HttpStatusCode.Created, createProjectEvent.ProjectID);
    }

    #endregion projects


    #region TCCAuthorization

    // POST: api/v2/preferences/tcc
    /// <summary>
    /// TBC ValidateTCCAuthorization. This validates that 
    ///      a) the customer has access to the TCC organization and 
    ///      b) that the Folder structure exists in TCC.
    /// Footprint must remain the same as CGen: 
    ///     POST /t/trimble.com/vss-projectmonitoring/1.0/api/v2/preferences/tcc HTTP/1.1
    ///     Body: {"organization":"vssnz19"}     
    ///     Response: {"success":true}
    /// 
    /// Happy path only to be handled in this US. ServiceExceptions will be mapped in a future US.
    /// However this is a faillure Response:
    ///     {"status":500,"message":"invalidUser001\r\n\r\n","errorcode":1000,"link":null}
    /// 
    /// </summary>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v2/preferences/tcc")]
    [HttpPost]
    public async Task<ReturnSuccessV2Result> ValidateTccAuthorization(
      [FromBody] ValidateTccAuthorizationRequest tccAuthorizationRequest)
    {
      if (tccAuthorizationRequest == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 86);
      }

      //Note: This is a very old legacy code that validates subs against TCC. This is not needed anymore as we allow project creation regardless of TCC subscription to support Earthworks machines.

       /* log.LogInformation(
        $"ValidateTCCAuthorization. tccAuthorizationRequest: {JsonConvert.SerializeObject(tccAuthorizationRequest)}");

      tccAuthorizationRequest.Validate();

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<ValidateTccOrgExecutor>(logger, configStore, serviceExceptionHandler,
            customerUid, null, null, customHeaders,
            null, null,
            null, null, null,
            null, fileRepo, customerRepo)
          .ProcessAsync(tccAuthorizationRequest)
      );*/

      log.LogInformation("ValidateTccAuthorization. completed succesfully");
      return ReturnSuccessV2Result.CreateReturnSuccessV2Result(HttpStatusCode.OK, true);
    }

    #endregion TCCAuthorization
  }
}