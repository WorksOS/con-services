using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Masterdata;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.TagFileAuth.Service.WebApiModels.Executors;

namespace VSS.TagFileAuth.Service.Controllers
{
  public class AssetController : Controller
  {
    /// <summary>
    /// Repository factory for use by executor
    /// </summary>
    private readonly IRepositoryFactory factory;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Constructor with injected repository factory and logger
    /// </summary>
    /// <param name="factory">Repository factory</param>
    /// <param name="logger">Logger</param>
    public AssetController(IRepositoryFactory factory, ILogger<AssetController> logger)
    {
      this.factory = factory;
      this.log = logger;
    }

    /// <summary>
    ///   Gets the legacyAssetID and serviceType satisfying requirements
    ///      for the device or project - whichever is provided
    ///  
    ///      ProjectID is -1 for auto processing of tag files and non-zero for manual processing.
    ///      Radio serial may not be present in the tag file. The logic below replaces the 'john doe' handling in Raptor for these tag files.
    ///      Special case: Allow manual import of tag file if user has manual 3D subscription.
    /// </summary>
    /// <returns>AssetId and/or serviceType and True for success, 
    ///           AssetId==-1, serviceType = 0 and False for failure</returns>
    /// <executor>AssetIdExecutor</executor>
    [Route("api/v1/asset/getId")]
    [HttpPost]
    public GetAssetIdResult Post([FromBody]GetAssetIdRequest request)
    {
      log.LogInformation("GetAssetID: {0}", Request.QueryString);
            
      request.Validate(); 
      return RequestExecutorContainer.Build<AssetIdExecutor>(factory, log).Process(request) as GetAssetIdResult;
    }
  }
}
