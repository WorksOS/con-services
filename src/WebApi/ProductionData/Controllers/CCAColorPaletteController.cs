using System;
using System.Security.Principal;
using Common.Filters.Authentication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Filters.Authentication;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.Contracts;
using VSS.Raptor.Service.WebApiModels.ProductionData.Executors;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;


namespace VSS.Raptor.Service.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Controller for CCA data colour palettes resource.
  /// </summary>
  /// 
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class CCAColorPaletteController : Controller, ICCAColorPaletteContract
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
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="raptorClient">Raptor client</param>
    public CCAColorPaletteController(ILoggerFactory logger, IASNodeClient raptorClient)
    {
      this.logger = logger;
      this.log = logger.CreateLogger<CCAColorPaletteController>();
      this.raptorClient = raptorClient;
    }

    /// <summary>
    /// Gets CCA data colour palette requested from Raptor with a project identifier.
    /// </summary>
    /// <param name="projectId">Raptor's data model/project identifier.</param>
    /// <param name="assetId">Raptor's machine identifier.</param>
    /// <param name="startUtc">Start date of the requeted CCA data in UTC.</param>
    /// <param name="endUtc">End date of the requested CCA data in UTC.</param>
    /// <param name="liftId">Lift identifier of the requested CCA data.</param>
    /// <returns>Execution result with a list of CCA data colour palettes.</returns>
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [Route("api/v1/ccacolors")]
    [HttpGet]
    public CCAColorPaletteResult Get([FromQuery] long projectId,
                                     [FromQuery] long assetId, 
                                     [FromQuery] DateTime? startUtc = null, 
                                     [FromQuery] DateTime? endUtc = null, 
                                     [FromQuery] int? liftId = null)
    {
      log.LogInformation("Get: " + Request.QueryString);

      var request = CCAColorPaletteRequest.CreateCCAColorPaletteRequest(projectId, assetId, startUtc, endUtc, liftId);

      request.Validate();

      return RequestExecutorContainer.Build<CCAColorPaletteExecutor>(logger, raptorClient, null).Process(request) as CCAColorPaletteResult;
    }

    /// <summary>
    /// Gets CCA data colour palette requested from Raptor with a project unique identifier.
    /// </summary>
    /// <param name="projectUid">Raptor's data model/project unique identifier.</param>
    /// <param name="assetId">Raptor's machine identifier.</param>
    /// <param name="startUtc">Start date of the requeted CCA data in UTC.</param>
    /// <param name="endUtc">End date of the requested CCA data in UTC.</param>
    /// <param name="liftId">Lift identifier of the requested CCA data.</param>
    /// <returns>Execution result with a list of CCA data colour palettes.</returns>
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/ccacolors")]
    [HttpGet]
    public CCAColorPaletteResult Get([FromQuery] Guid? projectUid,
                                     [FromQuery] long assetId,
                                     [FromQuery] DateTime? startUtc = null,
                                     [FromQuery] DateTime? endUtc = null,
                                     [FromQuery] int? liftId = null)
    {
      log.LogInformation("Get: " + Request.QueryString);

      long projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      var request = CCAColorPaletteRequest.CreateCCAColorPaletteRequest(projectId, assetId, startUtc, endUtc, liftId);
      request.Validate();

      return RequestExecutorContainer.Build<CCAColorPaletteExecutor>(logger, raptorClient, null).Process(request) as CCAColorPaletteResult;
    }
  }
}