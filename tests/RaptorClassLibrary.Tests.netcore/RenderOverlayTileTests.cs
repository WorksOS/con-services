using System;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Geometry;
using System.Drawing;
using VSS.VisionLink.Raptor.Rendering.Executors;
using Xunit;

namespace VSS.VisionLink.Raptor.Executors.Tests
{
        public class RenderOverlayTileTests
    {
        [Fact()]
        public void Test_RenderOverlayTile_Creation()
        {
            RenderOverlayTile render = new RenderOverlayTile(1,
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
                                                             String.Empty);

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