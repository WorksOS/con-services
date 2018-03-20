using System;
using VSS.VisionLink.Raptor.Volumes.GridFabric.ComputeFuncs;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests.Volumes
{
        public class SimpleVolumesRequestComputeFuncTests
    {
        [Fact]
        public void Test_SimpleVolumesRequestComputeFunc_Creation()
        {
            SimpleVolumesRequestComputeFunc_ClusterCompute func = new SimpleVolumesRequestComputeFunc_ClusterCompute();

            Assert.NotNull(func);
        }
    }
}
