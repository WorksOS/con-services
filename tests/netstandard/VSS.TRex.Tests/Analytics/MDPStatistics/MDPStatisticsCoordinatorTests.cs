using System;
using VSS.TRex.Analytics.MDPStatistics;
using VSS.TRex.Analytics.MDPStatistics.GridFabric;
using VSS.TRex.Common;
using VSS.TRex.Filters;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.MDPStatistics
{
  public class MDPStatisticsCoordinatorTests : BaseCoordinatorTests
  {
    private MDPStatisticsArgument Arg_Details => new MDPStatisticsArgument()
    {
      ProjectID = _siteModel.ID,
      Filters = new FilterSet(new CombinedFilter()),
      MDPDetailValues = new[] { 1, 5, 10, 15, 20, 25, 31 }
    };

    private MDPStatisticsArgument Arg_Summary => new MDPStatisticsArgument()
    {
      ProjectID = _siteModel.ID,
      Filters = new FilterSet(new CombinedFilter()),
      OverrideMachineMDP = true,
      OverridingMachineMDP = 70
    };
    private MDPStatisticsCoordinator _getCoordinator()
    {
      return new MDPStatisticsCoordinator() { RequestDescriptor = Guid.NewGuid(), SiteModel = _siteModel };
    }

    private MDPStatisticsAggregator _getMDPAggregator(MDPStatisticsArgument arg)
    {
      var coordinator = _getCoordinator();

      return coordinator.ConstructAggregator(arg) as MDPStatisticsAggregator;
    }

    [Fact]
    public void Test_MDPStatisticsCoordinator_Creation()
    {
      var coordinator = new MDPStatisticsCoordinator();

      Assert.True(coordinator.SiteModel == null, "Invalid initial value for SiteModel.");
      Assert.True(coordinator.RequestDescriptor == Guid.Empty, "Invalid initial value for RequestDescriptor.");
    }

    [Fact]
    public void Test_MDPStatisticsCoordinator_ConstructAggregator_Details_Successful()
    {
      var aggregator = _getMDPAggregator(Arg_Details);

      Assert.True(aggregator.RequiresSerialisation, "Invalid aggregator value for RequiresSerialisation.");
      Assert.True(aggregator.SiteModelID == Arg_Details.ProjectID, "Invalid aggregator value for SiteModelID.");
      Assert.True(Math.Abs(aggregator.CellSize - _siteModel.Grid.CellSize) < Consts.TOLERANCE_DIMENSION, "Invalid aggregator value for CellSize.");

      Assert.True(aggregator.DetailsDataValues.Length == Arg_Details.MDPDetailValues.Length, "Invalid aggregator value for DetailsDataValues.Length.");

      for (int i = 0; i < aggregator.Counts.Length; i++)
        Assert.True(aggregator.DetailsDataValues[i] == Arg_Details.MDPDetailValues[i], $"Invalid aggregated value for DetailsDataValues[{i}].");
    }

    [Fact]
    public void Test_MDPStatisticsCoordinator_ConstructAggregator_Summary_Successful()
    {
      var aggregator = _getMDPAggregator(Arg_Summary);

      Assert.True(aggregator.RequiresSerialisation, "Invalid aggregator value for RequiresSerialisation.");
      Assert.True(aggregator.SiteModelID == Arg_Summary.ProjectID, "Invalid aggregator value for SiteModelID.");
      Assert.True(Math.Abs(aggregator.CellSize - _siteModel.Grid.CellSize) < Consts.TOLERANCE_DIMENSION, "Invalid aggregator value for CellSize.");
      Assert.True(aggregator.OverrideMachineMDP == Arg_Summary.OverrideMachineMDP, "Invalid aggregator value for OverrideMachineMDP.");
      Assert.True(aggregator.OverridingMachineMDP == Arg_Summary.OverridingMachineMDP, "Invalid aggregator value for OverridingMachineMDP.");
    }

    [Fact]
    public void Test_MDPStatisticsCoordinator_ConstructComputor_Successful()
    {
      var aggregator = _getMDPAggregator(Arg_Details);
      var coordinator = _getCoordinator();
      var computor = coordinator.ConstructComputor(Arg_Details, aggregator);

      Assert.True(computor.RequestDescriptor == coordinator.RequestDescriptor, "Invalid computor value for RequestDescriptor.");
      Assert.True(computor.SiteModel == coordinator.SiteModel, "Invalid computor value for SiteModel.");
      Assert.True(computor.Aggregator.Equals(aggregator), "Invalid computor value for Aggregator.");
      Assert.True(computor.Filters.Filters.Length == Arg_Details.Filters.Filters.Length, "Invalid computor value for Filters length as different to Arg.");
      Assert.True(computor.Filters.Filters.Length == 1, "Invalid computor value for Filters length.");
      Assert.True(computor.IncludeSurveyedSurfaces, "Invalid computor value for IncludeSurveyedSurfaces.");
      Assert.True(computor.RequestedGridDataType == GridDataType.MDP, "Invalid computor value for RequestedGridDataType.");
    }

    [Fact]
    public void Test_MDPStatisticsCoordinator_ReadOutResults_Details_Successful()
    {
      var aggregator = _getMDPAggregator(Arg_Details);
      var coordinator = _getCoordinator();

      var response = new MDPStatisticsResponse();

      coordinator.ReadOutResults(aggregator, response);

      Assert.True(Math.Abs(response.CellSize - aggregator.CellSize) < Consts.TOLERANCE_DIMENSION, "CellSize invalid after result read-out.");

      Assert.True(response.Counts.Length == aggregator.Counts.Length, "Invalid read-out value for Counts.Length.");

      for (int i = 0; i < response.Counts.Length; i++)
        Assert.True(response.Counts[i] == aggregator.Counts[i], $"Invalid aggregated value for Counts[{i}].");
    }

    public void Test_MDPStatisticsCoordinator_ReadOutResults_Summary_Successful()
    {
      var aggregator = _getMDPAggregator(Arg_Summary);
      var coordinator = _getCoordinator();

      var response = new MDPStatisticsResponse();

      coordinator.ReadOutResults(aggregator, response);

      Assert.True(Math.Abs(response.CellSize - aggregator.CellSize) < Consts.TOLERANCE_DIMENSION, "CellSize invalid after result read-out.");
      Assert.True(response.SummaryCellsScanned == aggregator.SummaryCellsScanned, "Invalid read-out value for SummaryCellsScanned.");
      Assert.True(response.LastTargetMDP == aggregator.LastTargetMDP, "Invalid read-out value for LastTargetMDP.");
      Assert.True(response.CellsScannedOverTarget == aggregator.CellsScannedOverTarget, "Invalid read-out value for CellsScannedOverTarget.");
      Assert.True(response.CellsScannedAtTarget == aggregator.CellsScannedAtTarget, "Invalid read-out value for CellsScannedAtTarget.");
      Assert.True(response.CellsScannedUnderTarget == aggregator.CellsScannedUnderTarget, "Invalid read-out value for CellsScannedUnderTarget.");
      Assert.True(response.IsTargetValueConstant == aggregator.IsTargetValueConstant, "Invalid read-out value for IsTargetValueConstant.");
      Assert.True(response.MissingTargetValue == aggregator.MissingTargetValue, "Invalid initial read-out for MissingTargetValue.");
    }
  }
}
