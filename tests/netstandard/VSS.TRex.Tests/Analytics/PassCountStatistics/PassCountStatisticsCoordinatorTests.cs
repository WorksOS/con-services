using System;
using VSS.TRex.Analytics.PassCountStatistics;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Common;
using VSS.TRex.Filters;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.PassCountStatistics
{
  public class PassCountStatisticsCoordinatorTests : BaseCoordinatorTests
  {
    private PassCountStatisticsArgument Arg_Details => new PassCountStatisticsArgument()
    {
      ProjectID = _siteModel.ID,
      Filters = new FilterSet(new CombinedFilter()),
      PassCountDetailValues = new[] { 1, 5, 10, 15, 20, 25, 31 }
    };

    private PassCountStatisticsArgument Arg_Summary => new PassCountStatisticsArgument()
    {
      ProjectID = _siteModel.ID,
      Filters = new FilterSet(new CombinedFilter()),
      OverrideTargetPassCount = true,
      OverridingTargetPassCountRange = new PassCountRangeRecord(3, 10),
    };

    private PassCountStatisticsCoordinator _getCoordinator()
    {
      return new PassCountStatisticsCoordinator() { RequestDescriptor = Guid.NewGuid(), SiteModel = _siteModel };
    }

    private PassCountStatisticsAggregator _getPassCountAggregator(PassCountStatisticsArgument arg)
    {
      var coordinator = _getCoordinator();

      return coordinator.ConstructAggregator(arg) as PassCountStatisticsAggregator;
    }

    [Fact]
    public void Test_PassCountStatisticsCoordinator_Creation()
    {
      var coordinator = new PassCountStatisticsCoordinator();

      Assert.True(coordinator.SiteModel == null, "Invalid initial value for SiteModel.");
      Assert.True(coordinator.RequestDescriptor == Guid.Empty, "Invalid initial value for RequestDescriptor.");
    }

    [Fact]
    public void Test_PassCountStatisticsCoordinator_ConstructAggregator_Details_Successful()
    {
      var aggregator = _getPassCountAggregator(Arg_Details);

      Assert.True(aggregator.RequiresSerialisation, "Invalid aggregator value for RequiresSerialisation.");
      Assert.True(aggregator.SiteModelID == Arg_Details.ProjectID, "Invalid aggregator value for SiteModelID.");
      Assert.True(Math.Abs(aggregator.CellSize - _siteModel.Grid.CellSize) < Consts.TOLERANCE_DIMENSION, "Invalid aggregator value for CellSize.");

      Assert.True(aggregator.DetailsDataValues.Length == Arg_Details.PassCountDetailValues.Length, "Invalid aggregator value for DetailsDataValues.Length.");

      for (int i = 0; i < aggregator.Counts.Length; i++)
        Assert.True(aggregator.DetailsDataValues[i] == Arg_Details.PassCountDetailValues[i], $"Invalid aggregated value for DetailsDataValues[{i}].");
    }

    [Fact]
    public void Test_PassCountStatisticsCoordinator_ConstructAggregator_Summary_Successful()
    {
      var aggregator = _getPassCountAggregator(Arg_Summary);

      Assert.True(aggregator.RequiresSerialisation, "Invalid aggregator value for RequiresSerialisation.");
      Assert.True(aggregator.SiteModelID == Arg_Summary.ProjectID, "Invalid aggregator value for SiteModelID.");
      Assert.True(Math.Abs(aggregator.CellSize - _siteModel.Grid.CellSize) < Consts.TOLERANCE_DIMENSION, "Invalid aggregator value for CellSize.");
      Assert.True(aggregator.OverrideTargetPassCount == Arg_Summary.OverrideTargetPassCount, "Invalid aggregator value for OverrideTargetPassCount.");
      Assert.True(aggregator.OverridingTargetPassCountRange.Min == Arg_Summary.OverridingTargetPassCountRange.Min, "Invalid aggregator value for OverridingTargetPassCountRange.Min.");
      Assert.True(aggregator.OverridingTargetPassCountRange.Max == Arg_Summary.OverridingTargetPassCountRange.Max, "Invalid aggregator value for OverridingTargetPassCountRange.Max.");
    }

    [Fact]
    public void Test_PassCountStatisticsCoordinator_ConstructComputor_Successful()
    {
      var aggregator = _getPassCountAggregator(Arg_Details);
      var coordinator = _getCoordinator();
      var computor = coordinator.ConstructComputor(Arg_Details, aggregator);

      Assert.True(computor.RequestDescriptor == coordinator.RequestDescriptor, "Invalid computor value for RequestDescriptor.");
      Assert.True(computor.SiteModel == coordinator.SiteModel, "Invalid computor value for SiteModel.");
      Assert.True(computor.Aggregator.Equals(aggregator), "Invalid computor value for Aggregator.");
      Assert.True(computor.Filters.Filters.Length == Arg_Details.Filters.Filters.Length, "Invalid computor value for Filters length as different to Arg.");
      Assert.True(computor.Filters.Filters.Length == 1, "Invalid computor value for Filters length.");
      Assert.True(computor.IncludeSurveyedSurfaces, "Invalid computor value for IncludeSurveyedSurfaces.");
      Assert.True(computor.RequestedGridDataType == GridDataType.PassCount, "Invalid computor value for RequestedGridDataType.");
    }

    [Fact]
    public void Test_PassCountStatisticsCoordinator_ReadOutResults_Details_Successful()
    {
      var aggregator = _getPassCountAggregator(Arg_Details);
      var coordinator = _getCoordinator();

      var response = new PassCountStatisticsResponse();

      coordinator.ReadOutResults(aggregator, response);

      Assert.True(Math.Abs(response.CellSize - aggregator.CellSize) < Consts.TOLERANCE_DIMENSION, "CellSize invalid after result read-out.");

      Assert.True(response.Counts.Length == aggregator.Counts.Length, "Invalid read-out value for Counts.Length.");

      for (int i = 0; i < response.Counts.Length; i++)
        Assert.True(response.Counts[i] == aggregator.Counts[i], $"Invalid aggregated value for Counts[{i}].");
    }

    [Fact]
    public void Test_PassCountStatisticsCoordinator_ReadOutResults_Sumary_Successful()
    {
      var aggregator = _getPassCountAggregator(Arg_Summary);
      var coordinator = _getCoordinator();

      var response = new PassCountStatisticsResponse();

      coordinator.ReadOutResults(aggregator, response);

      Assert.True(Math.Abs(response.CellSize - aggregator.CellSize) < Consts.TOLERANCE_DIMENSION, "CellSize invalid after result read-out.");
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
