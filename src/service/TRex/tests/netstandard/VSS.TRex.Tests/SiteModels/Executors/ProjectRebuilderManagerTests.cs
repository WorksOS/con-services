using FluentAssertions;
using VSS.TRex.SiteModels;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric.Executors
{
  public class SiteModelRebuilderManagerTests
  {
    [Fact]
    public void Creation()
    {
      var rebuilder = new SiteModelRebuilderManager();
      rebuilder.Should().NotBeNull();
    }
  }
}
