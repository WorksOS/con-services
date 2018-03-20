using System;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Arguments;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests.Volumes
{
        public class SimpleVolumesRequestArgumentTests
    {
        [Fact]
        public void Test_SimpleVolumesRequestArgument_Creation()
        {
            SimpleVolumesRequestArgument arg = new SimpleVolumesRequestArgument();

            Assert.NotNull(arg);
        }
    }
}
