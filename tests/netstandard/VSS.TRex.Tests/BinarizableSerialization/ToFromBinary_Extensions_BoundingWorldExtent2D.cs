using Apache.Ignite.Core.Binary;
using Xunit;
using VSS.TRex.Geometry;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_Extensions_BoundingWorldExtent2D
  {
    [Fact]
    public void Test_FromTo_BoundingIntegerExtent2D()
    {
      var extent = new TestBinarizable_Struct_Extension<BoundingIntegerExtent2D>
      {
        member = new BoundingIntegerExtent2D(0, 1, 100, 101)
      };

      var binObj = TestBinarizable_DefaultIgniteNode.GetIgnite().GetBinary().ToBinary<IBinaryObject>(extent);
      var result = binObj.Deserialize<TestBinarizable_Struct_Extension<BoundingIntegerExtent2D>>();

      Assert.True(extent.member.Equals(result.member), "Bounding integer 2D extent not same after round trip serialization");
    }
  }
}
