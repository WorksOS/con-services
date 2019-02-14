using System;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.Executors;
using Xunit;
using Draw = System.Drawing;

namespace VSS.TRex.Tests
{
  public class RenderOverlayTileTests
    {
        [Fact()]
        public void Test_RenderOverlayTile_Creation()
        {
            RenderOverlayTile render = new RenderOverlayTile(Guid.NewGuid(),
                                                             DisplayMode.Height,
                                                             new XYZ(0, 0),
                                                             new XYZ(100, 100),
                                                             true, // CoordsAreGrid
                                                             100, //PixelsX
                                                             100, // PixelsY
                                                             null, // Filter1
                                                             null, // Filter2
                                                             Guid.Empty,  // DesignDescriptor.Null(),
                                                            Draw.Color.Black,
                                                             string.Empty);

            Assert.NotNull(render);
        }
    }
}
