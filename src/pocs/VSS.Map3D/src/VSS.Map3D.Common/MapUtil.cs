using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using VSS.Map3D.Models;
/*
  Note Cesium starts two tiles wide and one high. Different from most
 */

namespace VSS.Map3D.Common
{


  public struct PointD
  {
    public double X;
    public double Y;

    public PointD(double x, double y)
    {
      X = x;
      Y = y;
    }
  }


  public static class MapUtil
  {
    private static Double WGS84_A  = 6378137.0;
    private static Double WGS84_E = 0.0818191908;
    private static Double a = 6378137;
    private static Double f = 0.0034;
    private static Double b = 6.3568e6;
    private static Double e = Math.Sqrt((Math.Pow(a, 2) - Math.Pow(b, 2)) / Math.Pow(a, 2));
    private static Double e2 = Math.Sqrt((Math.Pow(a, 2) - Math.Pow(b, 2)) / Math.Pow(b, 2));


    public static int GridSizeToTriangleCount(int gridSize)
    {
      return (gridSize - 1) * (gridSize - 1) * 2;
    }


    public static double NumberOfTiles(int n)
    {
      // only good for quad tiles
      return Math.Pow(4, n);
    }

    public static double NumberOfTotalTiles(int n)
    {
      // only good for quad tiles
      return (Math.Pow(4, n+1) -1) /3;
    }

    public static double MaximumLevel(int n)
    {
      return Math.Ceiling(Math.Log(n) / Math.Log(4.0));
    }

    public static double Deg2Rad(double degrees)
    {
      double radians = (Math.PI / 180) * degrees;
      return (radians);
    }
    public static double Rad2Deg(double radians)
    {
      double degrees = (180 / Math.PI) * radians;
      return (degrees);
    }

    // Via https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#Tile_bounding_box
    /// <summary>
    /// lon lat position in decimal degrees. returns geographical 4326 coordinates
    /// </summary>
    /// <param name="lon"></param>
    /// <param name="lat"></param>
    /// <param name="zoom"></param>
    /// <returns></returns>
    public static PointF WorldToTilePos(double lon, double lat, int zoom)
    {
      PointF p = new Point();
      p.X = (float)((lon + 180.0) / 360.0 * (1 << zoom));
      p.Y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                                    1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

      return p;
    }

    // Via https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#Tile_bounding_box
    /// <summary>
    /// returns lon lat position in decimal degrees. returns geographical 4326 coordinates
    /// </summary>
    /// <param name="tile_x"></param>
    /// <param name="tile_y"></param>
    /// <param name="zoom"></param>
    /// <returns></returns>
    public static  PointF TileToWorldPos(double tile_x, double tile_y, int zoom)
    {
      PointF p = new Point();
      double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, zoom));

      p.X = (float)((tile_x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
      p.Y = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));
      return p;
    }

    public static float Lerp(float a0, float a1, float b0, float b1, float a)
    {
      return b0 + (b1 - b0) * ((a - a0) / (a1 - a0));
    }

    public static byte[] Compress(byte[] data)
    {
      using (var compressedStream = new MemoryStream())
      using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
      {
        zipStream.Write(data, 0, data.Length);
        zipStream.Close();
        return compressedStream.ToArray();
      }
    }

    public static byte[] Decompress(byte[] data)
    {
      using (var compressedStream = new MemoryStream(data))
      using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
      using (var resultStream = new MemoryStream())
      {
        zipStream.CopyTo(resultStream);
        return resultStream.ToArray();
      }
    }



    /*
     *
public static final double a = 6378137;
public static final double f = 0.0034;
public static final double b = 6.3568e6;
public static final double e = Math.sqrt((Math.pow(a, 2) - Math.pow(b, 2)) / Math.pow(a, 2));
public static final double e2 = Math.sqrt((Math.pow(a, 2) - Math.pow(b, 2)) / Math.pow(b, 2));

public static double[] ecef2lla(double x, double y, double z) {

    double[] lla = { 0, 0, 0 };
    double lan, lon, height, N , theta, p;

    p = Math.sqrt(Math.pow(x, 2) + Math.pow(y, 2));

    theta = Math.atan((z * a) / (p * b));

    lon = Math.atan(y / x);

    lat = Math.atan(((z + Math.pow(e2, 2) * b * Math.pow(Math.sin(theta), 3)) / ((p - Math.pow(e, 2) * a * Math.pow(Math.cos(theta), 3)))));
    N = a / (Math.sqrt(1 - (Math.pow(e, 2) * Math.pow(Math.sin(lat), 2))));

    double m = (p / Math.cos(lat));
    height = m - N;


    lon = lon * 180 / Math.PI;
    lat = lat * 180 / Math.PI; 
    lla[0] = lat;
    lla[1] = lon;
    lla[2] = height;
    return lla;
}     
     *
     */


    // This works. Lat Lon to ECEF coordinates 
    public static Vector3 LatLonToEcef(double lat, double lon, double alt)
    {
      // returns ECEF in meters
      double clat = Math.Cos(Deg2Rad(lat));
      double slat = Math.Sin(Deg2Rad(lat));
      double clon = Math.Cos(Deg2Rad(lon));
      double slon = Math.Sin(Deg2Rad(lon));
      double N = WGS84_A / Math.Sqrt(1.0 - WGS84_E * WGS84_E * slat * slat);
      var vec3 = new Vector3();
      vec3.X = (N + alt) * clat * clon;
      vec3.Y = (N + alt) * clat * slon;
      vec3.Z = (N * (1.0 - WGS84_E * WGS84_E) + alt) * slat;
      return vec3;
    }

    /*
     oringinal
    public static void LatLonToEcef(double lat, double lon, double alt, out double x, out double y, out double z)
    {
      // returns meters
      double clat = Math.Cos(Deg2Rad(lat));
      double slat = Math.Sin(Deg2Rad(lat));
      double clon = Math.Cos(Deg2Rad(lon));
      double slon = Math.Sin(Deg2Rad(lon));

      double N = WGS84_A / Math.Sqrt(1.0 - WGS84_E * WGS84_E * slat * slat);
      x = (N + alt) * clat * clon;
      y = (N + alt) * clat * slon;
      z = (N * (1.0 - WGS84_E * WGS84_E) + alt) * slat;
    }
     */

    // Coverts ECEF to ENU coordinates centered at given lat, lon
    public static void EcefToEnu(double lat, double lon, double x, double y, double z, double xr, double yr, double zr, out double e, out double n, out double u)
    {
      double clat = Math.Cos(Deg2Rad(lat));
      double slat = Math.Sin(Deg2Rad(lat));
      double clon = Math.Cos(Deg2Rad(lon));
      double slon = Math.Sin(Deg2Rad(lon));
      double dx = x - xr;
      double dy = y - yr;
      double dz = z - zr;

      e = -slon * dx + clon * dy;
      n = -slat * clon * dx - slat * slon * dy + clat * dz;
      u = clat * clon * dx + clat * slon * dy + slat * dz;
    }


    public static MapPoint MidPointLL(double lat1, double lon1, double lat2, double lon2)
    {
      double newLat = lat2 + ((lat1 - lat2) / 2);
      double newLon = lon2 + ((lon1 - lon2) / 2);
      return new MapPoint(newLon, newLat);
    }


    public static MapPoint MidPointRad(MapPoint pt1, MapPoint pt2)
    {
      
      double dLon = pt2.Longitude - pt1.Longitude;

      double Bx = Math.Cos(pt2.Latitude) * Math.Cos(dLon);
      double By = Math.Cos(pt2.Latitude) * Math.Sin(dLon);
      double lat3 = Math.Atan2(Math.Sin(pt1.Latitude) + Math.Sin(pt2.Latitude), Math.Sqrt((Math.Cos(pt1.Latitude) + Bx) * (Math.Cos(pt1.Latitude) + Bx) + By * By));
      double lon3 = pt1.Longitude + Math.Atan2(By, Math.Cos(pt1.Latitude) + Bx);

      //print out in degrees
      return new MapPoint(lon3,lat3);
    }




    public static MapPoint MidPointLL3(MapPoint posA, MapPoint posB)
    {
      var midPoint = new MapPoint(0,0);

      double dLon = Deg2Rad(posB.Longitude - posA.Longitude);
      double Bx = Math.Cos(Deg2Rad(posB.Latitude)) * Math.Cos(dLon);
      double By = Math.Cos(Deg2Rad(posB.Latitude)) * Math.Sin(dLon);

      midPoint.Latitude = Rad2Deg(Math.Atan2(
        Math.Sin(Deg2Rad(posA.Latitude)) + Math.Sin(Deg2Rad(posB.Latitude)),
        Math.Sqrt(
          (Math.Cos(Deg2Rad(posA.Latitude)) + Bx) *
          (Math.Cos(Deg2Rad(posA.Latitude)) + Bx) + By * By)));

      midPoint.Longitude = posA.Longitude + Rad2Deg(Math.Atan2(By, Math.Cos(Deg2Rad(posA.Latitude)) + Bx));

      return midPoint;
    }



    public static double DistanceTo(double lon1, double lat1, double lon2, double lat2, char unit = 'K')
    {
      double rlat1 = Math.PI * lat1 / 180;
      double rlat2 = Math.PI * lat2 / 180;
      double theta = lon1 - lon2;
      double rtheta = Math.PI * theta / 180;
      double dist =
        Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
        Math.Cos(rlat2) * Math.Cos(rtheta);
      dist = Math.Acos(dist);
      dist = dist * 180 / Math.PI;
      dist = dist * 60 * 1.1515;

      switch (unit)
      {
        case 'K': //Kilometers -> default
          return dist * 1.609344;
        case 'N': //Nautical Miles 
          return dist * 0.8684;
        case 'M': //Miles
          return dist;
      }

      return dist;
    }

    /// <summary>
    /// Works best for pole to pole
    /// </summary>
    /// <param name="longPt1"></param>
    /// <param name="latPt1"></param>
    /// <param name="lonPt2"></param>
    /// <param name="latPt2"></param>
    /// <returns></returns>
    public static double GetDistance(double longPt1, double latPt1, double lonPt2, double latPt2)
    {
      var d1 = latPt1 * (Math.PI / 180.0);
      var num1 = longPt1 * (Math.PI / 180.0);
      var d2 = latPt2 * (Math.PI / 180.0);
      var num2 = lonPt2 * (Math.PI / 180.0) - num1;
      var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

      return (6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)))) / 1000;
    }



    public static double Distance2(double lon1, double lat1, double lon2, double lat2, char unit)
    {
      if ((lat1 == lat2) && (lon1 == lon2))
      {
        return 0;
      }
      else
      {
        double theta = Math.Abs(lon1 - lon2);
        double dist = Math.Sin(Deg2Rad(lat1)) * Math.Sin(Deg2Rad(lat2)) + Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) * Math.Cos(Deg2Rad(theta));
        dist = Math.Acos(dist);
        dist = Rad2Deg(dist);
        dist = dist * 60 * 1.1515;
        if (unit == 'K')
        {
          dist = dist * 1.609344;
        }
        else if (unit == 'N')
        {
          dist = dist * 0.8684;
        }
        return (dist);
      }
    }

    public static double Distance3(double lon1, double lat1, double lon2, double lat2, char unit)
    {
      double circumference = 40075.0; // Earth's circumference at the equator in km
      double distance = 0.0;
      double latitude1Rad = Deg2Rad(lat1);
      double latititude2Rad = Deg2Rad(lat2);
      double longitude1Rad = Deg2Rad(lon1);
      double longitude2Rad = Deg2Rad(lon2);
      double logitudeDiff = Math.Abs(longitude1Rad - longitude2Rad);
      if (logitudeDiff > Math.PI)
      {
        logitudeDiff = 2.0 * Math.PI - logitudeDiff;
      }

      double angleCalculation =
        Math.Acos(
          Math.Sin(latititude2Rad) * Math.Sin(latitude1Rad) +
          Math.Cos(latititude2Rad) * Math.Cos(latitude1Rad) * Math.Cos(logitudeDiff));
      distance = circumference * angleCalculation / (2.0 * Math.PI);
      return distance;

    }

    // Experiemental below this line
    /// <summary>
    /// Returns the distance in miles or kilometers of any two
    /// latitude / longitude points.
    /// </summary>
    /// <param name="pos1">Location 1</param>
    /// <param name="pos2">Location 2</param>
    /// <param name="unit">Miles or Kilometers</param>
    /// <returns>Distance in the requested unit</returns>
    public static double HaversineDistance(MapPoint pos1, MapPoint pos2)
    {
      double pos1LatRad = Deg2Rad(pos1.Latitude);
      double pos2LatRad = Deg2Rad(pos2.Latitude);

      double R = 6371; // km
      var lat = Deg2Rad( pos2.Latitude - pos1.Latitude);
      var lng = Deg2Rad(pos2.Longitude - pos1.Longitude);

      var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
               Math.Cos(pos1LatRad) * Math.Cos(pos2LatRad) *
               Math.Sin(lng / 2) * Math.Sin(lng / 2);
      var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
      return R * h2;
    }

    public static int FlipY(int y, int zoom)
    {
      return (int) Math.Pow(2, zoom) - y - 1; // flip Y for our service
    }


    public static  double tile2long(int x, int z) { return (x / Math.Pow(2, z) * 360 - 180); }

    public static double tile2lat(int y, int z)
    {
      var n = Math.PI - 2 * Math.PI * y / Math.Pow(2, z);
      return (180 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n))));
    }


  }
}
