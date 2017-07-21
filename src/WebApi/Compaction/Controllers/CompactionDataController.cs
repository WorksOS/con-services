using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Controllers;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Executors;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class CompactionDataController : Controller
  {
    /// <summary>
    /// Raptor client for use by executor
    /// 
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
    /// For getting imported files for a project
    /// </summary>
    private readonly IFileListProxy fileListProxy;


    /// <summary>
    /// For getting project settings for a project
    /// </summary>
    private readonly IProjectSettingsProxy projectSettingsProxy;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="fileListProxy">File list proxy</param>
    /// <param name="projectSettingsProxy">Project settings proxy</param>
    public CompactionDataController(IASNodeClient raptorClient, ILoggerFactory logger, IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<CompactionDataController>();
      this.fileListProxy = fileListProxy;
      this.projectSettingsProxy = projectSettingsProxy;
    }

 
    #region Summary Data for Widgets

    /// <summary>
    /// Get CMV summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
    /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
    /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
    /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
    /// <param name="layerNumber">The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
    ///  to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
    /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.</param>
    /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
    /// All three parameters must be specified to specify a machine. 
    /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
    /// <param name="machineName">See assetID</param>
    /// <param name="isJohnDoe">See assetIDL</param>
    /// <returns>CMV summary</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/cmv/summary")]
    [HttpGet]
    public async Task<CompactionCmvSummaryResult> GetCmvSummary(
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] bool? vibeStateOn,
      [FromQuery] ElevationType? elevationType,
      [FromQuery] int? layerNumber,
      [FromQuery] long? onMachineDesignId,
      [FromQuery] long? assetID,
      [FromQuery] string machineName,
      [FromQuery] bool? isJohnDoe)
    {
      log.LogInformation("GetCmvSummary: " + Request.QueryString);
      CMVRequest request = await GetCMVRequest(projectId, projectUid, startUtc, endUtc, vibeStateOn, elevationType,
        layerNumber, onMachineDesignId, assetID, machineName, isJohnDoe);
      request.Validate();

      try
      {
        var result =
          RequestExecutorContainer.Build<SummaryCMVExecutor>(logger, raptorClient, null).Process(request) as
            CMVSummaryResult;
        var returnResult = CompactionCmvSummaryResult.CreateCmvSummaryResult(result, request.cmvSettings);
        log.LogInformation("GetCmvSummary result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        this.ProcessStatusCode(se);
        throw;
      }
      finally
      {
        log.LogInformation("GetCmvSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get MDP summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
    /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
    /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
    /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
    /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
    ///  to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
    /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.</param>
    /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
    /// All three parameters must be specified to specify a machine. 
    /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
    /// <param name="machineName">See assetID</param>
    /// <param name="isJohnDoe">See assetIDL</param>
    /// <returns>MDP summary</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/mdp/summary")]
    [HttpGet]
    public async Task<CompactionMdpSummaryResult> GetMdpSummary(
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] bool? vibeStateOn,
      [FromQuery] ElevationType? elevationType,
      [FromQuery] int? layerNumber,
      [FromQuery] long? onMachineDesignId,
      [FromQuery] long? assetID,
      [FromQuery] string machineName,
      [FromQuery] bool? isJohnDoe)
    {
      log.LogInformation("GetMdpSummary: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      }
      var headers = Request.Headers.GetCustomHeaders();
      var projectSettings = await this.GetProjectSettings(projectSettingsProxy, projectUid.Value, headers, log);
      MDPSettings mdpSettings = CompactionSettings.CompactionMdpSettings(projectSettings);
      LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings(projectSettings);
      var excludedIds = await this.GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid.Value,
        headers);
      Filter filter = CompactionSettings.CompactionFilter(
        startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
        this.GetMachines(assetID, machineName, isJohnDoe), excludedIds);
      MDPRequest request = MDPRequest.CreateMDPRequest(projectId.Value, null, mdpSettings, liftSettings, filter,
        -1,
        null, null, null);
      request.Validate();
      try
      {
        var result = RequestExecutorContainer.Build<SummaryMDPExecutor>(logger, raptorClient, null)
          .Process(request) as MDPSummaryResult;
        var returnResult = CompactionMdpSummaryResult.CreateMdpSummaryResult(result, mdpSettings);
        log.LogInformation("GetMdpSummary result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        this.ProcessStatusCode(se);
        throw;
      }
      finally
      {
        log.LogInformation("GetMdpSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get pass count summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
    /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
    /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
    /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
    /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
    ///  to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
    /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.</param>
    /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
    /// All three parameters must be specified to specify a machine. 
    /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
    /// <param name="machineName">See assetID</param>
    /// <param name="isJohnDoe">See assetIDL</param>
    /// <returns>Pass count summary</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/passcounts/summary")]
    [HttpGet]
    public async Task<CompactionPassCountSummaryResult> GetPassCountSummary(
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] bool? vibeStateOn,
      [FromQuery] ElevationType? elevationType,
      [FromQuery] int? layerNumber,
      [FromQuery] long? onMachineDesignId,
      [FromQuery] long? assetID,
      [FromQuery] string machineName,
      [FromQuery] bool? isJohnDoe)
    {
      log.LogInformation("GetPassCountSummary: " + Request.QueryString);

      PassCounts request = await GetPassCountRequest(projectId, projectUid, startUtc, endUtc, vibeStateOn,
        elevationType, layerNumber, onMachineDesignId, assetID, machineName, isJohnDoe, true);
      request.Validate();

      try
      {
        var result = RequestExecutorContainer.Build<SummaryPassCountsExecutor>(logger, raptorClient, null)
          .Process(request) as PassCountSummaryResult;
        var returnResult = CompactionPassCountSummaryResult.CreatePassCountSummaryResult(result);
        log.LogInformation("GetPassCountSummary result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        this.ProcessStatusCode(se);
        throw;
      }
      finally
      {
        log.LogInformation("GetPassCountSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get Temperature summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
    /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
    /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
    /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
    /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
    ///  to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
    /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.</param>
    /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
    /// All three parameters must be specified to specify a machine. 
    /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
    /// <param name="machineName">See assetID</param>
    /// <param name="isJohnDoe">See assetIDL</param>
    /// <returns>Temperature summary</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/temperature/summary")]
    [HttpGet]
    public async Task<CompactionTemperatureSummaryResult> GetTemperatureSummary(
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] bool? vibeStateOn,
      [FromQuery] ElevationType? elevationType,
      [FromQuery] int? layerNumber,
      [FromQuery] long? onMachineDesignId,
      [FromQuery] long? assetID,
      [FromQuery] string machineName,
      [FromQuery] bool? isJohnDoe)
    {
      log.LogInformation("GetTemperatureSummary: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      }
      //TODO: if projectuid is null get from raptorprincipal project
      var headers = Request.Headers.GetCustomHeaders();
      var projectSettings = await this.GetProjectSettings(projectSettingsProxy, projectUid.Value, headers, log);
      TemperatureSettings temperatureSettings = CompactionSettings.CompactionTemperatureSettings(projectSettings);
      LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings(projectSettings);
      var excludedIds = await this.GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid.Value, headers);
      Filter filter = CompactionSettings.CompactionFilter(
        startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
        this.GetMachines(assetID, machineName, isJohnDoe), excludedIds);

      TemperatureRequest request = TemperatureRequest.CreateTemperatureRequest(projectId.Value, null,
        temperatureSettings, liftSettings, filter, -1, null, null, null);
      request.Validate();
      try
      {
        var result =
          RequestExecutorContainer.Build<SummaryTemperatureExecutor>(logger, raptorClient, null)
            .Process(request) as TemperatureSummaryResult;
        var returnResult = CompactionTemperatureSummaryResult.CreateTemperatureSummaryResult(result);
        log.LogInformation("GetTemperatureSummary result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        this.ProcessStatusCode(se);
        throw;
      }
      finally
      {
        log.LogInformation("GetTemperatureSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get Speed summary from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
    /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
    /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
    /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
    /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
    ///  to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
    /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.</param>
    /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
    /// All three parameters must be specified to specify a machine. 
    /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
    /// <param name="machineName">See assetID</param>
    /// <param name="isJohnDoe">See assetIDL</param>
    /// <returns>Speed summary</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/speed/summary")]
    [HttpGet]
    public async Task<CompactionSpeedSummaryResult> GetSpeedSummary(
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] bool? vibeStateOn,
      [FromQuery] ElevationType? elevationType,
      [FromQuery] int? layerNumber,
      [FromQuery] long? onMachineDesignId,
      [FromQuery] long? assetID,
      [FromQuery] string machineName,
      [FromQuery] bool? isJohnDoe)
    {
      log.LogInformation("GetSpeedSummary: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      }
      var headers = Request.Headers.GetCustomHeaders();
      var projectSettings = await this.GetProjectSettings(projectSettingsProxy, projectUid.Value, headers, log);
      //Speed settings are in LiftBuildSettings
      LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings(projectSettings);
      var excludedIds = await this.GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid.Value, headers);
      Filter filter = CompactionSettings.CompactionFilter(
        startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
        this.GetMachines(assetID, machineName, isJohnDoe), excludedIds);

      SummarySpeedRequest request =
        SummarySpeedRequest.CreateSummarySpeedRequestt(projectId.Value, null, liftSettings, filter, -1);
      request.Validate();
      try
      {
        var result = RequestExecutorContainer.Build<SummarySpeedExecutor>(logger, raptorClient, null)
          .Process(request) as SummarySpeedResult;
        var returnResult =
          CompactionSpeedSummaryResult.CreateSpeedSummaryResult(result, liftSettings.machineSpeedTarget);
        log.LogInformation("GetSpeedSummary result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        this.ProcessStatusCode(se);
        throw;
      }
      finally
      {
        log.LogInformation("GetSpeedSummary returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get CMV % change from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
    /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
    /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
    /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
    /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
    ///  to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
    /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.</param>
    /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
    /// All three parameters must be specified to specify a machine. 
    /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
    /// <param name="machineName">See assetID</param>
    /// <param name="isJohnDoe">See assetIDL</param>
    /// <returns>CMV % change</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/cmv/percentchange")]
    [HttpGet]
    public async Task<CompactionCmvPercentChangeResult> GetCmvPercentChange(
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] bool? vibeStateOn,
      [FromQuery] ElevationType? elevationType,
      [FromQuery] int? layerNumber,
      [FromQuery] long? onMachineDesignId,
      [FromQuery] long? assetID,
      [FromQuery] string machineName,
      [FromQuery] bool? isJohnDoe)
    {
      log.LogInformation("GetCmvPercentChange: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      }
      var headers = Request.Headers.GetCustomHeaders();
      var projectSettings = await this.GetProjectSettings(projectSettingsProxy, projectUid.Value, headers, log);
      LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings(projectSettings);
      var excludedIds = await this.GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid.Value, headers);
      Filter filter = CompactionSettings.CompactionFilter(
        startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
        this.GetMachines(assetID, machineName, isJohnDoe), excludedIds);
      double[] cmvChangeSummarySettings = CompactionSettings.CompactionCmvPercentChangeSettings;
      CMVChangeSummaryRequest request = CMVChangeSummaryRequest.CreateCMVChangeSummaryRequest(
        projectId.Value, null, liftSettings, filter, -1, cmvChangeSummarySettings);
      request.Validate();
      try
      {
        var result = RequestExecutorContainer.Build<CMVChangeSummaryExecutor>(logger, raptorClient, null)
          .Process(request) as CMVChangeSummaryResult;
        var returnResult = CompactionCmvPercentChangeResult.CreateCmvPercentChangeResult(result);
        log.LogInformation("GetCmvPercentChange result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        this.ProcessStatusCode(se);
        throw;
      }
      finally
      {
        log.LogInformation("GetCmvPercentChange returned: " + Response.StatusCode);
      }
    }

    #endregion

    #region Detailed Data for the map

    /// <summary>
    /// Get CMV details from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
    /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
    /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
    /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
    /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
    ///  to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
    /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.</param>
    /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
    /// All three parameters must be specified to specify a machine. 
    /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
    /// <param name="machineName">See assetID</param>
    /// <param name="isJohnDoe">See assetIDL</param>
    /// <returns>CMV details</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/cmv/details")]
    [HttpGet]
    public async Task<CompactionCmvDetailedResult> GetCmvDetails(
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] bool? vibeStateOn,
      [FromQuery] ElevationType? elevationType,
      [FromQuery] int? layerNumber,
      [FromQuery] long? onMachineDesignId,
      [FromQuery] long? assetID,
      [FromQuery] string machineName,
      [FromQuery] bool? isJohnDoe)
    {
      /**************************************************************************************************
       * NOTE: This end point for CMV details is currently not called from the Compaction UI.
       * It still uses the old Raptor CMV settings with CMV min, max and target to calculate the percents
       * data to return. However, the palette now uses 16 colors and values (the last being 'above' color
       * and value) and this code needs to be updated to be consistent. This requires a change to Raptor
       * to accept a list of CMV values like for pass count details.
       **************************************************************************************************/
        
      log.LogInformation("GetCmvDetails: " + Request.QueryString);

      CMVRequest request = await GetCMVRequest(projectId, projectUid, startUtc, endUtc, vibeStateOn, elevationType,
        layerNumber, onMachineDesignId, assetID, machineName, isJohnDoe);
      request.Validate();

      try
      {
        var result = RequestExecutorContainer.Build<DetailedCMVExecutor>(logger, raptorClient, null)
          .Process(request) as CMVDetailedResult;
        var returnResult = CompactionCmvDetailedResult.CreateCmvDetailedResult(result);

        log.LogInformation("GetCmvDetails result: " + JsonConvert.SerializeObject(returnResult));

        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        this.ProcessStatusCode(se);
        throw;
      }
      finally
      {
        log.LogInformation("GetCmvDetails returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Get pass count details from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
    /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
    /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
    /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
    /// <param name="layerNumber"> The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
    ///  to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
    /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.</param>
    /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
    /// All three parameters must be specified to specify a machine. 
    /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
    /// <param name="machineName">See assetID</param>
    /// <param name="isJohnDoe">See assetIDL</param>
    /// <returns>Pass count details</returns>
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/compaction/passcounts/details")]
    [HttpGet]
    public async Task<CompactionPassCountDetailedResult> GetPassCountDetails(
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] bool? vibeStateOn,
      [FromQuery] ElevationType? elevationType,
      [FromQuery] int? layerNumber,
      [FromQuery] long? onMachineDesignId,
      [FromQuery] long? assetID,
      [FromQuery] string machineName,
      [FromQuery] bool? isJohnDoe)
    {
      log.LogInformation("GetPassCountDetails: " + Request.QueryString);

      PassCounts request = await GetPassCountRequest(projectId, projectUid, startUtc, endUtc, vibeStateOn,
        elevationType, layerNumber, onMachineDesignId, assetID, machineName, isJohnDoe, false);
      request.Validate();

      try
      {
        var result = RequestExecutorContainer.Build<DetailedPassCountExecutor>(logger, raptorClient, null)
          .Process(request) as PassCountDetailedResult;
        var returnResult = CompactionPassCountDetailedResult.CreatePassCountDetailedResult(result);
        log.LogInformation("GetPassCountDetails result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        this.ProcessStatusCode(se);
        throw;
      }
      finally
      {
        log.LogInformation("GetPassCountDetails returned: " + Response.StatusCode);
      }
    }

    #endregion

    #region privates
    /// <summary>
    /// Creates an instance of the CMVRequest class and populate it with data.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectUid"></param>
    /// <param name="startUtc"></param>
    /// <param name="endUtc"></param>
    /// <param name="vibeStateOn"></param>
    /// <param name="elevationType"></param>
    /// <param name="layerNumber"></param>
    /// <param name="onMachineDesignId"></param>
    /// <param name="assetID"></param>
    /// <param name="machineName"></param>
    /// <param name="isJohnDoe"></param>
    /// <returns>An instance of the CMVRequest class.</returns>
    private async Task<CMVRequest> GetCMVRequest(long? projectId, Guid? projectUid, DateTime? startUtc, DateTime? endUtc,
      bool? vibeStateOn, ElevationType? elevationType, int? layerNumber, long? onMachineDesignId, long? assetID,
      string machineName, bool? isJohnDoe)
    {
      if (!projectId.HasValue)
      {
        projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      }
      var headers = Request.Headers.GetCustomHeaders();
      var projectSettings = await this.GetProjectSettings(projectSettingsProxy, projectUid.Value, headers, log);
      CMVSettings cmvSettings = CompactionSettings.CompactionCmvSettings(projectSettings);
      LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings(projectSettings);
      var excludedIds = await this.GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid.Value, headers);
      Filter filter = CompactionSettings.CompactionFilter(
        startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
        this.GetMachines(assetID, machineName, isJohnDoe), excludedIds);

      return CMVRequest.CreateCMVRequest(projectId.Value, null, cmvSettings, liftSettings, filter, -1, null, null,
        null);
    }

    /// <summary>
    /// Creates an instance of the PassCounts class and populate it with data.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectUid"></param>
    /// <param name="startUtc"></param>
    /// <param name="endUtc"></param>
    /// <param name="vibeStateOn"></param>
    /// <param name="elevationType"></param>
    /// <param name="layerNumber"></param>
    /// <param name="onMachineDesignId"></param>
    /// <param name="assetID"></param>
    /// <param name="machineName"></param>
    /// <param name="isJohnDoe"></param>
    /// <param name="isSummary"></param>
    /// <returns>An instance of the PassCounts class.</returns>
    private async Task<PassCounts> GetPassCountRequest(long? projectId, Guid? projectUid, DateTime? startUtc, DateTime? endUtc,
      bool? vibeStateOn, ElevationType? elevationType, int? layerNumber, long? onMachineDesignId, long? assetID,
      string machineName, bool? isJohnDoe, bool isSummary)
    {
      if (!projectId.HasValue)
      {
        projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      }
      var headers = Request.Headers.GetCustomHeaders();
      var projectSettings = await this.GetProjectSettings(projectSettingsProxy, projectUid.Value, headers, log);
      PassCountSettings passCountSettings = isSummary ? null : CompactionSettings.CompactionPassCountSettings;
      LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings(projectSettings);
      var excludedIds = await this.GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid.Value, headers);
      Filter filter = CompactionSettings.CompactionFilter(
        startUtc, endUtc, onMachineDesignId, vibeStateOn, elevationType, layerNumber,
        this.GetMachines(assetID, machineName, isJohnDoe), excludedIds);

      return PassCounts.CreatePassCountsRequest(projectId.Value, null, passCountSettings, liftSettings, filter,
        -1, null, null, null);
    }

    #endregion

  }
}
