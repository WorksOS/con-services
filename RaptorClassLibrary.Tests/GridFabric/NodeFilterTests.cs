using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.GridFabric.NodeFilters;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.RaptorClassLibrary.Tests.MockedClasses.GridFabric;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.GridFabric.Tests
{
    [TestClass]
    public class NodeFilterTests
    {
        [TestMethod]
        public void Test_RoleBasedFilter_PSNode()
        {
            RoleBasedNodeFilter filter = new PSNodeRoleBasedNodeFilter();

            Assert.IsTrue(filter.Invoke(new MockedClusterServerNode(ServerRoles.PSNODE)), "PSNode role based node did not meet the filter");
            Assert.IsFalse(filter.Invoke(new MockedClusterServerNode("NotAValidRole")), "PSNode role based node matched invalid filter");
        }

        [TestMethod]
        public void Test_RoleBasedFilter_ASNode()
        {
            RoleBasedNodeFilter filter = new ASNodeRoleBasedNodeFilter();

            Assert.IsTrue(filter.Invoke(new MockedClusterServerNode(ServerRoles.ASNODE)), "PSNode role based node did not meet the filter");
            Assert.IsFalse(filter.Invoke(new MockedClusterServerNode("NotAValidRole")), "PSNode role based node matched invalid filter");
        }
    }
}
