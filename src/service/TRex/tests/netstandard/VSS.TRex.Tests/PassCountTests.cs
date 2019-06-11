using System;
using VSS.TRex.Common.Utilities;
using Xunit;

namespace VSS.TRex.Tests
{
        public class PassCountTests
    {
        [Fact]
        public void Test_PassCountSizes()
        {
            Assert.Equal(PassCountSize.ONE_BYTE, PassCountSize.Calculate(0));
            Assert.Equal(PassCountSize.ONE_BYTE, PassCountSize.Calculate(1));
            Assert.Equal(PassCountSize.ONE_BYTE, PassCountSize.Calculate(255));
            Assert.Equal(PassCountSize.TWO_BYTES, PassCountSize.Calculate(256));
            Assert.Equal(PassCountSize.TWO_BYTES, PassCountSize.Calculate(32767));
            Assert.Equal(PassCountSize.FOUR_BYTES, PassCountSize.Calculate(32768));
            Assert.Equal(PassCountSize.FOUR_BYTES, PassCountSize.Calculate(1000000));
        }
    }
}
