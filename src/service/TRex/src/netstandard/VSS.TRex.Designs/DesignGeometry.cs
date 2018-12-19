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
    /// Determines if the bounds of a subgrid intersects a given triangle
    /// </summary>
    /// <param name="Extents"></param>
    /// <param name="H1"></param>
    /// <param name="H2"></param>
    /// <param name="V"></param>
    /// <returns></returns>
    public static bool SubGridIntersectsTriangle(BoundingWorldExtent3D Extents, XYZ H1, XYZ H2, XYZ V)
    {
      // If any of the triangle vertices are in the cell extents then 'yes'
      if (Extents.Includes(H1.X, H1.Y) || Extents.Includes(H2.X, H2.Y) || Extents.Includes(V.X, V.Y))
      {
        return true;
      }

      // If any of the subgrid corners sit in the triangle then 'yes'
      {
        if (XYZ.PointInTriangle(H1, H2, V, Extents.MinX, Extents.MinY) ||
            XYZ.PointInTriangle(H1, H2, V, Extents.MinX, Extents.MaxY) ||
            XYZ.PointInTriangle(H1, H2, V, Extents.MaxX, Extents.MaxY) ||
            XYZ.PointInTriangle(H1, H2, V, Extents.MaxX, Extents.MinY))
        {
          return true;
        }
      }

      // If any of the extent and triangle lines intersect then also 'yes'
      if (LineIntersection.LinesIntersect(Extents.MinX, Extents.MinY, Extents.MaxX, Extents.MinY, H1.X, H1.Y, H2.X, H2.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(Extents.MaxX, Extents.MinY, Extents.MaxX, Extents.MaxY, H1.X, H1.Y, H2.X, H2.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(Extents.MinX, Extents.MaxY, Extents.MaxX, Extents.MaxY, H1.X, H1.Y, H2.X, H2.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(Extents.MinX, Extents.MinY, Extents.MinX, Extents.MaxY, H1.X, H1.Y, H2.X, H2.Y, out _, out _, false, out _) ||

          LineIntersection.LinesIntersect(Extents.MinX, Extents.MinY, Extents.MaxX, Extents.MinY, H1.X, H1.Y, V.X, V.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(Extents.MaxX, Extents.MinY, Extents.MaxX, Extents.MaxY, H1.X, H1.Y, V.X, V.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(Extents.MinX, Extents.MaxY, Extents.MaxX, Extents.MaxY, H1.X, H1.Y, V.X, V.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(Extents.MinX, Extents.MinY, Extents.MinX, Extents.MaxY, H1.X, H1.Y, V.X, V.Y, out _, out _, false, out _) ||

          LineIntersection.LinesIntersect(Extents.MinX, Extents.MinY, Extents.MaxX, Extents.MinY, V.X, V.Y, H2.X, H2.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(Extents.MaxX, Extents.MinY, Extents.MaxX, Extents.MaxY, V.X, V.Y, H2.X, H2.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(Extents.MinX, Extents.MaxY, Extents.MaxX, Extents.MaxY, V.X, V.Y, H2.X, H2.Y, out _, out _, false, out _) ||
          LineIntersection.LinesIntersect(Extents.MinX, Extents.MinY, Extents.MinX, Extents.MaxY, V.X, V.Y, H2.X, H2.Y, out _, out _, false, out _))
      {
        return true;
      }

      // Otherwise 'no'
      return false;
    }

    /// <summary>
    /// Simple utility function for swapping to XYZ vertices
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    public static void SwapVertices(ref XYZ A, ref XYZ B) => MinMax.Swap(ref A, ref B);
  }
}
