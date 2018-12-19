using System;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_SegmentRetirementQueueItem : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_SegmentRetirementQueueItem_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<SegmentRetirementQueueItem>("Empty SegmentRetirementQueueItem not same after round trip serialisation");
    }

    [Fact]
    public void Test_SegmentRetirementQueueItem()
    {
      Guid projectGuid = Guid.NewGuid();

      var item = new SegmentRetirementQueueItem
      {
        ProjectUID = projectGuid,
        InsertUTCAsLong = 1234567890,
        SegmentKeys = new ISubGridSpatialAffinityKey[]
        {
          new SubGridSpatialAffinityKey
          {
            ProjectUID = projectGuid,
            SegmentIdentifier = "SegmentIdentifier",
            SubGridX = 12345,
            SubGridY = 67890
          }
        }
      };

      var result = SimpleBinarizableInstanceTester.TestClass(item, "Custom TAGFileBufferQueueItem not same after round trip serialisation");

      Assert.True(result.member.ProjectUID.Equals(projectGuid) &&
                  result.member.InsertUTCAsLong.Equals(1234567890) &&
                  result.member.SegmentKeys.Length == 1 &&
                  result.member.SegmentKeys[0].SegmentIdentifier == "SegmentIdentifier" &&
                  result.member.SegmentKeys[0].ProjectUID.Equals(projectGuid) &&
                  result.member.SegmentKeys[0].SubGridX == 12345 &&
                  result.member.SegmentKeys[0].SubGridY == 67890,
        "Post IEquality<T> comparer based comparison failure");
    }
  }
}
