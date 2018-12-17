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
            Assert.Equal(1, PassCountSize.Calculate(0));
            Assert.Equal(1, PassCountSize.Calculate(1));
            Assert.Equal(1, PassCountSize.Calculate(255));
            Assert.Equal(2, PassCountSize.Calculate(256));
            Assert.Equal(2, PassCountSize.Calculate(65535));
            Assert.Equal(3, PassCountSize.Calculate(65536));
            Assert.Equal(3, PassCountSize.Calculate(1000000));
        }
    }
}
