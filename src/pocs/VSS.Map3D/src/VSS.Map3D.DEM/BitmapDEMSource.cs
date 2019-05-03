
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using VSS.Map3D.Common;
using VSS.Map3D.Models;
using VSS.Map3D.Terrain;
/*
Bitmap implementation of IDEMSource
Bitmaps work in pixels and 3D works in vertices. For one pixel we have four vertices.
The testing purposes a pixel value represents the SE vertex. e.g. GridSize = Pixel Count + 1
Its only a test case so we can improve on BM implementation later. e.g. Averaging values
*/


namespace VSS.Map3D.DEM
{
  /// <summary>
  /// Simple class to return elevation data from a heightmap
  /// </summary>
  public class BitmapDEMSource : DEMSourceBase
  {
    public int GridSize { get; set; }
    public double MinHeight { get; set; }
    public double MaxHeight { get; set; }
    public string DEMLocation { get; set; }

    //      throw new NotImplementedException();
    private double South;
    private double North;
    private double West;
    private double East;
    private int Width;
    private int Depth;
    private double ymin;
    private double ymax;
    private double xFactor;
    private double yFactor;
    private Bitmap Map;
    private float ElevMin;
    private float ElevMax;
    private bool MapLoaded = false;
    static readonly object _object = new object();



    public override void Initalize(MapHeader mapHeader)
    {
      // World Map
      if (!MapLoaded)
      {
        if (mapHeader.MaxElevation > 0)
        {
          MaxHeight = mapHeader.MaxElevation;
          MinHeight = mapHeader.MinElevation;

        }
        else
        {
          MaxHeight = 100;
          MinHeight = 0;
        }
        LoadMap();

        South = MapUtil.Deg2Rad(-83.0);
        North = MapUtil.Deg2Rad(83.0);
    //    South = MapUtil.Deg2Rad(-85.0);
     //   North = MapUtil.Deg2Rad(85.0);

        West = MapUtil.Deg2Rad(-180.0);
        East = MapUtil.Deg2Rad(180.0);
        ymin = MercY(South);
        ymax = MercY(North);
        xFactor = Width / (East - West);
        yFactor = Depth / (ymax - ymin);
        GridSize = mapHeader.GridSize;
        MapLoaded = true;
      }
    }

    // Formula for mercator projection y coordinate:
    private double MercY(double lat)
    {
      return Math.Log(Math.Tan(lat / 2 + Math.PI / 4));
    }

    private void LoadMap()
    {
      //  HeightMapScale = scale;
//      FileInfo fi = new FileInfo("C:\\map\\heightmap\\GDEM-10km-BW.png");
      FileInfo fi = new FileInfo("C:\\map\\heightmap\\World.bmp");
      ElevMin = (float) MinHeight;
      ElevMax = (float) MaxHeight;
      // Nuget system.drawing.common for bitmap

      Map = Image.FromFile(fi.FullName) as Bitmap;

      Width = Map.Width;

      Depth = Map.Height;
      /*
      var Heightmap = new float[Width, Depth];
      for (int x = 0; x < Width; x++) // left to right
      {

        for (int z = 0; z < Depth; z++) // Top to bottom
        {

          Heightmap[x, z] = Map.GetPixel(x, z).GetBrightness(); // 0=black, 1= white

        }

      }
      */

    }

    /* by coords
    private ElevationData MakeTileDEM(Vector2 min, Vector2 max)
    {

      ElevationData ed = new ElevationData(GridSize,GridSize,GridSize,0,8000);
      // we are going to walk up from sw to ne for returning dem in latlon coordinates
      var yRange = min.y - max.y; // y runs from top to bottom
      var xRange = max.x - min.x;
      var xStep = xRange / GridSize;
      var yStep = yRange / GridSize;

      int i = 0;
      for (int y = 0; y < GridSize; y++)
      {
        for (int x = 0; x < GridSize; x++)
        {
          var v = MapProject(min.x +(x * xStep), min.y -(y * yStep));

          float hgt = Map.GetPixel((int)Math.Round(v.x), (int)Math.Round(v.y)).GetBrightness(); // 0=black, 1= white
          ed.Elev[i] = MapUtil.Lerp(ElevMin, ElevMax, hgt);
          i++;
        }
      }

      return ed;
    }
    */


    public static void ComputeHeaderInfo(ref ElevationData evlData)
    {
      int m = 0;
      for (int y = 0; y < evlData.GridSize; y++)
      {
        for (int x = 0; x < evlData.GridSize; x++)
        {
          //   vertices.AddVertex(m, (ushort)(x * ratio), (ushort)(y * ratio), QuantizeHeight(evlData.MinElevation, evlData.MaxElevation, evlData.Elev[m]));
          m++;
        }
      }

    }

    /*
     https://cesium.com/blog/2013/05/09/computing-the-horizon-occlusion-point/
    private computeMagnitude(ellipsoid, position, scaledSpaceDirectionToPoint)
    {
      var scaledSpacePosition = ellipsoid.transformPositionToScaledSpace(position);
      var magnitudeSquared = scaledSpacePosition.magnitudeSquared();
      var magnitude = Math.sqrt(magnitudeSquared);
      var direction = scaledSpacePosition.divideByScalar(magnitude);

      // For the purpose of this computation, points below the ellipsoid
      // are considered to be on it instead.
      magnitudeSquared = Math.max(1.0, magnitudeSquared);
      magnitude = Math.max(1.0, magnitude);

      var cosAlpha = direction.dot(scaledSpaceDirectionToPoint);
      var sinAlpha = direction.cross(scaledSpaceDirectionToPoint).magnitude();
      var cosBeta = 1.0 / magnitude;
      var sinBeta = Math.sqrt(magnitudeSquared - 1.0) * cosBeta;

      return 1.0 / (cosAlpha * cosBeta - sinAlpha * sinBeta);
    }
    */

    /// <summary>
    /// By LatLon
    /// </summary>
    /// <param name="bb"></param>
    /// <returns></returns>
    private ElevationData MakeTileDEM(LLBoundingBox bb)
    {

      // todo we also need to comput header info for the quantized mesh


      Vector2 v;
      v.X = 0;
      v.Y = 0;
      int x2 = 0;
      int y2 = 0;
      int PixelCount = GridSize - 1;


      var DoRandom = false;
      int HgtMode = 2;

      float minElev = float.PositiveInfinity;
      float maxElev = float.NegativeInfinity;

      Random rnd = new Random();

      ElevationData ed = new ElevationData(GridSize);

      var midPt = bb.GetCenter();
    //  var pt2 =  Coord.geo_to_ecef(new Vector3( MapUtil.Deg2Rad(midPt.Longitude), MapUtil.Deg2Rad(midPt.Latitude),0)); // zero elevation for now
      var pt =  MapUtil.LatLonToEcef(midPt.Latitude, midPt.Longitude,0); // zero elevation for now

      Vector3[] ecefPoints = new Vector3[GridSize * GridSize];

      try
      {
        // we are going to walk up from sw to ne for returning dem in latlon coordinates
        var yRange = bb.North - bb.South;
        var xRange = bb.East - bb.West;
        var xStep = xRange / PixelCount;
        var yStep = yRange / PixelCount;


        int i = 0;
        float hgt = 0;
        for (int y = 0; y < GridSize; y++)
        {
          for (int x = 0; x < GridSize; x++)
          {
            var lat = bb.South + (y * yStep);
            var lon = bb.West  + (x * xStep);
            v = MapProject(lat, lon); // MapProject is lat long


            if (v.X >= Width)
              v.X = Width - 1;
            if (v.Y >= Depth)
              v.Y = Depth - 1;
            if (v.X < 0)
              v.X = 0;
            if (v.Y < 0)
              v.Y =0;

            x2 =(int)v.X;
            y2 =(int) v.Y;

            //hgt = 0;
           // if (i == 4)
            //  hgt = 100;

            if (HgtMode == 1)
            {
              ed.Elev[i] = rnd.Next(1, 8000);
            }
            else if (HgtMode == 2)
            {
              ed.Elev[i] = 0;
              if (lat >= -43.548006 & lat <= -43.547679)
                if (lon >= 172.632951 & lon <= 172.633542)
                   ed.Elev[i] = (float)10.0;
            }
            else
            {
              hgt = Map.GetPixel(x2, y2).GetBrightness(); // 0=black, 1= white
              ed.Elev[i] = MapUtil.Lerp(0, 1, ElevMin, ElevMax, hgt);
            }

            if (ed.Elev[i] < minElev)
              minElev = ed.Elev[i];

            if (ed.Elev[i] > maxElev)
              maxElev = ed.Elev[i];

            // Make ecef point list for later calculations
            ecefPoints[i] = Coord.geo_to_ecef(new Vector3() { X=MapUtil.Deg2Rad(lon),Y=MapUtil.Deg2Rad(lat),Z= ed.Elev[i]});


            i++;
          }
        }
      }
      catch (Exception ex)
      {
        
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"**** MakeTileDEM Exception ****:{ex} v:{v} x2{x2} y2{y2} ");
// file writer        System.Diagnostics.Trace.WriteLine($"**** MakeTileDEM Exception *********:{ex} v:{v} x2{x2} y2{y2} ");
#endif
      }

      // todo now make a ecfc array to calculate header info
      TileInfo tileInfo = new TileInfo();
      var hdr = tileInfo.CalculateHeaderInfo(ref ecefPoints, false);
      ed.CenterX = hdr.CenterX;
      ed.CenterY = hdr.CenterY;
      ed.CenterZ = hdr.CenterZ;
      ed.BoundingSphereCenterX = hdr.CenterX;
      ed.BoundingSphereCenterY = hdr.CenterY;
      ed.BoundingSphereCenterZ = hdr.CenterZ;
      ed.BoundingSphereRadius = hdr.BoundingSphereRadius;
      ed.MaximumHeight = maxElev;
      ed.MinimumHeight = minElev;

      
      var hop = HorizonOcclusionPoint.FromPoints(ecefPoints, tileInfo.BoundingSphere);
      ed.HorizonOcclusionPointX = hop.X;
      ed.HorizonOcclusionPointY = hop.Y;
      ed.HorizonOcclusionPointZ = hop.Z;
      

      /*
    // FIXME: is there a better choice for a horizon occlusion point?
    // Currently it's the center of tile elevated to bbox's max Z
    header.horizon_occlusion = c;
    header.horizon_occlusion.z = bbox.max.z;       
       

      ed.HorizonOcclusionPointX = hdr.CenterX;
      ed.HorizonOcclusionPointY = hdr.CenterY;
      ed.HorizonOcclusionPointZ = maxElev;
      */
      return ed;
    }


    public Vector2 MapProject(double lat, double lon)
    {
      if (lat > 83.0)
        lat = 83.0;
      if (lat < -83.0)
        lat = -83.0;

      var x = MapUtil.Deg2Rad(lon);
      var y = MercY(MapUtil.Deg2Rad(lat));
      x = (x - West) * xFactor;
      y = (ymax - y)*yFactor; // y points south
      return new Vector2(x,y);
    }

    public async override Task<ElevationData> GetDemLL(double minLon, double minLat, double maxLon, double maxLat)

    {
      var rect = new LLBoundingBox(new MapPoint(minLon, minLat), new MapPoint(maxLon, maxLat));
      return MakeTileDEM(rect);
    }

    public async override Task<ElevationData> GetDemXYZ(int x, int y, int z)
    {
      lock (_object) // thread safe bitmap lookup contention. Todo use thread safe readonly list
      {
        y = MapUtil.FlipY(y,z);
        // Get the lat lon boundary from xyz tile
        var rect = Geographic.TileXYToRectangleLL(x,y,z);

        // Make the DEM that will be used to make a quantized mesh
        return MakeTileDEM(rect);

      }
    }
  }
}
