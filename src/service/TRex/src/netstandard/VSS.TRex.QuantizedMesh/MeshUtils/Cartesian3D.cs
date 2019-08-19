using System;
using VSS.TRex.QuantizedMesh.Models;

namespace VSS.TRex.QuantizedMesh.MeshUtils
{
  public static class Cartesian3D
  {

    public static Double MagnitudeSquared(Vector3 p)
    {
      return Math.Pow(p.X, 2) + Math.Pow(p.Y, 2) + Math.Pow(p.Z, 2);
    }

    public static Double Magnitude(Vector3 p)
    {
      return Math.Sqrt(MagnitudeSquared(p));
    }

    public static Vector3 Add(Vector3 left, Vector3 right)
    {
      return new Vector3()
      {
        X = left.X + right.X,
        Y = left.Y + right.Y,
        Z = left.Z + right.Z
      };
    }

    public static Vector3 Subtract(Vector3 left, Vector3 right)
    {
      return new Vector3()
      {
        X = left.X - right.X,
        Y = left.Y - right.Y,
        Z = left.Z - right.Z
      };
    }

    public static Double DistanceSquared(Vector3 p1, Vector3 p2)
    {
      return Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2) + Math.Pow(p1.Z - p2.Z, 2);
    }

    public static Double Distance(Vector3 p1, Vector3 p2)
    {
      return Math.Sqrt(DistanceSquared(p1, p2));
    }

    public static Vector3 MultiplyByScalar(Vector3 p, Double scalar)
    {
      return new Vector3()
      {
        X = p.X * scalar,
        Y = p.Y * scalar,
        Z = p.Z * scalar
      };
    }

    public static Vector3 Normalize(Vector3 p)
    {
      var mgn = Magnitude(p);
      return new Vector3()
      {
        X = p.X / mgn,
        Y = p.Y / mgn,
        Z = p.Z / mgn
      };
    }

  }
}
