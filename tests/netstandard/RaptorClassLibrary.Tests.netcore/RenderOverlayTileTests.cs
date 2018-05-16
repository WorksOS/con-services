using System;
using System.Drawing;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.Executors;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Executors.Tests
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
                                                             long.MinValue, // DesignDescriptor.Null(),
                                                             Color.Black,
                                                             string.Empty);

            Assert.NotNull(render);
        }

        [Fact(Skip = "not implemented")]
        //TODO need to implement this
        public void Test_RenderOverlayTile_Execute()
        {
        /*    RenderOverlayTile render = new RenderOverlayTile(1,
                                                             DisplayMode.Height,
                                                             new XYZ(0, 0),
                                                             new XYZ(100, 100),
                                                             true, // CoordsAreGrid
                                                             100, //PixelsX
                                                             100, // PixelsY
                                                             null, // Filter1
                                                             null, // Filter2
                                                             long.MinValue,// DesignDescriptor.Null(),
                                                             Color.Black,
                                                             String.Empty);
           Bitmap bmp = render.Execute();
           Assert.NotNull(bmp);*/
        }
    }
}