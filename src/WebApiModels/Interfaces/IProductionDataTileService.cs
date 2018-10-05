using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Interfaces
{
  public interface IProductionDataTileService
  {
    TileResult GetProductionDataTile(CompactionProjectSettings projectSettings, CompactionProjectSettingsColors projectSettingsColors, FilterResult filter, long projectId, Guid projectUid, DisplayMode mode, ushort width, ushort height, BoundingBox2DLatLon bbox, DesignDescriptor cutFillDesign, FilterResult baseFilter, FilterResult topFilter, DesignDescriptor volumeDesign, VolumeCalcType? volumeCalcType, IDictionary<string, string> customHeaders);
  }
}
