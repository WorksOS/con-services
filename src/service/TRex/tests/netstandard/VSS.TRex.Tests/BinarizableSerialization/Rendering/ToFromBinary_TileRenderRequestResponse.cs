using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Rendering
{
  public class ToFromBinary_TileRenderRequestResponse : IClassFixture<DIRenderingFixture>
  {
    [Fact]
    public void Test_TileRenderResponse_Core2_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<TileRenderResponse>("Empty TileRenderResponse_Core2 not same after round trip serialisation");
    }

    [Fact]
    public void Test_TileRenderResponse_Core2()
    {
      var response = new TileRenderResponse
      {
        TileBitmapData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10},
        ResultStatus = Types.RequestErrorStatus.OK
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom TileRenderResponse_Core2 not same after round trip serialisation");
    }
  }
}
