using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Profiling
{
  public class ProfileBuilderTests_Fixture : IDisposable
  {
    public ProfileBuilderTests_Fixture()
    {
        var factory = new Mock<IProfilerBuilderFactory<ProfileCell>>();
        var newCellLiftBuilder = new Mock<ICellLiftBuilder>();
        var newCellProfileBuilder = new Mock<ICellProfileBuilder<ProfileCell>>();
        var newProfileLiftBuilder = new Mock<ICellProfileAnalyzer<ProfileCell>>();

        factory.Setup(mk => mk.NewCellLiftBuilder(null, GridDataType.All, null, null, null))
          .Returns(newCellLiftBuilder.Object);
        factory.Setup(mk => mk.NewCellProfileBuilder(null, null, null, true)).Returns(newCellProfileBuilder.Object);
        factory.Setup(mk => mk.NewCellProfileAnalyzer(ProfileStyle.CellPasses, null, null, null, null, null, It.IsAny<ICellLiftBuilder>(), It.IsAny<VolumeComputationType>(), null))
          .Returns(newProfileLiftBuilder.Object);

        DIBuilder
          .New()
          .Add(x => x.AddSingleton<ICellLiftBuilder>(newCellLiftBuilder.Object))
          .Add(x => x.AddSingleton<ICellProfileBuilder<ProfileCell>>(newCellProfileBuilder.Object))
          .Add(x => x.AddSingleton<ICellProfileAnalyzer<ProfileCell>>(newProfileLiftBuilder.Object))
          .Add(x => x.AddSingleton<IProfilerBuilderFactory<ProfileCell>>(factory.Object)).Complete();
    }

    public void Dispose()
    {
      DIBuilder.Eject();
    }
  }

  [Collection("ProfileBuilderTests")]
  public class ProfileBuilderTests : IClassFixture<ProfileBuilderTests_Fixture>
  {
    [Fact]
    public void Test_ProfilerBuilder_Creation_Null()
    {
      var builder = new ProfilerBuilder<ProfileCell>();
      builder.Configure(ProfileStyle.CellPasses, null, null, GridDataType.All, null, null, null, null, null, VolumeComputationType.None, null);

      Assert.True(builder != null, "Builder failed to construct");
    }

    [Fact]
    public void Test_ProfilerBuilder_Creation_ProfileBuilders()
    {
      var builder = new ProfilerBuilder<ProfileCell>();
      builder.Configure(ProfileStyle.CellPasses, null, null, GridDataType.All, null, null, null, null, null, VolumeComputationType.None, null);

      Assert.True(builder.CellLiftBuilder == DIContext.Obtain<ICellLiftBuilder>(), "Cell lift builder not expected one");
      Assert.True(builder.CellProfileBuilder == DIContext.Obtain<ICellProfileBuilder<ProfileCell>>(),
        "Cell profile builder not expected one");
      Assert.True(builder.CellProfileAnalyzer == DIContext.Obtain<ICellProfileAnalyzer<ProfileCell>>(),
        "Profile lift builder not expected one");
    }
  }
}
