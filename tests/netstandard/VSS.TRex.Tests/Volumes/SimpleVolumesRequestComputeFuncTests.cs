using System;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Volumes.GridFabric.ComputeFuncs;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
        public class SimpleVolumesRequestComputeFuncTests : IClassFixture<DILoggingFixture>
  {
        [Fact]
        public void Test_SimpleVolumesRequestComputeFunc_Creation()
        {
            SimpleVolumesRequestComputeFunc_ClusterCompute func = new SimpleVolumesRequestComputeFunc_ClusterCompute();

            Assert.NotNull(func);
        }
    }
}
