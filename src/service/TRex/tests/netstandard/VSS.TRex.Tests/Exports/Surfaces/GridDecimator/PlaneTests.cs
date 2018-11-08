using Xunit;

namespace VSS.TRex.Tests.Exports.Surfaces.GridDecimator
{
    public class PlaneTests
    {
      [Fact]
      public void Test_Plane_Creation()
      {
        TRex.Exports.Surfaces.GridDecimator.Plane plane = new TRex.Exports.Surfaces.GridDecimator.Plane();

        Assert.True(true);
      }

      [Fact]
      public void Test_Plane_Init()
      {
        TRex.Exports.Surfaces.GridDecimator.Plane plane = new TRex.Exports.Surfaces.GridDecimator.Plane();

        plane.Init(0, 0, 0, 2, 0, 0, 0, 2, 0);

        Assert.True(plane.a == 0, "Plane equation coefficient a incorrect");
        Assert.True(plane.b == 0, "Plane equation coefficient b incorrect");
        Assert.True(plane.c == 0, "Plane equation coefficient c incorrect");
    }

      [Fact]
      public void Test_Plane_Evaluate()
      {
        TRex.Exports.Surfaces.GridDecimator.Plane plane = new TRex.Exports.Surfaces.GridDecimator.Plane();

        plane.Init(0, 0, 0, 2, 0, 0, 0, 2, 0);
        Assert.True(plane.Evaluate(0, 0) == 0.0, "Plane evaluation incorrect");  
        Assert.True(plane.Evaluate(1, 1) == 0.0, "Plane evaluation incorrect");
    }
  }
}
