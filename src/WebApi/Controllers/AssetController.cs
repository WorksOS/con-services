﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Asset controller.
  /// </summary>
  public class AssetController : BaseController
  {
    private readonly ILogger log;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="logger">Service implementation of ILogger</param>
    /// <param name="assetRepository"></param>
    /// <param name="deviceRepository"></param>
    /// <param name="customerRepository"></param>
    /// <param name="projectRepository"></param>
    /// <param name="subscriptionsRepository"></param>
    public AssetController(ILogger logger, IAssetRepository assetRepository, IDeviceRepository deviceRepository,
      ICustomerRepository customerRepository, IProjectRepository projectRepository,
      ISubscriptionRepository subscriptionsRepository)
      :base(logger, assetRepository, deviceRepository,
            customerRepository, projectRepository,
            subscriptionsRepository)
    {
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
    public async Task<GetAssetIdResult> GetAssetId([FromBody]GetAssetIdRequest request)
    {
      log.LogDebug("GetAssetId: request:{0}", JsonConvert.SerializeObject(request) );            
      request.Validate();

      var executor = RequestExecutorContainer.Build<AssetIdExecutor>(log, assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionsRepository);
      var result = await executor.ProcessAsync(request) as GetAssetIdResult;

      log.LogResult(methodName: this.ToString(), request: request, result: result);
      return result;
    }
  }
}