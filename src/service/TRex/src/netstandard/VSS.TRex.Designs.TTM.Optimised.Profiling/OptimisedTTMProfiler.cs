using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Profiling;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Utilities;

namespace VSS.TRex.Designs.TTM.Optimised.Profiling
{
  public struct XYZS
  {
    public double X, Y, Z, Station;

    public XYZS(double x, double y, double z, double station)
    {
      X = x;
      Y = y;
      Z = z;
      Station = station;
    }
  }

  public interface IOptimisedTTMProfiler
  {
    List<XYZS> Compute(XYZ startPt, XYZ endPoint);
  }

  /// <summary>
  /// Implements support for computing profile lines across a TIN surface expressed in the
  /// VSS.TRex.Designs.TTM.Optimised schema
  /// </summary>
  public class OptimisedTTMProfiler : IOptimisedTTMProfiler
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<OptimisedTTMProfiler>();

    private readonly ISiteModel SiteModel;
    private readonly IDesign Design;
    private readonly OptimisedSpatialIndexSubGridTree Index;

    /// <summary>
    /// Creates an empty profiler context
    /// </summary>
    public OptimisedTTMProfiler(ISiteModel siteModel,
                                IDesign design,
                                OptimisedSpatialIndexSubGridTree index)
    {
      if (!(design is VSS.TRex.Designs.TTM.Optimised.TrimbleTINModel))
        throw new ArgumentException("Design must be a VSS.TRex.Designs.TTM.Optimised.TrimbleTINModel instance");

      SiteModel = siteModel;
      Design = design;
      Index = index;
    }

    public List<XYZS> Compute(XYZ startPoint, XYZ endPoint)
    {
      // 1. Determine the set of subgrids the profile line cross using the same logic used to
      // compute cell cross by production data profiling

      // ...
      var cellProfileBuilder = new OptimisedTTCellProfileBuilder(SiteModel, true);
      if (!cellProfileBuilder.Build(new [] {startPoint, endPoint}))
        return null;

      // 2. Iterate across each subgrid in turn locating all triangles in that subgrid
      // that intersect the line and sorting them according to the distance of the closest
      // intercept from the start of the line

      // Get the resulting vertical and horizontal intercept list
      var VtHzIntercepts = cellProfileBuilder.VtHzIntercepts;

      // Iterate through the intercepts looking for ones that hit a subgrid in the TTM
      // spatial index that contains triangles

      var intercepts = new List<XYZS>();
      var TTM = (VSS.TRex.Designs.TTM.Optimised.TrimbleTINModel)Design;

      foreach (var intercept in VtHzIntercepts.Items)
      {
        if (!Index.CalculateIndexOfCellContainingPosition(intercept.MidPointX, intercept.MidPointY, out uint cellX, out uint cellY))
        {
          Log.LogWarning($"No cell address computable for location {intercept.MidPointX}:{intercept.MidPointY}");
          continue;
        }

        var subGrid = Index.LocateSubGridContaining(cellX, cellY);

        if (subGrid == null)
        {
          // No triangles are present in this 'node' subgrid
          // Todo: Mark this as a gap in the profile if there are points added to it
        }

        if (!(subGrid is TriangleArrayReferenceSubGrid referenceSubGrid))
        {
          Log.LogCritical($"Subgrid is not a TriangleArrayReferenceSubGrid, is is a {subGrid?.GetType()}");
          continue;
        }

        subGrid.GetSubGridCellIndex(cellX, cellY, out byte subGridX, out byte subGridY);

        TriangleArrayReference referenceList = referenceSubGrid.Items[subGridX, subGridY];

        if (referenceList.Count == 0)
        {
          // There are no triangles in this 'leaf' subgrid
        }

        // Locate all triangles in this subgrid that intersect the profile line

        var endIndex = referenceList.TriangleArrayIndex + referenceList.Count;
        for (int i = referenceList.TriangleArrayIndex; i < endIndex; i++)
        {
          double intersectX, intersectY;
          bool linesAreColinear;

          Triangle tri = TTM.Triangles.Items[i];

          // Does this triangle intersect the line?
          XYZ v0 = TTM.Vertices.Items[tri.Vertex0];
          XYZ v1 = TTM.Vertices.Items[tri.Vertex1];
          XYZ v2 = TTM.Vertices.Items[tri.Vertex2];

          int interceptCount = 0;

          if (LineIntersection.LinesIntersect(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y,
             v0.X, v0.Y, v1.X, v1.Y, out intersectX, out intersectY, true, out linesAreColinear))
          {
            // If the lines are co-linear there is nothing more to do. The two vertices need to be added and no more checking is required
            if (linesAreColinear)
            {
              intercepts.Add(new XYZS(v0.X, v0.Y, v0.Z, MathUtilities.Hypot(startPoint.X - v0.X, startPoint.Y - v0.Y)));
              intercepts.Add(new XYZS(v1.X, v1.Y, v1.Z, MathUtilities.Hypot(startPoint.X - v1.X, startPoint.Y - v1.Y)));
              continue;
            }

            // Otherwise, add the intercept location to the list
            intercepts.Add(new XYZS(intersectX, intersectY, 
              XYZ.GetTriangleHeight(v0, v1, v2, intersectX, intersectY), 
              MathUtilities.Hypot(startPoint.X - intersectX, startPoint.Y - intersectY)));

            interceptCount++;
          }

          if (LineIntersection.LinesIntersect(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y,
            v0.X, v0.Y, v2.X, v2.Y, out intersectX, out intersectY, true, out linesAreColinear))
          {
            // If the lines are co-linear there is nothing more to do. The two vertices need to be added and no more checking is required
            if (linesAreColinear)
            {
              intercepts.Add(new XYZS(v0.X, v0.Y, v0.Z, MathUtilities.Hypot(startPoint.X - v0.X, startPoint.Y - v0.Y)));
              intercepts.Add(new XYZS(v2.X, v2.Y, v2.Z, MathUtilities.Hypot(startPoint.X - v2.X, startPoint.Y - v2.Y)));
              continue;
            }

            // Otherwise, add the intercept location to the list
            intercepts.Add(new XYZS(intersectX, intersectY,
              XYZ.GetTriangleHeight(v0, v1, v2, intersectX, intersectY), 
              MathUtilities.Hypot(startPoint.X - intersectX, startPoint.Y - intersectY)));

            interceptCount++;
          }

          if (interceptCount == 2)
          {
            // No need to check the third edge
            continue;
          }

          if (LineIntersection.LinesIntersect(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y,
            v1.X, v1.Y, v2.X, v2.Y, out intersectX, out intersectY, true, out linesAreColinear))
          {
            // If the lines are co-linear there is nothing more to do. The two vertices need to be added and no more checking is required
            if (linesAreColinear)
            {
              intercepts.Add(new XYZS(v1.X, v1.Y, v1.Z, MathUtilities.Hypot(startPoint.X - v1.X, startPoint.Y - v1.Y)));
              intercepts.Add(new XYZS(v2.X, v2.Y, v2.Z, MathUtilities.Hypot(startPoint.X - v2.X, startPoint.Y - v2.Y)));
              continue;
            }

            // Otherwise, add the intercept location to the list
            intercepts.Add(new XYZS(intersectX, intersectY,
              XYZ.GetTriangleHeight(v0, v1, v2, intersectX, intersectY),
              MathUtilities.Hypot(startPoint.X - intersectX, startPoint.Y - intersectY)));
          }
        }
      }

      // Sort the computed intercepts into station order
      intercepts.Sort((a, b) => a.Station.CompareTo(b.Station)); //x => x.Station);

      // remove any duplicates
      intercepts = intercepts.Where((x, i) => x.Station > intercepts[i].Station).ToList();

      return intercepts;
    }
  }
}
