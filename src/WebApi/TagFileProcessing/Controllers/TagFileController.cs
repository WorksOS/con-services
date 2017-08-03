using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApiModels.TagfileProcessing.Contracts;
using VSS.Productivity3D.WebApiModels.TagfileProcessing.Executors;
using VSS.Productivity3D.WebApiModels.TagfileProcessing.Models;
using VSS.Productivity3D.WebApiModels.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApi.TagFileProcessing.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(NoStore = true)]
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
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="tagProcessor">Tag processor client</param>
    /// <param name="logger">Logger</param>
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
    [ProjectIdVerifier]
    [ProjectWritableVerifier]
    [NotLandFillProjectVerifier]
    [ProjectUidVerifier]
    [ProjectWritableWithUIDVerifier]
    [NotLandFillProjectWithUIDVerifier]
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
    [PostRequestVerifier]
    [ProjectUidVerifier]
    [ProjectWritableWithUIDVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/tagfiles")]
    [HttpPost]
    public ContractExecutionResult PostTagFile([FromBody]CompactionTagFileRequest request)
    {
      log.LogDebug("PostTagFile: " + JsonConvert.SerializeObject(request));
      var projectDescr = (User as RaptorPrincipal).GetProject(request.projectUid);
      var boundary = WGS84Fence.CreateWGS84Fence(RaptorConverters.geometryToPoints(projectDescr.projectGeofenceWKT).ToArray());
      TagFileRequest tfRequest = TagFileRequest.CreateTagFile(request.fileName, request.data, projectDescr.projectId, boundary, -1, false, false);
      tfRequest.Validate();
      return RequestExecutorContainerFactory.Build<TagFileExecutor>(logger, raptorClient, tagProcessor).Process(tfRequest);
    }
  }
}