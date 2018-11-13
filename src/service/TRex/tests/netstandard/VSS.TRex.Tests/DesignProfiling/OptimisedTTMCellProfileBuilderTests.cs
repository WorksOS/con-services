using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Designs.TTM.Optimised.Profiling;
using VSS.TRex.SiteModels;
using Xunit;

namespace VSS.TRex.Tests.DesignProfiling
{
  public class OptimisedTTMCellProfileBuilderTests
  {
    [Fact]
    public void Test_OptimisedTTMCellProfileBuilder_Creation()
    {
      var builder = new OptimisedTTMCellProfileBuilder(new SiteModel(Guid.Empty, 1.0), false);
    }

    [Fact]
    public void Test_OptimisedTTMDesignBuilder_OneTriangle()
    {
      var oneTriangleModel = OptimisedTTMDesignBuilder.CreateOptimisedTTM_WithOneTriangleAtOrigin();

      Assert.True(oneTriangleModel.Vertices.Items.Length == 3, "Invalid number of vertices for single triangle model");
      Assert.True(oneTriangleModel.Triangles.Items.Length == 1, "Invalid number of triangles for single triangle model");
    }
  }
}
