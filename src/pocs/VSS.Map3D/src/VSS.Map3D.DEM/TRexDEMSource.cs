using System;
using System.Threading.Tasks;
using VSS.Map3D.Common;
using VSS.Map3D.Models;
using VSS.Map3D.Terrain;
/*
* this class implements the TRex version of DEM data source
*/
namespace VSS.Map3D.DEM
{
  public class TRexDEMSource : DEMSourceBase
  {

    static readonly object _object = new object();

    public override void Initalize(MapHeader mapHeader)
    {
      GridSize = mapHeader.GridSize;
    }


    private ElevationData GetData(LLBoundingBox bbox, out Vector3[] ecefPoints)
    {
      //todo get data from TRex
      //Boolean dataAvailable = true;
     // if ((lat >= -43.548006 & lat <= -43.547679) & (lon >= 172.632951 & lon <= 172.633542))
      ElevationData ed;

      LLBoundingBox data = new LLBoundingBox(172.632951, -43.548006, 172.633542, -43.547679);
      bool dataAvailable = data.East >= bbox.West && data.West <= bbox.East && data.North >= bbox.South && data.South <= bbox.North;
      LLBoundingBox data2 = new LLBoundingBox(-0.124629, 51.500745, 1.0, 51.6);
      bool dataAvailable2 = data2.East >= bbox.West && data2.West <= bbox.East && data2.North >= bbox.South && data2.South <= bbox.North;

      int _gridSize;

      if (dataAvailable | dataAvailable2 )
        _gridSize = GridSize;
      else
        _gridSize = 2;
      ed = new ElevationData(_gridSize);
      ecefPoints = new Vector3[_gridSize * _gridSize];

      int PixelCount = _gridSize - 1;

      var yRange = bbox.North - bbox.South;
      var xRange = bbox.East - bbox.West;
      var xStep = xRange / PixelCount;
      var yStep = yRange / PixelCount;
      float minElev = float.PositiveInfinity;
      float maxElev = float.NegativeInfinity;
      Random rnd = new Random();
      int i = 0;
      float hgt = 0;
      var HgtMode = 2;
      for (int y = 0; y < GridSize; y++)
      {
        for (int x = 0; x < GridSize; x++)
        {
          var lat = bbox.South + (y * yStep);
          var lon = bbox.West + (x * xStep);


          if (HgtMode == 1)
          {
            ed.Elev[i] = rnd.Next(1, 8000);
          }
          else if (HgtMode == 2)
          {
            if ((lat >= -43.548006 & lat <= -43.547679) & (lon >= 172.632951 & lon <= 172.633542))
              ed.Elev[i] = (float)10.0;
            else if ((lat >= 51.500745 & lat <= 51.6) & (lon >= -0.124629 & lon <= 1.0))
              ed.Elev[i] = rnd.Next(1, 10);
            else
              ed.Elev[i] = 0;
          }

          if (ed.Elev[i] < minElev)
            minElev = ed.Elev[i];

          if (ed.Elev[i] > maxElev)
            maxElev = ed.Elev[i];

          // Make ecef point list for later header calculations
          ecefPoints[i] = Coord.geo_to_ecef(new Vector3() { X = MapUtil.Deg2Rad(lon), Y = MapUtil.Deg2Rad(lat), Z = ed.Elev[i] });

          i++;
        }
      }

      ed.MaximumHeight = maxElev;
      ed.MinimumHeight = minElev;
      return ed;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bbox"></param>
    /// <returns></returns>
    private ElevationData MakeTileDEM(LLBoundingBox bbox)
    {

      // Steps
      // Using bounding box get elevation data for tile
      // Calculate header info

      Vector3[] ecefPoints;
      ElevationData ed = GetData(bbox, out ecefPoints);

      // Work out bounding sphere
      TileInfo tileInfo = new TileInfo();
      var hdr = tileInfo.CalculateHeaderInfo(ref ecefPoints, false);
      ed.CenterX = hdr.CenterX;
      ed.CenterY = hdr.CenterY;
      ed.CenterZ = hdr.CenterZ;
      ed.BoundingSphereCenterX = hdr.CenterX;
      ed.BoundingSphereCenterY = hdr.CenterY;
      ed.BoundingSphereCenterZ = hdr.CenterZ;
      ed.BoundingSphereRadius = hdr.BoundingSphereRadius;

      // Work out HorizonOcclusionPoint
      var hop = HorizonOcclusionPoint.FromPoints(ecefPoints, tileInfo.BoundingSphere);
      ed.HorizonOcclusionPointX = hop.X;
      ed.HorizonOcclusionPointY = hop.Y;
      ed.HorizonOcclusionPointZ = hop.Z;

      /* Quicker cheat method untested
      ed.HorizonOcclusionPointX = hdr.CenterX;
      ed.HorizonOcclusionPointY = hdr.CenterY;
      ed.HorizonOcclusionPointZ = maxElev;
      */

      // todo lighting

      return ed;
      
    }



    public override Task<ElevationData> GetDemLL(double lonMin, double latMin, double lonMax, double latMax)
    {
      throw new NotImplementedException();
    }




    public async override Task<ElevationData> GetDemXYZ(int x, int y, int z)
    {
      lock (_object) // thread safe lookup contention. 
      {
     
        // Get the lat lon boundary from xyz tile
        var rect = MapGeo.TileXYZToRectLL(x, y, z);

        // Make the DEM that will be used to make a quantized mesh
        return MakeTileDEM(rect);

      }
    }
  }
}
