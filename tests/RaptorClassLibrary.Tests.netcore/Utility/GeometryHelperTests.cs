using System;
using VSS.VisionLink.Raptor.Utilities;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Utilities.Tests
{
        public class GeometryHelperTests
    {
        [Fact]
        public void Test_GeometryHelper_AntiClockwise_AtOrigin()
        {
            GeometryHelper.RotatePointAbout((Math.PI / 2), 0, 100, out double toX, out double toY, 0, 0);

            Assert.True(Math.Abs(toX - -100) < 1E-10 && Math.Abs(toY) < 1E-10, $"Rotated end point of X:{toX}, Y:{toY} is not {-100, 0} as expected.");
        }

        [Fact]
        public void Test_GeometryHelper_Clockwise_AtOrigin()
        {
            GeometryHelper.RotatePointAbout(-(Math.PI / 2), 0, 100, out double toX, out double toY, 0, 0);

            Assert.True(Math.Abs(toX - 100) < 1E-10 && Math.Abs(toY) < 1E-10, $"Rotated end point of X:{toX}, Y:{toY} is not {100, 0} as expected.");
        }

        [Fact]
        public void Test_GeometryHelper_AntiClockwise_NotAtOrigin()
        {
            GeometryHelper.RotatePointAbout((Math.PI / 2), 100, 200, out double toX, out double toY, 100, 100);

            Assert.True(Math.Abs(toX) < 1E-10 && Math.Abs(toY - 100) < 1E-10, $"Rotated end point of X:{toX}, Y:{toY} is not {0, 100} as expected.");
        }

        [Fact]
        public void Test_GeometryHelper_Clockwise_NotAtOrigin()
        {
            GeometryHelper.RotatePointAbout(-(Math.PI / 2), 100, 200, out double toX, out double toY, 100, 100);

            Assert.True(Math.Abs(toX - 200) < 1E-10 && Math.Abs(toY - 100) < 1E-10, $"Rotated end point of X:{toX}, Y:{toY} is not {200, 100} as expected.");
        }
    }
}
