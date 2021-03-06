﻿using System;
using FluentAssertions;
using VSS.TRex.Analytics.ElevationStatistics;
using VSS.TRex.Analytics.ElevationStatistics.GridFabric;
using VSS.TRex.Common;
using VSS.TRex.Filters;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.ElevationStatistics
{
  public class ElevationStatisticsCoordinatorTests : BaseCoordinatorTests
  {
    private ElevationStatisticsArgument Arg => new ElevationStatisticsArgument()
    {
      ProjectID = _siteModel.ID,
      Filters = new FilterSet(new CombinedFilter())
    };

    private ElevationStatisticsCoordinator _getCoordinator()
    {
      return new ElevationStatisticsCoordinator { RequestDescriptor = Guid.NewGuid(), SiteModel = _siteModel };
    }

    private ElevationStatisticsAggregator _getElevationAggregator()
    {
      var coordinator = _getCoordinator();

      return coordinator.ConstructAggregator(Arg) as ElevationStatisticsAggregator;
    }

    [Fact]
    public void Test_ElevationStatisticsCoordinator_Creation()
    {
      var coordinator = new ElevationStatisticsCoordinator();

      Assert.True(coordinator.SiteModel == null, "Invalid initial value for SiteModel.");
      Assert.True(coordinator.RequestDescriptor == Guid.Empty, "Invalid initial value for RequestDescriptor.");
    }

    [Fact]
    public void Test_ElevationStatisticsCoordinator_ConstructAggregator_Successful()
    {
      var aggregator = _getElevationAggregator();

      Assert.True(aggregator.SiteModelID == Arg.ProjectID, "Invalid aggregator value for SiteModelID.");
      Assert.True(Math.Abs(aggregator.CellSize - _siteModel.CellSize) < Consts.TOLERANCE_DIMENSION, "Invalid aggregator value for CellSize.");
      Assert.True(Math.Abs(aggregator.MinElevation - Consts.MAX_ELEVATION) < Consts.TOLERANCE_HEIGHT, "Invalid aggregator value for MinElevation.");
      Assert.True(Math.Abs(aggregator.MaxElevation - Consts.MIN_ELEVATION) < Consts.TOLERANCE_HEIGHT, "Invalid aggregator value for MaxElevation.");
      Assert.True(aggregator.CellsUsed == 0, "Invalid aggregator value for CellsUsed.");
      Assert.True(aggregator.CellsScanned == 0, "Invalid aggregator value for CellsScanned.");
      Assert.True(!aggregator.BoundingExtents.IsValidPlanExtent, "Aggregator BoundingExtents should be inverted.");
    }

    [Fact]
    public void Test_ElevationCoordinator_ConstructComputor_Successful()
    {
      var aggregator = _getElevationAggregator();
      var coordinator = _getCoordinator();
      var computor = coordinator.ConstructComputor(Arg, aggregator);

      Assert.True(computor.RequestDescriptor == coordinator.RequestDescriptor, "Invalid computor value for RequestDescriptor.");
      Assert.True(computor.SiteModel == coordinator.SiteModel, "Invalid computor value for SiteModel.");
      Assert.True(computor.Aggregator.Equals(aggregator), "Invalid computor value for Aggregator.");
      Assert.True(computor.Filters.Filters.Length == Arg.Filters.Filters.Length, "Invalid computor value for Filters length as different to Arg.");
      Assert.True(computor.Filters.Filters.Length == 1, "Invalid computor value for Filters length.");
      Assert.True(computor.IncludeSurveyedSurfaces, "Invalid computor value for IncludeSurveyedSurfaces.");
      Assert.True(computor.RequestedGridDataType == GridDataType.Height, "Invalid computor value for RequestedGridDataType.");
    }

    [Fact]
    public void Test_ElevationCoordinator_ReadOutResults_Successful()
    {
      var aggregator = _getElevationAggregator();
      var coordinator = _getCoordinator();

      var response = new ElevationStatisticsResponse();

      coordinator.ReadOutResults(aggregator, response);

      Assert.True(Math.Abs(response.CellSize - aggregator.CellSize) < Consts.TOLERANCE_DIMENSION, "CellSize invalid after result read-out.");
      Assert.True(Math.Abs(response.MinElevation - aggregator.MinElevation) < Consts.TOLERANCE_HEIGHT, "Invalid read-out value for MinElevation.");
      Assert.True(Math.Abs(response.MaxElevation - aggregator.MaxElevation) < Consts.TOLERANCE_HEIGHT, "Invalid read-out value for MaxElevation.");
      Assert.True(response.CellsUsed == aggregator.CellsUsed, "Invalid read-out value for CellsUsed.");
      Assert.True(response.CellsScanned == aggregator.CellsScanned, "Invalid read-out value for CellsScanned.");
      Assert.True(!response.BoundingExtents.IsValidPlanExtent, "Response BoundingExtents should be inverted.");
      Assert.True(!aggregator.BoundingExtents.IsValidPlanExtent, "Aggregator BoundingExtents should be inverted.");
      Assert.True(response.BoundingExtents.Equals(aggregator.BoundingExtents), "Aggregator and response BoundingExtents are not equal.");

      response.TotalArea.Should().Be(0);
    }
  }
}
