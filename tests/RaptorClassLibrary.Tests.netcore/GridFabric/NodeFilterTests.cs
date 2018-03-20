using System;
using VSS.VisionLink.Raptor.GridFabric.NodeFilters;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.RaptorClassLibrary.Tests.MockedClasses.GridFabric;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.GridFabric.Tests
{
        public class NodeFilterTests
    {
        [Fact]
        public void Test_RoleBasedFilter_PSNode()
        {
            RoleBasedNodeFilter filter = new PSNodeRoleBasedNodeFilter();

            Assert.True(filter.Invoke(new MockedClusterServerNode(ServerRoles.PSNODE)), "PSNode role based node did not meet the filter");
            Assert.False(filter.Invoke(new MockedClusterServerNode("NotAValidRole")), "PSNode role based node matched invalid filter");
        }

        [Fact]
        public void Test_RoleBasedFilter_ASNode()
        {
            RoleBasedNodeFilter filter = new ASNodeRoleBasedNodeFilter();

            Assert.True(filter.Invoke(new MockedClusterServerNode(ServerRoles.ASNODE)), "PSNode role based node did not meet the filter");
            Assert.False(filter.Invoke(new MockedClusterServerNode("NotAValidRole")), "PSNode role based node matched invalid filter");
        }
    }
}
