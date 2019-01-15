using VSS.TRex.Profiling;
using VSS.TRex.Profiling.Factories;
using VSS.TRex.Profiling.Models;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Profiling.Factories
{
    public class ProfileBuilderFactoryTests : IClassFixture<DIProfilingFixture>
    {
      [Fact]
      public void Test_ProfileBuilderFactory_Creation()
      {
        var factory = new ProfilerBuilderFactory<ProfileCell>();

        Assert.NotNull(factory);
      }

      [Fact]
      public void Test_ProfileBuilderFactory_NewCellLiftBuilder()
      {
        var factory = new ProfilerBuilderFactory<ProfileCell>();

        Assert.True(factory.NewCellLiftBuilder(null, GridDataType.All, null, null, null) != null, "Failed to construct new cell lift builder");
      }

      [Fact]
      public void Test_ProfileBuilderFactory_NewCellProfileBuilder()
      {
        var factory = new ProfilerBuilderFactory<ProfileCell>();

        Assert.True(factory.NewCellProfileBuilder(null, null, null, true) != null, "Failed to construct new cell profile builder");
      }

      [Fact(Skip = "Requires SiteModel instance not currently mocked")]
      public void Test_ProfileBuilderFactory_NewProfileLiftBuilder()
      {
        var factory = new ProfilerBuilderFactory<ProfileCell>();

        Assert.True(factory.NewCellProfileAnalyzer(ProfileStyle.CellPasses, null, null, null, null, null, null) != null, "Failed to construct new profile lift builder");
      }
  }
}
