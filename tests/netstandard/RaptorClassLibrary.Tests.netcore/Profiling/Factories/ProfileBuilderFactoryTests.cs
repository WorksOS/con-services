using VSS.TRex.Profiling.Factories;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Types;
using Xunit;

namespace RaptorClassLibrary.Tests.netcore.Profiling.Factories
{
    public class ProfileBuilderFactoryTests
    {
      [Fact]
      public void Test_ProfileBuilderFactory_Creation()
      {
        IProfilerBuilderFactory factory = new ProfilerBuilderFactory();

        Assert.NotNull(factory);
      }

      [Fact]
      public void Test_ProfileBuilderFactory_NewCellLiftBuilder()
      {
        IProfilerBuilderFactory factory = new ProfilerBuilderFactory();

        Assert.True(factory.NewCellLiftBuilder(null, GridDataType.All, null, null, null) != null, "Failed to construct new cell lift builder");
      }

      [Fact]
      public void Test_ProfileBuilderFactory_NewCellProfileBuilder()
      {
        IProfilerBuilderFactory factory = new ProfilerBuilderFactory();

        Assert.True(factory.NewCellProfileBuilder(null, null, null) != null, "Failed to construct new cell profile builder");
      }

      [Fact]
      public void Test_ProfileBuilderFactory_NewProfileLiftBuilder()
      {
        IProfilerBuilderFactory factory = new ProfilerBuilderFactory();

        Assert.True(factory.NewProfileLiftBuilder(null, null, null, null, null, null) != null, "Failed to construct new profile lift builder");
      }
  }
}
