using System;
using VSS.TRex.Volumes.GridFabric.Arguments;
using Xunit;

namespace VSS.TRex.RaptorClassLibrary.Tests.Volumes
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
