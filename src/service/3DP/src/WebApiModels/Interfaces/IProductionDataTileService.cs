using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Compaction;

namespace VSS.Productivity3D.WebApi.Models.Interfaces
{
  public interface IProductionDataTileService
  {
    Task<TileResult> GetProductionDataTile(CompactionProjectSettings projectSettings, CompactionProjectSettingsColors projectSettingsColors, FilterResult filter, long projectId, Guid projectUid, DisplayMode mode, ushort width, ushort height, BoundingBox2DLatLon bbox, DesignDescriptor cutFillDesign, FilterResult baseFilter, FilterResult topFilter, DesignDescriptor volumeDesign, VolumeCalcType? volumeCalcType, IDictionary<string, string> customHeaders, bool explicitFilters);
  }
}
