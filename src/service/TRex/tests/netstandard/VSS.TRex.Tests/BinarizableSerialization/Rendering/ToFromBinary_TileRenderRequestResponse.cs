using System.Drawing;
using VSS.MasterData.Models.Models;
using VSS.TRex.DI;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Rendering
{
  public class ToFromBinary_TileRenderRequestResponse : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_TileRenderResponse_Core2_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<TileRenderResponse_Core2>("Empty TileRenderResponse_Core2 not same after round trip serialisation");
    }

    [Fact]
    public void Test_TileRenderResponse_Core2()
    {
      var bitmap = new Bitmap(WebMercatorProjection.TILE_SIZE, WebMercatorProjection.TILE_SIZE);

      
      var renderingFactory = DIContext.Obtain<IRenderingFactory>();
      var response = renderingFactory.CreateTileRenderResponse(bitmap) as TileRenderResponse_Core2;


      SimpleBinarizableInstanceTester.TestClass(response, "Custom TileRenderResponse_Core2 not same after round trip serialisation");
    }
  }
}
