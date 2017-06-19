using System.Linq;
using System.Net;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.TagfileProcessing.Contracts;
using VSS.Raptor.Service.WebApiModels.TagfileProcessing.Executors;
using VSS.Raptor.Service.WebApiModels.TagfileProcessing.Models;
using VSS.Raptor.Service.WebApiModels.TagfileProcessing.ResultHandling;
using WebApiModels.TagfileProcessing.Models;

namespace VSS.Raptor.Service.WebApi.TagFileProcessing.Controllers
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
    /// Used to get list of projects for customer
    /// </summary>
    private readonly IAuthenticatedProjectsStore authProjectsStore;

    /// <summary>
    /// Constructor with injected raptor client and logger
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="tagProcessor">Tag processor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="authProjectsStore">Authenticated projects store</param>
    public TagFileController(IASNodeClient raptorClient, ITagProcessor tagProcessor, ILoggerFactory logger, IAuthenticatedProjectsStore authProjectsStore)
    {
      this.raptorClient = raptorClient;
      this.tagProcessor = tagProcessor;
      this.logger = logger;
      this.log = logger.CreateLogger<TagFileController>();
      this.authProjectsStore = authProjectsStore;
    }


    /// <summary>
    /// Posts TAG file to Raptor. 
    /// </summary>
    /// <param name="request">TAG file structure.</param>
    /// <returns>Returns JSON structure wtih operation result. {"Code":0,"Message":"User-friendly"}
    /// </returns>
    /// <executor>TagFileExecutor</executor>
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
      return RequestExecutorContainer.Build<TagFileExecutor>(logger, raptorClient, tagProcessor).Process(request) as TAGFilePostResult;
    }

    /// <summary>
    /// Posts TAG file to Raptor. 
    /// </summary>
    /// <param name="request">TAG file structure.</param>
    /// <returns>Returns JSON structure wtih operation result. {"Code":0,"Message":"User-friendly"}
    /// </returns>
    /// <executor>TagFileExecutor</executor>
    [ProjectUidVerifier]
    [ProjectWritableWithUIDVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/tagfiles")]
    [HttpPost]
    public ContractExecutionResult PostTagFile([FromBody]CompactionTagFileRequest request)
    {
      log.LogDebug("PostTagFile: " + JsonConvert.SerializeObject(request));

      var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      long projectId = ProjectID.GetProjectId(customerUid, request.projectUid, authProjectsStore);
      var projectsById = authProjectsStore.GetProjectsById(customerUid);
      string projectGeofenceWKT = projectsById[projectId].projectGeofenceWKT;
      var boundary = WGS84Fence.CreateWGS84Fence(RaptorConverters.geometryToPoints(projectGeofenceWKT).ToArray());
      TagFileRequest tfRequest = TagFileRequest.CreateTagFile(request.fileName, request.data, projectId, boundary, -1, false, false);
      tfRequest.Validate();
      return RequestExecutorContainer.Build<TagFileExecutor>(logger, raptorClient, tagProcessor).Process(tfRequest);
    }
  }
}
