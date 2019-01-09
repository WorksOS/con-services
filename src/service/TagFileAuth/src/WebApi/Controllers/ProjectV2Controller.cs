using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Project controller.
  /// </summary>
  public class ProjectV2Controller : BaseController
  {
    private readonly ILogger _log;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProjectV2Controller(ILoggerFactory logger, IConfigurationStore configStore,
      IRepository<IAssetEvent> assetRepository, IRepository<IDeviceEvent> deviceRepository,
      IRepository<ICustomerEvent> customerRepository, IRepository<IProjectEvent> projectRepository,
      IRepository<ISubscriptionEvent> subscriptionsRepository, IKafka producer)
      : base(logger, configStore, assetRepository, deviceRepository,
        customerRepository, projectRepository,
        subscriptionsRepository, producer)
    {
      _log = logger.CreateLogger<ProjectV2Controller>();
    }

    /// <summary>
    /// This endpoint is used by CTCTs Earthworks product,
    ///      the endpoint is also known as the 'ProjectDiscovery' endpoint. 
    ///      It allows a user once or twice a day
    ///      to obtain a Cut/fill or other map from 3dpService. 
    ///      This step tries to identify a unique projectUid
    ///          before 3dpmSvc is used to retrive a map.
    /// 
    /// The device info, location and customerUid are provided.
    /// 
    /// Get the ProjectUid 
    ///     which belongs to the devices Customer and 
    ///     whose boundary the location is inside at the given date time. 
    ///     authority is determined by servicePlans from the provided deviceUid.
    /// </summary>
    /// <param name="request">Details of the device and location</param>
    /// <returns>
    /// The project Uid if the asset is inside ONE project
    ///     else a returnCode.
    /// </returns>
    /// <executor>ProjectUidExecutor</executor>
    [Route("api/v2/project/getUid")]
    [HttpPost]
    public async Task<GetProjectUidResult> GetProjectUid([FromBody]GetProjectUidRequest request)
    {
      _log.LogDebug($"GetProjectUid: request:{JsonConvert.SerializeObject(request)}");
      var errorCodeResult = request.Validate();
      if (errorCodeResult > 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, ProjectUidHelper.FormatResult("", "", errorCodeResult));
      }

      var executor = RequestExecutorContainer.Build<ProjectUidExecutor>(_log, configStore, assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionsRepository);
      var result = await executor.ProcessAsync(request) as GetProjectUidResult;

      _log.LogResult(ToString(), request, result);
      return result;
    }

    /// <summary>
    /// This endpoint is used by TRex to identify a project to assign a tag file to.
    ///      It is called for each tag file, with as much information as is available e.g. device; location; projectUid; tccOrgId.
    ///      Attempts to identify a unique projectUid and AssetUid, which the tag file could be applied to
    ///         or an error which is/may be, preventing identifying one.
    ///     On error returns:
    ///         If validation of request fails, returns BadRequest plus a unique error code and message
    ///         If it fails to identify/verify a project, returns BadRequest plus a unique error code and message
    ///         If something internal has gone wrong, which may be retryable e.g. database unavailable
    ///            it returns InternalError plus a unique error code and message
    ///
    /// Note that for this endpoint we use the Gen3 Guids to identify projects etc
    /// 
    /// </summary>
    /// <param name="request">Details of the project, asset and tccOrgId. Also location and its date time</param>
    /// <returns>
    /// The project Uid and possibly assetUid
    ///      otherwise a returnCode.
    /// </returns>
    /// <executor>GetProjectAndAssetUidsExecutor</executor>
    [Route("api/v2/project/getUids")]
    [HttpPost]
    public async Task<GetProjectAndAssetUidsResult> GetProjectAndAssetUids([FromBody]GetProjectAndAssetUidsRequest request)
    {
      _log.LogDebug($"GetProjectAndAssetUids: request:{JsonConvert.SerializeObject(request)}");
      var errorCodeResult = request.Validate();
      if (errorCodeResult > 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, ProjectUidHelper.FormatResult("", "", errorCodeResult));
      }

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_log, configStore, assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionsRepository);
      var result = await executor.ProcessAsync(request) as GetProjectAndAssetUidsResult;

      _log.LogResult(ToString(), request, result);
      return result;
    }

  }
}
