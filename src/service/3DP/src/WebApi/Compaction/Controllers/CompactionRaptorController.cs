using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Models.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// End points for getting Raptor data e.g. for tile service to get alignment points
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionRaptorController : BaseTileController<CompactionRaptorController>
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionRaptorController(IConfigurationStore configStore, IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager) :
      base(configStore, fileImportProxy, settingsManager)
    { }

    /// <summary>
    /// Gets a "best fit" bounding box for the requested project and given query parameters.
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/raptor/boundingbox")]
    [HttpGet]
    public async Task<string> GetBoundingBox(
      [FromQuery] Guid projectUid,
      [FromQuery] TileOverlayType[] overlays,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? cutFillDesignUid,
      [FromQuery] Guid? baseUid,
      [FromQuery] Guid? topUid,
      [FromQuery] VolumeCalcType? volumeCalcType,
      [FromServices] IBoundingBoxService boundingBoxService)
    {
      Log.LogInformation("GetBoundingBox: " + Request.QueryString);
      //Check we have at least one overlay
      if (overlays == null || overlays.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "At least one overlay type must be specified to calculate bounding box"));
      }
      var project = ((RaptorPrincipal)User).GetProject(projectUid);
      var filter = GetCompactionFilter(projectUid, filterUid);
      var cutFillDesign = GetAndValidateDesignDescriptor(projectUid, cutFillDesignUid);
      var sumVolParametersTask = GetSummaryVolumesParameters(projectUid, volumeCalcType, baseUid, topUid);

      var sumVolParameters = sumVolParametersTask.Result;

      await Task.WhenAll(project, filter, cutFillDesign, sumVolParametersTask);

      var designDescriptor = (!volumeCalcType.HasValue || volumeCalcType.Value == VolumeCalcType.None)
        ? cutFillDesign.Result
        : sumVolParameters.Item3;

      var overlayTypes = overlays.ToList();
      if (overlays.Contains(TileOverlayType.AllOverlays))
      {
        overlayTypes = new List<TileOverlayType>((TileOverlayType[])Enum.GetValues(typeof(TileOverlayType)));
        overlayTypes.Remove(TileOverlayType.AllOverlays);
      }

      var result = await boundingBoxService.GetBoundingBox(project.Result, filter.Result, overlayTypes.ToArray(), sumVolParameters.Item1,
        sumVolParameters.Item2, designDescriptor, GetUserId(), CustomHeaders);
      var bbox = $"{result.minLatDegrees},{result.minLngDegrees},{result.maxLatDegrees},{result.maxLngDegrees}";
      Log.LogInformation($"GetBoundingBox: returning {bbox}");
      return bbox;
    }

    /// <summary>
    /// Gets the boundary points of a design
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/raptor/designboundarypoints")]
    [HttpGet]
    public async Task<PointsListResult> GetDesignBoundaryPoints(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid designUid,
      [FromServices] IBoundingBoxService boundingBoxService)
    {
      Log.LogInformation("GetDesignBoundaryPoints: " + Request.QueryString);

      var projectId = GetLegacyProjectId(projectUid);
      var designDescriptor = GetAndValidateDesignDescriptor(projectUid, designUid);

      await Task.WhenAll(projectId, designDescriptor);

      var project = new ProjectData { ProjectUid = projectUid.ToString(), LegacyProjectId = (int)projectId.Result};
      var result = new PointsListResult();
      var polygons = await boundingBoxService.GetDesignBoundaryPolygons(project, designDescriptor.Result, CustomHeaders);
      result.PointsList = ConvertPoints(polygons);

      return result;
    }

    /// <summary>
    /// Gets the spatial boundary points of a filter
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/raptor/filterpoints")]
    [HttpGet]
    public async Task<PointsListResult> GetFilterPoints(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid filterUid,
      [FromServices] IBoundingBoxService boundingBoxService)
    {
      Log.LogInformation("GetFilterPoints: " + Request.QueryString);

      var project = ((RaptorPrincipal)User).GetProject(projectUid);
      var filter = GetCompactionFilter(projectUid, filterUid);

      await Task.WhenAll(project, filter);

      var result = new PointsListResult();
      if (filter.Result != null)
      {
        var polygons = await boundingBoxService.GetFilterBoundaries(
          project.Result, filter.Result, FilterBoundaryType.All, CustomHeaders);
        result.PointsList = ConvertPoints(polygons);
      }

      return result;
    }

    /// <summary>
    /// Gets the spatial boundary points of the requested filters for the required boundary type
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/raptor/filterpointslist")]
    [HttpGet]
    public async Task<PointsListResult> GetFilterPointsList(
      [FromServices] ISummaryDataHelper summaryDataHelper,
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? baseUid,
      [FromQuery] Guid? topUid,
      [FromQuery] FilterBoundaryType boundaryType,
      [FromServices] IBoundingBoxService boundingBoxService)
    {
      Log.LogInformation($"{nameof(GetFilterPointsList)}: " + Request.QueryString);

      var projectTask = ((RaptorPrincipal)User).GetProject(projectUid);
      var filterTask = GetCompactionFilter(projectUid, filterUid);
      //Base or top may be a design UID
      var baseFilterTask = summaryDataHelper.WithSwallowExceptionExecute(() => GetCompactionFilter(projectUid, baseUid));
      var topFilterTask = summaryDataHelper.WithSwallowExceptionExecute(() => GetCompactionFilter(projectUid, topUid));

      var result = new PointsListResult();

      await Task.WhenAll(projectTask, filterTask, baseFilterTask, topFilterTask);

      var polygons = await boundingBoxService.GetFilterBoundaries(projectTask.Result, filterTask.Result, baseFilterTask.Result, topFilterTask.Result, boundaryType, CustomHeaders);

      result.PointsList = ConvertPoints(polygons);

      return result;
    }

    /// <summary>
    /// Gets the boundary points for an alignment file in a project
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/raptor/alignmentpoints")]
    [HttpGet]
    public async Task<AlignmentPointsResult> GetAlignmentPoints(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid alignmentUid,
      [FromServices] IBoundingBoxService boundingBoxService)
    {
      Log.LogInformation($"{nameof(GetAlignmentPoints)}: " + Request.QueryString);

      var projectId = GetLegacyProjectId(projectUid);
      var alignmentDescriptor = GetAndValidateDesignDescriptor(projectUid, alignmentUid);

      await Task.WhenAll(projectId, alignmentDescriptor);

      var result = new AlignmentPointsResult();
      var alignmentPoints = boundingBoxService.GetAlignmentPoints(
        new ProjectData { ProjectUid = projectUid.ToString(), LegacyProjectId = (int)projectId.Result }, 
        alignmentDescriptor.Result);

      if (alignmentPoints != null && alignmentPoints.Any())
      {
        //TODO: Fix this when WGSPoint & WGSPoint3D aligned
        result.AlignmentPoints = alignmentPoints.Select(x => new WGSPoint(x.Lat, x.Lon)).ToList();
      }

      return result;
    }

    /// <summary>
    /// Gets the boundary points for all active alignment files in a project
    /// </summary>
    [ProjectVerifier]
    [Route("api/v2/raptor/alignmentpointslist")]
    [HttpGet]
    public async Task<PointsListResult> GetAlignmentPointsList(
      [FromQuery] Guid projectUid,
      [FromServices] IBoundingBoxService boundingBoxService)
    {
      Log.LogInformation($"{nameof(GetAlignmentPointsList)}: " + Request.QueryString);

      var projectId = GetLegacyProjectId(projectUid);
      var alignmentDescriptors = GetAlignmentDescriptors(projectUid);

      await Task.WhenAll(projectId, alignmentDescriptors);

      var result = new PointsListResult();
      var list = new List<List<WGSPoint>>();
      
      foreach (var alignmentDescriptor in alignmentDescriptors.Result)
      {
        var alignmentPoints = boundingBoxService.GetAlignmentPoints(
          new ProjectData { ProjectUid = projectUid.ToString(), LegacyProjectId = (int)projectId.Result },
          alignmentDescriptor,
          0, 0, 0, 0,
          CustomHeaders);

        if (alignmentPoints != null && alignmentPoints.Any())
          list.Add(alignmentPoints.ToList());
      }

      result.PointsList = ConvertPoints(list);

      return result;
    }


    /// <summary>
    /// Gets alignments descriptors of all active alignment files for the project
    /// </summary>
    /// <param name="projectUid">The project UID</param>
    /// <returns>A list of alignment design descriptors</returns>
    private async Task<List<DesignDescriptor>> GetAlignmentDescriptors(Guid projectUid)
    {
      var alignmentFiles = await GetFilesOfType(projectUid, ImportedFileType.Alignment);
      var alignmentDescriptors = new List<DesignDescriptor>();
      if (alignmentFiles != null)
      {
        foreach (var alignmentFile in alignmentFiles)
          alignmentDescriptors.Add(await GetAndValidateDesignDescriptor(projectUid, new Guid(alignmentFile.ImportedFileUid)));
      }
      return alignmentDescriptors;
    }

    /// <summary>
    /// Gets the imported files of the specified type in a project
    /// </summary>
    /// <param name="projectUid">The project UID</param>
    /// <param name="fileType">The type of files to retrieve</param>
    /// <returns>List of active imported files of specified type</returns>
    private async Task<List<FileData>> GetFilesOfType(Guid projectUid, ImportedFileType fileType)
    {
      var fileList = await FileImportProxy.GetFiles(projectUid.ToString(), GetUserId(), CustomHeaders);
      if (fileList == null || fileList.Count == 0)
        return new List<FileData>();

      return fileList.Where(f => f.ImportedFileType == fileType && f.IsActivated).ToList();
    }

    /// <summary>
    /// Temporary method to convert between WGSPoint3D and WGSPoint
    /// </summary>
    private static List<List<WGSPoint>> ConvertPoints(List<List<WGSPoint>> polygons)
    {
      List<List<WGSPoint>> result = null;
      if (polygons != null && polygons.Any())
      {
        foreach (var polygon in polygons)
        {
          if (result == null)
            result = new List<List<WGSPoint>>();

          //TODO: Fix this when WGSPoint & WGSPoint3D aligned
          result.Add(polygon.Select(x => new WGSPoint(x.Lat, x.Lon)).ToList());
        }
      }

      return result;
    }
  }
}
