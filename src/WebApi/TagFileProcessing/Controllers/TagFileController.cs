using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApi.TagFileProcessing.Controllers
{
  /// <summary>
  /// For handling Tag file submissions from either TCC or machines equiped with direct submission capable units.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class TagFileController : Controller
  {
    private readonly ITagProcessor tagProcessor;
    private readonly IASNodeClient raptorClient;
    private readonly ILogger log;
    private readonly ILoggerFactory logger;
    private readonly ITransferProxy transferProxy;
    private readonly IConfigurationStore configStore;
    private readonly ITRexTagFileProxy tRexTagFileProxy;
    private IDictionary<string, string> customHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public TagFileController(IASNodeClient raptorClient, ITagProcessor tagProcessor, ILoggerFactory logger, ITransferProxy transferProxy, ITRexTagFileProxy tRexTagFileProxy, IConfigurationStore configStore)
    {
      this.raptorClient = raptorClient;
      this.tagProcessor = tagProcessor;
      this.logger = logger;
      log = logger.CreateLogger<TagFileController>();
      this.transferProxy = transferProxy;
      this.tRexTagFileProxy = tRexTagFileProxy;
      this.configStore = configStore;
    }

    /// <summary>
    /// For accepting and loading manually or automatically submitted tag files.
    /// </summary>
    /// <remarks>
    /// Manually submitted tag files include a project Id, the service performs a lookup for the boundary.
    /// </remarks>
    [ProjectVerifier(AllowArchivedState = false)]
    [PostRequestVerifier]
    [Route("api/v2/tagfiles")]
    [HttpPost]
    public async Task<IActionResult> PostTagFileNonDirectSubmission([FromBody]CompactionTagFileRequest request)
    {
      var serializedRequest = SerializeObjectIgnoringProperties(request, "Data");
      log.LogDebug("PostTagFile: " + serializedRequest);

      WGS84Fence boundary = null;
      if (request.ProjectUid != null)
      {
        request.ProjectId = GetLegacyProjectId(request.ProjectUid).Result;
        boundary = await GetProjectBoundary(request.ProjectUid.Value);
      }

      var requestExt =
        CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended(request, boundary);

      var responseObj = await RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(logger, raptorClient, tagProcessor, configStore, null, null, null, null, transferProxy, tRexTagFileProxy, customHeaders)
        .ProcessAsync(requestExt).ConfigureAwait(false);

      return responseObj.Code == 0
        ? (IActionResult)Ok(responseObj)
        : BadRequest(responseObj);
    }
    
    /// <summary>
    /// For the direct submission of tag files from GNSS capable machines.
    /// </summary>
    /// <remarks>
    /// Direct submission tag files don't include a project Id or boundary.
    /// </remarks>
    [PostRequestVerifier]
    [Route("api/v2/tagfiles/direct")]
    [HttpPost]
    public async Task<ObjectResult> PostTagFileDirectSubmission([FromBody] CompactionTagFileRequest request)
    {
      var serializedRequest = SerializeObjectIgnoringProperties(request, "Data");
      log.LogDebug("PostTagFile (Direct): " + serializedRequest);

      var result = await RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionExecutor>(logger, raptorClient, tagProcessor, configStore, null, null, null, null, transferProxy, tRexTagFileProxy, customHeaders)
        .ProcessAsync(request).ConfigureAwait(false) as TagFileDirectSubmissionResult;

      if (result.Code == 0)
      {
        return StatusCode((int) HttpStatusCode.OK, result);
      }

      return StatusCode((int)HttpStatusCode.BadRequest, result);
    }
    
    /// <summary>
    /// Serialize the request ignoring the Data property so not to overwhelm the logs.
    /// </summary>
    private static string SerializeObjectIgnoringProperties(CompactionTagFileRequest request, params string[] properties)
    {
      return JsonConvert.SerializeObject(
        request,
        Formatting.None,
        new JsonSerializerSettings { ContractResolver = new JsonContractPropertyResolver(properties) });
    }


    private async Task<long> GetLegacyProjectId(Guid? projectUid) => projectUid == null
      ? VelociraptorConstants.NO_PROJECT_ID
      : await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid.Value);

    /// <summary>
    /// Gets the WGS84 project boundary geofence for a given project Id.
    /// </summary>
    private async Task<WGS84Fence> GetProjectBoundary(Guid projectUid)
    {
      var projectData = await ((RaptorPrincipal)User).GetProject(projectUid);

      return projectData.ProjectGeofenceWKT == null
        ? null
        : new WGS84Fence(RaptorConverters.geometryToPoints(projectData.ProjectGeofenceWKT).ToArray());
    }

  }
}
