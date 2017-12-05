using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Geometry;
using System.Drawing;
using VSS.VisionLink.Raptor.Rendering.Executors;

namespace VSS.VisionLink.Raptor.Executors.Tests
{
    [TestClass()]
    public class RenderOverlayTileTests
    {
        [TestMethod()]
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

            Assert.IsTrue(render != null, "Did not create renderer as expected");
        }

        [Ignore] // This requires a running Ignite server which cant run in unit testing framework
        [TestMethod()]
        public void Test_RenderOverlayTile_Execute()
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
                                                             long.MinValue,// DesignDescriptor.Null(),
                                                             Color.Black,
                                                             String.Empty);
           Bitmap bmp = render.Execute();
           Assert.IsTrue(bmp != null, "Render did not return a Bitmap");
        }
    }
}