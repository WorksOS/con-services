using FluentAssertions;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
  public class SegmentRetirementQueueItemTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_SegmentRetirementQueueItem_Creation()
    {
      var item = new SegmentRetirementQueueItem();

      item.Should().NotBeNull();
    }
  }
}
