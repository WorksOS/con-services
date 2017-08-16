﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Controllers;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.Report.Executors;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting elevation data from Raptor
  /// </summary>
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class CompactionElevationController : BaseController
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
    /// Proxy for getting elevation statistics from Raptor
    /// </summary>
    private readonly IElevationExtentsProxy elevProxy;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="configStore">Configuration store</param>
    /// <param name="elevProxy">Elevation extents proxy</param>
    /// <param name="fileListProxy">File list proxy</param>
    /// <param name="projectSettingsProxy">Project settings proxy</param>
    /// <param name="settingsManager">Compaction settings manager</param>
    /// <param name="exceptionHandler">Service exception handler</param>
    /// <param name="filterServiceProxy">Filter service proxy</param>
    public CompactionElevationController(IASNodeClient raptorClient, ILoggerFactory logger, IConfigurationStore configStore, 
      IElevationExtentsProxy elevProxy, IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, 
      ICompactionSettingsManager settingsManager, IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy) : 
      base(logger.CreateLogger<BaseController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<CompactionElevationController>();
      this.elevProxy = elevProxy;
    }


    #region Elevation Range

    /// <summary>
    /// Get elevation range from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
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
    /// <returns>Elevation statistics</returns>
    [ProjectUidVerifier]
    [Route("api/v2/compaction/elevationrange")]
    [HttpGet]
    public async Task<ElevationStatisticsResult> GetElevationRange(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
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
      log.LogInformation("GetElevationRange: " + Request.QueryString);
      var projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      try
      {
        var projectSettings = await GetProjectSettings(projectUid,  log);

        var filter = await GetCompactionFilter(projectUid, filterUid, startUtc, endUtc, vibeStateOn, elevationType, layerNumber, onMachineDesignId, assetID, machineName, isJohnDoe);

        ElevationStatisticsResult result = elevProxy.GetElevationRange(projectId, filter, projectSettings);
        if (result == null)
        {
          //Ideally want to return an error code and message only here
          result = ElevationStatisticsResult.CreateElevationStatisticsResult(null, 0, 0, 0);
        }
        log.LogInformation("GetElevationRange result: " + JsonConvert.SerializeObject(result));
        return result;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        throw new ServiceException(HttpStatusCode.NoContent,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, se.Message));
      }
      finally
      {
        log.LogInformation("GetElevationRange returned: " + Response.StatusCode);
      }
    }

    #endregion
    #region Project Extents

    /// <summary>
    /// Gets project statistics from Raptor.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <returns>Project statistics</returns>
    /// <executor>ProjectStatisticsExecutor</executor>
    [ProjectUidVerifier]
    [Route("api/v2/compaction/projectstatistics")]
    [HttpGet]
    public async Task<ProjectStatisticsResult> GetProjectStatistics(
      [FromQuery] Guid projectUid)
    {
      log.LogInformation("GetProjectStatistics: " + Request.QueryString);
      var projectId = (User as RaptorPrincipal).GetProjectId(projectUid);
      var excludedIds = await this.GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid);
      ProjectStatisticsRequest request = ProjectStatisticsRequest.CreateStatisticsParameters(projectId, excludedIds?.ToArray());
      request.Validate();
      try
      {
        var returnResult =
          RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(logger, raptorClient)
            .Process(request) as ProjectStatisticsResult;
        log.LogInformation("GetProjectStatistics result: " + JsonConvert.SerializeObject(returnResult));
        return returnResult;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        throw new ServiceException(HttpStatusCode.NoContent,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, se.Message));
      }
      finally
      {
        log.LogInformation("GetProjectStatistics returned: " + Response.StatusCode);
      }
    }

    #endregion
  }
}