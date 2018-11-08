using Xunit;
using VSS.TRex.Geometry;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_Extensions
  {
    [Fact]
    public void Test_FromTo_BoundingWorldExtent3D_Simple()
    {
      SimpleBinarizableInstanceTester.TestExtension<BoundingWorldExtent3D>();
    }

    [Fact]
    public void Test_FromTo_BoundingWorldExtent3D_PlanExtent()
    {
      SimpleBinarizableInstanceTester.TestExtension<BoundingWorldExtent3D>(new BoundingWorldExtent3D(0, 1, 100, 101),
        "Bounding world 3D with plan extent not same after round trip serialization");
    }

    [Fact]
    public void Test_FromTo_BoundingWorldExtent3D_Full3DExtent()
    {
      SimpleBinarizableInstanceTester.TestExtension<BoundingWorldExtent3D>(new BoundingWorldExtent3D(0, 1, 100, 101, 200, 201),
        "Bounding world 3D with full 3D extent not same after round trip serialization");
    }
  }
}
