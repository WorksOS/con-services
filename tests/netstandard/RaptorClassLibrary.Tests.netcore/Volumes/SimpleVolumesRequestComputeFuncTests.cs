using System;
using VSS.TRex.Volumes.GridFabric.ComputeFuncs;
using Xunit;

namespace VSS.TRex.Tests.Volumes
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
