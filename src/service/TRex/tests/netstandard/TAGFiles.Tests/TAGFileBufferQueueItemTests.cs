using FluentAssertions;
using VSS.TRex.TAGFiles.Models;
using Xunit;

namespace TAGFiles.Tests
{
  public class TAGFileBufferQueueItemTests
  {
    [Fact]
    public void Test_TAGFileBufferQueueItem_Creation()
    {
      var item = new TAGFileBufferQueueItem();

      item.Should().NotBeNull();
    }
  }
}
