using VSS.TRex.Geometry;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_Fence
  {
    [Fact]
    public void Test_Fence_Simple()
    {
      SimpleBinarizableInstanceTester.TestExtension<Fence>("Empty fence not same after round trip serialisation");
    }

    [Fact]
    public void Test_Fence()
    {
      var fence = new Fence();
      fence.Points.Add(new FencePoint(0, 0));
      fence.Points.Add(new FencePoint(0, 10));
      fence.Points.Add(new FencePoint(10, 10));
      fence.Points.Add(new FencePoint(10, 0));

      SimpleBinarizableInstanceTester.TestExtension(fence, "Fence with points not same after round trip serialisation");
    }  
  }
}

