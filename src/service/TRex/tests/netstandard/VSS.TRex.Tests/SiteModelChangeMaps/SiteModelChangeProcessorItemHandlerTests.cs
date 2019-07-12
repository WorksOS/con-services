using FluentAssertions;
using VSS.TRex.SiteModelChangeMaps;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModelChangeMaps
{
  public class SiteModelChangeProcessorItemHandlerTests : SiteModelChangeTestsBase
  {
    [Fact]
    public void Creation()
    {
      using (var handler = new SiteModelChangeProcessorItemHandler())
      {
        handler.Should().NotBeNull();
        handler.Active.Should().BeFalse();
        handler.Aborted.Should().BeFalse();
      }
    }

    [Fact]
    public void Activation()
    {
      using (var handler = new SiteModelChangeProcessorItemHandler())
      {
        handler.Activate();
        handler.Active.Should().BeTrue();
        handler.Aborted.Should().BeFalse();
      }
    }

    [Fact]
    public void Abort()
    {
      using (var handler = new SiteModelChangeProcessorItemHandler())
      {
        handler.Abort();
        handler.Aborted.Should().BeTrue();
      }
    }
  }
}
