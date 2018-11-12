using System;
using VSS.TRex.GridFabric.Affinity;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_SubGridSpatialAffinityKey
  {
    [Fact]
    public void ToFromBinary_SubGridSpatialAffinityKey_Simple()
    {
      SimpleBinarizableInstanceTester.TestStruct<SubGridSpatialAffinityKey>("Empty SubGridSpatialAffinityKey not same after round trip serialisation");
    }

    [Fact]
    public void ToFromBinary_SubGridSpatialAffinityKey_WithProjectUID()
    {
      SimpleBinarizableInstanceTester.TestStruct(new SubGridSpatialAffinityKey
      { ProjectUID = Guid.NewGuid()},
        "SubGridSpatialAffinityKey with project UID not same after round trip serialisation");
    }

    [Fact]
    public void ToFromBinary_SubGridSpatialAffinityKey_WithProjectUIDAndLocation()
    {
      SimpleBinarizableInstanceTester.TestStruct(new SubGridSpatialAffinityKey
        {
          ProjectUID = Guid.NewGuid(),
          SubGridX = 12345,
          SubGridY = 56789
      },
        "SubGridSpatialAffinityKey with Project UID and location not same after round trip serialisation");
    }

    [Fact]
    public void ToFromBinary_SubGridSpatialAffinityKey_WithProjectUIDLocationAndSegmentIdentifier()
    {
      SimpleBinarizableInstanceTester.TestStruct(new SubGridSpatialAffinityKey
        {
          ProjectUID = Guid.NewGuid(),
          SubGridX = 12345,
          SubGridY = 56789,
          SegmentIdentifier = "segment identifier"
      },
        "SubGridSpatialAffinityKey with Project UID, location and segment identifier not same after round trip serialisation");
    }
  }
}

