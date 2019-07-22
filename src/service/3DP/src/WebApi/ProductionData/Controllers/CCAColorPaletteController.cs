using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Controller for CCA data colour palettes resource.
  /// </summary>
  /// 
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class CCAColorPaletteController : Controller, ICCAColorPaletteContract
  {
#if RAPTOR
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;
#endif
      
    /// <summary>
    /// LoggerFactory for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// The TRex Gateway proxy for use by executor.
    /// </summary>
    private readonly ITRexCompactionDataProxy TRexCompactionDataProxy;

    private readonly IConfigurationStore configurationStore;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public CCAColorPaletteController(ILoggerFactory logger,
#if RAPTOR
      IASNodeClient raptorClient,
#endif
      IConfigurationStore configStore,
      ITRexCompactionDataProxy trexCompactionDataProxy)
    {
      this.logger = logger;
      this.log = logger.CreateLogger<CCAColorPaletteController>();
#if RAPTOR
      this.raptorClient = raptorClient;
#endif
      this.TRexCompactionDataProxy = trexCompactionDataProxy;
      configurationStore = configStore;
    }

    /// <summary>
    /// Gets CCA data colour palette requested from Raptor with a project identifier.
    /// </summary>
    /// <param name="projectId">Raptor's data model/project identifier.</param>
    /// <param name="assetId">Raptor's machine identifier.</param>
    /// <param name="startUtc">Start date of the requeted CCA data in UTC.</param>
    /// <param name="endUtc">End date of the requested CCA data in UTC.</param>
    /// <param name="liftId">Lift identifier of the requested CCA data.</param>
    /// <param name="assetUid">TRex's machine identifier.</param>
    /// <returns>Execution result with a list of CCA data colour palettes.</returns>
    [ProjectVerifier]
    [Route("api/v1/ccacolors")]
    [HttpGet]
    public async Task<CCAColorPaletteResult> Get(
      [FromQuery] long projectId,
      [FromQuery] long assetId, 
      [FromQuery] DateTime? startUtc = null, 
      [FromQuery] DateTime? endUtc = null, 
      [FromQuery] int? liftId = null,
      [FromQuery] Guid? assetUid = null)
    {
      log.LogInformation("Get: " + Request.QueryString);

      var projectUid = await((RaptorPrincipal)User).GetProjectUid(projectId);
      var request = CCAColorPaletteRequest.CreateCCAColorPaletteRequest(projectId, assetId, startUtc, endUtc, liftId, projectUid, assetUid);
      request.Validate();
      return await RequestExecutorContainerFactory.Build<CCAColorPaletteExecutor>(logger,
#if RAPTOR
        raptorClient,
#endif
        configStore: configurationStore,
        trexCompactionDataProxy: TRexCompactionDataProxy).ProcessAsync(request) as CCAColorPaletteResult;
    }

    /// <summary>
    /// Gets CCA data colour palette requested from Raptor with a project unique identifier.
    /// </summary>
    /// <param name="projectUid">Raptor's data model/project unique identifier.</param>
    /// <param name="assetId">Raptor's machine identifier.</param>
    /// <param name="startUtc">Start date of the requeted CCA data in UTC.</param>
    /// <param name="endUtc">End date of the requested CCA data in UTC.</param>
    /// <param name="liftId">Lift identifier of the requested CCA data.</param>
    /// <param name="assetUid">TRex's machine identifier.</param>
    /// <returns>Execution result with a list of CCA data colour palettes.</returns>
    [ProjectVerifier]
    [Route("api/v2/ccacolors")]
    [HttpGet]
    public async Task<CCAColorPaletteResult> Get(
      [FromQuery] Guid projectUid,
      [FromQuery] long assetId,
      [FromQuery] DateTime? startUtc = null,
      [FromQuery] DateTime? endUtc = null,
      [FromQuery] int? liftId = null,
      [FromQuery] Guid? assetUid = null)
    {
      log.LogInformation("Get: " + Request.QueryString);

      long projectId = await ((RaptorPrincipal) User).GetLegacyProjectId(projectUid);
      var request = CCAColorPaletteRequest.CreateCCAColorPaletteRequest(projectId, assetId, startUtc, endUtc, liftId, projectUid, assetUid);
      request.Validate();
      return await RequestExecutorContainerFactory.Build<CCAColorPaletteExecutor>(logger,
#if RAPTOR
        raptorClient,
#endif   
        configStore: configurationStore,
        trexCompactionDataProxy: TRexCompactionDataProxy).ProcessAsync(request) as CCAColorPaletteResult;
    }
  }
}
