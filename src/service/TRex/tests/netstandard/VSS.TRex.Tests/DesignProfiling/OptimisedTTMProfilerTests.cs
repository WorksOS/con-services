using System;
using VSS.TRex.Designs.TTM.Optimised.Profiling;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Common.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace VSS.TRex.Tests.DesignProfiling
{
  public class OptimisedTTMProfilerTests : IClassFixture<DILoggingFixture>
  {
    private const double epsilon = 1e-6;

    private readonly ITestOutputHelper output;

    public OptimisedTTMProfilerTests(ITestOutputHelper output)
    {
      this.output = output;
    }

    [Fact]
    public void Test_OptimisedTTMProfiler_Creation()
    {
      var siteModel = new SiteModel(Guid.Empty, 1.0);
      var profiler = new OptimisedTTMProfiler(null, null, null);
    }

    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(123.456)]
    [Theory]
    public void Test_OptimisedTTMProfiler_SingleTriangleAtOrigin_Bisection(double atElevation)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (-100, -100) to (100, 100) to bisect the single triangle 
      var profiler = new OptimisedTTMProfiler(oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new [] {new XYZ(-100, -100), new XYZ(100, 100)});

      Assert.True(profilePoints.Count == 2, $"Profile operation returned {profilePoints.Count} intercepts instead of 2");
      Assert.True(Math.Abs(profilePoints[0].X) < epsilon &&
                  Math.Abs(profilePoints[0].Y) < epsilon &&
                  Math.Abs(profilePoints[0].Z - atElevation) < epsilon, $"First profile point not at origin (0, 0, {atElevation}), but is at {profilePoints[0]}");
      Assert.True(profilePoints[0].TriIndex == 0, "Triangle index not set");

      Assert.True(Math.Abs(profilePoints[1].X - 0.5) < epsilon &&
                  Math.Abs(profilePoints[1].Y - 0.5) < epsilon &&
                  Math.Abs(profilePoints[1].Z - atElevation) < epsilon, $"Second profile point not at origin (0.5, 0.5, {atElevation}), but is at {profilePoints[1]}");
      Assert.True(profilePoints[1].TriIndex == 0, "Triangle index not set");
    }

    [InlineData(0.0)]
    [Theory]
    public void Test_OptimisedTTMProfiler_SingleTriangleAtOrigin_OutsideToInsideWithBisection(double atElevation)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (-100, -100) to (0.25, 0.25) to enter the single triangle at a corner and terminate within it
      var profiler = new OptimisedTTMProfiler(oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new[] { new XYZ(-100, -100), new XYZ(0.25, 0.25)});

      Assert.True(profilePoints.Count == 2, $"Profile operation returned {profilePoints.Count} intercepts instead of 2");
      Assert.True(Math.Abs(profilePoints[0].X) < epsilon &&
                  Math.Abs(profilePoints[0].Y) < epsilon &&
                  Math.Abs(profilePoints[0].Z - atElevation) < epsilon, $"First profile point not at origin (0, 0, {atElevation}), but is at {profilePoints[0]}");

      Assert.True(Math.Abs(profilePoints[1].X - 0.25) < epsilon &&
                  Math.Abs(profilePoints[1].Y - 0.25) < epsilon &&
                  Math.Abs(profilePoints[1].Z - atElevation) < epsilon, $"Second profile point not at origin (0.25, 0.25, {atElevation}), but is at {profilePoints[1]}");
    }

    [InlineData(0.0)]
    [Theory]
    public void Test_OptimisedTTMProfiler_SingleTriangleAtOrigin_MultipleProfileSegments(double atElevation)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (-100, -100) to (0.25, 0.25) to (100, 100) to enter the single triangle at a corner
      // stop within it then exit via the diagonal face
      var profiler = new OptimisedTTMProfiler(oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new[] { new XYZ(-100, -100), new XYZ(0.25, 0.25), new XYZ(100, 100)});

      Assert.True(profilePoints.Count == 3, $"Profile operation returned {profilePoints.Count} intercepts instead of 3");
      Assert.True(Math.Abs(profilePoints[0].X) < epsilon &&
                  Math.Abs(profilePoints[0].Y) < epsilon &&
                  Math.Abs(profilePoints[0].Z - atElevation) < epsilon, $"First profile point not at origin (0, 0, {atElevation}), but is at {profilePoints[0]}");

      Assert.True(Math.Abs(profilePoints[1].X - 0.25) < epsilon &&
                  Math.Abs(profilePoints[1].Y - 0.25) < epsilon &&
                  Math.Abs(profilePoints[1].Z - atElevation) < epsilon, $"Second profile point not at origin (0.25, 0.25, {atElevation}), but is at {profilePoints[1]}");

      Assert.True(Math.Abs(profilePoints[2].X - 0.5) < epsilon &&
                  Math.Abs(profilePoints[2].Y - 0.5) < epsilon &&
                  Math.Abs(profilePoints[2].Z - atElevation) < epsilon, $"Second profile point not at origin (0.5, 0.5, {atElevation}), but is at {profilePoints[2]}");
    }

    [InlineData(0.0)]
    [Theory]
    public void Test_OptimisedTTMProfiler_SingleTriangleAtOrigin_InsideToOutsideWithBisection(double atElevation)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (0.25, 0.25) to (-100, -100) to exit the single triangle at a corner and terminate outside it
      var profiler = new OptimisedTTMProfiler(oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new[] { new XYZ(0.25, 0.25), new XYZ(-100, -100)});

      Assert.True(profilePoints.Count == 2, $"Profile operation returned {profilePoints.Count} intercepts instead of 2");
      Assert.True(Math.Abs(profilePoints[1].X) < epsilon &&
                  Math.Abs(profilePoints[1].Y) < epsilon &&
                  Math.Abs(profilePoints[1].Z - atElevation) < epsilon, $"First profile point not at origin (0, 0, {atElevation}), but is at {profilePoints[0]}");

      Assert.True(Math.Abs(profilePoints[0].X - 0.25) < epsilon &&
                  Math.Abs(profilePoints[0].Y - 0.25) < epsilon &&
                  Math.Abs(profilePoints[0].Z - atElevation) < epsilon, $"Second profile point not at origin (0.25, 0.25, {atElevation}), but is at {profilePoints[1]}");
    }

    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [Theory]
    public void Test_OptimisedTTMProfiler_SingleTriangleAtOrigin_Bisection_ManyTimes(int runCount)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(0.0);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (-100, -100) to (100, 100) to bisect the single triangle 
      var profiler = new OptimisedTTMProfiler(oneTriangleModel, index, indices);

      DateTime startTime = DateTime.Now;
      for (int i = 0; i < runCount; i++)
      {
        var profilePoints = profiler.Compute(new[] { new XYZ(-100, -100), new XYZ(100, 100)});
        Assert.True(profilePoints.Count > 0);
      }

      output.WriteLine($"Times to run profile {runCount} times: {DateTime.Now - startTime}");

      Assert.True(true);
    }

    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(123.456)]
    [Theory]
    public void Test_OptimisedTTMProfiler_TwoTrianglesAtOrigin(double atElevation)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithTwoFlatUnitTrianglesAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (-100, -100) to (100, 100) to bisect the triangles
      var profiler = new OptimisedTTMProfiler(oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new[] { new XYZ(-100, -100), new XYZ(100, 100)});

      Assert.True(profilePoints.Count == 3, $"Profile operation returned {profilePoints.Count} intercepts instead of 3");

      Assert.True(Math.Abs(profilePoints[0].X) < epsilon &&
                  Math.Abs(profilePoints[0].Y) < epsilon &&
                  Math.Abs(profilePoints[0].Z - atElevation) < epsilon, $"First profile point not at (0, 0, {atElevation}), but is at {profilePoints[0]}");
      Assert.True(profilePoints[0].TriIndex == 0, "Triangle index not set");

      Assert.True(Math.Abs(profilePoints[1].X - 0.5) < epsilon &&
                  Math.Abs(profilePoints[1].Y - 0.5) < epsilon &&
                  Math.Abs(profilePoints[1].Z - atElevation) < epsilon, $"Second profile point not at (0.5, 0.5, {atElevation}), but is at {profilePoints[1]}");
      Assert.True(profilePoints[1].TriIndex == 0, "Triangle index not set");

      Assert.True(Math.Abs(profilePoints[2].X - 1.0) < epsilon &&
                  Math.Abs(profilePoints[2].Y - 1.0) < epsilon &&
                  Math.Abs(profilePoints[2].Z - atElevation) < epsilon, $"Second profile point not at (1.0, 1.0, {atElevation}), but is at {profilePoints[2]}");
      Assert.True(profilePoints[2].TriIndex == 1, "Triangle index not set");
    }

    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [Theory]
    public void Test_OptimisedTTMProfiler_TwoTrianglesAtOrigin_ManyTimes(int runCount)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithTwoFlatUnitTrianglesAtOrigin(0.0);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (-100, -100) to (100, 100) to bisect the single triangle 
      var profiler = new OptimisedTTMProfiler(oneTriangleModel, index, indices);

      DateTime startTime = DateTime.Now;
      for (int i = 0; i < runCount; i++)
      {
        var profilePoints = profiler.Compute(new[] { new XYZ(-100, -100), new XYZ(100, 100)});
        Assert.True(profilePoints.Count > 0);
      }

      output.WriteLine($"Times to run profile {runCount} times: {DateTime.Now - startTime}");

      Assert.True(true);
    }

    [InlineData(0.0)]
    [InlineData(123.456)]
    [Theory]
    public void Test_OptimisedTTMProfiler_SingleTriangleAtOrigin_YAxisColinearProfileLine(double atElevation)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (0, -100) to (0, 100) to be co-linear with vertical edge of triangle
      var profiler = new OptimisedTTMProfiler(oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new[] { new XYZ(0, -100), new XYZ(0, 100)});

      Assert.True(profilePoints.Count == 2, $"Profile operation returned {profilePoints.Count} intercepts instead of 2");
      Assert.True(Math.Abs(profilePoints[0].X) < epsilon &&
                  Math.Abs(profilePoints[0].Y) < epsilon &&
                  Math.Abs(profilePoints[0].Z - atElevation) < epsilon, $"First profile point not at (0, 0, {atElevation}), but is at {profilePoints[0]}");

      Assert.True(Math.Abs(profilePoints[1].X) < epsilon &&
                  Math.Abs(profilePoints[1].Y - 1.0) < epsilon &&
                  Math.Abs(profilePoints[1].Z - atElevation) < epsilon, $"Second profile point not at 0, 1.0, {atElevation}), but is at {profilePoints[1]}");
    }

    [InlineData(0.0)]
    [InlineData(123.456)]
    [Theory]
    public void Test_OptimisedTTMProfiler_SingleTriangleAtOrigin_XAxisColinearProfileLine(double atElevation)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (-100, 0) to (100, 0) to be co-linear with horizontal edge of triangle
      var profiler = new OptimisedTTMProfiler(oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new[] { new XYZ(-100, 0), new XYZ(100, 0)});

      Assert.True(profilePoints.Count == 2, $"Profile operation returned {profilePoints.Count} intercepts instead of 2");
      Assert.True(Math.Abs(profilePoints[0].X) < epsilon &&
                  Math.Abs(profilePoints[0].Y) < epsilon &&
                  Math.Abs(profilePoints[0].Z - atElevation) < epsilon, $"First profile point not at (0, 0, {atElevation}), but is at {profilePoints[0]}");

      Assert.True(Math.Abs(profilePoints[1].X - 1.0) < epsilon &&
                  Math.Abs(profilePoints[1].Y) < epsilon &&
                  Math.Abs(profilePoints[1].Z - atElevation) < epsilon, $"Second profile point not at (1.0, 0, {atElevation}), but is at {profilePoints[1]}");
    }


    [InlineData(0.0)]
    [InlineData(123.456)]
    [Theory]
    public void Test_OptimisedTTMProfiler_SingleTriangleAtOrigin_DiagonalColinearProfileLine(double atElevation)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (-1, 2) to (2, -1) to be co-linear with diagonal edge of triangle
      var profiler = new OptimisedTTMProfiler(oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new[] { new XYZ(-1, 2), new XYZ(2, -1)});

      Assert.True(profilePoints.Count == 2, $"Profile operation returned {profilePoints.Count} intercepts instead of 2");
      Assert.True(Math.Abs(profilePoints[0].X) < epsilon &&
                  Math.Abs(profilePoints[0].Y - 1.0) < epsilon &&
                  Math.Abs(profilePoints[0].Z - atElevation) < epsilon, $"First profile point not at (0, 1.0, {atElevation}), but is at {profilePoints[0]}");

      Assert.True(Math.Abs(profilePoints[1].X - 1.0) < epsilon &&
                  Math.Abs(profilePoints[1].Y) < epsilon &&
                  Math.Abs(profilePoints[1].Z - atElevation) < epsilon, $"Second profile point not at (1.0, 0, {atElevation}), but is at {profilePoints[1]}");
    }

    [InlineData(0.0)]
    [InlineData(123.456)]
    [Theory]
    public void Test_OptimisedTTMProfiler_TwoTrianglesAtOrigin_DiagonalColinearProfileLine(double atElevation)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (-1, 2) to (2, -1) to be co-linear with diagonal edge of triangle
      var profiler = new OptimisedTTMProfiler(oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new[] { new XYZ(-1, 2), new XYZ(2, -1)});

      Assert.True(profilePoints.Count == 2, $"Profile operation returned {profilePoints.Count} intercepts instead of 2");
      Assert.True(Math.Abs(profilePoints[0].X) < epsilon &&
                  Math.Abs(profilePoints[0].Y - 1.0) < epsilon &&
                  Math.Abs(profilePoints[0].Z - atElevation) < epsilon, $"First profile point not at (0, 1.0, {atElevation}), but is at {profilePoints[0]}");

      Assert.True(Math.Abs(profilePoints[1].X - 1.0) < epsilon &&
                  Math.Abs(profilePoints[1].Y) < epsilon &&
                  Math.Abs(profilePoints[1].Z - atElevation) < epsilon, $"Second profile point not at (1.0, 0, {atElevation}), but is at {profilePoints[1]}");
    }

    [InlineData(0.0)]
    [Theory]
    public void Test_OptimisedTTMProfiler_TwoTrianglesWitGapAtOrigin_DiagonalProfileLine(double atElevation)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithTwoFlatUnitTrianglesWithGapAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (-100, -100) to (100, 100) to bisect the two triangles with a gap
      // between (0.5, 0.5) and (1.0, 1.0)
      var profiler = new OptimisedTTMProfiler(oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new[] { new XYZ(-100, -100), new XYZ(100, 100) });

      Assert.True(profilePoints.Count == 5, $"Profile operation returned {profilePoints.Count} intercepts instead of 5");
      Assert.True(Math.Abs(profilePoints[0].X) < epsilon &&
                  Math.Abs(profilePoints[0].Y) < epsilon &&
                  Math.Abs(profilePoints[0].Z - atElevation) < epsilon, $"First profile point not at (0, 0, {atElevation}), but is at {profilePoints[0]}");

      Assert.True(Math.Abs(profilePoints[1].X - 0.5) < epsilon &&
                  Math.Abs(profilePoints[1].Y - 0.5) < epsilon &&
                  Math.Abs(profilePoints[1].Z - atElevation) < epsilon, $"Second profile point not at (0.5, 0.5, {atElevation}), but is at {profilePoints[1]}");

      Assert.True(Math.Abs(profilePoints[2].X - 1.0) < epsilon &&
                  Math.Abs(profilePoints[2].Y - 1.0) < epsilon &&
                  profilePoints[2].Z == Common.Consts.NullDouble, $"Gap profile point not at (1.0, 1.0, {atElevation}), but is at {profilePoints[2]}");

      Assert.True(Math.Abs(profilePoints[3].X - 1.0) < epsilon &&
                  Math.Abs(profilePoints[3].Y - 1.0) < epsilon &&
                  Math.Abs(profilePoints[3].Z - atElevation) < epsilon, $"Third profile point not at (1.0, 1.0, {atElevation}), but is at {profilePoints[3]}");

      Assert.True(Math.Abs(profilePoints[4].X - 1.5) < epsilon &&
                  Math.Abs(profilePoints[4].Y - 1.5) < epsilon &&
                  Math.Abs(profilePoints[4].Z - atElevation) < epsilon, $"Fourth profile point not at (1.5, 1.5, {atElevation}), but is at {profilePoints[4]}");
    }

    [InlineData(0.0, -100, -100, 100, 100, 63)]
    [InlineData(123.456, -100, -100, 100, 100, 63)]
    [InlineData(0, -100, 0.5, 100, 0.5, 63)]
    [InlineData(123.456, -100, 0.5, 100, 0.5, 63)]
    [InlineData(0, 0.5, -100, 0.5, 100, 63)]
    [InlineData(123.456, 0.5, -100, 0.5, 100, 63)]
    [Theory]
    public void Test_OptimisedTTMProfiler_1024TrianglesAtOrigin(double atElevation, double startX, double startY, double endX, double endY, int expectedPointCount)
    {
      var startTime = DateTime.Now;

      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_With32x32FlatTrianglesAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (startX, startX) to (endX, endY) to cross the patch of triangles 
      var profiler = new OptimisedTTMProfiler(oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new[] { new XYZ(startX, startY), new XYZ(endX, endY)});

      output.WriteLine($"Total time to perform profile: {DateTime.Now - startTime}");

      Assert.True(profilePoints.Count == expectedPointCount, $"Profile operation returned {profilePoints.Count} intercepts instead of {expectedPointCount}");

      foreach (var pt in profilePoints)
      {
        Assert.True(Math.Abs(pt.Z - atElevation) < epsilon, $"Elevation {pt.Z} incorrect, should be {atElevation}");
        Assert.True(Math.Abs(pt.Station - MathUtilities.Hypot(startX - pt.X, startY - pt.Y)) < epsilon, $"Station {pt.Station} incorrect, should be {MathUtilities.Hypot(startX - pt.X, startY - pt.Y)}");
      }
    }
  }
}
