using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.Productivity3D.Abstractions.Interfaces
{
  public interface IProductivity3dV2ProxyCompactionTile : IProductivity3dV2Proxy
  {
    Task<byte[]> GetProductionDataTile(Guid projectUid, Guid? filterUid, Guid? cutFillDesignUid, ushort width,
      ushort height, string bbox, DisplayMode mode, Guid? baseUid, Guid? topUid, VolumeCalcType? volCalcType,
      IHeaderDictionary customHeaders = null, bool explicitFilters = false);

    Task<byte[]> GetLineworkTile(Guid projectUid, ushort width, ushort height,
      string bbox, string filetype, IHeaderDictionary customHeaders = null);

    Task<PointsListResult> GetAlignmentPointsList(Guid projectUid, IHeaderDictionary customHeaders = null);

    Task<PointsListResult> GetDesignBoundaryPoints(Guid projectUid, Guid designUid,
      IHeaderDictionary customHeaders = null);

    Task<PointsListResult> GetFilterPointsList(Guid projectUid, Guid? filterUid, Guid? baseUid, Guid? topUid, FilterBoundaryType boundaryType,
      IHeaderDictionary customHeaders = null);

    Task<string> GetBoundingBox(Guid projectUid, TileOverlayType[] overlays, Guid? filterUid, Guid? cutFillDesignUid,
      Guid? baseUid, Guid? topUid, VolumeCalcType? volCalcType, IHeaderDictionary customHeaders = null);
  }
}
