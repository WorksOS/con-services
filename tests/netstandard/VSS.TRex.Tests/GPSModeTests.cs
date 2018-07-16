using System;
using System.IO;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests
{
        public class GPSModeTests
    {
        [Fact]
        public void Test_GPSMode()
        {
            Assert.Equal(10, Enum.GetNames(typeof(GPSMode)).Length);
        }
    }
}
