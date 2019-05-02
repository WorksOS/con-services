using System;
using System.Collections.Generic;
using System.Numerics;
using Vector3 = VSS.Map3D.Common.Vector3;

namespace VSS.Map3D.Terrain
{
  public static class HorizonOcclusionPoint
  {

    // Constants taken from http://cesiumjs.org/2013/04/25/Horizon-culling/
    private static Double radiusX = 6378137.0;
    private static Double radiusY = 6378137.0;
    private static Double radiusZ = 6356752.3142451793;
    private static Double rX = 1.0 / radiusX;
    private static Double rY = 1.0 / radiusY;
    private static Double rZ = 1.0 / radiusZ;


    /*
    wgs84_a = radiusX               # Semi-major axis
      wgs84_b = radiusZ          # Semi-minor axis
    wgs84_e2 = 0.0066943799901975848  # First eccentricity squared
    wgs84_a2 = wgs84_a** 2           # To speed things up a bit
    wgs84_b2 = wgs84_b** 2
    */

    public static Double DotProduct(Vector3 pt1, Vector3 pt2)
    {
      return pt1.X * pt2.X + pt1.Y * pt2.Y + pt1.Z * pt2.Z; 
    }

    public static Vector3 CrossProduct(Vector3 A, Vector3 B)
    {
      return new Vector3(A.Y * B.Z - B.Y * A.Z, A.Z * B.X - B.Z * A.X, A.X * B.Y - B.X * A.Y);
    }

    // Functions assumes ellipsoid scaled coordinates
    public static Double ComputeMagnitude(Vector3 point, Vector3 sphereCenter)
    {
      var magnitudeSquared = Cartesian3D.MagnitudeSquared(point);
      var magnitude = Math.Sqrt(magnitudeSquared);
      var direction = Cartesian3D.MultiplyByScalar(point, 1 / magnitude);
      magnitudeSquared = Math.Max(1.0, magnitudeSquared);
      magnitude = Math.Max(1.0, magnitude);
      var cosAlpha = DotProduct(direction, sphereCenter);
      var sinAlpha = Cartesian3D.Magnitude(CrossProduct(direction, sphereCenter));
      var cosBeta = 1.0 / magnitude;
      var sinBeta = Math.Sqrt(magnitudeSquared - 1.0) * cosBeta;
      return 1.0 / (cosAlpha * cosBeta - sinAlpha * sinBeta);
 
    }
    /*

    public static Vector3[] ScaleDown(ref Vector3[] pts)
    {
      for (int i = 0; i < pts.Length; i++)
      {
        pts[i].X = pts[i].X * rX;
        pts[i].Y = pts[i].X * rY;
        pts[i].Z = pts[i].X * rZ;
      }

    }
    */

    // from https://cesiumjs.org/2013/05/09/Computing-the-horizon-occlusion-point/
    public static Vector3 FromPoints(Vector3[] points, BBSphere boundingSphere)
    {
      if (points.Length < 1)
      {
        throw new Exception("Your list of points must contain at least 2 points");
      }

      // Bring coordinates to ellipsoid scaled coordinates
      for (int i = 0; i < points.Length; i++)
      {
        points[i].X = points[i].X * rX;
        points[i].Y = points[i].Y * rY;
        points[i].Z = points[i].Z * rZ;
      }

      // var scaledPoints = map(scaleDown, points).ToList();

      //   var scaledSphereCenter = scaleDown(boundingSphere.center);
      boundingSphere.Center.X = boundingSphere.Center.X * rX;
      boundingSphere.Center.Y = boundingSphere.Center.Y * rY;
      boundingSphere.Center.Z = boundingSphere.Center.Z * rZ;

      //      Func<object, object> magnitude = coord => {
      //      return computeMagnitude(coord, scaledSphereCenter);
      //   };

      //  List<Double> magnitudes = new List<double>();
      Double maxMagnitude = double.NegativeInfinity;
      for (int i = 0; i < points.Length; i++)
      {
        //magnitudes.Add(ComputeMagnitude(points[i], boundingSphere.Center));
        var magnitude = ComputeMagnitude(points[i], boundingSphere.Center);
        if (magnitude > maxMagnitude)
          maxMagnitude = magnitude;
      }


      //  var magnitudes = map(magnitude, scaledPoints).ToList();

      return Cartesian3D.MultiplyByScalar(boundingSphere.Center, maxMagnitude);
    }



  }



  /*
   Python version
   listed here for curiosity

def LLH2ECEF(lon, lat, alt):
    lat *= (old_div(math.pi, 180.0))
    lon *= (old_div(math.pi, 180.0))

    def n(x):
        return old_div(wgs84_a, math.sqrt(
            1 - wgs84_e2 * (math.sin(x) ** 2)))

    x = (n(lat) + alt) * math.cos(lat) * math.cos(lon)
    y = (n(lat) + alt) * math.cos(lat) * math.sin(lon)
    z = (n(lat) * (1 - wgs84_e2) + alt) * math.sin(lat)

    return [x, y, z]

# alt is in meters


def ECEF2LLH(x, y, z):
    ep = math.sqrt(old_div((wgs84_a2 - wgs84_b2), wgs84_b2))
    p = math.sqrt(x ** 2 + y ** 2)
    th = math.atan2(wgs84_a * z, wgs84_b * p)
    lon = math.atan2(y, x)
    lat = math.atan2(
        z + ep ** 2 * wgs84_b * math.sin(th) ** 3,
        p - wgs84_e2 * wgs84_a * math.cos(th) ** 3
    )
    N = old_div(wgs84_a, math.sqrt(1 - wgs84_e2 * math.sin(lat) ** 2))
    alt = old_div(p, math.cos(lat)) - N

    lon *= (old_div(180., math.pi))
    lat *= (old_div(180., math.pi))

    return [lon, lat, alt]

   */



}
