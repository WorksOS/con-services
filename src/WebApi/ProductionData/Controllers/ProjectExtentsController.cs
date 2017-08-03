using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApiModels.ProductionData.Contracts;
using VSS.Productivity3D.WebApiModels.ProductionData.Executors;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class ProjectExtentsController : Controller, IProjectExtentsContract
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
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    public ProjectExtentsController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<ProjectExtentsController>();
    }


    /// <summary>
    /// Returns a projects data extents information.
    /// </summary>
    /// <param name="request">Parameters to request project data extents</param>
    /// <returns></returns>
    /// <executor>ProjectExtentsSubmitter</executor>
    /// 
    [PostRequestVerifier]
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v1/projectextents")]
    [HttpPost]

    public ProjectExtentsResult Post([FromBody] ExtentRequest request)
    {
      return RequestExecutorContainerFactory.Build<ProjectExtentsSubmitter>(logger, raptorClient, null).Process(request) as ProjectExtentsResult;
    }
  }
}
