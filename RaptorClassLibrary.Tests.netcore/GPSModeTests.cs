using System;
using System.IO;
using VSS.VisionLink.Raptor.Types;
using Xunit;

namespace VSS.VisionLink.Raptor.Compression.Tests
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
