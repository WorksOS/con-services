using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Utilities;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Asset controller for C2S2 solution, to support Raptor.
  /// </summary>
  public class AssetV3RaptorController : BaseController
  {
    private readonly ILogger _log;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public AssetV3RaptorController(ILoggerFactory logger, IConfigurationStore configStore, 
      IProjectProxy projectProxy, ICustomerProxy customerProxy, IDeviceProxy deviceProxy)
      : base(logger, configStore, projectProxy, customerProxy, deviceProxy)
    {
      _log = logger.CreateLogger<AssetV3RaptorController>();
    }

    /// <summary>
    ///   Gets the shortRaptorAssetId and serviceType satisfying requirements
    ///      for the device or project - whichever is provided
    ///  
    ///      ProjectID (now known as shortRaptorProjectId) is -1 for auto processing of tag files and non-zero for manual processing.
    ///      Radio serial may not be present in the tag file. The logic below replaces the 'john doe' handling in Raptor for these tag files.
    ///      In C2S2 there are no customer/asset/project subscriptions. We are piggybacking (for now) on WorksManager device package entitlements.
    /// </summary>
    /// <returns>shortRaptorAssetId and/or serviceType and True for success, 
    ///          shortRaptorAssetId==-1, serviceType = 0 and False for failure</returns>
    /// <executor>AssetIdExecutor</executor>
    [Route("api/v3/asset/getId")]
    [HttpPost]
    public async Task<GetAssetIdResult> GetAssetId([FromBody] GetAssetIdRequest request)
    {
      _log.LogDebug($"{nameof(GetAssetId)}: request: {JsonConvert.SerializeObject(request)}");
      request.Validate();

      var executor = RequestExecutorContainer.Build<AssetIdExecutor>(_log, configStore, projectProxy, customerProxy, deviceProxy);
      var result = await executor.ProcessAsync(request) as GetAssetIdResult;

      _log.LogResult(nameof(GetAssetId), request: request, result: result);
      return result;
    }
  }
}
