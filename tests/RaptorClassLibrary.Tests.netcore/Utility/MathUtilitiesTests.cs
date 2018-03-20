using System;
using VSS.VisionLink.Raptor.Utilities;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Utilities.Tests
{
        public class MathUtilitiesTests
    {
        [Fact]
        public void Test_MathUtilities_Hypot()
        {
            Assert.Equal(5, MathUtilities.Hypot(3, 4));
            Assert.Equal(MathUtilities.Hypot(1, 1), Math.Sqrt(2));
        }
    }
}
