using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.ConfigurationStore;
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
  [ProjectVerifier(AllowArchivedState = false)]
  public class TagFileController : Controller
  {
    private readonly ITagProcessor tagProcessor;
    private readonly IASNodeClient raptorClient;
    private readonly ILogger log;
    private readonly ILoggerFactory logger;
    private readonly ITransferProxy transferProxy;
    private readonly IConfigurationStore configStore;

    private async Task<long> GetLegacyProjectId(Guid? projectUid) => projectUid == null
      ? VelociraptorConstants.NO_PROJECT_ID
      : await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid.Value);

    /// <summary>
    /// Default constructor.
    /// </summary>
    public TagFileController(IASNodeClient raptorClient, ITagProcessor tagProcessor, ILoggerFactory logger, ITransferProxy transferProxy, IConfigurationStore configStore)
    {
      this.raptorClient = raptorClient;
      this.tagProcessor = tagProcessor;
      this.logger = logger;
      log = logger.CreateLogger<TagFileController>();
      this.transferProxy = transferProxy;
      this.configStore = configStore;
    }

    /// <summary>
    /// Posts TAG file to Raptor. 
    /// </summary>
    /// <remarks>
    /// This endpoint is only used as a tool for for reprocessing tagfiles for Raptor (not TRex).
    /// </remarks>
    [PostRequestVerifier]
    [Route("api/v1/tagfiles")]
    [HttpPost]
    public IActionResult Post([FromBody]TagFileRequestLegacy request)
    {
      request.Validate();

      return ExecuteRequest(request);
    }

    /// <summary>
    /// For accepting and loading manually or automatically submitted tag files.
    /// </summary>
    /// <remarks>
    /// Manually submitted tag files include a project Id, the service performs a lookup for the boundary.
    /// </remarks>
    [PostRequestVerifier]
    [Route("api/v2/tagfiles")]
    [HttpPost]
    public async Task<IActionResult> PostTagFile([FromBody]CompactionTagFileRequest request)
    {
      var serializedRequest = SerializeObjectIgnoringProperties(request, "Data");
      log.LogDebug("PostTagFile: " + serializedRequest);

      var legacyProjectId = GetLegacyProjectId(request.ProjectUid).Result;
      WGS84Fence boundary = null;

      if (legacyProjectId != VelociraptorConstants.NO_PROJECT_ID)
      {
        boundary = await GetProjectBoundary(legacyProjectId);
      }

      var tagFileRequest = TagFileRequestLegacy.CreateTagFile(request.FileName, request.Data, legacyProjectId, boundary, VelociraptorConstants.NO_MACHINE_ID, false, false, request.OrgId);
      tagFileRequest.Validate();

      return ExecuteRequest(tagFileRequest);
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
    public ObjectResult PostTagFileDirectSubmission([FromBody]CompactionTagFileRequest request)
    {
      var serializedRequest = SerializeObjectIgnoringProperties(request, "Data");
      log.LogDebug("PostTagFile (Direct): " + serializedRequest);

      var tfRequest = TagFileRequestLegacy.CreateTagFile(request.FileName, request.Data, VelociraptorConstants.NO_PROJECT_ID, null, VelociraptorConstants.NO_MACHINE_ID, false, false);
      tfRequest.Validate();

      var result = RequestExecutorContainerFactory
             .Build<TagFileDirectSubmissionExecutor>(logger, raptorClient, tagProcessor, configStore, null, null, null, null, transferProxy)
             .Process(tfRequest) as TagFileDirectSubmissionResult;

      if (result.Code == 0)
      {
        log.LogDebug($"PostTagFile (Direct): Successfully imported TAG file '{request.FileName}'.");
        return StatusCode((int)HttpStatusCode.OK, result);
      }

      log.LogDebug($"PostTagFile (Direct): Failed to import TAG file '{request.FileName}', {result.Message}");
      return StatusCode((int)HttpStatusCode.BadRequest, result);
    }

    private IActionResult ExecuteRequest(TagFileRequestLegacy tfRequest)
    {
      var responseObj = RequestExecutorContainerFactory
                        .Build<TagFileExecutor>(logger, raptorClient, tagProcessor)
                        .Process(tfRequest);

      return responseObj.Code == 0
        ? (IActionResult)Ok(responseObj)
        : BadRequest(responseObj);
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

    /// <summary>
    /// Gets the WGS84 project boundary geofence for a given project Id.
    /// </summary>
    private async Task<WGS84Fence> GetProjectBoundary(long legacyProjectId)
    {
      var projectData = await((RaptorPrincipal)User).GetProject(legacyProjectId);

      return projectData.ProjectGeofenceWKT == null
        ? null
        : new WGS84Fence(RaptorConverters.geometryToPoints(projectData.ProjectGeofenceWKT).ToArray());
    }
  }
}
