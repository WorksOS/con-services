using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Xunit;
using VSS.TRex.Geometry;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_Extensions
  {
    private IIgnite _ignite = null;

    private IIgnite GetIgnite()
    {
      lock (this)
      {
        if (_ignite != null)
          return _ignite;

        _ignite = Ignition.TryGetIgnite() ?? Ignition.Start();
        return _ignite;
      }
    }

    /// <summary>
    /// Test round trip serialization of bounding world extent 3D
    /// </summary>
    [Fact]
    public void Test_FromTo_BoundingWorldExtent3D_PlanExtent()
    {
      var extent = new BoundingWorldExtent3D(0, 1, 100, 101);
      var binObj = GetIgnite().GetBinary().ToBinary<IBinaryObject>(extent);
      var result = binObj.ToBuilder().Build().Deserialize<BoundingWorldExtent3D>();

      Assert.True(extent.Equals(result), "Bounding world 3D with plan extent not same after round trip serialization");
    }

    /// <summary>
    /// Test round trip serialization of bounding world extent 3D
    /// </summary>
    [Fact]
    public void Test_FromTo_BoundingWorldExtent3D_Full3DExtent()
    {
      var extent = new BoundingWorldExtent3D(0, 1, 100, 101, 200, 201);
      var binObj = GetIgnite().GetBinary().ToBinary<IBinaryObject>(extent);
      var result = binObj.ToBuilder().Build().Deserialize<BoundingWorldExtent3D>();

      Assert.True(extent.Equals(result), "Bounding world 3D with full 3D extent not same after round trip serialization");
    }

    /// <summary>
    /// Test round trip serialization of bounding world extent 3D
    /// </summary>
    [Fact]
    public void Test_FromTo_BoundingIntegerExtent2D()
    {
      var extent = new BoundingIntegerExtent2D(0, 1, 100, 101);
      var binObj = GetIgnite().GetBinary().ToBinary<IBinaryObject>(extent);
      var result = binObj.ToBuilder().Build().Deserialize<BoundingIntegerExtent2D>();

      Assert.True(extent.Equals(result), "Bounding integer 2D extent not same after round trip serialization");
    }
  }
}
