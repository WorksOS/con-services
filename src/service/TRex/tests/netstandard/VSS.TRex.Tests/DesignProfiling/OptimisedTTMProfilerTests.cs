using System;
using VSS.TRex.Designs.TTM.Optimised.Profiling;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels;
using VSS.TRex.Tests.TestFixtures;
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
      var profiler = new OptimisedTTMProfiler(siteModel, null, null);

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
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index);
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
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index);

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
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index);
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
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index);

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
    public void Test_OptimisedTTMProfiler_SingleTriangleAtOrigin_YAxisProfileLine(double atElevation)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (0, -100) to (0, 100) to be co-linear with vertical edge of triangle
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index);
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
    public void Test_OptimisedTTMProfiler_SingleTriangleAtOrigin_XAxisProfileLine(double atElevation)
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin(atElevation);
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (-100, 0) to (100, 0) to be co-linear with vertical edge of triangle
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index);
      var profilePoints = profiler.Compute(new XYZ(-100, 0), new XYZ(100, 0));

      Assert.True(profilePoints.Count == 2, $"Profile operation returned {profilePoints.Count} intercepts instead of 2");
      Assert.True(Math.Abs(profilePoints[0].X) < epsilon &&
                  Math.Abs(profilePoints[0].Y) < epsilon &&
                  Math.Abs(profilePoints[0].Z - atElevation) < epsilon, $"First profile point not at (0, 0, {atElevation}), but is at {profilePoints[0]}");

      Assert.True(Math.Abs(profilePoints[1].X - 1.0) < epsilon &&
                  Math.Abs(profilePoints[1].Y) < epsilon &&
                  Math.Abs(profilePoints[1].Z - atElevation) < epsilon, $"Second profile point not at (1.0, 0, {atElevation}), but is at {profilePoints[1]}");
    }
  }
}
