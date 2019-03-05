﻿using System;
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
    public void AcquireIgniteTopologyProjections_FailWithNullRole()
    {
      var ignite = new BaseIgniteClass
      {
        Role = "",
        GridName = TRexGrids.GridName(StorageMutability.Immutable)
      };

      Action act = () => ignite.AcquireIgniteTopologyProjections();

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
        Role = "TestTole",
        GridName = TRexGrids.GridName(StorageMutability.Immutable)
      };

      Action act = () => ignite.AcquireIgniteTopologyProjections();

      act.Should().Throw<TRexException>().WithMessage("Ignite reference is null in AcquireIgniteTopologyProjections");
    }

    [Fact]
    public void AcquireIgniteTopologyProjections_FailWithNullClusterGroup()
    {
      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructIgniteMock(
        out var mockCompute,
        out var mockClusterNode,
        out var mockClusterNodes,
        out var mockMessaging,
        out var mockClusterGroup,
        out var mockCluster,
        out var mockIgnite
      );

      // Nobble the Ignite.GetCluster setup to mimic no available cluster group

      mockIgnite.Setup(x => x.GetCluster()).Returns((ICluster) null);

      DIBuilder.Continue()
        .Add(TRexGridFactory.AddGridFactoriesToDI)

        // Override the main Ignite grid factory method DI'ed from TRexGridFactory.AddGridFactoriesToDI()
        .Add(x => x.AddSingleton<Func<string, IgniteConfiguration, IIgnite>>(factory => (gridName, cfg) => mockIgnite.Object))
        .Complete();

      var ignite = new BaseIgniteClass
      {
        Role = "TestTole",
        GridName = TRexGrids.GridName(StorageMutability.Immutable)
      };

      Action act = () => ignite.AcquireIgniteTopologyProjections();

      act.Should().Throw<TRexException>().WithMessage($"Cluster group reference is null in AcquireIgniteTopologyProjections for role {ignite.Role} on grid {ignite.GridName}");
    }

    [Fact]
    public void AcquireIgniteTopologyProjections_FailWithEmptyClusterGroup()
    {
      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructIgniteMock(
        out var mockCompute,
        out var mockClusterNode,
        out var mockClusterNodes,
        out var mockMessaging,
        out var mockClusterGroup,
        out var mockCluster,
        out var mockIgnite
      );

      // Nobble the IClusterNodes.Count() setup to mimic an empty cluster group

      mockClusterNodes.Setup(x => x.Count).Returns(0);

      DIBuilder.Continue()
        .Add(TRexGridFactory.AddGridFactoriesToDI)

        // Override the main Ignite grid factory method DI'ed from TRexGridFactory.AddGridFactoriesToDI()
        .Add(x => x.AddSingleton<Func<string, IgniteConfiguration, IIgnite>>(factory => (gridName, cfg) => mockIgnite.Object))
        .Complete();

      var ignite = new BaseIgniteClass
      {
        Role = "TestTole",
        GridName = TRexGrids.GridName(StorageMutability.Immutable)
      };

      Action act = () => ignite.AcquireIgniteTopologyProjections();

      act.Should().Throw<TRexException>().WithMessage($"Group cluster topology is empty for role {ignite.Role} on grid {ignite.GridName}");
    }

    [Fact]
    public void AcquireIgniteTopologyProjections_FailWithNullComputeProjection()
    {
      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructIgniteMock(
        out var mockCompute,
        out var mockClusterNode,
        out var mockClusterNodes,
        out var mockMessaging,
        out var mockClusterGroup,
        out var mockCluster,
        out var mockIgnite
      );

      // Nobble the ICompute setup to mimic a null compute projection
      mockClusterGroup.Setup(x => x.GetCompute()).Returns((ICompute) null);

      DIBuilder.Continue()
        .Add(TRexGridFactory.AddGridFactoriesToDI)

        // Override the main Ignite grid factory method DI'ed from TRexGridFactory.AddGridFactoriesToDI()
        .Add(x => x.AddSingleton<Func<string, IgniteConfiguration, IIgnite>>(factory => (gridName, cfg) => mockIgnite.Object))
        .Complete();

      var ignite = new BaseIgniteClass
      {
        Role = "TestTole",
        GridName = TRexGrids.GridName(StorageMutability.Immutable)
      };

      Action act = () => ignite.AcquireIgniteTopologyProjections();

      act.Should().Throw<TRexException>().WithMessage($"Compute projection is null in AcquireIgniteTopologyProjections on grid {ignite.GridName}");
    }

    [Fact]
    public void AcquireIgniteTopologyProjections_FailWithUnknownGrid()
    {
      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructIgniteMock(
        out var mockCompute,
        out var mockClusterNode,
        out var mockClusterNodes,
        out var mockMessaging,
        out var mockClusterGroup,
        out var mockCluster,
        out var mockIgnite
      );

      DIBuilder.Continue()
        .Add(TRexGridFactory.AddGridFactoriesToDI)

        // Override the main Ignite grid factory method DI'ed from TRexGridFactory.AddGridFactoriesToDI()
        .Add(x => x.AddSingleton<Func<string, IgniteConfiguration, IIgnite>>(factory => (gridName, cfg) => mockIgnite.Object))
        //.Add(x => x.AddSingleton(mockIgnite))
        .Complete();

      var ignite = new BaseIgniteClass
      {
        Role = "TestTole",
        GridName = "TestGrid"
      };

      Action act = () => ignite.AcquireIgniteTopologyProjections();

      act.Should().Throw<TRexException>().WithMessage($"{ignite.GridName} is an unknown grid to create a reference for.");
    }

    [Fact]
    public void AcquireIgniteTopologyProjections_Success()
    {
      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructIgniteMock(
        out var mockCompute,
        out var mockClusterNode,
        out var mockClusterNodes,
        out var mockMessaging,
        out var mockClusterGroup,
        out var mockCluster,
        out var mockIgnite
      );

      DIBuilder.Continue()
        .Add(TRexGridFactory.AddGridFactoriesToDI)

        // Override the main Ignite grid factory method DI'ed from TRexGridFactory.AddGridFactoriesToDI()
        .Add(x => x.AddSingleton<Func<string, IgniteConfiguration, IIgnite>>(factory => (gridName, cfg) => mockIgnite.Object))
        .Complete();

      var ignite = new BaseIgniteClass
      {
        Role = "TestTole",
        GridName = TRexGrids.GridName(StorageMutability.Immutable)
      };

      ignite.AcquireIgniteTopologyProjections();
    }
  }
}
