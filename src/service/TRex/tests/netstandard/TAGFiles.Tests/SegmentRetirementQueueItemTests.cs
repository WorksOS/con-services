using FluentAssertions;
using VSS.TRex.TAGFiles.Models;
using Xunit;

namespace TAGFiles.Tests
{
  public class SegmentRetirementQueueItemTests
  {
    [Fact]
    public void Test_SegmentRetirementQueueItem_Creation()
    {
      var item = new SegmentRetirementQueueItem();

      item.Should().NotBeNull();
    }
  }
}
