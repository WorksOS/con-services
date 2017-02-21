using System;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Filters.Authentication;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Utilities;
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
    /// Proxy for getting projects from master data. Used to convert project UID into project ID for Raptor.
    /// </summary>
    private readonly IProjectProxy projectProxy;
    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="projectProxy">Proxy client to use for getting projects</param>
    /// <param name="logger">Logger</param>
    /// <param name="raptorClient">Raptor client</param>
    public CCAColorPaletteController(IProjectProxy projectProxy, ILoggerFactory logger, IASNodeClient raptorClient)
    {
      this.projectProxy = projectProxy;
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
    [System.Web.Http.Route("api/v1/ccacolors")]
    [System.Web.Http.HttpGet]
    public CCAColorPaletteResult Get([FromUri] long projectId,
                                     [FromUri] long assetId, 
                                     [FromUri] DateTime? startUtc = null, 
                                     [FromUri] DateTime? endUtc = null, 
                                     [FromUri] int? liftId = null)
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
    [System.Web.Http.Route("api/v2/ccacolors")]
    [System.Web.Http.HttpGet]
    public CCAColorPaletteResult Get([FromUri] Guid? projectUid,
                                     [FromUri] long assetId,
                                     [FromUri] DateTime? startUtc = null,
                                     [FromUri] DateTime? endUtc = null,
                                     [FromUri] int? liftId = null)
    {
      log.LogInformation("Get: " + Request.QueryString);

      long projectId = 0;

      
      ProjectID.CheckProjectId(projectUid, ref projectId, projectProxy, RequestUtils.GetCustomHeaders(Request.Headers));

      var request = CCAColorPaletteRequest.CreateCCAColorPaletteRequest(projectId, assetId, startUtc, endUtc, liftId);

      request.Validate();

      return RequestExecutorContainer.Build<CCAColorPaletteExecutor>(logger, raptorClient, null).Process(request) as CCAColorPaletteResult;
    }
  }
}