using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class GeometryHelperTests
    {
        [TestMethod]
        public void Test_GeometryHelper_AntiClockwise()
        {
            GeometryHelper.RotatePointAbout((Math.PI / 2), 0, 100, out double toX, out double toY, 0, 0);

            Assert.IsTrue(Math.Abs(toX - -100) < 1E-10 && Math.Abs(toY) < 1E-10, $"Rotated end point of X:{toX}, Y:{toY} is not {-100, 0} as expected.");
        }

        [TestMethod]
        public void Test_GeometryHelper_Clockwise()
        {
            GeometryHelper.RotatePointAbout(-(Math.PI / 2), 0, 100, out double toX, out double toY, 0, 0);

            Assert.IsTrue(Math.Abs(toX - 100) < 1E-10 && Math.Abs(toY) < 1E-10, $"Rotated end point of X:{toX}, Y:{toY} is not {100,0} as expected.");
        }
    }
}
