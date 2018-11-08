using System;
using VSS.TRex.GridFabric.Affinity;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_SegmentRetirementQueueKey
  { 
    [Fact]
    public void ToFromBinary_SegmentRetirementQueueKey_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<SegmentRetirementQueueKey>("Empty SegmentRetirementQueueKey not same after round trip serialisation");
    }

    [Fact]
    public void ToFromBinary_SegmentRetirementQueueKey_WithProject()
    {
      SimpleBinarizableInstanceTester.TestClass(new SegmentRetirementQueueKey
          {ProjectUID = Guid.NewGuid()},
        "SegmentRetirementQueueKey with project uid not same after round trip serialisation");
    }

    [Fact]
    public void ToFromBinary_SegmentRetirementQueueKey_WithProjectAndDateTime()
    {
      SimpleBinarizableInstanceTester.TestClass(new SegmentRetirementQueueKey
        {
          ProjectUID = Guid.NewGuid(),
          InsertUTCAsLong = DateTime.Now.Ticks
        },
        "SegmentRetirementQueueKey with project uid and time not same after round trip serialisation");
    }
  }
}

