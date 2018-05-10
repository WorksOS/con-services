using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
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

    private async Task<long> GetLegacyProjectId(Guid? projectUid) => projectUid == null
      ? VelociraptorConstants.NO_PROJECT_ID
      : await ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);

    /// <summary>
    /// Default constructor.
    /// </summary>
    public TagFileController(IASNodeClient raptorClient, ITagProcessor tagProcessor, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.tagProcessor = tagProcessor;
      this.logger = logger;
      log = logger.CreateLogger<TagFileController>();
    }

    /// <summary>
    /// Posts TAG file to Raptor. 
    /// </summary>
    [PostRequestVerifier]
    [Route("api/v1/tagfiles")]
    [HttpPost]
    public TagFilePostResult Post([FromBody]TagFileRequest request)
    {
      request.Validate();

      return RequestExecutorContainerFactory
             .Build<TagFileExecutor>(logger, raptorClient, tagProcessor)
             .Process(request) as TagFilePostResult;
    }

    /// <summary>
    /// For accepting and loading manually or automatically submitted tag files.
    /// </summary>
    [PostRequestVerifier]
    [Route("api/v2/tagfiles")]
    [HttpPost]
    public ContractExecutionResult PostTagFile([FromBody]CompactionTagFileRequest request)
    {
      // Serialize the request ignoring the Data property so not to overwhelm the logs.
      var serializedRequest = JsonConvert.SerializeObject(
        request,
        Formatting.None,
        new JsonSerializerSettings { ContractResolver = new DynamicContractResolver("Data") });

      log.LogDebug("PostTagFile: " + serializedRequest);

      var projectId = GetLegacyProjectId(request.ProjectUid).Result;

      var tfRequest = TagFileRequest.CreateTagFile(request.FileName, request.Data, projectId, null, VelociraptorConstants.NO_MACHINE_ID, false, false, request.OrgId);
      tfRequest.Validate();

      return RequestExecutorContainerFactory
             .Build<TagFileExecutor>(logger, raptorClient, tagProcessor)
             .Process(tfRequest);
    }

    /// <summary>
    /// For the direct submission of tag files from GNSS capable machines.
    /// </summary>
    [PostRequestVerifier]
    [Route("api/v3/tagfiles")]
    [HttpPost]
    public ObjectResult PostTagFileDirectSubmission([FromBody]CompactionTagFileRequest request)
    {
      // Serialize the request ignoring the Data property so not to overwhelm the logs.
      var serializedRequest = JsonConvert.SerializeObject(
        request,
        Formatting.None,
        new JsonSerializerSettings { ContractResolver = new DynamicContractResolver("Data") });

      log.LogDebug("PostTagFile (Direct): " + serializedRequest);

      var projectId = GetLegacyProjectId(request.ProjectUid).Result;

      var tfRequest = TagFileRequest.CreateTagFile(request.FileName, request.Data, projectId, null, VelociraptorConstants.NO_MACHINE_ID, false, false);
      tfRequest.Validate();

      var result = RequestExecutorContainerFactory
             .Build<TagFileDirectSubmissionExecutor>(logger, raptorClient, tagProcessor)
             .Process(tfRequest) as TagFileDirectSubmissionResult;

      if (result.Code == 0)
      {
        log.LogDebug($"PostTagFile (Direct): Successfully imported TAG file '{request.FileName}'.");
        return StatusCode((int)HttpStatusCode.OK, result);
      }

      log.LogDebug($"PostTagFile (Direct): Failed to import TAG file '{request.FileName}', {result.Message}");
      return StatusCode((int)HttpStatusCode.BadRequest, result);
    }
  }
}
