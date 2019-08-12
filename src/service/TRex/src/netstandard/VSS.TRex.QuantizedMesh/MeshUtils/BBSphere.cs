using System;
using VSS.TRex.QuantizedMesh.Models;

namespace VSS.TRex.QuantizedMesh.MeshUtils
{
  public class BBSphere
  {

    private static int nbPositions = 0;

    private static float MAX = float.PositiveInfinity;
    private static float MIN = float.NegativeInfinity;
    public Vector3 Center;
    public float Radius;
    private Vector3 minPointX = new Vector3() { X = MAX, Y = MAX, Z = MAX };
    private Vector3 minPointY = new Vector3() { X = MAX, Y = MAX, Z = MAX };
    private Vector3 minPointZ = new Vector3() { X = MAX, Y = MAX, Z = MAX };
    private Vector3 maxPointX = new Vector3() { X = MIN, Y = MIN, Z = MIN };
    private Vector3 maxPointY = new Vector3() { X = MIN, Y = MIN, Z = MIN };
    private Vector3 maxPointZ = new Vector3() { X = MIN, Y = MIN, Z = MIN };

    // Based on Ritter's algorithm
    public void FromPoints(Vector3[] points) // ECFC
    {
      nbPositions = points.Length;
      if (nbPositions < 2)
      {
        throw new Exception("Your list of points must contain at least 2 points");
      }

      for (int i = 0; i < nbPositions; i++)
      {
        var point = points[i];
        // Store the points containing the smallest and largest component used for the naive approach
        if (point.X < minPointX.X)
          minPointX = point;
        if (point.Y < minPointY.Y)
          minPointY = point;
        if (point.Z < minPointZ.Z)
          minPointZ = point;
        if (point.X > maxPointX.X)
          maxPointX = point;
        if (point.Y > maxPointY.Y)
          maxPointY = point;
        if (point.Z > maxPointZ.Z)
          maxPointZ = point;
      }

      // Squared distance between each component min and max
      var xSpan = Cartesian3D.MagnitudeSquared(Cartesian3D.Subtract(maxPointX, minPointX));
      var ySpan = Cartesian3D.MagnitudeSquared(Cartesian3D.Subtract(maxPointY, minPointY));
      var zSpan = Cartesian3D.MagnitudeSquared(Cartesian3D.Subtract(maxPointZ, minPointZ));
      var diameter1 = minPointX;
      var diameter2 = maxPointX;
      var maxSpan = xSpan;
      if (ySpan > maxSpan)
      {
        maxSpan = ySpan;
        diameter1 = minPointY;
        diameter2 = maxPointY;
      }
      if (zSpan > maxSpan)
      {
        maxSpan = zSpan;
        diameter1 = minPointZ;
        diameter2 = maxPointZ;
      }
      var ritterCenter = new Vector3()
      {
        X = (diameter1.X + diameter2.X) * 0.5,
        Y = (diameter1.Y + diameter2.Y) * 0.5,
        Z = (diameter1.Z + diameter2.Z) * 0.5
      };

      var radiusSquared = Cartesian3D.MagnitudeSquared(Cartesian3D.Subtract(diameter2, ritterCenter));
      var ritterRadius = Math.Sqrt(radiusSquared);
      // Initial center and radius (naive) get min and max box
      var minBoxPt = new Vector3()
      {
        X = minPointX.X,
        Y = minPointY.Y,
        Z = minPointZ.Z
      };
      var maxBoxPt = new Vector3()
      {
        X = maxPointX.X,
        Y = maxPointY.Y,
        Z = maxPointZ.Z
      };
      var naiveCenter = Cartesian3D.MultiplyByScalar(Cartesian3D.Add(minBoxPt, maxBoxPt), 0.5);
      var naiveRadius = 0.0;

      for (int i = 0; i < nbPositions; i++)
      //      foreach (var i in xrange(0, nbPositions))
      {
        var currentP = points[i];
        // Find the furthest point from the naive center to calculate the naive radius.
        var r = Cartesian3D.Magnitude(Cartesian3D.Subtract(currentP, naiveCenter));
        if (r > naiveRadius)
          naiveRadius = r;
        // Make adjustments to the Ritter Sphere to include all points.
        var oldCenterToPointSquared = Cartesian3D.MagnitudeSquared(Cartesian3D.Subtract(currentP, ritterCenter));
        if (oldCenterToPointSquared > radiusSquared)
        {
          var oldCenterToPoint = Math.Sqrt(oldCenterToPointSquared);
          ritterRadius = (ritterRadius + oldCenterToPoint) * 0.5;
          // Calculate center of new Ritter sphere
          var oldToNew = oldCenterToPoint - ritterRadius;
          ritterCenter = new Vector3()
          {
            X = (ritterRadius * ritterCenter.X + oldToNew * currentP.X) / oldCenterToPoint,
            Y = (ritterRadius * ritterCenter.Y + oldToNew * currentP.Y) / oldCenterToPoint,
            Z = (ritterRadius * ritterCenter.Z + oldToNew * currentP.Z) / oldCenterToPoint
          };
        }
      }
      // Keep the naive sphere if smaller
      if (naiveRadius < ritterRadius)
      {
        Radius = (float)ritterRadius;
        Center = ritterCenter;
      }
      else
      {
        Radius = (float)naiveRadius;
        Center = naiveCenter;
      }

    }
  }
}
