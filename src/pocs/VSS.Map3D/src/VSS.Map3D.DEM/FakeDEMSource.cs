using System;
using System.Net;
using System.Threading.Tasks;
using VSS.Map3D.Models;
/// <summary>
/// Simple class to create a tile for testing
/// </summary>
namespace VSS.Map3D.DEM
{
  public class FakeDEMSource : DEMSourceBase
  {
    public int GridSize { get; set; }
    public double MinHeight { get; set; }
    public double MaxHeight { get; set; }

    public override void Initalize(MapHeader mapHeader)
    {
      GridSize = mapHeader.GridSize;
    }

    public async override Task<ElevationData> GetDemXYZ(int x, int y, int z)
    {
      throw new NotImplementedException();
    }

    public async override Task<ElevationData> GetDemLL(double lonMin, double latMin, double lonMax, double latMax)
    {
      // create grid of uniform heights
      var dem = new ElevationData();
      dem.MinimumHeight = float.MaxValue;
      dem.MaximumHeight = float.MinValue;
      dem.GridSize = GridSize;
      float hgt;

      dem.Elev = new float[GridSize * GridSize];
      // elevation order will be from sw, se, and up
      // old standard example form Cesium code for tilesize 1, 2 triangles. slopes from nw to se
      if (GridSize == 2)
      {
        dem.Elev[0] = 100;
        dem.Elev[1] = 200;
        dem.Elev[2] = 0;
        dem.Elev[3] = 100;
        dem.MinimumHeight = 0;
        dem.MaximumHeight = 400;
      }
      else if (GridSize == 3)
      {
        dem.Elev[0] = 0;
        dem.Elev[1] = 0;
        dem.Elev[2] = 0;
        dem.Elev[3] = 0;
        dem.Elev[4] = 100;
        dem.Elev[5] = 0;
        dem.Elev[6] = 0;
        dem.Elev[7] = 0;
        dem.Elev[8] = 0;
        dem.MinimumHeight = 0;
        dem.MaximumHeight = 100;
      }
      else
      {
        //Basic slope. Todo staircase
        hgt = 0;
        int pos = 0;
        for (int y = 0; y < GridSize; y++)
        {
          for (int x = 0; x < GridSize; x++)
          {
            if (x == 0 || y == 0   )
              dem.Elev[pos] = 0;
            else
              dem.Elev[pos] = hgt;
            if (hgt < dem.MinimumHeight)
              dem.MinimumHeight = hgt;
            if (hgt > dem.MaximumHeight)
              dem.MaximumHeight = hgt;
            pos++;
          }

          if (y > GridSize / 2)
            hgt = hgt + (float)-0.5;
          else
            hgt = hgt + (float)0.5;
        }
      }

      return dem;
    }
  }
}
