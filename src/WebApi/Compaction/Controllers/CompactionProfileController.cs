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
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Factories.ProductionData;
using VSS.Productivity3D.WebApi.ProductionData.Controllers;
using VSS.Productivity3D.WebApiModels.ProductionData.Executors;
using VSS.Productivity3D.WebApiModels.ProductionData.Helpers;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class CompactionProfileController : Controller
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
    /// For getting list of imported files for a project
    /// </summary>
    private readonly IFileListProxy fileListProxy;

    /// <summary>
    /// For getting project settings for a project
    /// </summary>
    private readonly IProjectSettingsProxy projectSettingsProxy;

    /// <summary>
    /// 
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="raptorClient"></param>
    /// <param name="logger"></param>
    /// <param name="fileListProxy"></param>
    /// <param name="projectSettingsProxy"></param>
    /// <param name="requestFactory"></param>
    public CompactionProfileController(IASNodeClient raptorClient, ILoggerFactory logger,
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy,
      IProductionDataRequestFactory requestFactory)
    {
      this.logger = logger;
      log = logger.CreateLogger<ProfileProductionDataController>();
      this.fileListProxy = fileListProxy;
      this.projectSettingsProxy = projectSettingsProxy;
      this.requestFactory = requestFactory;
    }

    /// <summary>
    /// Posts a profile production data request to a Raptor's data model/project.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startLatDegrees">Start profileLine Lat</param>
    /// <param name="startLonDegrees">Start profileLine Lon</param>
    /// <param name="endLatDegrees">End profileLine Lat</param>
    /// <param name="endLonDegrees">End profileLine Lon</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="cutfillDesignUid">Design UID</param>
    /// <returns>
    /// Returns JSON structure wtih operation result as profile calculations <see cref="Common.Contracts.ContractExecutionResult"/>
    /// </returns>
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/profiles/productiondata/slicer")]
    [HttpGet]
    public async Task<ProfileResult> GetProfileProductionDataSlicer(
      [FromQuery] Guid projectUid,
      [FromQuery] double startLatDegrees,
      [FromQuery] double startLonDegrees,
      [FromQuery] double endLatDegrees,
      [FromQuery] double endLonDegrees,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] Guid? cutfillDesignUid
    )
    {
      log.LogInformation("GetProfileProduction: " + Request.QueryString);

      var projectId = ((RaptorPrincipal)User).GetProjectId(projectUid);
      var headers = Request.Headers.GetCustomHeaders();
      var projectSettings = await this.GetProjectSettings(projectSettingsProxy, projectUid, headers, log);

      var slicerProfileResult = requestFactory.Create<SliceProfileDataRequestHelper>(async r => r
          .ProjectId(projectId)
          .Headers(headers)
          .ProjectSettings(projectSettings)
          .ExcludedIds(await this.GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid, headers)))
        .CreateSlicerProfileResponse(projectUid, startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees, startUtc,
          endUtc, cutfillDesignUid);

      slicerProfileResult.Validate();

      try
      {
        var result = RequestExecutorContainer.Build<ProfileProductionDataExecutor>(logger, raptorClient)
          .Process(slicerProfileResult) as ProfileResult;
        log.LogInformation("GetProfileProduction result: " + JsonConvert.SerializeObject(result));
        return result;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        this.ProcessStatusCode(se);
        throw;
      }
      finally
      {
        log.LogInformation("GetProfileProduction returned: " + Response.StatusCode);
      }
    }

    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/profiles/productiondata/design")]
    [HttpGet]
    public async Task<ProfileResult> GetProfileProductionDataDesign(
      [FromQuery] Guid projectUid,
      [FromQuery] double startLatDegrees,
      [FromQuery] double startLonDegrees,
      [FromQuery] double endLatDegrees,
      [FromQuery] double endLonDegrees,
      [FromQuery] DateTime startUtc,
      [FromQuery] DateTime endUtc,
      [FromQuery] Guid cutfillDesignUid
    )
    {
      log.LogInformation("GetDesignProduction: " + Request.QueryString);

      throw new NotImplementedException();
    }
  }
}