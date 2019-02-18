using System.IO;
using Apache.Ignite.Core.Binary;
using Xunit;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.ExtensionMethods;

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

      var bw = new TestBinaryWriter();
      extent.WriteBinary(bw);

      var br = new TestBinaryReader(bw._stream.BaseStream as MemoryStream);
      var result = new TestBinarizable_Struct_Extension<BoundingIntegerExtent2D>();

      result.ReadBinary(br);

      Assert.True(extent.member.Equals(result.member), "Bounding integer 2D extent not same after round trip serialization");
    }
  }
}
