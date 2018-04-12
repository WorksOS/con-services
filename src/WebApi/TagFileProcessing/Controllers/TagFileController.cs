using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.Productivity3D.WebApiModels.TagfileProcessing.Contracts;
using VSS.Productivity3D.WebApiModels.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApi.TagFileProcessing.Controllers
{
  /// <summary>
  /// Controller for handling v1 and v2 Tag File submissions to Raptor.
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
    /// Default constructor.
    /// </summary>
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
    [PostRequestVerifier]
    [Route("api/v1/tagfiles")]
    [HttpPost]
    public TAGFilePostResult Post([FromBody]TagFileRequest request)
    {
      request.Validate();

      return RequestExecutorContainerFactory
             .Build<TagFileExecutor>(logger, raptorClient, tagProcessor)
             .Process(request) as TAGFilePostResult;
    }

    /// <summary>
    /// Posts TAG file to Raptor. 
    /// </summary>
    /// <param name="request">TAG file structure.</param>
    [PostRequestVerifier]
    [Route("api/v2/tagfiles")]
    [HttpPost]
    public async Task<ContractExecutionResult> PostTagFile([FromBody]CompactionTagFileRequest request)
    {
      log.LogDebug("PostTagFile: " + JsonConvert.SerializeObject(request));

      var projectId = request.ProjectUid != null
        ? await (User as RaptorPrincipal).GetLegacyProjectId(request.ProjectUid)
        : -1;

      var tfRequest = TagFileRequest.CreateTagFile(request.FileName, request.Data, projectId, null, -1, false, false, request.OrgId);
      tfRequest.Validate();

      return RequestExecutorContainerFactory
             .Build<TagFileExecutor>(logger, raptorClient, tagProcessor)
             .Process(tfRequest);
    }
  }
}
