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
      var ignite = new BaseIgniteClass
      {
        Role = null,
        GridName = TRexGrids.GridName(StorageMutability.Immutable)
      };

      Action act = () => ignite.AcquireIgniteTopologyProjections();
      act.Should().Throw<TRexException>().WithMessage("Role name not defined when acquiring topology projection");

      ignite.Role = null;
      act.Should().Throw<TRexException>().WithMessage("Role name not defined when acquiring topology projection");
    }

    [Fact]
    public void AcquireIgniteTopologyProjections_FailWithNullGridName()
    {
      var ignite = new BaseIgniteClass
      {
        Role = "Test",
        GridName = ""
      };

      Action act = () => ignite.AcquireIgniteTopologyProjections();
      act.Should().Throw<TRexException>().WithMessage("GridName name not defined when acquiring topology projection");

      ignite.GridName = null;
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

      var ignite = new BaseIgniteClass
      {
        Role = "TestRole",
        GridName = TRexGrids.GridName(StorageMutability.Immutable)
      };

      Action act = () => ignite.AcquireIgniteTopologyProjections();

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

      var ignite = new BaseIgniteClass
      {
        Role = "TestRole",
        GridName = TRexGrids.GridName(StorageMutability.Immutable)
      };

      Action act = () => ignite.AcquireIgniteTopologyProjections();

      act.Should().Throw<TRexException>().WithMessage($"Cluster group reference is null in AcquireIgniteTopologyProjections for role {ignite.Role} on grid {ignite.GridName}");
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

      var ignite = new BaseIgniteClass
      {
        Role = "TestRole",
        GridName = TRexGrids.GridName(StorageMutability.Immutable)
      };

      Action act = () => ignite.AcquireIgniteTopologyProjections();

      act.Should().Throw<TRexException>().WithMessage($"Group cluster topology is empty for role {ignite.Role} on grid {ignite.GridName}");
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

      var ignite = new BaseIgniteClass
      {
        Role = "TestRole",
        GridName = TRexGrids.GridName(StorageMutability.Immutable)
      };

      Action act = () => ignite.AcquireIgniteTopologyProjections();

      act.Should().Throw<TRexException>().WithMessage($"Compute projection is null in AcquireIgniteTopologyProjections on grid {ignite.GridName}");
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

      var ignite = new BaseIgniteClass
      {
        Role = "TestRole",
        GridName = "TestGrid"
      };

      Action act = () => ignite.AcquireIgniteTopologyProjections();

      act.Should().Throw<TRexException>().WithMessage($"{ignite.GridName} is an unknown grid to create a reference for.");
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

      var ignite = new BaseIgniteClass
      {
        Role = "TestRole",
        GridName = TRexGrids.GridName(StorageMutability.Immutable)
      };

      ignite.AcquireIgniteTopologyProjections();
    }
  }
}
