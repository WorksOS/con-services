using System;
using System.IO;
using System.IO.Compression;
using VSS.TRex.QuantizedMesh.Models;

namespace VSS.TRex.QuantizedMesh.MeshUtils
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

  public static class MapUtils
  {
    private static Double WGS84_A = 6378137.0;
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
      return (Math.Pow(4, n + 1) - 1) / 3;
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

    // Lat Lon to ECEF coordinates 
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
      return new MapPoint(lon3, lat3);
    }

    public static MapPoint MidPointLL3(MapPoint posA, MapPoint posB)
    {
      var midPoint = new MapPoint(0, 0);
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

    public static double DistanceToMeters(double lon1, double lat1, double lon2, double lat2)
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
      return dist * 1.609344 * 1000;
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
      var lat = Deg2Rad(pos2.Latitude - pos1.Latitude);
      var lng = Deg2Rad(pos2.Longitude - pos1.Longitude);

      var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
               Math.Cos(pos1LatRad) * Math.Cos(pos2LatRad) *
               Math.Sin(lng / 2) * Math.Sin(lng / 2);
      var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
      return R * h2;
    }

    public static int FlipY(int y, int zoom)
    {
      return (int)Math.Pow(2, zoom) - y - 1; // flip Y for our service
    }

    public static double tile2long(int x, int z) { return (x / Math.Pow(2, z) * 360 - 180); }

    public static double tile2lat(int y, int z)
    {
      var n = Math.PI - 2 * Math.PI * y / Math.Pow(2, z);
      return (180 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n))));
    }

  }

}
