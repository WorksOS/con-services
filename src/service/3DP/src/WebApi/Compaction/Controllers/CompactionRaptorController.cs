using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;
using VSS.Productivity3D.WebApi.Models.MapHandling;
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
    public CompactionRaptorController(IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager) :
      base(configStore, fileListProxy, settingsManager)
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
      var project = await((RaptorPrincipal)User).GetProject(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      DesignDescriptor cutFillDesign = cutFillDesignUid.HasValue ? await GetAndValidateDesignDescriptor(projectUid, cutFillDesignUid.Value) : null;
      var sumVolParameters = await GetSummaryVolumesParameters(projectUid, volumeCalcType, baseUid, topUid);
      var designDescriptor = (!volumeCalcType.HasValue || volumeCalcType.Value == VolumeCalcType.None)
        ? cutFillDesign
        : sumVolParameters.Item3;
      var overlayTypes = overlays.ToList();
      if (overlays.Contains(TileOverlayType.AllOverlays))
      {
        overlayTypes = new List<TileOverlayType>((TileOverlayType[])Enum.GetValues(typeof(TileOverlayType)));
        overlayTypes.Remove(TileOverlayType.AllOverlays);
      }
      var result = boundingBoxService.GetBoundingBox(project, filter, overlayTypes.ToArray(), sumVolParameters.Item1,
        sumVolParameters.Item2, designDescriptor);
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

      var projectId = await GetLegacyProjectId(projectUid);
      DesignDescriptor designDescriptor = await GetAndValidateDesignDescriptor(projectUid, designUid);

      PointsListResult result = new PointsListResult();
      var polygons = boundingBoxService.GetDesignBoundaryPolygons(
        projectId, designDescriptor);
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

      var project = await ((RaptorPrincipal)User).GetProject(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);

      PointsListResult result = new PointsListResult();
      if (filter != null)
      {
        var polygons = boundingBoxService.GetFilterBoundaries(
          project, filter, FilterBoundaryType.All);
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
      Log.LogInformation("GetFilterPointsList: " + Request.QueryString);
       
      var project = await ((RaptorPrincipal)User).GetProject(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      //Base or top may be a design UID
      var baseFilter = await summaryDataHelper.WithSwallowExceptionExecute(async () => await GetCompactionFilter(projectUid, baseUid));
      var topFilter = await summaryDataHelper.WithSwallowExceptionExecute(async () => await GetCompactionFilter(projectUid, topUid));

      PointsListResult result = new PointsListResult();
 
      var polygons = boundingBoxService.GetFilterBoundaries(
        project, filter, baseFilter, topFilter, boundaryType);
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
      Log.LogInformation("GetAlignmentPoints: " + Request.QueryString);

      var projectId = await GetLegacyProjectId(projectUid);

      var alignmentDescriptor = await GetAndValidateDesignDescriptor(projectUid, alignmentUid);
      AlignmentPointsResult result = new AlignmentPointsResult();
      var alignmentPoints = boundingBoxService.GetAlignmentPoints(projectId, alignmentDescriptor);

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
      Log.LogInformation("GetAlignmentPointsList: " + Request.QueryString);

      var projectId = await GetLegacyProjectId(projectUid);

      PointsListResult result = new PointsListResult();
      List<List<WGSPoint>> list = new List<List<WGSPoint>>();
      var alignmentDescriptors = await GetAlignmentDescriptors(projectUid);
      foreach (var alignmentDescriptor in alignmentDescriptors)
      {
        var alignmentPoints = boundingBoxService.GetAlignmentPoints(projectId, alignmentDescriptor);

        if (alignmentPoints != null && alignmentPoints.Any())
        {
          list.Add(alignmentPoints.ToList());      
        }
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
      List<DesignDescriptor> alignmentDescriptors = new List<DesignDescriptor>();
      if (alignmentFiles != null)
      {
        foreach (var alignmentFile in alignmentFiles)
        {
          alignmentDescriptors.Add(await GetAndValidateDesignDescriptor(projectUid, new Guid(alignmentFile.ImportedFileUid)));
        }
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
      var fileList = await FileListProxy.GetFiles(projectUid.ToString(), GetUserId(), CustomHeaders);
      if (fileList == null || fileList.Count == 0)
      {
        return new List<FileData>();
      }

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
          {
            result = new List<List<WGSPoint>>();
          }

          //TODO: Fix this when WGSPoint & WGSPoint3D aligned
          result.Add(polygon.Select(x => new WGSPoint(x.Lat, x.Lon)).ToList());
        }
      }

      return result;
    }
  }
}
