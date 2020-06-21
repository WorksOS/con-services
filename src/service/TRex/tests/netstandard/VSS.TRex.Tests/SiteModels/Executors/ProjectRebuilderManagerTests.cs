using FluentAssertions;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.GridFabric.ComputeFuncs;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric.Executors
{
  public class SiteModelRebuilderManagerTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting()
    {
      IgniteMock.Mutable.AddApplicationGridRouting<DeleteSiteModelRequestComputeFunc, DeleteSiteModelRequestArgument, DeleteSiteModelRequestResponse>();
    }

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

      var state = rebuilder.GetRebuildersState();
      state.Should().NotBeNull();
      state.Count.Should().Be(0);
    }
  }
}
