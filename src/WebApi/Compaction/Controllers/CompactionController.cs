using System;
using System.Net;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Compaction.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Report.Executors;
using VSS.Raptor.Service.WebApiModels.Report.Models;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;

namespace VSS.Raptor.Service.WebApi.Compaction.Controllers
{
  public class CompactionController : Controller
  {
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
    /// Constructor with injected raptor client, logger and authenticated projects
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="authProjectsStore">Authenticated projects store</param>
    public CompactionController(IASNodeClient raptorClient, ILoggerFactory logger,
      IAuthenticatedProjectsStore authProjectsStore)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<CompactionController>();
      this.authProjectsStore = authProjectsStore;
    }

    /// <summary>
    /// Get CMV summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns></returns>
    [Microsoft.AspNetCore.Mvc.Route("api/v2/compaction/cmv/summary")]
    [Microsoft.AspNetCore.Mvc.HttpGet]
    public CmvSummaryResult GetCmvSummary([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetCmvSummary: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      CMVSettings cmvSettings;
      LiftBuildSettings liftSettings;
      Filter filter;
      try
      {
        cmvSettings = JsonConvert.DeserializeObject<CMVSettings>("{'overrideTargetCMV': 'false', 'minCMVPercent': '80', 'maxCMVPercent': '120'}");
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}");//4 = None
        filter = !startUtc.HasValue && !endUtc.HasValue ? null :
          JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
                      new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                          ex.Message));
      }
      CMVRequest request = CMVRequest.CreateCMVRequest(projectId.Value, null, cmvSettings, liftSettings, filter, -1, null, null, null);
      request.Validate();
      var result = RequestExecutorContainer.Build<SummaryCMVExecutor>(logger, raptorClient, null).Process(request) as CMVSummaryResult;
      return CmvSummaryResult.CreateCmvSummaryResult(result, cmvSettings);
    }
  }
}
