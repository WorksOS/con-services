using System;
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
using VSS.Raptor.Service.WebApiModels.Compaction.Models;
using VSS.Raptor.Service.WebApiModels.Compaction.ResultHandling;
using VSS.Raptor.Service.WebApiModels.ProductionData.Executors;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;
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

    private const int NO_CCV = SVOICDecls.__Global.kICNullCCVValue;


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

    #region Summary Data for Widgets
    /// <summary>
    /// Get CMV summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>CMV summary</returns>
    [Route("api/v2/compaction/cmv/summary")]
    [HttpGet]
    public CompactionCmvSummaryResult GetCmvSummary([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
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
        cmvSettings =
          JsonConvert.DeserializeObject<CMVSettings>(
            "{'overrideTargetCMV': 'false', 'minCMVPercent': '80', 'maxCMVPercent': '120'}");
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
        filter = !startUtc.HasValue && !endUtc.HasValue
          ? null
          : JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }
      CMVRequest request = CMVRequest.CreateCMVRequest(projectId.Value, null, cmvSettings, liftSettings, filter, -1,
        null, null, null);
      request.Validate();
      try
      {
        var result = RequestExecutorContainer.Build<SummaryCMVExecutor>(logger, raptorClient, null).Process(request) as CMVSummaryResult;
        return CompactionCmvSummaryResult.CreateCmvSummaryResult(result, cmvSettings);
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }
        throw;
      }

    }

    /// <summary>
    /// Get MDP summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>MDP summary</returns>
    [Route("api/v2/compaction/mdp/summary")]
    [HttpGet]
    public CompactionMdpSummaryResult GetMdpSummary([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetMdpSummary: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      MDPSettings mdpSettings;
      LiftBuildSettings liftSettings;
      Filter filter;
      try
      {
        mdpSettings =
          JsonConvert.DeserializeObject<MDPSettings>(
            "{'overrideTargetMDP': 'false', 'minMDPPercent': '80', 'maxMDPPercent': '120'}");
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
        filter = !startUtc.HasValue && !endUtc.HasValue
          ? null
          : JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }
      MDPRequest request = MDPRequest.CreateMDPRequest(projectId.Value, null, mdpSettings, liftSettings, filter, -1,
        null, null, null);
      request.Validate();
      try
      {
        var result = RequestExecutorContainer.Build<SummaryMDPExecutor>(logger, raptorClient, null).Process(request) as MDPSummaryResult;
        return CompactionMdpSummaryResult.CreateMdpSummaryResult(result, mdpSettings);
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }
        throw;
      }
    }

    /// <summary>
    /// Get pass count summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>Pass count summary</returns>
    [Route("api/v2/compaction/passcounts/summary")]
    [HttpGet]
    public CompactionPassCountSummaryResult GetPassCountSummary([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetPassCountSummary: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      PassCountSettings passCountSettings;
      LiftBuildSettings liftSettings;
      Filter filter;
      try
      {
        //TODO: wire pass count settings when we have them as a parameter to this call
        passCountSettings = JsonConvert.DeserializeObject<PassCountSettings>(
            "{'passCounts': '[4,7]'}");
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
        filter = !startUtc.HasValue && !endUtc.HasValue
          ? null
          : JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }
      PassCounts request = PassCounts.CreatePassCountsRequest(projectId.Value, null, null, liftSettings, filter, -1,
        null, null, null);
      request.Validate();
      try
      {
        var result = RequestExecutorContainer.Build<SummaryPassCountsExecutor>(logger, raptorClient, null).Process(request) as PassCountSummaryResult;
        return CompactionPassCountSummaryResult.CreatePassCountSummaryResult(result, passCountSettings);
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }
        throw;
      }
    }

    /// <summary>
    /// Get Temperature summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>Temperature summary</returns>
    [Route("api/v2/compaction/temperature/summary")]
    [HttpGet]
    public CompactionTemperatureSummaryResult GetTemperatureSummary([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetTemperatureSummary: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      TemperatureSettings temperatureSettings;
      LiftBuildSettings liftSettings;
      Filter filter;
      try
      {
        temperatureSettings = 
          JsonConvert.DeserializeObject<TemperatureSettings>(
            "{'overrideTemperatureRange': 'false', 'minTemperature': '65.0', 'maxTemperature': '175.0'}");
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
        filter = !startUtc.HasValue && !endUtc.HasValue
          ? null
          : JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }
      TemperatureRequest request = TemperatureRequest.CreateTemperatureRequest(projectId.Value, null, temperatureSettings, liftSettings, filter, -1,
        null, null, null);
      request.Validate();
      try
      {
        var result = RequestExecutorContainer.Build<SummaryTemperatureExecutor>(logger, raptorClient, null).Process(request) as TemperatureSummaryResult;
        return CompactionTemperatureSummaryResult.CreateTemperatureSummaryResult(result);
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }
        throw;
      }
    }

    /// <summary>
    /// Get Speed summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>Speed summary</returns>
    [Route("api/v2/compaction/speed/summary")]
    [HttpGet]
    public CompactionSpeedSummaryResult GetSpeedSummary([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetSpeedSummary: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      LiftBuildSettings liftSettings;
      Filter filter;
      try
      {
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
        filter = !startUtc.HasValue && !endUtc.HasValue
          ? null
          : JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }
      SummarySpeedRequest request = SummarySpeedRequest.CreateSummarySpeedRequestt(projectId.Value, null, liftSettings, filter, -1);
      request.Validate();
      try
      {
        var result = RequestExecutorContainer.Build<SummarySpeedExecutor>(logger, raptorClient, null).Process(request) as SummarySpeedResult;
        return CompactionSpeedSummaryResult.CreateSpeedSummaryResult(result);
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }
        throw;
      }
    }

    /// <summary>
    /// Get CMV % change from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <returns>CMV % change</returns>
    [Route("api/v2/compaction/cmv/percentchange")]
    [HttpGet]
    public CompactionCmvPercentChangeResult GetCmvPercentChange([FromQuery] long? projectId, [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc, [FromQuery] DateTime? endUtc)
    {
      log.LogInformation("GetCmvPercentChange: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      double[] cmvChangeSummarySettings;
      LiftBuildSettings liftSettings;
      Filter filter;
      try
      {
        cmvChangeSummarySettings = new double[] {5, 20, 50, NO_CCV};
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
        filter = !startUtc.HasValue && !endUtc.HasValue
          ? null
          : JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }
      CMVChangeSummaryRequest request = CMVChangeSummaryRequest.CreateCMVChangeSummaryRequest(projectId.Value, null, liftSettings, filter, -1,
        cmvChangeSummarySettings);
      request.Validate();
      try
      {
        var result = RequestExecutorContainer.Build<CMVChangeSummaryExecutor>(logger, raptorClient, null).Process(request) as CMVChangeSummaryResult;
        return CompactionCmvPercentChangeResult.CreateCmvPercentChangeResult(result, cmvChangeSummarySettings);
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        if (se.Response.StatusCode == HttpStatusCode.BadRequest &&
            se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
        {
          se.Response.StatusCode = HttpStatusCode.NoContent;
        }
        throw;
      }
    }

    #endregion

    /*
    #region Palettes

    /// <summary>
    /// Get elevation color palette. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <returns>Elevation color palette</returns>
    [Route("api/v2/compaction/elevationcolorpalette")]
    [HttpGet]
    public CompactionCmvSummaryResult GetElevationColorPalette([FromQuery] long? projectId, [FromQuery] Guid? projectUid)
    {
      //hard coded elevation colors as per CG - see WebMapCache.ElevationPalatte
      //calculate values from min-max elevation - see WebMapCache.ConvertProjectColors
      //return color, value pairs + min/max elev, over and under colors with -1 value
      throw new NotImplementedException();

      //TODO: Make a palettes endpoint that will return a palette for any display type
      //so UI can call for values to then send back to raptor for tiles 
    }

    #endregion
  */

    #region Tiles
    /// <summary>
    /// Supplies tiles of rendered overlays for a number of different thematic sets of data held in a project such as 
    /// elevation, compaction, temperature, cut/fill, volumes etc
    /// </summary>
    /// <param name="request">A representation of the tile rendering request.</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds.</returns>
    /// <executor>TilesExecutor</executor> 
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/compaction/tiles")]
    [HttpPost]
    public TileResult PostTile([FromBody] CompactionTileRequest request)
    {
      request.Validate();

      LiftBuildSettings liftSettings;
      try
      {
        liftSettings = JsonConvert.DeserializeObject<LiftBuildSettings>("{'liftDetectionType': '4'}"); //4 = None
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }
      var layerMethod = request.filter == null || !request.filter.layerNumber.HasValue ? (FilterLayerMethod?)null : FilterLayerMethod.TagfileLayerNumber;

      Filter filter = request.filter == null ? null : Filter.CreateFilter(null, null, null, request.filter.startUTC, request.filter.endUTC, 
        request.filter.onMachineDesignID, null, request.filter.vibeStateOn, null, request.filter.elevationType, 
        null, null, null, null, null, null, null, null, null, layerMethod, null, null, 
        request.filter.layerNumber, null, request.filter.contributingMachines, null, null, null, null, null, null, null);
      filter?.Validate();

      //TODO: Check with Dmitry/Alan FilterLayerMethod in both filter and request !!!

      layerMethod = request.filter == null || !request.filter.layerNumber.HasValue ? FilterLayerMethod.None : FilterLayerMethod.TagfileLayerNumber;

      TileRequest tileRequest = TileRequest.CreateTileRequest(request.projectId.Value, null, request.mode, request.palette,
        liftSettings, RaptorConverters.VolumesType.None, 0, null, filter, 0, null, 0, layerMethod.Value, 
        request.boundBoxLL, null, request.width, request.height, 0);
      tileRequest.Validate();
      var tileResult = RequestExecutorContainer.Build<TilesExecutor>(logger, raptorClient, null).Process(request) as TileResult;
      return tileResult;
    }
    #endregion

  }
}
