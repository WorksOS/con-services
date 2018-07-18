using System;
using VSS.TRex.Utilities;
using Xunit;

namespace VSS.TRex.Tests.Utility
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
