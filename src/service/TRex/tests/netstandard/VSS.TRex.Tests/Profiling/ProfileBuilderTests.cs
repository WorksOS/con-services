using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
    private static Object Lock = new object();

    public static Mock<IProfilerBuilderFactory<ProfileCell>> factory;
    public static Mock<ICellLiftBuilder> newCellLiftBuilder;
    public static Mock<ICellProfileBuilder<ProfileCell>> newCellProfileBuilder;
    public static Mock<ICellProfileAnalyzer<ProfileCell>> newProfileLiftBuilder;

    public ProfileBuilderTests_Fixture()
    {
      lock (Lock)
      {
        factory = new Mock<IProfilerBuilderFactory<ProfileCell>>();
        newCellLiftBuilder = new Mock<ICellLiftBuilder>();
        newCellProfileBuilder = new Mock<ICellProfileBuilder<ProfileCell>>();
        newProfileLiftBuilder = new Mock<ICellProfileAnalyzer<ProfileCell>>();

        factory.Setup(mk => mk.NewCellLiftBuilder(null, GridDataType.All, null, null, null))
          .Returns(newCellLiftBuilder.Object);
        factory.Setup(mk => mk.NewCellProfileBuilder(null, null, null, true)).Returns(newCellProfileBuilder.Object);
        factory.Setup(mk => mk.NewCellProfileAnalyzer(ProfileStyle.CellPasses, null, null, null, null, null, It.IsAny<ICellLiftBuilder>()))
          .Returns(newProfileLiftBuilder.Object);

        DIBuilder.New().Add(x => x.AddSingleton<IProfilerBuilderFactory<ProfileCell>>(factory.Object)).Complete();
      }
    }

    public void Dispose()
    {
    }
  }

  [Collection("ProfileBuilderTests")]
  public class ProfileBuilderTests : IClassFixture<ProfileBuilderTests_Fixture>
  {
    [Fact]
    public void Test_ProfilerBuilder_Creation_Null()
    {
      var builder = new ProfilerBuilder<ProfileCell>();
      builder.Configure(ProfileStyle.CellPasses, null, null, GridDataType.All, null, null, null, null, null, null);

      Assert.True(builder != null, "Builder failed to construct");
    }

    [Fact]
    public void Test_ProfilerBuilder_Creation_ProfileBuilders()
    {
      var builder = new ProfilerBuilder<ProfileCell>();
      builder.Configure(ProfileStyle.CellPasses, null, null, GridDataType.All, null, null, null, null, null, null);

      Assert.True(builder.CellLiftBuilder == ProfileBuilderTests_Fixture.newCellLiftBuilder.Object, "Cell lift builder not expected one");
      Assert.True(builder.CellProfileBuilder == ProfileBuilderTests_Fixture.newCellProfileBuilder.Object,
        "Cell profile builder not expected one");
      Assert.True(builder.CellProfileAnalyzer == ProfileBuilderTests_Fixture.newProfileLiftBuilder.Object,
        "Profile lift builder not expected one");
    }
  }
}
