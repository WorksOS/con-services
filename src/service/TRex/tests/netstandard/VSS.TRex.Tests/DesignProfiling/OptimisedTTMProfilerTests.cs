using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Designs.TTM.Optimised.Profiling;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.DesignProfiling
{
  public class OptimisedTTMProfilerTests : IClassFixture<DILoggingFixture>
  {
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
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithOneTriangleAtOrigin();
      OptimisedTTMDesignBuilder.CreateOptimisedIndexForModel(oneTriangleModel, out var index, out var indices);

      // Build a profile line from (-100, -100) to (100, 100) to bisect the single triangle 
      var profiler = new OptimisedTTMProfiler(new SiteModel(Guid.Empty, 1.0), oneTriangleModel, index);
      var profilePoints = profiler.Compute(new XYZ(-100, -100), new XYZ(100, 100));

      Assert.True(profilePoints.Count > 0, "Profile operation returned no intercepts");
    }
  }
}
