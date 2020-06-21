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

    [Fact]
    public void RebuilderCount_None()
    {
      var rebuilder = new SiteModelRebuilderManager();
      rebuilder.RebuildCount().Should().Be(0);
    }

    [Fact]
    public void GetRebuilderState_None()
    {
      var rebuilder = new SiteModelRebuilderManager();

      var state = rebuilder.GetRebuilersState();
      state.Should().NotBeNull();
      state.Count.Should().Be(0);
    }
  }
}
