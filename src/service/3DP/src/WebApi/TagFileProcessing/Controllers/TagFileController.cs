using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.WebApi.Compaction.Utilities;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;
using VSS.TCCFileAccess;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.TagFileProcessing.Controllers
{
  /// <summary>
  /// For handling Tag file submissions from either TCC or machines equiped with direct submission capable units.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class TagFileController : Controller
  {
#if RAPTOR
    private readonly ITagProcessor _tagProcessor;
    private readonly IASNodeClient _raptorClient;
#endif
    private readonly ILogger _log;
    private readonly ILoggerFactory _logger;
    private readonly ITransferProxy _transferProxy;
    private readonly IConfigurationStore _configStore;
    private readonly ITRexTagFileProxy _tRexTagFileProxy;
    private readonly ITRexConnectedSiteProxy _tRexConnectedSiteProxy;
    private readonly IFileRepository _tccRepository;
    private IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public TagFileController(
#if RAPTOR
      IASNodeClient raptorClient, 
      ITagProcessor tagProcessor, 
#endif
      ILoggerFactory logger, ITransferProxy transferProxy, ITRexTagFileProxy tRexTagFileProxy, ITRexConnectedSiteProxy tRexConnectedSiteProxy, IConfigurationStore configStore, IFileRepository tccRepository)
    {
#if RAPTOR
      _raptorClient = raptorClient;
      _tagProcessor = tagProcessor;
#endif
      _logger = logger;
      _log = logger.CreateLogger<TagFileController>();
      _transferProxy = transferProxy;
      _tRexTagFileProxy = tRexTagFileProxy;
      _tRexConnectedSiteProxy = tRexConnectedSiteProxy;
      _configStore = configStore;
      _tccRepository = tccRepository;
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
    public async Task<IActionResult> PostTagFileNonDirectSubmission([FromBody]CompactionTagFileRequest request)
    {
      var serializedRequest = JsonUtilities.SerializeObjectIgnoringProperties(request, "Data");
      _log.LogDebug($"{nameof(PostTagFileNonDirectSubmission)}: request {serializedRequest}");

      //First submit tag file to connected site gateway
      // Don't need to await as this process should be fire and forget there are more robust ways to do this but this will do for the moment
#pragma warning disable 4014
      RequestExecutorContainerFactory
        .Build<TagFileConnectedSiteSubmissionExecutor>(_logger,
#if RAPTOR
          _raptorClient, 
          _tagProcessor, 
#endif
          _configStore, transferProxy:_transferProxy, tRexTagFileProxy:_tRexTagFileProxy, tRexConnectedSiteProxy:_tRexConnectedSiteProxy, customHeaders: CustomHeaders)
        .ProcessAsync(request).ContinueWith((task) =>
        {
          if (task.IsFaulted)
          {
            _log.LogError(task.Exception, $"{nameof(PostTagFileNonDirectSubmission)}: Error Sending to Connected Site", null);
          }
        }, TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore 4014

      //Now submit tag file to Raptor and/or TRex
      ProjectData projectData = null;

      if (request.ProjectId != null)
        projectData = await ((RaptorPrincipal)User).GetProject(request.ProjectId.Value);
      else if (request.ProjectUid != null)
        projectData = await ((RaptorPrincipal)User).GetProject(request.ProjectUid.Value);

      if (projectData != null && projectData.IsArchived)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "The project has been archived and this function is not allowed."));
      }

      Task<WGS84Fence> boundary = null;
      if (request.ProjectUid != null)
      {
        var projectTask = GetLegacyProjectId(request.ProjectUid);
        boundary = GetProjectBoundary(request.ProjectUid.Value);

        await Task.WhenAll(projectTask, boundary);

        request.ProjectId = projectTask.Result;
      }

      var requestExt = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended(request, boundary?.Result);

      var responseObj = await RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(_logger,
#if RAPTOR
          _raptorClient, 
          _tagProcessor, 
#endif
          _configStore, transferProxy:_transferProxy,tRexTagFileProxy:_tRexTagFileProxy, tRexConnectedSiteProxy:_tRexConnectedSiteProxy, customHeaders: CustomHeaders)
        .ProcessAsync(requestExt);

      // when we disable Raptor, allowing Trex response to return to harvester,
      //  will need to rewrite the Trex result and handle these new codes in the Harvester.
      //  IMHO it would be nice to return the same response as for the DirectSubmission,
      //        which indicates whether a failure is permanent etc

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
      var serializedRequest = JsonUtilities.SerializeObjectIgnoringProperties(request, "Data");
      _log.LogDebug($"{nameof(PostTagFileDirectSubmission)}: request {serializedRequest}");

      // todoJeannie temporary to look into the device info available.
      _log.LogDebug($"{nameof(PostTagFileDirectSubmission)}: customHeaders {CustomHeaders.LogHeaders()}");

      var result = await RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionExecutor>(_logger,
#if RAPTOR
          _raptorClient, 
          _tagProcessor, 
#endif
          _configStore, _tccRepository, transferProxy:_transferProxy,tRexTagFileProxy:_tRexTagFileProxy,tRexConnectedSiteProxy:_tRexConnectedSiteProxy, customHeaders: CustomHeaders)
        .ProcessAsync(request) as TagFileDirectSubmissionResult;

      if (result?.Code == 0)
        return StatusCode((int)HttpStatusCode.OK, result);
      return StatusCode((int)HttpStatusCode.BadRequest, result);
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
      
      return projectData.ProjectTimeZoneIana == null
        ? null
        : new WGS84Fence(CommonConverters.GeometryToPoints(projectData.ProjectTimeZoneIana).ToArray());
    }
  }
}
