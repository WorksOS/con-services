﻿using VSS.TRex.Designs;
using VSS.TRex.Geometry;
using Triangle = VSS.TRex.Designs.TTM.Optimised.Triangle;
using TrimbleTINModel = VSS.TRex.Designs.TTM.Optimised.TrimbleTINModel;

namespace VSS.TRex.Tests.DesignProfiling
{
  /// <summary>
  /// Builds a known optimized TTM model for use in tests
  /// </summary>
  public static class OptimisedTTMDesignBuilder
  {
    public static TrimbleTINModel CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(double withElevation)
    {
      var TTM = new TrimbleTINModel
      {
        Vertices =
        {
          Items = new [] {new XYZ(0, 0, withElevation), new XYZ(1, 0, withElevation), new XYZ(0, 1, withElevation) }
        },
        Triangles =
        {
          Items = new [] {new Triangle(0, 1, 2)}
        }
      };

      return TTM;
    }

    public static TrimbleTINModel CreateOptimisedTTM_WithTwoFlatUnitTrianglesAtOrigin(double withElevation)
    {
      var TTM = new TrimbleTINModel
      {
        Vertices =
        {
          Items = new [] {new XYZ(0, 0, withElevation), new XYZ(1, 0, withElevation), new XYZ(0, 1, withElevation), new XYZ(1, 1, withElevation) }
        },

        Triangles =
        {
          Items = new [] {new Triangle(0, 1, 2), new Triangle(1, 2, 3)}
        }
      };

      return TTM;
    }

    public static bool CreateOptimisedIndexForModel(VSS.TRex.Designs.TTM.Optimised.TrimbleTINModel ttm,
      out OptimisedSpatialIndexSubGridTree tree,
      out int[] triangleIndices)
    {
      const double CellSize = 1.0;

      tree = null;
      triangleIndices = null;

      var indexBuilder = new OptimisedTTMSpatialIndexBuilder(ttm, CellSize);
      if (indexBuilder.ConstructSpatialIndex())
      {
        tree = indexBuilder.SpatialIndexOptimised;
        triangleIndices = indexBuilder.SpatialIndexOptimisedTriangles;

        return true;
      }

      return false;

/*      // Create a mutable TIN model, convert the optimised mode into it, then extract the optimized index from it
      var mutableTTM = new VSS.TRex.Designs.TTM.TrimbleTINModel();

      mutableTTM.Vertices.InitPointSearch(ttm.Vertices.Items.Min(x => x.X) - 1, ttm.Vertices.Items.Min(x => x.Y) - 1,
                                          ttm.Vertices.Items.Max(x => x.X) + 1, ttm.Vertices.Items.Max(x => x.Y) + 1,
                                          ttm.Vertices.Items.Length);

      foreach (var tri in ttm.Triangles.Items) 
      {
        mutableTTM.Triangles.AddTriangle(
          new TriVertex(ttm.Vertices.Items[tri.Vertex0]), 
          new TriVertex(ttm.Vertices.Items[tri.Vertex1]), 
          new TriVertex(ttm.Vertices.Items[tri.Vertex2]));
      }

      var SpatialIndexOptimised = new OptimisedSpatialIndexSubGridTree(SubGridTreeConsts.SubGridTreeLevels - 1, SubGridTreeConsts.SubGridTreeDimension * CellSize);
*/
    }
  }
}
