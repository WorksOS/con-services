using System.Collections.Generic;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  public interface IGeofenceTileService
  {
    byte[] GetSitesBitmap(MapParameters parameters, IEnumerable<GeofenceData> sites);
    byte[] GetBoundariesBitmap(MapParameters parameters, IEnumerable<GeofenceData> customBoundaries);
    byte[] GetFilterBoundaryBitmap(MapParameters parameters, List<List<WGSPoint>> filterPoints, FilterBoundaryType boundaryType);
  }
}
