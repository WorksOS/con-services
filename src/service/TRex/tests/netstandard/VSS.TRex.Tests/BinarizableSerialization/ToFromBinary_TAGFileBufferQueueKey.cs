using System;
using VSS.TRex.GridFabric.Affinity;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_TAGFileBufferQueueKey
  { 
    [Fact]
    public void ToFromBinary_TAGFileBufferQueueKey_Simple()
    {
      SimpleBinarizableInstanceTester.TestStruct<TAGFileBufferQueueKey>("Empty TAGFileBufferQueueKey not same after round trip serialisation");
    }

    [Fact]
    public void ToFromBinary_TAGFileBufferQueueKey_WithFileName()
    {
      SimpleBinarizableInstanceTester.TestStruct(new TAGFileBufferQueueKey
          {FileName = "A-TAG-File.tag"},
        "TAGFileBufferQueueKey with file name not same after round trip serialisation");
    }

    [Fact]
    public void ToFromBinary_TAGFileBufferQueueKey_WithFileNameProjectAndAsset()
    {
      SimpleBinarizableInstanceTester.TestStruct(new TAGFileBufferQueueKey
        {
          FileName = "A-TAG-File.tag",
          ProjectUID = Guid.NewGuid(),
          AssetUID = Guid.NewGuid()
        },
        "TAGFileBufferQueueKey with file name, project and asset UIDs not same after round trip serialisation");
    }
  }
}

