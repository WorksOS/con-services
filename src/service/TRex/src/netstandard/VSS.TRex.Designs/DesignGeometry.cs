using VSS.TRex.Geometry;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Designs
{
  /// <summary>
  /// A container for some common design related management methods
  /// </summary>
  public static class DesignGeometry
  {
    /// <summary>
    /// Determines if the bounds of a sub grid intersects a given triangle
    /// </summary>
    public static bool SubGridIntersectsTriangle(BoundingWorldExtent3D extents, XYZ h1, XYZ h2, XYZ v)
    {
      // If any of the triangle vertices are in the cell extents then 'yes'
      if (extents.Includes(h1.X, h1.Y) || extents.Includes(h2.X, h2.Y) || extents.Includes(v.X, v.Y))
      {
        return true;
      }

      // If any of the sub grid corners sit in the triangle then 'yes'
      {
        if (XYZ.PointInTriangle(h1, h2, v, extents.MinX, extents.MinY) ||
            XYZ.PointInTriangle(h1, h2, v, extents.MinX, extents.MaxY) ||
            XYZ.PointInTriangle(h1, h2, v, extents.MaxX, extents.MaxY) ||
            XYZ.PointInTriangle(h1, h2, v, extents.MaxX, extents.MinY))
        {
          return true;
        }
      }

      // If any of the extent and triangle lines intersect then also 'yes'
      if (LineIntersection.LinesIntersect(extents.MinX, extents.MinY, extents.MaxX, extents.MinY, h1.X, h1.Y, h2.X, h2.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(extents.MaxX, extents.MinY, extents.MaxX, extents.MaxY, h1.X, h1.Y, h2.X, h2.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(extents.MinX, extents.MaxY, extents.MaxX, extents.MaxY, h1.X, h1.Y, h2.X, h2.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(extents.MinX, extents.MinY, extents.MinX, extents.MaxY, h1.X, h1.Y, h2.X, h2.Y, out _, out _, false, out _) ||

          LineIntersection.LinesIntersect(extents.MinX, extents.MinY, extents.MaxX, extents.MinY, h1.X, h1.Y, v.X, v.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(extents.MaxX, extents.MinY, extents.MaxX, extents.MaxY, h1.X, h1.Y, v.X, v.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(extents.MinX, extents.MaxY, extents.MaxX, extents.MaxY, h1.X, h1.Y, v.X, v.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(extents.MinX, extents.MinY, extents.MinX, extents.MaxY, h1.X, h1.Y, v.X, v.Y, out _, out _, false, out _) ||

          LineIntersection.LinesIntersect(extents.MinX, extents.MinY, extents.MaxX, extents.MinY, v.X, v.Y, h2.X, h2.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(extents.MaxX, extents.MinY, extents.MaxX, extents.MaxY, v.X, v.Y, h2.X, h2.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(extents.MinX, extents.MaxY, extents.MaxX, extents.MaxY, v.X, v.Y, h2.X, h2.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(extents.MinX, extents.MinY, extents.MinX, extents.MaxY, v.X, v.Y, h2.X, h2.Y, out _, out _, false, out _))
      {
        return true;
      }

      // Otherwise 'no'
      return false;
    }

    /// <summary>
    /// Simple utility function for swapping to XYZ vertices
    /// </summary>
    public static void SwapVertices(ref XYZ a, ref XYZ b) => MinMax.Swap(ref a, ref b);
  }
}
