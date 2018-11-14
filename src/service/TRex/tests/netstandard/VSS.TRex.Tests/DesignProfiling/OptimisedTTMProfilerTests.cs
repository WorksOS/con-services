using System;
using VSS.TRex.Designs.TTM.Optimised.Profiling;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Utilities;
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
      var profiler = new OptimisedTTMProfiler(siteModel, null, null, null);

      Assert.True(profiler.SiteModel == siteModel, "SiteModel not set in profiler");
    }

    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(123.456)]
    [Theory]
    public void Test_OptimisedTTMProfiler_SingleTriangleAtOrigin(double atElevation)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (-100, -100) to (100, 100) to bisect the single triangle 
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new XYZ(-100, -100), new XYZ(100, 100));

      Assert.True(profilePoints.Count == 2, $"Profile operation returned {profilePoints.Count} intercepts instead of 2");
      Assert.True(Math.Abs(profilePoints[0].X) < epsilon &&
                  Math.Abs(profilePoints[0].Y) < epsilon &&
                  Math.Abs(profilePoints[0].Z - atElevation) < epsilon, $"First profile point not at origin (0, 0, {atElevation}), but is at {profilePoints[0]}");

      Assert.True(Math.Abs(profilePoints[1].X - 0.5) < epsilon &&
                  Math.Abs(profilePoints[1].Y - 0.5) < epsilon &&
                  Math.Abs(profilePoints[1].Z - atElevation) < epsilon, $"Second profile point not at origin (0.5, 0.5, {atElevation}), but is at {profilePoints[1]}");
    }

    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [Theory]
    public void Test_OptimisedTTMProfiler_SingleTriangleAtOrigin_ManyTimes(int runCount)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(0.0);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (-100, -100) to (100, 100) to bisect the single triangle 
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index, indices);

      DateTime startTime = DateTime.Now;
      for (int i = 0; i < runCount; i++)
      {
        var profilePoints = profiler.Compute(new XYZ(-100, -100), new XYZ(100, 100));
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
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new XYZ(-100, -100), new XYZ(100, 100));

      Assert.True(profilePoints.Count == 3, $"Profile operation returned {profilePoints.Count} intercepts instead of 3");

      Assert.True(Math.Abs(profilePoints[0].X) < epsilon &&
                  Math.Abs(profilePoints[0].Y) < epsilon &&
                  Math.Abs(profilePoints[0].Z - atElevation) < epsilon, $"First profile point not at origin (0, 0, {atElevation}), but is at {profilePoints[0]}");

      Assert.True(Math.Abs(profilePoints[1].X - 0.5) < epsilon &&
                  Math.Abs(profilePoints[1].Y - 0.5) < epsilon &&
                  Math.Abs(profilePoints[1].Z - atElevation) < epsilon, $"Second profile point not at origin (0.5, 0.5, {atElevation}), but is at {profilePoints[1]}");

      Assert.True(Math.Abs(profilePoints[2].X - 1.0) < epsilon &&
                  Math.Abs(profilePoints[2].Y - 1.0) < epsilon &&
                  Math.Abs(profilePoints[2].Z - atElevation) < epsilon, $"Second profile point not at unit point (1.0, 1.0, {atElevation}), but is at {profilePoints[2]}");
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
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index, indices);

      DateTime startTime = DateTime.Now;
      for (int i = 0; i < runCount; i++)
      {
        var profilePoints = profiler.Compute(new XYZ(-100, -100), new XYZ(100, 100));
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
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new XYZ(0, -100), new XYZ(0, 100));

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
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new XYZ(-100, 0), new XYZ(100, 0));

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
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new XYZ(-1, 2), new XYZ(2, -1));

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
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new XYZ(-1, 2), new XYZ(2, -1));

      Assert.True(profilePoints.Count == 2, $"Profile operation returned {profilePoints.Count} intercepts instead of 2");
      Assert.True(Math.Abs(profilePoints[0].X) < epsilon &&
                  Math.Abs(profilePoints[0].Y - 1.0) < epsilon &&
                  Math.Abs(profilePoints[0].Z - atElevation) < epsilon, $"First profile point not at (0, 1.0, {atElevation}), but is at {profilePoints[0]}");

      Assert.True(Math.Abs(profilePoints[1].X - 1.0) < epsilon &&
                  Math.Abs(profilePoints[1].Y) < epsilon &&
                  Math.Abs(profilePoints[1].Z - atElevation) < epsilon, $"Second profile point not at (1.0, 0, {atElevation}), but is at {profilePoints[1]}");
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
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_With32x32FlatTrianglesAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (startX, startX) to (endX, endY) to cross the patch of triangles 
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index, indices);
      var profilePoints = profiler.Compute(new XYZ(startX, startY), new XYZ(endX, endY));

      Assert.True(profilePoints.Count == expectedPointCount, $"Profile operation returned {profilePoints.Count} intercepts instead of {expectedPointCount}");

      foreach (var pt in profilePoints)
      {
        Assert.True(Math.Abs(pt.Z - atElevation) < epsilon, $"Elevation {pt.Z} incorrect, should be {atElevation}");
        Assert.True(Math.Abs(pt.Station - MathUtilities.Hypot(startX - pt.X, startY - pt.Y)) < epsilon, $"Station {pt.Station} incorrect, should be {MathUtilities.Hypot(startX - pt.X, startY - pt.Y)}");
      }
    }
  }
}
