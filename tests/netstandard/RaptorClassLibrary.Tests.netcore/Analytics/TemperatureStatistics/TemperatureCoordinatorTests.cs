using System;
using VSS.TRex.Analytics.TemperatureStatistics;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Tests.netcore.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.netcore.Analytics.TemperatureStatistics
{
  public class TemperatureCoordinatorTests : BaseCoordinatorTests
  {
    private TemperatureStatisticsArgument Arg => new TemperatureStatisticsArgument()
    {
      ProjectID = _siteModel.ID,
      Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
      OverrideTemperatureWarningLevels = true,
      OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord(10, 150)
    };

    private TemperatureCoordinator _getCoordinator()
    {
      return new TemperatureCoordinator() { RequestDescriptor = Guid.NewGuid(), SiteModel = _siteModel };
    }

    private TemperatureAggregator _getTemperatureAggregator()
    {
      var coordinator = _getCoordinator();

      return coordinator.ConstructAggregator(Arg) as TemperatureAggregator;
    }

    [Fact]
    public void Test_TemperatureCoordinator_Creation()
    {
      var coordinator = new TemperatureCoordinator();

      Assert.True(coordinator.SiteModel == null, "Invalid initial value for SiteModel.");
      Assert.True(coordinator.RequestDescriptor == Guid.Empty, "Invalid initial value for RequestDescriptor.");
    }

    [Fact]
    public void Test_TemperatureCoordinator_ConstructAggregator_Successful()
    {
      var aggregator = _getTemperatureAggregator();

      Assert.True(aggregator.RequiresSerialisation, "Invalid aggregator value for RequiresSerialisation.");
      Assert.True(aggregator.SiteModelID == Arg.ProjectID, "Invalid aggregator value for SiteModelID.");
      Assert.True(Math.Abs(aggregator.CellSize - _siteModel.Grid.CellSize) < TOLERANCE, "Invalid aggregator value for CellSize.");
      Assert.True(aggregator.OverrideTemperatureWarningLevels == Arg.OverrideTemperatureWarningLevels, "Invalid aggregator value for OverrideTemperatureWarningLevels.");
      Assert.True(aggregator.OverridingTemperatureWarningLevels.Max == Arg.OverridingTemperatureWarningLevels.Max, "Invalid aggregator value for OverridingTemperatureWarningLevels.Max.");
      Assert.True(aggregator.OverridingTemperatureWarningLevels.Min == Arg.OverridingTemperatureWarningLevels.Min, "Invalid aggregator value for OverridingTemperatureWarningLevels.Min.");
    }

    [Fact]
    public void Test_TemperatureCoordinator_ConstructComputor_Successful()
    {
      var aggregator = _getTemperatureAggregator();
      var coordinator = _getCoordinator();
      var computor = coordinator.ConstructComputor(Arg, aggregator);

      Assert.True(computor.RequestDescriptor == coordinator.RequestDescriptor, "Invalid computor value for RequestDescriptor.");
      Assert.True(computor.SiteModel == coordinator.SiteModel, "Invalid computor value for SiteModel.");
      Assert.True(computor.Aggregator.Equals(aggregator), "Invalid computor value for Aggregator.");
      //Assert.True(computor.Filters.Equals(Arg.Filters), "Invalid computor value for Filters.");
      Assert.True(computor.Filters.Filters.Length == Arg.Filters.Filters.Length, "Invalid computor value for Filters length as different to Arg.");
      Assert.True(computor.Filters.Filters.Length == 1, "Invalid computor value for Filters length.");
      //Assert.True(computor.Filters.Filters[0].AttributeFilter.Equals(Arg.Filters.Filters[0].AttributeFilter), "Invalid computor value for Filters.Filters[0].AttributeFilter.");
      //Assert.True(computor.Filters.Filters[0].SpatialFilter.Equals(Arg.Filters.Filters[0].SpatialFilter), "Invalid computor value for Filters.Filters[0].SpatialFilter.");
      Assert.True(computor.IncludeSurveyedSurfaces, "Invalid computor value for IncludeSurveyedSurfaces.");
      Assert.True(computor.RequestedGridDataType == GridDataType.Temperature, "Invalid computor value for RequestedGridDataType.");
    }

    [Fact]
    public void Test_TemperatureCoordinator_ReadOutResults_Successful()
    {
      var aggregator = _getTemperatureAggregator();
      var coordinator = _getCoordinator();

      var response = new TemperatureStatisticsResponse();

      coordinator.ReadOutResults(aggregator, response);

      Assert.True(Math.Abs(response.CellSize - aggregator.CellSize) < TOLERANCE, "CellSize invalid after result read-out.");
      Assert.True(response.SummaryCellsScanned == aggregator.SummaryCellsScanned, "Invalid read-out value for SummaryCellsScanned.");
      Assert.True(response.LastTempRangeMax == aggregator.LastTempRangeMax, "Invalid read-out value for LastTempRangeMax.");
      Assert.True(response.LastTempRangeMin == aggregator.LastTempRangeMin, "Invalid read-out value for LastTempRangeMin.");
      Assert.True(response.CellsScannedOverTarget == aggregator.CellsScannedOverTarget, "Invalid read-out value for CellsScannedOverTarget.");
      Assert.True(response.CellsScannedAtTarget == aggregator.CellsScannedAtTarget, "Invalid read-out value for CellsScannedAtTarget.");
      Assert.True(response.CellsScannedUnderTarget == aggregator.CellsScannedUnderTarget, "Invalid read-out value for CellsScannedUnderTarget.");
      Assert.True(response.IsTargetValueConstant == aggregator.IsTargetValueConstant, "Invalid read-out value for IsTargetValueConstant.");
      Assert.True(response.MissingTargetValue == aggregator.MissingTargetValue, "Invalid initial read-out for MissingTargetValue.");
    }
  }
}
