using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.MapHandling;

namespace VSS.Productivity3D.WebApi.Models.Interfaces
{

  public interface IBoundingBoxService
  {
    MapBoundingBox GetBoundingBox(ProjectData project, FilterResult filter, TileOverlayType[] overlays,
      FilterResult baseFilter, FilterResult topFilter, DesignDescriptor designDescriptor,
      string userId, IDictionary<string, string> customHeaders);

    List<List<WGSPoint>> GetFilterBoundaries(ProjectData project, FilterResult filter,
      FilterResult baseFilter, FilterResult topFilter, FilterBoundaryType boundaryType);

    List<List<WGSPoint>> GetFilterBoundaries(ProjectData project, FilterResult filter, FilterBoundaryType boundaryType);

    IEnumerable<WGSPoint> GetAlignmentPoints(long projectId, DesignDescriptor alignDescriptor,
      double startStation = 0, double endStation = 0, double leftOffset = 0, double rightOffset = 0);

    List<List<WGSPoint>> GetDesignBoundaryPolygons(long projectId, DesignDescriptor designDescriptor);

    AlignmentStationResult GetAlignmentStationRange(long projectId, DesignDescriptor alignDescriptor);

    CoordinateConversionResult GetProductionDataExtents(Guid projectUid, long projectId, List<long> excludedIds, string userId, IDictionary<string, string> customHeaders);
  }
}
