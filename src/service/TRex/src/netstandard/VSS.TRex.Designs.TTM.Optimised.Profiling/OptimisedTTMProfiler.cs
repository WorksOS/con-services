using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Profiling;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Utilities;

namespace VSS.TRex.Designs.TTM.Optimised.Profiling
{
  /// <summary>
  /// Implements support for computing profile lines across a TIN surface expressed in the
  /// VSS.TRex.Designs.TTM.Optimised schema
  /// </summary>
  public class OptimisedTTMProfiler : IOptimisedTTMProfiler
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<OptimisedTTMProfiler>();

    public ISiteModel SiteModel { get; private set; }

//    private readonly IDesign Design;
    private readonly TrimbleTINModel TTM;

    private readonly OptimisedSpatialIndexSubGridTree Index;

    /// <summary>
    /// Creates an empty profiler context
    /// </summary>
    public OptimisedTTMProfiler(ISiteModel siteModel,
                                TrimbleTINModel ttm, // IDesign design,
                                OptimisedSpatialIndexSubGridTree index)
    {
//      if (!(design is VSS.TRex.Designs.TTM.Optimised.TrimbleTINModel))
//        throw new ArgumentException("Design must be a VSS.TRex.Designs.TTM.Optimised.TrimbleTINModel instance");

      SiteModel = siteModel;
   //   Design = design;
      TTM = ttm;
      Index = index;
    }

    private void AddEndIntercept(XYZ point, List<XYZS> intercepts, double station)
    {
      if (!Index.CalculateIndexOfCellContainingPosition(point.X, point.Y, out uint cellX, out uint cellY))
      {
        Log.LogWarning($"No cell address computable for end point location {point.X}:{point.Y}");
        return;
      }

      var subGrid = Index.LocateSubGridContaining(cellX, cellY);

      if (subGrid == null) // No triangles in this 'node' subgrid
        return;

      if (!(subGrid is TriangleArrayReferenceSubGrid referenceSubGrid))
      {
        Log.LogCritical($"Subgrid is not a TriangleArrayReferenceSubGrid, is is a {subGrid?.GetType()}");
        return;
      }

      // Get the cell representing a leaf subgrid and determine if there is a triangle at the point location
      subGrid.GetSubGridCellIndex(cellX, cellY, out byte subGridX, out byte subGridY);

      TriangleArrayReference referenceList = referenceSubGrid.Items[subGridX, subGridY];

      int endIndex = referenceList.TriangleArrayIndex + referenceList.Count;

      for (int i = referenceList.TriangleArrayIndex; i < endIndex; i++)
      {
        double height = XYZ.GetTriangleHeight
         (TTM.Vertices.Items[TTM.Triangles.Items[i].Vertex0],
          TTM.Vertices.Items[TTM.Triangles.Items[i].Vertex1],
          TTM.Vertices.Items[TTM.Triangles.Items[i].Vertex2], point.X, point.Y);

        if (height != Common.Consts.NullDouble)
        {
          intercepts.Add(new XYZS(point.X, point.Y, height, station));
          return;
        }
      }
    }

    public List<XYZS> Compute(XYZ startPoint, XYZ endPoint)
    {
      // 1. Determine the set of subgrids the profile line cross using the same logic used to
      // compute cell cross by production data profiling

      // ...
      var cellProfileBuilder = new OptimisedTTMCellProfileBuilder(SiteModel.Grid.CellSize, true);
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

      // Add an initial intercept if the start point is located within a triangle
      AddEndIntercept(startPoint, intercepts, 0);

      for (int interceptIndex = 0; interceptIndex < VtHzIntercepts.Count; interceptIndex++)
      {
        InterceptRec intercept = VtHzIntercepts.Items[interceptIndex];

        if (!Index.CalculateIndexOfCellContainingPosition(intercept.MidPointX, intercept.MidPointY, out uint cellX, out uint cellY))
        {
          Log.LogWarning($"No cell address computable for location {intercept.MidPointX}:{intercept.MidPointY}");
          continue;
        }

        var subGrid = Index.LocateSubGridContaining(cellX, cellY);

        if (subGrid == null)
        {
          // No triangles are present in this 'node' subgrid
          // Mark this as a gap in the profile if there are points added to it, but not if there is already
          // a gap marker in the intercept list
          if (intercepts.Count > 0 && intercepts[intercepts.Count - 1].Z == Common.Consts.NullDouble)
          {
            intercepts.Add(new XYZS(intercepts[intercepts.Count - 1])
            { 
              Z = Common.Consts.NullDouble,
              Station = intercepts[intercepts.Count - 1].Station + 0.0000001
            });
          }

          continue;
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
          Triangle tri = TTM.Triangles.Items[i];

          // Does this triangle intersect the line?
          XYZ v0 = TTM.Vertices.Items[tri.Vertex0];
          XYZ v1 = TTM.Vertices.Items[tri.Vertex1];
          XYZ v2 = TTM.Vertices.Items[tri.Vertex2];

          if (LineIntersection.LinesIntersect(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, v0.X, v0.Y, v1.X, v1.Y, out double intersectX, out double intersectY, true, out bool linesAreColinear))
          {
            // If the lines are co-linear there is nothing more to do. The two vertices need to be added and no more checking is required
            if (linesAreColinear)
            {
              intercepts.Add(new XYZS(v0.X, v0.Y, v0.Z, MathUtilities.Hypot(startPoint.X - v0.X, startPoint.Y - v0.Y)));
              intercepts.Add(new XYZS(v1.X, v1.Y, v1.Z, MathUtilities.Hypot(startPoint.X - v1.X, startPoint.Y - v1.Y)));
              continue;
            }

            // Otherwise, add the intercept location to the list
            intercepts.Add(new XYZS(intersectX, intersectY, XYZ.GetTriangleHeight(v0, v1, v2, intersectX, intersectY), MathUtilities.Hypot(startPoint.X - intersectX, startPoint.Y - intersectY)));
          }

          if (LineIntersection.LinesIntersect(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, v0.X, v0.Y, v2.X, v2.Y, out intersectX, out intersectY, true, out linesAreColinear))
          {
            // If the lines are co-linear there is nothing more to do. The two vertices need to be added and no more checking is required
            if (linesAreColinear)
            {
              intercepts.Add(new XYZS(v0.X, v0.Y, v0.Z, MathUtilities.Hypot(startPoint.X - v0.X, startPoint.Y - v0.Y)));
              intercepts.Add(new XYZS(v2.X, v2.Y, v2.Z, MathUtilities.Hypot(startPoint.X - v2.X, startPoint.Y - v2.Y)));
              continue;
            }

            // Otherwise, add the intercept location to the list
            intercepts.Add(new XYZS(intersectX, intersectY, XYZ.GetTriangleHeight(v0, v1, v2, intersectX, intersectY), MathUtilities.Hypot(startPoint.X - intersectX, startPoint.Y - intersectY)));
          }

          if (LineIntersection.LinesIntersect(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, v1.X, v1.Y, v2.X, v2.Y, out intersectX, out intersectY, true, out linesAreColinear))
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

      // Add an initial intercept if the start point is located within a triangle
      AddEndIntercept(endPoint, intercepts, MathUtilities.Hypot(startPoint.X - endPoint.X, startPoint.Y - endPoint.Y));

      // Sort the computed intercepts into station order
      intercepts.Sort((a, b) => a.Station.CompareTo(b.Station)); 

      // remove any duplicates. todo: Determine is this is more efficient to do once all subgrids of triangle intercept are aggregated
      intercepts = intercepts.Where((x, i) => (i == 0) || x.Station > intercepts[i - 1].Station).ToList();

      return intercepts;
    }
  }
}
