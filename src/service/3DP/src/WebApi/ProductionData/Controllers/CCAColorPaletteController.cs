using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Controller for CCA data colour palettes resource.
  /// </summary>
  /// 
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class CCAColorPaletteController : Controller, ICCAColorPaletteContract
  {
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
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="logger">LoggerFactory</param>
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
    [ProjectVerifier]
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

      return RequestExecutorContainerFactory.Build<CCAColorPaletteExecutor>(logger, raptorClient).Process(request) as CCAColorPaletteResult;
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
    [ProjectVerifier]
    [Route("api/v2/ccacolors")]
    [HttpGet]
    public async Task<CCAColorPaletteResult> Get([FromQuery] Guid projectUid,
                                     [FromQuery] long assetId,
                                     [FromQuery] DateTime? startUtc = null,
                                     [FromQuery] DateTime? endUtc = null,
                                     [FromQuery] int? liftId = null)
    {
      log.LogInformation("Get: " + Request.QueryString);

      long projectId = await ((RaptorPrincipal) User).GetLegacyProjectId(projectUid);
      var request = CCAColorPaletteRequest.CreateCCAColorPaletteRequest(projectId, assetId, startUtc, endUtc, liftId);
      request.Validate();

      return RequestExecutorContainerFactory.Build<CCAColorPaletteExecutor>(logger, raptorClient).Process(request) as CCAColorPaletteResult;
    }
  }
}
