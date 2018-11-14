using System.Linq;
using VSS.TRex.Designs;
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

    public static TrimbleTINModel CreateOptimisedTTM_With32x32FlatTrianglesAtOrigin(double withElevation)
    {
      // Create a mesh of triangles (32 x 32) with it's bottom-left corner on the origin
      var TTM = new TrimbleTINModel
      {
        Vertices =
        {
          Items = Enumerable.Range(0, 1024).Select(x => new XYZ(x % 32, (int)(x / 32), withElevation)).ToArray()
        },

        Triangles = {
          Items = Enumerable.Range(0, 2 * 31 * 31).Select(x => new Triangle(0, 0, 0)).ToArray()
        }
      };

      int TriIndex = 0;
      for (int i = 0; i < 31; i++)
      {
        int baseIndex = i * 32;

        for (int j = 0; j < 31; j++)
        {
          TTM.Triangles.Items[TriIndex++] = new Triangle(baseIndex + j, baseIndex + j + 1, baseIndex + j + 32);
          TTM.Triangles.Items[TriIndex++] = new Triangle(baseIndex + j + 32, baseIndex + j + 33, baseIndex + j + 1);
        }
      }

      // ConvertOptimisedTTMToMutableTTM(TTM, $@"c:\temp\ThousandTriMutableTestTIN-{DateTime.Now.Ticks}.ttm");

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
    }

    public static void ConvertOptimisedTTMToMutableTTM(VSS.TRex.Designs.TTM.Optimised.TrimbleTINModel ttm, string fileName)
    {
      // Create a mutable TIN model, convert the optimized model into it, then extract the optimized index from it
      var mutableTTM = new VSS.TRex.Designs.TTM.TrimbleTINModel();

      mutableTTM.Vertices.InitPointSearch(ttm.Vertices.Items.Min(x => x.X) - 1, ttm.Vertices.Items.Min(x => x.Y) - 1,
                                          ttm.Vertices.Items.Max(x => x.X) + 1, ttm.Vertices.Items.Max(x => x.Y) + 1,
                                          ttm.Vertices.Items.Length);

      // Create the vertices
      foreach (var v in ttm.Vertices.Items)
        mutableTTM.Vertices.AddPoint(v.X, v.Y, v.Z);

      // Create the triangles
      foreach (var tri in ttm.Triangles.Items) 
      {
        mutableTTM.Triangles.AddTriangle(
          mutableTTM.Vertices[tri.Vertex0], 
          mutableTTM.Vertices[tri.Vertex1], 
          mutableTTM.Vertices[tri.Vertex2]);
      }

      mutableTTM.SaveToFile(fileName, true);
    }
  }
}
