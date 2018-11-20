using VSS.TRex.GridFabric.NodeFilters;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_RoleBaseNodeFilters
  {
    [Fact]
    public void Test_RoleBasedServerNodeFilter_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<RoleBasedServerNodeFilter>("Empty RoleBasedServerNodeFilter not same after round trip serialisation");
    }

    [Fact]
    public void Test_RoleBasedServerNodeFilter()
    {
      var filter = new RoleBasedServerNodeFilter("Role");
      SimpleBinarizableInstanceTester.TestClass(filter, "Custom RoleBasedServerNodeFilter not same after round trip serialisation");
    }

    [Fact]
    public void Test_RoleBasedClientNodeFilter_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<RoleBasedClientNodeFilter>("Empty RoleBasedClientNodeFilter not same after round trip serialisation");
    }

    [Fact]
    public void Test_RoleBasedClientNodeFilter()
    {
      var filter = new RoleBasedClientNodeFilter("Role");
      SimpleBinarizableInstanceTester.TestClass(filter, "Custom RoleBasedClientNodeFilter not same after round trip serialisation");
    }

    [Fact]
    public void Test_ASNodeRoleBasedNodeFilter_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<ASNodeRoleBasedNodeFilter>("Empty ASNodeRoleBasedNodeFilter not same after round trip serialisation");
    }

    [Fact]
    public void Test_ASNodeRoleBasedNodeFilter()
    {
      var filter = new ASNodeRoleBasedNodeFilter();
      SimpleBinarizableInstanceTester.TestClass(filter, "Custom ASNodeRoleBasedNodeFilter not same after round trip serialisation");
    }

    [Fact]
    public void Test_PSNodeRoleBasedNodeFilter_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<PSNodeRoleBasedNodeFilter>("Empty PSNodeRoleBasedNodeFilter not same after round trip serialisation");
    }

    [Fact]
    public void Test_PSNodeRoleBasedNodeFilter()
    {
      var filter = new PSNodeRoleBasedNodeFilter();
      SimpleBinarizableInstanceTester.TestClass(filter, "Custom PSNodeRoleBasedNodeFilter not same after round trip serialisation");
    }
  }
}
