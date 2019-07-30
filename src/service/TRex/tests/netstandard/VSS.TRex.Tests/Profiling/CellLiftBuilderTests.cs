using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using VSS.TRex.Cells;
using VSS.TRex.Common.Models;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Models;
using VSS.TRex.Profiling;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Profiling
{
  public class CellLiftBuilderTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      var builder = new CellLiftBuilder(siteModel, GridDataType.Height, new FilteredValuePopulationControl(),
        new FilterSet(new CombinedFilter()), new CellPassFastEventLookerUpper(siteModel));

      builder.Should().NotBeNull();
    }

    [Fact]
    public void BuildLiftsForSinglePassCell()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

     var cellPasses = new []
      {
        new CellPass
        {
          Time = DateTime.UtcNow,
          Height = 1.0f
        }
      };

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      IClientLeafSubGrid clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;

      var serverGrid = TRex.SubGridTrees.Server.Utilities.SubGridUtilities.LocateSubGridContaining(
        siteModel.PrimaryStorageProxy, siteModel.Grid,
        SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        siteModel.Grid.NumLevels, false, false) as IServerLeafSubGrid;

      var builder = new CellLiftBuilder(siteModel, GridDataType.Height, new FilteredValuePopulationControl(),
        new FilterSet(new CombinedFilter()), new CellPassFastEventLookerUpper(siteModel));

      var cell = new ProfileCell();
      var segmentIterator = new SubGridSegmentIterator(serverGrid, serverGrid.Directory, siteModel.PrimaryStorageProxy);
      var cellPassIterator = new SubGridSegmentCellPassIterator_NonStatic(segmentIterator);

      builder.Build(cell, new LiftParameters(), clientGrid, new FilteredValueAssignmentContext(), cellPassIterator, false).Should().BeTrue();

      cell.Layers.Count().Should().Be(1);
      cell.Layers[0].PassCount.Should().Be(1);
      cell.Layers[0].MinimumPassHeight.Should().Be(1);
      cell.Layers[0].MaximumPassHeight.Should().Be(1);
      cell.Layers[0].FirstPassHeight.Should().Be(1);
      cell.Layers[0].LastPassHeight.Should().Be(1);
    }

    [Theory]
    [InlineData(1, 2, 3, 1, 3, 1, 3)]
    [InlineData(3, 2, 1, 3, 1, 1, 3)]
    [InlineData(1, 3, 2, 1, 2, 1, 3)]
    public void BuildLiftsForMultiplePassCell(float height1, float height2, float height3, float first, float last, float lowest, float heighest)
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var baseTime = DateTime.UtcNow;
      var cellPasses = new[]
       {
        new CellPass
        {
          Time = baseTime,
          Height = height1
        },
        new CellPass
        {
          Time = baseTime.AddHours(1),
          Height = height2
        },
        new CellPass
        {
          Time = baseTime.AddHours(2),
          Height = height3
        }
      };

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      IClientLeafSubGrid clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.Height) as ClientHeightLeafSubGrid;

      var serverGrid = TRex.SubGridTrees.Server.Utilities.SubGridUtilities.LocateSubGridContaining(
        siteModel.PrimaryStorageProxy, siteModel.Grid,
        SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        siteModel.Grid.NumLevels, false, false) as IServerLeafSubGrid;

      var builder = new CellLiftBuilder(siteModel, GridDataType.Height, new FilteredValuePopulationControl(),
        new FilterSet(new CombinedFilter()), new CellPassFastEventLookerUpper(siteModel));

      var cell = new ProfileCell();

      var segmentIterator = new SubGridSegmentIterator(serverGrid, serverGrid.Directory, siteModel.PrimaryStorageProxy);
      var cellPassIterator = new SubGridSegmentCellPassIterator_NonStatic(segmentIterator);

      builder.Build(cell, new LiftParameters(), clientGrid, new FilteredValueAssignmentContext(), cellPassIterator, false).Should().BeTrue();

      cell.Layers.Count().Should().Be(1);
      cell.Layers[0].PassCount.Should().Be(3);
      cell.Layers[0].MinimumPassHeight.Should().Be(lowest);
      cell.Layers[0].MaximumPassHeight.Should().Be(heighest);
      cell.Layers[0].FirstPassHeight.Should().Be(first);
      cell.Layers[0].LastPassHeight.Should().Be(last);
    }

    [Theory]
    [InlineData(new []{10.0F, 20.0F, 30.0F, 40.0F}, new short[]{35, 45, 65, 75})]

    public void BuildLiftsForSinglePassCell_CMVPercentChange(float[] heights, short[] cmvs)
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var baseTime = DateTime.UtcNow;

      var cellPasses = new[]
      {
        new CellPass
        {
          Time = baseTime,
          Height = heights[0],
          CCV = cmvs[0]
        },
        new CellPass
        {
          Time = baseTime.AddHours(1),
          Height = heights[1],
          CCV = cmvs[1]
        },
        new CellPass
        {
          Time = baseTime.AddHours(2),
          Height = heights[2],
          CCV = cmvs[2]
        },
        new CellPass
        {
          Time = baseTime.AddHours(3),
          Height = heights[3],
          CCV = cmvs[3]
        }
      };

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      IClientLeafSubGrid clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CCVPercentChange) as ClientCMVLeafSubGrid;
      
      var serverGrid = TRex.SubGridTrees.Server.Utilities.SubGridUtilities.LocateSubGridContaining(
        siteModel.PrimaryStorageProxy, siteModel.Grid,
        SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        siteModel.Grid.NumLevels, false, false) as IServerLeafSubGrid;

      var builder = new CellLiftBuilder(siteModel, GridDataType.CCVPercentChange, new FilteredValuePopulationControl(),
        new FilterSet(new CombinedFilter()), new CellPassFastEventLookerUpper(siteModel));

      var cell = new ProfileCell();

      var segmentIterator = new SubGridSegmentIterator(serverGrid, serverGrid.Directory, siteModel.PrimaryStorageProxy);
      var cellPassIterator = new SubGridSegmentCellPassIterator_NonStatic(segmentIterator);

      var filteredValueAssignmentContext = new FilteredValueAssignmentContext();

      builder.Build(cell, new LiftParameters(), clientGrid, filteredValueAssignmentContext, cellPassIterator, true).Should().BeTrue();

      cell.Layers.Count().Should().Be(1);
      cell.Layers[0].PassCount.Should().Be(4);
      filteredValueAssignmentContext.FilteredValue.FilteredPassData.FilteredPass.CCV.Should().Be(cmvs[cmvs.Length - 1]);
      filteredValueAssignmentContext.PreviousFilteredValue.FilteredPassData.FilteredPass.CCV.Should().Be(cmvs[cmvs.Length - 2]);
    }
  }
}
