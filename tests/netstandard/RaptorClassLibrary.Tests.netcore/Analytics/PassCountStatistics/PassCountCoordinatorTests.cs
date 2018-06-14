using System;
using VSS.TRex.Analytics.PassCountStatistics;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Tests.netcore.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.PassCountStatistics
{
  public class PassCountCoordinatorTests : BaseCoordinatorTests
  {
    private PassCountStatisticsArgument Arg => new PassCountStatisticsArgument()
    {
      ProjectID = _siteModel.ID,
      Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
      OverrideTargetPassCount = true,
      OverridingTargetPassCountRange = new PassCountRangeRecord(3, 10)
    };

    private PassCountCoordinator _getCoordinator()
    {
      return new PassCountCoordinator() { RequestDescriptor = Guid.NewGuid(), SiteModel = _siteModel };
    }

    private PassCountAggregator _getPassCountAggregator()
    {
      var coordinator = _getCoordinator();

      return coordinator.ConstructAggregator(Arg) as PassCountAggregator;
    }

    [Fact]
    public void Test_PassCountCoordinator_Creation()
    {
      var coordinator = new PassCountCoordinator();

      Assert.True(coordinator.SiteModel == null, "Invalid initial value for SiteModel.");
      Assert.True(coordinator.RequestDescriptor == Guid.Empty, "Invalid initial value for RequestDescriptor.");
    }

    [Fact]
    public void Test_PassCountCoordinator_ConstructAggregator_Successful()
    {
      var aggregator = _getPassCountAggregator();

      Assert.True(aggregator.RequiresSerialisation, "Invalid aggregator value for RequiresSerialisation.");
      Assert.True(aggregator.SiteModelID == Arg.ProjectID, "Invalid aggregator value for SiteModelID.");
      Assert.True(Math.Abs(aggregator.CellSize - _siteModel.Grid.CellSize) < TOLERANCE, "Invalid aggregator value for CellSize.");
      Assert.True(aggregator.OverrideTargetPassCount == Arg.OverrideTargetPassCount, "Invalid aggregator value for OverrideTargetPassCount.");
      Assert.True(aggregator.OverridingTargetPassCountRange.Min == Arg.OverridingTargetPassCountRange.Min, "Invalid aggregator value for OverridingTargetPassCountRange.Min.");
      Assert.True(aggregator.OverridingTargetPassCountRange.Max == Arg.OverridingTargetPassCountRange.Max, "Invalid aggregator value for OverridingTargetPassCountRange.Max.");
    }

    [Fact]
    public void Test_PassCountCoordinator_ConstructComputor_Successful()
    {
      var aggregator = _getPassCountAggregator();
      var coordinator = _getCoordinator();
      var computor = coordinator.ConstructComputor(Arg, aggregator);

      Assert.True(computor.RequestDescriptor == coordinator.RequestDescriptor, "Invalid computor value for RequestDescriptor.");
      Assert.True(computor.SiteModel == coordinator.SiteModel, "Invalid computor value for SiteModel.");
      Assert.True(computor.Aggregator.Equals(aggregator), "Invalid computor value for Aggregator.");
      Assert.True(computor.Filters.Filters.Length == Arg.Filters.Filters.Length, "Invalid computor value for Filters length as different to Arg.");
      Assert.True(computor.Filters.Filters.Length == 1, "Invalid computor value for Filters length.");
      Assert.True(computor.IncludeSurveyedSurfaces, "Invalid computor value for IncludeSurveyedSurfaces.");
      Assert.True(computor.RequestedGridDataType == GridDataType.PassCount, "Invalid computor value for RequestedGridDataType.");
    }

    [Fact]
    public void Test_PassCountCoordinator_ReadOutResults_Successful()
    {
      var aggregator = _getPassCountAggregator();
      var coordinator = _getCoordinator();

      var response = new PassCountStatisticsResponse();

      coordinator.ReadOutResults(aggregator, response);

      Assert.True(Math.Abs(response.CellSize - aggregator.CellSize) < TOLERANCE, "CellSize invalid after result read-out.");
      Assert.True(response.SummaryCellsScanned == aggregator.SummaryCellsScanned, "Invalid read-out value for SummaryCellsScanned.");
      Assert.True(response.LastPassCountTargetRange.Min == aggregator.LastPassCountTargetRange.Min, "Invalid read-out value for LastPassCountTargetRange.Min.");
      Assert.True(response.LastPassCountTargetRange.Max == aggregator.LastPassCountTargetRange.Max, "Invalid read-out value for LastPassCountTargetRange.Max.");
      Assert.True(response.CellsScannedOverTarget == aggregator.CellsScannedOverTarget, "Invalid read-out value for CellsScannedOverTarget.");
      Assert.True(response.CellsScannedAtTarget == aggregator.CellsScannedAtTarget, "Invalid read-out value for CellsScannedAtTarget.");
      Assert.True(response.CellsScannedUnderTarget == aggregator.CellsScannedUnderTarget, "Invalid read-out value for CellsScannedUnderTarget.");
      Assert.True(response.IsTargetValueConstant == aggregator.IsTargetValueConstant, "Invalid read-out value for IsTargetValueConstant.");
      Assert.True(response.MissingTargetValue == aggregator.MissingTargetValue, "Invalid initial read-out for MissingTargetValue.");
    }
  }
}
