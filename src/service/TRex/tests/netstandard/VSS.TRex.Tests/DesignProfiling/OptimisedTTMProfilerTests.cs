using System;
using VSS.TRex.Designs.TTM.Optimised.Profiling;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.DesignProfiling
{
  public class OptimisedTTMProfilerTests : IClassFixture<DILoggingFixture>
  {
    private const double epsilon = 1e-6;

    [Fact]
    public void Test_OptimisedTTMProfiler_Creation()
    {
      var siteModel = new SiteModel(Guid.Empty, 1.0);
      var profiler = new OptimisedTTMProfiler(siteModel, null, null);

      Assert.True(profiler.SiteModel == siteModel, "SiteModel not set in profiler");
    }

    [Fact]
    public void Test_OptimisedTTMProfiler_SingleTriangleAtOrigin()
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithFlatUnitTriangleAtOrigin();
      Assert.True(OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices));

      // Build a profile line from (-100, -100) to (100, 100) to bisect the single triangle 
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index);
      var profilePoints = profiler.Compute(new XYZ(-100, -100), new XYZ(100, 100));

      Assert.True(profilePoints.Count == 2, $"Profile operation returned {profilePoints.Count} intercepts instead of 2");
      Assert.True(Math.Abs(profilePoints[0].X) < epsilon &&
                  Math.Abs(profilePoints[0].Y) < epsilon &&
                  Math.Abs(profilePoints[0].Z) < epsilon, "First profile point not at origin (0, 0, 0)");

      Assert.True(Math.Abs(profilePoints[1].X - 0.5) < epsilon &&
                  Math.Abs(profilePoints[1].Y - 0.5) < epsilon &&
                  Math.Abs(profilePoints[1].Z) < epsilon, "Second profile point not at origin (0.5, 0.5, 0)");
    }
  }
}
