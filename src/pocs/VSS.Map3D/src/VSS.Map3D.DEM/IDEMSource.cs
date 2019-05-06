using System;
using System.Threading.Tasks;
using VSS.Map3D.Models;

namespace VSS.Map3D.DEM
{
  /// <summary>
  /// Interface to DEM source for producing tiles
  /// </summary>
  public interface IDEMSource
  {
    int GridSize { get; set; }
    Double MinHeight { get; set; }
    Double MaxHeight { get; set; }
    void Initalize(MapHeader mapHeader);
    Task<ElevationData> GetDemLL(double lonMin, double latMin, double lonMax, double latMax);
    Task<ElevationData> GetDemXYZ(int x, int y, int z);

    // todo get map image
  }
}
