using VSS.TRex.TAGFiles.Classes.Queues;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_SegmentRetirementQueueQueryFilter
  {
    [Fact]
    public void Test_RoleBasedServerNodeFilter_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<SegmentRetirementQueueQueryFilter>("Empty SegmentRetirementQueueQueryFilter not same after round trip serialisation");
    }

    [Fact]
    public void Test_RoleBasedServerNodeFilter()
    {
      var filter = new SegmentRetirementQueueQueryFilter(123456);

      var result = SimpleBinarizableInstanceTester.TestClass(filter, "Custom SegmentRetirementQueueQueryFilter not same after round trip serialisation");

      Assert.True(result.member.retirementDateAsLong == 123456);
    }
  }
}
