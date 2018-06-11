using System;
using VSS.TRex.Analytics.MDPStatistics;
using VSS.TRex.Analytics.MDPStatistics.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Tests.netcore.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.MDPStatistics
{
  public class MDPCoordinatorTests : BaseCoordinatorTests
  {
    private MDPStatisticsArgument Arg => new MDPStatisticsArgument()
    {
      ProjectID = _siteModel.ID,
      Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
      OverrideMachineMDP = true,
      OverridingMachineMDP = 70
    };

    private MDPCoordinator _getCoordinator()
    {
      return new MDPCoordinator() { RequestDescriptor = Guid.NewGuid(), SiteModel = _siteModel };
    }

    private MDPAggregator _getMDPAggregator()
    {
      var coordinator = _getCoordinator();

      return coordinator.ConstructAggregator(Arg) as MDPAggregator;
    }

    [Fact]
    public void Test_MDPCoordinator_Creation()
    {
      var coordinator = new MDPCoordinator();

      Assert.True(coordinator.SiteModel == null, "Invalid initial value for SiteModel.");
      Assert.True(coordinator.RequestDescriptor == Guid.Empty, "Invalid initial value for RequestDescriptor.");
    }

    [Fact]
    public void Test_MDPCoordinator_ConstructAggregator_Successful()
    {
      var aggregator = _getMDPAggregator();

      Assert.True(aggregator.RequiresSerialisation, "Invalid aggregator value for RequiresSerialisation.");
      Assert.True(aggregator.SiteModelID == Arg.ProjectID, "Invalid aggregator value for SiteModelID.");
      Assert.True(Math.Abs(aggregator.CellSize - _siteModel.Grid.CellSize) < TOLERANCE, "Invalid aggregator value for CellSize.");
      Assert.True(aggregator.OverrideMachineMDP == Arg.OverrideMachineMDP, "Invalid aggregator value for OverrideMachineMDP.");
      Assert.True(aggregator.OverridingMachineMDP == Arg.OverridingMachineMDP, "Invalid aggregator value for OverridingMachineMDP.");
    }

    [Fact]
    public void Test_MDPCoordinator_ConstructComputor_Successful()
    {
      var aggregator = _getMDPAggregator();
      var coordinator = _getCoordinator();
      var computor = coordinator.ConstructComputor(Arg, aggregator);

      Assert.True(computor.RequestDescriptor == coordinator.RequestDescriptor, "Invalid computor value for RequestDescriptor.");
      Assert.True(computor.SiteModel == coordinator.SiteModel, "Invalid computor value for SiteModel.");
      Assert.True(computor.Aggregator.Equals(aggregator), "Invalid computor value for Aggregator.");
      Assert.True(computor.Filters.Filters.Length == Arg.Filters.Filters.Length, "Invalid computor value for Filters length as different to Arg.");
      Assert.True(computor.Filters.Filters.Length == 1, "Invalid computor value for Filters length.");
      Assert.True(computor.IncludeSurveyedSurfaces, "Invalid computor value for IncludeSurveyedSurfaces.");
      Assert.True(computor.RequestedGridDataType == GridDataType.MDP, "Invalid computor value for RequestedGridDataType.");
    }

    [Fact]
    public void Test_MDPCoordinator_ReadOutResults_Successful()
    {
      var aggregator = _getMDPAggregator();
      var coordinator = _getCoordinator();

      var response = new MDPStatisticsResponse();

      coordinator.ReadOutResults(aggregator, response);

      Assert.True(Math.Abs(response.CellSize - aggregator.CellSize) < TOLERANCE, "CellSize invalid after result read-out.");
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
