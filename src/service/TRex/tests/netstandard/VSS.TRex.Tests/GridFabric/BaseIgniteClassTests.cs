using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Storage.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.GridFabric
{
  public class BaseIgniteClassTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void AcquireIgniteTopologyProjections_FailWithNullOrEmptyRole()
    {
      string GridName = TRexGrids.GridName(StorageMutability.Immutable);
      string Role = null;

      Action act = () => new BaseIgniteClass(GridName, Role);
      act.Should().Throw<TRexException>().WithMessage("Role name not defined when acquiring topology projection");

      act = () => new BaseIgniteClass(GridName, "");
      act.Should().Throw<TRexException>().WithMessage("Role name not defined when acquiring topology projection");
    }

    [Fact]
    public void AcquireIgniteTopologyProjections_FailWithNullGridName()
    {
      Action act = () => new BaseIgniteClass("", "Test");
      act.Should().Throw<TRexException>().WithMessage("GridName name not defined when acquiring topology projection");

      act = () => new BaseIgniteClass(null, "Test");
      act.Should().Throw<TRexException>().WithMessage("GridName name not defined when acquiring topology projection");
    }

    [Fact]
    public void AcquireIgniteTopologyProjections_FailWithNullIgnite()
    {
      // ENsure any DI'ed IIgnite is removed

      DIBuilder.Continue()
        .RemoveSingle<IIgnite>()
        .RemoveSingle<ITRexGridFactory>()
        .Complete();

      string GridName = TRexGrids.GridName(StorageMutability.Immutable);
      string Role = "TestRole";

      Action act = () => new BaseIgniteClass(GridName, Role);

      act.Should().Throw<TRexException>().WithMessage("Ignite reference is null in AcquireIgniteTopologyProjections");
    }

    [Fact]
    public void AcquireIgniteTopologyProjections_FailWithNullClusterGroup()
    {
      var igniteMock = new IgniteMock();

      // Nobble the Ignite.GetCluster setup to mimic no available cluster group

      igniteMock.mockIgnite.Setup(x => x.GetCluster()).Returns((ICluster) null);

      DIBuilder.Continue()
        .Add(TRexGridFactory.AddGridFactoriesToDI)

        // Override the main Ignite grid factory method DI'ed from TRexGridFactory.AddGridFactoriesToDI()
        .Add(x => x.AddSingleton<Func<string, IgniteConfiguration, IIgnite>>(factory => (gridName, cfg) => igniteMock.mockIgnite.Object))
        .Complete();

      string GridName = TRexGrids.GridName(StorageMutability.Immutable);
      string Role = "TestRole";

      Action act = () => new BaseIgniteClass(GridName, Role);

      act.Should().Throw<TRexException>().WithMessage($"Cluster group reference is null in AcquireIgniteTopologyProjections for role {Role} on grid {GridName}");
    }

    [Fact]
    public void AcquireIgniteTopologyProjections_FailWithEmptyClusterGroup()
    {
      var igniteMock = new IgniteMock();

      // Nobble the IClusterNodes.Count() setup to mimic an empty cluster group

      igniteMock.mockClusterNodes.Setup(x => x.Count).Returns(0);

      DIBuilder.Continue()
        .Add(TRexGridFactory.AddGridFactoriesToDI)

        // Override the main Ignite grid factory method DI'ed from TRexGridFactory.AddGridFactoriesToDI()
        .Add(x => x.AddSingleton<Func<string, IgniteConfiguration, IIgnite>>(factory => (gridName, cfg) => igniteMock.mockIgnite.Object))
        .Complete();

      string GridName = TRexGrids.GridName(StorageMutability.Immutable);
      string Role = "TestRole";

      Action act = () => new BaseIgniteClass(GridName, Role);

      act.Should().Throw<TRexException>().WithMessage($"Group cluster topology is empty for role {Role} on grid {GridName}");
    }

    [Fact]
    public void AcquireIgniteTopologyProjections_FailWithNullComputeProjection()
    {
      var igniteMock = new IgniteMock();

      // Nobble the ICompute setup to mimic a null compute projection
      igniteMock.mockClusterGroup.Setup(x => x.GetCompute()).Returns((ICompute) null);

      DIBuilder.Continue()
        .Add(TRexGridFactory.AddGridFactoriesToDI)

        // Override the main Ignite grid factory method DI'ed from TRexGridFactory.AddGridFactoriesToDI()
        .Add(x => x.AddSingleton<Func<string, IgniteConfiguration, IIgnite>>(factory => (gridName, cfg) => igniteMock.mockIgnite.Object))
        .Complete();

      string GridName = TRexGrids.GridName(StorageMutability.Immutable);
      string Role = "TestRole";

      Action act = () => new BaseIgniteClass(GridName, Role);

      act.Should().Throw<TRexException>().WithMessage($"Compute projection is null in AcquireIgniteTopologyProjections on grid {GridName}");
    }

    [Fact]
    public void AcquireIgniteTopologyProjections_FailWithUnknownGrid()
    {
      var igniteMock = new IgniteMock();

      DIBuilder.Continue()
        .Add(TRexGridFactory.AddGridFactoriesToDI)

        // Override the main Ignite grid factory method DI'ed from TRexGridFactory.AddGridFactoriesToDI()
        .Add(x => x.AddSingleton<Func<string, IgniteConfiguration, IIgnite>>(factory => (gridName, cfg) => igniteMock.mockIgnite.Object))
        .Complete();

      string GridName = "TestGrid";
      string Role = "TestRole";

      Action act = () => new BaseIgniteClass(GridName, Role);

      act.Should().Throw<TRexException>().WithMessage($"{GridName} is an unknown grid to create a reference for.");
    }

    [Fact]
    public void AcquireIgniteTopologyProjections_Success()
    {
      var igniteMock = new IgniteMock();

      DIBuilder.Continue()
        .Add(TRexGridFactory.AddGridFactoriesToDI)

        // Override the main Ignite grid factory method DI'ed from TRexGridFactory.AddGridFactoriesToDI()
        .Add(x => x.AddSingleton<Func<string, IgniteConfiguration, IIgnite>>(factory => (gridName, cfg) => igniteMock.mockIgnite.Object))
        .Complete();

      var ignite = new BaseIgniteClass(TRexGrids.GridName(StorageMutability.Immutable), "TestRole");

      ignite.AcquireIgniteTopologyProjections();
    }
  }
}
