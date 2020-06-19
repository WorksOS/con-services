using FluentAssertions;
using VSS.TRex.SiteModels;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric.Executors
{
  public class SiteModelRebuilderManagerTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var rebuilder = new SiteModelRebuilderManager();
      rebuilder.Should().NotBeNull();
    }
  }
}
