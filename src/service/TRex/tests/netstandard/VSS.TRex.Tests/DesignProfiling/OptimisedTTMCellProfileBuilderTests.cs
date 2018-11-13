using VSS.TRex.Designs.TTM.Optimised.Profiling;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.DesignProfiling
{
  public class OptimisedTTMCellProfileBuilderTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_OptimisedTTMCellProfileBuilder_Creation()
    {
      var builder = new OptimisedTTMCellProfileBuilder(1.0, false);
    }

    /// <summary>
    /// Tests the testing helper tool does the right thing!
    /// </summary>
    [Fact]
    public void Test_OptimisedTTMDesignBuilder_OneTriangle()
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithOneTriangleAtOrigin();

      Assert.True(oneTriangleModel.Vertices.Items.Length == 3, "Invalid number of vertices for single triangle model");
      Assert.True(oneTriangleModel.Triangles.Items.Length == 1, "Invalid number of triangles for single triangle model");

      OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var tree, out var indices);

      Assert.NotNull(tree);
      Assert.NotNull(indices);

      Assert.True(indices.Length == 1, $"Number of indices [{indices.Length}] incorrect, should be 1");
    }

    [Fact]
    public void Test_ProfilerBuilder_OneTriangle()
    {
      // Create a model with a single triangle at (0, ), (0, 10), (10, 0)
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithOneTriangleAtOrigin();
      OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var tree, out var indices);

      var builder = new OptimisedTTMCellProfileBuilder(1.0, false);

      // Build a profile line from (-100, -100) to (100, 100) to bisect the single triangle 
      var result = builder.Build(new XYZ[] {new XYZ(-100, -100), new XYZ(100, 100)});

      Assert.True(result, "Build() failed");

      Assert.True(builder.VtHzIntercepts.Count == 1, $"Intercept count [{builder.VtHzIntercepts.Count}] wrong, expected 1");
    }
  }
}
