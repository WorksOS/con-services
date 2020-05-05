using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Interfaces;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting elevation data from Raptor
  /// </summary>
  //Turn off caching until settings caching problem resolved
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionElevationController : BaseController<CompactionElevationController>
  {
    /// <summary>
    /// Proxy for getting elevation statistics from Raptor
    /// </summary>
    private readonly IElevationExtentsProxy elevProxy;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionElevationController(
      IConfigurationStore configStore, IElevationExtentsProxy elevProxy, IFileImportProxy fileImportProxy,
      ICompactionSettingsManager settingsManager, IProductionDataRequestFactory productionDataRequestFactory) :
      base(configStore, fileImportProxy, settingsManager)
    {
      this.elevProxy = elevProxy;
      requestFactory = productionDataRequestFactory;
    }

    /// <summary>
    /// Get elevation range from Raptor for the specified project and date range. Either legacy project ID or project UID must be provided.
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/elevationrange")]
    [HttpGet]
    public async Task<ElevationStatisticsResult> GetElevationRange(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid)
    {
      Log.LogInformation($"{nameof(GetElevationRange)}: {Request.QueryString}");

      try
      {
        var projectSettingsTask = GetProjectSettingsTargets(projectUid);

        var filterTask = GetCompactionFilter(projectUid, filterUid);
        var projectIdTask = GetLegacyProjectId(projectUid);

        await Task.WhenAll(projectSettingsTask, filterTask, projectIdTask);

        var result = await elevProxy.GetElevationRange(projectIdTask.Result, projectUid, filterTask.Result, projectSettingsTask.Result, CustomHeaders);

        if (result == null)
        {
          //Ideally want to return an error code and message only here
          result = new ElevationStatisticsResult(null, 0, 0, 0);
        }

        Log.LogInformation($"{nameof(GetElevationRange)} result: {JsonConvert.SerializeObject(result)}");
        return result;
      }
      catch (ServiceException se)
      {
        Log.LogError(se, $"{nameof(GetElevationRange)}: exception");
        throw;
      }
      finally
      {
        Log.LogInformation($"{nameof(GetElevationRange)} returned: {Response.StatusCode}");
      }
    }

    /// <summary>
    /// Gets alignment file extents (station range) from Raptor.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="alignmentFileUid">Alignment file UID</param>
    /// <param name="boundingBoxService">Project UID</param>
    /// <returns>Station range for alignment file</returns>
    [ProjectVerifier]
    [Route("api/v2/alignmentstationrange")]
    [HttpGet]
    public async Task<AlignmentStationRangeResult> GetAlignmentStationRange(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid alignmentFileUid,
      [FromServices] IBoundingBoxService boundingBoxService)
    {
      Log.LogInformation("GetAlignmentStationRange: " + Request.QueryString);

      var projectId = GetLegacyProjectId(projectUid);
      var alignmentDescriptor = GetAndValidateDesignDescriptor(projectUid, alignmentFileUid);

      await Task.WhenAll(projectId, alignmentDescriptor);

      var request = requestFactory.Create<AlignmentStationRangeRequestHelper>(r => r
          .ProjectId(projectId.Result)
          .Headers(CustomHeaders))
        .CreateAlignmentStationRangeRequest(alignmentDescriptor.Result);

      request.Validate();

      var result = await WithServiceExceptionTryExecuteAsync(() =>
        boundingBoxService.GetAlignmentStationRange(
          new ProjectData { ProjectUID = projectUid.ToString(), ShortRaptorProjectId = (int)projectId.Result },
          alignmentDescriptor.Result,
          CustomHeaders));

      return result;
    }

    /// <summary>
    /// Gets project statistics from Raptor.
    /// </summary>
    /// <returns>Project statistics</returns>
    [ProjectVerifier]
    [Route("api/v2/projectstatistics")]
    [HttpGet]
    public async Task<ProjectStatisticsResult> GetProjectStatistics(
      [FromQuery] Guid projectUid)
    {
      Log.LogInformation($"{nameof(GetProjectStatistics)}:  {Request.QueryString}");

      try
      {
        var projectId = await GetLegacyProjectId(projectUid);
        var result = await ProjectStatisticsHelper.GetProjectStatisticsWithProjectSsExclusions(projectUid, projectId, GetUserId(), CustomHeaders);

        Log.LogInformation($"{nameof(GetProjectStatistics)}: result: {JsonConvert.SerializeObject(result)}");
        return result;
      }
      catch (ServiceException se)
      {
        Log.LogError(se, $"{nameof(GetProjectStatistics)}: exception");
        throw;
      }
      finally
      {
        Log.LogInformation($"{nameof(GetProjectStatistics)}: returned: {Response.StatusCode}");
      }
    }

    /// <summary>
    /// Gets production data extents from Raptor in lat/lng degrees.
    /// </summary>
    /// <returns>Production data extents</returns>
    [ProjectVerifier]
    [Route("api/v2/productiondataextents")]
    [HttpGet]
    public async Task<ProjectExtentsResult> GetProjectExtentsV2(
      [FromQuery] Guid projectUid,
      [FromServices] IBoundingBoxService boundingBoxService)
    {
      Log.LogInformation("GetProjectExtents V2: " + Request.QueryString);
      var projectId = await GetLegacyProjectId(projectUid);
      return await GetProjectExtentsV1(projectId, boundingBoxService);
    }

    /// <summary>
    /// Gets production data extents from Raptor in lat/lng degrees.
    /// </summary>
    /// <returns>Production data extents</returns>
    [ProjectVerifier]
    [Route("api/v1/productiondataextents")]
    [HttpGet]
    public async Task<ProjectExtentsResult> GetProjectExtentsV1(
      [FromQuery] long projectId,
      [FromServices] IBoundingBoxService boundingBoxService)
    {
      Log.LogInformation($"{nameof(GetProjectExtentsV1)}: {Request.QueryString}");

      var projectUid = await ((RaptorPrincipal)User).GetProjectUid(projectId);
      var excludedIds = await ProjectStatisticsHelper.GetExcludedSurveyedSurfaceIds(projectUid, GetUserId(), CustomHeaders);

      try
      {
        var result = await boundingBoxService.GetProductionDataExtents(projectUid, projectId, excludedIds?.Select(e => e.Item1), excludedIds?.Select(e => e.Item2), GetUserId(), CustomHeaders);
        return await FormatProjectExtentsResult(projectUid, result);
      }
      catch (ServiceException se)
      {
        Log.LogError(se, $"{nameof(GetProjectExtentsV1)}: exception");
        throw;
      }
      finally
      {
        Log.LogInformation($"{nameof(GetProjectExtentsV1)} returned: {Response.StatusCode}");
      }
    }

    private async Task<ProjectExtentsResult> FormatProjectExtentsResult(
      Guid projectUid, CoordinateConversionResult coordinateConversionResult)
    {
      Log.LogInformation($"{nameof(FormatProjectExtentsResult)}: {coordinateConversionResult}");
      var project = await ((RaptorPrincipal)User).GetProject(projectUid);

      var returnResult = new ProjectExtentsResult
      {
        minLat = coordinateConversionResult.ConversionCoordinates[0].Y,
        minLng = coordinateConversionResult.ConversionCoordinates[0].X,
        maxLat = coordinateConversionResult.ConversionCoordinates[1].Y,
        maxLng = coordinateConversionResult.ConversionCoordinates[1].X
      };

      //In case we have rogue tag files distorting the extents, restrict to project boundary
      var projectPoints = CommonConverters.GeometryToPoints(project.IanaTimeZone).ToList();
      var projMinLat = projectPoints.Min(p => p.Lat);
      var projMinLng = projectPoints.Min(p => p.Lon);
      var projMaxLat = projectPoints.Max(p => p.Lat);
      var projMaxLng = projectPoints.Max(p => p.Lon);

      if (returnResult.minLat < projMinLat || returnResult.minLat > projMaxLat ||
          returnResult.maxLat < projMinLat || returnResult.maxLat > projMaxLat ||
          returnResult.minLng < projMinLng || returnResult.minLng > projMaxLng ||
          returnResult.maxLng < projMinLng || returnResult.maxLng > projMaxLng)
      {
        returnResult.minLat = projMinLat;
        returnResult.minLng = projMinLng;
        returnResult.maxLat = projMaxLat;
        returnResult.maxLng = projMaxLng;
      }

      //Convert to degrees to return
      returnResult.minLat = returnResult.minLat.LatRadiansToDegrees();
      returnResult.minLng = returnResult.minLng.LonRadiansToDegrees();
      returnResult.maxLat = returnResult.maxLat.LatRadiansToDegrees();
      returnResult.maxLng = returnResult.maxLng.LonRadiansToDegrees();

      Log.LogInformation("GetProjectExtents result: " + JsonConvert.SerializeObject(returnResult));
      return returnResult;
    }
  }
}
