using FluentAssertions;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
  public class TAGFileBufferQueueItemTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_TAGFileBufferQueueItem_Creation()
    {
      var item = new TAGFileBufferQueueItem();

      item.Should().NotBeNull();
    }
  }
}
