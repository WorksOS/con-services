using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.WebApi.Models.Interfaces
{
  public interface IProductionDataTileService
  {
    Task<TileResult> GetProductionDataTile(CompactionProjectSettings projectSettings, CompactionProjectSettingsColors projectSettingsColors, FilterResult filter, long projectId, Guid projectUid, DisplayMode mode, ushort width, ushort height, BoundingBox2DLatLon bbox, DesignDescriptor cutFillDesign, FilterResult baseFilter, FilterResult topFilter, DesignDescriptor volumeDesign, VolumeCalcType? volumeCalcType, IHeaderDictionary customHeaders, bool explicitFilters);
  }
}
