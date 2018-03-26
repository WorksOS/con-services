using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApiModels.TagfileProcessing.Contracts;
using VSS.Productivity3D.WebApiModels.TagfileProcessing.Executors;
using VSS.Productivity3D.WebApiModels.TagfileProcessing.Models;
using VSS.Productivity3D.WebApiModels.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApi.TagFileProcessing.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class TagFileController : Controller, ITagFileContract
  {
    /// <summary>
    /// Tag processor for use by executor
    /// </summary>
    private readonly ITagProcessor tagProcessor;

    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// LoggerFactory for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="tagProcessor">Tag processor client</param>
    /// <param name="logger">LoggerFactory</param>
    public TagFileController(IASNodeClient raptorClient, ITagProcessor tagProcessor, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.tagProcessor = tagProcessor;
      this.logger = logger;
      this.log = logger.CreateLogger<TagFileController>();
    }


    /// <summary>
    /// Posts TAG file to Raptor. 
    /// </summary>
    /// <param name="request">TAG file structure.</param>
    /// <returns>Returns JSON structure wtih operation result. {"Code":0,"Message":"User-friendly"}
    /// </returns>
    /// <executor>TagFileExecutor</executor>
    /// 
    [PostRequestVerifier]
        // [ProjectIdVerifier]  Not a requirement to present a TAG file for processing
        // [ProjectWritableVerifier]  Not a requirement to present a TAG file for processing
        // [NotLandFillProjectVerifier]  Not a requirement to present a TAG file for processing
        // [ProjectUidVerifier]  Not a requirement to present a TAG file for processing
        // [ProjectWritableWithUIDVerifier]  Not a requirement to present a TAG file for processing
        // [NotLandFillProjectWithUIDVerifier]  Not a requirement to present a TAG file for processing
    [Route("api/v1/tagfiles")]
    [HttpPost]
    public TAGFilePostResult Post([FromBody]TagFileRequest request)
    {
      request.Validate();
      return RequestExecutorContainerFactory.Build<TagFileExecutor>(logger, raptorClient, tagProcessor).Process(request) as TAGFilePostResult;
    }

    /// <summary>
    /// Posts TAG file to Raptor. 
    /// </summary>
    /// <param name="request">TAG file structure.</param>
    /// <returns>Returns JSON structure wtih operation result. {"Code":0,"Message":"User-friendly"}
    /// </returns>
    /// <executor>TagFileExecutor</executor>
    /// 
    [PostRequestVerifier] // TODO (Aaron) Is this required here? Is this API called by TBC ? If not it could be removed...?
        // [ProjectUidVerifier]  Not a requirement to present a TAG file for processing
        // [ProjectWritableWithUIDVerifier]  Not a requirement to present a TAG file for processing
        // [NotLandFillProjectWithUIDVerifier]  Not a requirement to present a TAG file for processing
    [Route("api/v2/tagfiles")]
    [HttpPost]
    public ContractExecutionResult PostTagFile([FromBody]CompactionTagFileRequest request)
    {
      log.LogDebug("PostTagFile: " + JsonConvert.SerializeObject(request));
      long projectId = -1;
      if (User is RaptorPrincipal && request.projectUid.HasValue)
      {
        var projectDescr = (User as RaptorPrincipal).GetProject(request.projectUid);
        projectId = projectDescr.projectId;

      }
//      var boundary = WGS84Fence.CreateWGS84Fence(RaptorConverters.geometryToPoints(projectDescr.projectGeofenceWKT).ToArray());
      TagFileRequest tfRequest = TagFileRequest.CreateTagFile(request.fileName, request.data, projectId, null /*boundary*/, -1, false, false);
      tfRequest.Validate();
      return RequestExecutorContainerFactory.Build<TagFileExecutor>(logger, raptorClient, tagProcessor).Process(tfRequest);
    }
  }
}