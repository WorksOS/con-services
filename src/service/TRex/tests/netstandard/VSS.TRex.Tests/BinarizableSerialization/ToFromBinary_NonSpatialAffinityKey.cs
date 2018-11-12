using System;
using VSS.TRex.GridFabric.Affinity;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_NonSpatialAffinityKey
  {
    [Fact]
    public void ToFromBinary_NonSpatialAffinityKey_Simple()
    {
      SimpleBinarizableInstanceTester.TestStruct<NonSpatialAffinityKey>("Empty NonSpatialAffinityKey not same after round trip serialisation");
    }

    [Fact]
    public void ToFromBinary_NonSpatialAffinityKey_WithProjectGuid()
    {
      SimpleBinarizableInstanceTester.TestStruct(new NonSpatialAffinityKey {ProjectUID = Guid.NewGuid()},
        "Empty NonSpatialAffinityKey not same after round trip serialisation with project UID");
    }

    [Fact]
    public void ToFromBinary_NonSpatialAffinityKey_WithKeyName()
    {
      SimpleBinarizableInstanceTester.TestStruct(new NonSpatialAffinityKey { KeyName = "key name" },
        "Empty NonSpatialAffinityKey not same after round trip serialisation with key name");
    }

    [Fact]
    public void ToFromBinary_NonSpatialAffinityKey_WithProjectGuidAndKeyName()
    {
      SimpleBinarizableInstanceTester.TestStruct(new NonSpatialAffinityKey { ProjectUID = Guid.NewGuid(), KeyName = "key name" },
        "Empty NonSpatialAffinityKey not same after round trip serialisation with project and key name");
    }

  }
}

