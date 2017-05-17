using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Filters.Authentication;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.WebApiModels.ProductionData.Contracts;
using VSS.Raptor.Service.WebApiModels.ProductionData.Executors;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Raptor.Service.WebApi.ProductionData.Controllers
{
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
    /// Constructor with injected raptor client and logger
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
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v1/projectextents")]
    [HttpPost]

    public ProjectExtentsResult Post([FromBody] ExtentRequest request)
    {
      return RequestExecutorContainer.Build<ProjectExtentsSubmitter>(logger, raptorClient, null).Process(request) as ProjectExtentsResult;
    }


  }
}
