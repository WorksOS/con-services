using System.Threading.Tasks;
using VSS.Map3D.Models;
/*
 * elevations will probably run SW to NE
 */

namespace VSS.Map3D.DEM
{
  public abstract class DEMSourceBase : IDEMSource
  {
    public int GridSize { get; set; }
    public TerrainType DEMType { get; set; }
    public double MinHeight { get; set; }
    public double MaxHeight { get; set; }
    public string DEMLocation { get; set; }
    public abstract void Initalize(MapHeader mapHeader);
    public abstract Task<ElevationData> GetDemLL(double lonMin, double latMin, double lonMax, double latMax);
    public abstract Task<ElevationData> GetDemXYZ(int x, int y, int z);
  }
}
