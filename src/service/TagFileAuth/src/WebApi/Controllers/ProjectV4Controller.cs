using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Utilities;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Project controller.
  /// </summary>
  public class ProjectV4Raptor : BaseController
  {
    private readonly ILogger _log;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProjectV4Raptor(ILoggerFactory logger, IConfigurationStore configStore,
      IProjectProxy projectProxy, ICustomerProxy customerProxy, IDeviceProxy deviceProxy)
      : base(logger, configStore, projectProxy, customerProxy, deviceProxy)
    {
      _log = logger.CreateLogger<ProjectV4Raptor>();
    }

    /// <summary>
    /// This endpoint is used by CTCTs Earthworks product.
    ///   It allows an operator, once or twice a day
    ///      to obtain data to enable it to generate a Cut/fill or other map from 3dpService. 
    ///   This step tries to identify a unique projectUid.
    /// 
    /// EC and/or radio, location and possibly TCCOrgID are provided.
    /// 
    /// Get the ProjectUid 
    ///     which belongs to the devices Customer and 
    ///     whose boundary the location is inside at the given date time. 
    ///     NOTE as of Sept 2019, VSS commercial model has not been determined,
    ///        current thinking is that:
    ///          1) if there is no traditional sub, they may get cutfill for surveyed surfaces only
    ///          2) if there is a traditional sub they get production data as well
    ///          3) there may be a completely new type of subscription, specific to EarthWorks cutfill ...
    /// </summary>
    /// <returns>
    /// The project Uid which satisfies spatial and time requirements
    ///      and possibly device
    ///      and an indicator of subscription availability
    ///      otherwise a returnCode.
    /// </returns>
    [Route("api/v2/project/getUidsEarthWorks")]
    [HttpPost]
    public async Task<GetProjectAndAssetUidsEarthWorksResult> GetProjectAndDeviceUidsEarthWorks([FromBody]GetProjectAndAssetUidsEarthWorksRequest request)
    {
      _log.LogDebug($"{nameof(GetProjectAndDeviceUidsEarthWorks)}: request: {JsonConvert.SerializeObject(request)}");
      request.Validate();
  
      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_log, configStore, projectProxy, customerProxy, deviceProxy);
      var result = await executor.ProcessAsync(request) as GetProjectAndAssetUidsEarthWorksResult;

      _log.LogResult(nameof(GetProjectAndDeviceUidsEarthWorks), request, result);
      return result;
    }

    /// <summary>
    /// This endpoint is used by TRex to identify a project to assign a tag file to.
    ///      It is called for each tag file, with as much information as is available e.g. device; location; projectUid
    ///      Attempts to identify a unique projectUid and DeviceUid, which the tag file could be applied to
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
    /// The project Uid which satisfies spatial, time and subscription requirements
    ///      and possibly assetUid
    ///      otherwise a returnCode.
    /// </returns>
    /// <executor>GetProjectAndAssetUidsExecutor</executor>
    [Route("api/v2/project/getUids")]
    [HttpPost]
    public async Task<GetProjectAndAssetUidsResult> GetProjectAndDeviceUids([FromBody]GetProjectAndAssetUidsRequest request)
    {
      _log.LogDebug($"{nameof(GetProjectAndDeviceUids)}: request:{JsonConvert.SerializeObject(request)}");
      request.Validate();

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_log, configStore, projectProxy, customerProxy, deviceProxy);
      var result = await executor.ProcessAsync(request) as GetProjectAndAssetUidsResult;

      _log.LogResult(nameof(GetProjectAndDeviceUids), request, result);
      return result;
    }

  }
}
