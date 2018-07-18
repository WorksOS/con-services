using System;
using VSS.TRex.GridFabric.NodeFilters;
using VSS.TRex.Servers;
using VSS.TRex.Tests.MockedClasses.GridFabric;
using Xunit;

namespace VSS.TRex.Tests.GridFabric
{
        public class NodeFilterTests
    {
        [Fact]
        public void Test_RoleBasedFilter_PSNode()
        {
            RoleBasedNodeFilter filter = new PSNodeRoleBasedNodeFilter();

            Assert.True(filter.Invoke(new MockedClusterServerNode(false, ServerRoles.PSNODE)), "PSNode role based node did not meet the filter");
            Assert.False(filter.Invoke(new MockedClusterServerNode(false, "NotAValidRole")), "PSNode role based node matched invalid filter");
        }

        [Fact]
        public void Test_RoleBasedFilter_ASNode()
        {
            RoleBasedNodeFilter filter = new ASNodeRoleBasedNodeFilter();

            Assert.True(filter.Invoke(new MockedClusterServerNode(true, ServerRoles.ASNODE)), "PSNode role based node did not meet the filter");
            Assert.False(filter.Invoke(new MockedClusterServerNode(true, "NotAValidRole")), "PSNode role based node matched invalid filter");
        }
    }
}
