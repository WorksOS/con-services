using System;
using VSS.TRex.Analytics.CMVStatistics;
using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Common;
using VSS.TRex.Filters;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CMVStatistics
{
  public class CMVStatisticsCoordinatorTests : BaseCoordinatorTests
  {
    private CMVStatisticsArgument Arg_Details => new CMVStatisticsArgument()
    {
      ProjectID = _siteModel.ID,
      Filters = new FilterSet(new CombinedFilter()),
      CMVDetailValues = new[] { 1, 5, 10, 15, 20, 25, 31 }
    };

    private CMVStatisticsArgument Arg_Summary => new CMVStatisticsArgument()
    {
      ProjectID = _siteModel.ID,
      Filters = new FilterSet(new CombinedFilter()),
      OverrideMachineCMV = true,
      OverridingMachineCMV = 70
    };

    private CMVStatisticsCoordinator _getCoordinator()
    {
      return new CMVStatisticsCoordinator() { RequestDescriptor = Guid.NewGuid(), SiteModel = _siteModel };
    }

    private CMVStatisticsAggregator _getCMVAggregator(CMVStatisticsArgument arg)
    {
      var coordinator = _getCoordinator();

      return coordinator.ConstructAggregator(arg) as CMVStatisticsAggregator;
    }

    [Fact]
    public void Test_CMVStatisticsCoordinator_Creation()
    {
      var coordinator = new CMVStatisticsCoordinator();

      Assert.True(coordinator.SiteModel == null, "Invalid initial value for SiteModel.");
      Assert.True(coordinator.RequestDescriptor == Guid.Empty, "Invalid initial value for RequestDescriptor.");
    }

    [Fact]
    public void Test_CMVStatisticsCoordinator_ConstructAggregator_Details_Successful()
    {
      var aggregator = _getCMVAggregator(Arg_Details);

      Assert.True(aggregator.RequiresSerialisation, "Invalid aggregator value for RequiresSerialisation.");
      Assert.True(aggregator.SiteModelID == Arg_Details.ProjectID, "Invalid aggregator value for SiteModelID.");
      Assert.True(Math.Abs(aggregator.CellSize - _siteModel.Grid.CellSize) < Consts.TOLERANCE_DIMENSION, "Invalid aggregator value for CellSize.");

      Assert.True(aggregator.DetailsDataValues.Length == Arg_Details.CMVDetailValues.Length, "Invalid aggregator value for DetailsDataValues.Length.");

      for (int i = 0; i < aggregator.Counts.Length; i++)
        Assert.True(aggregator.DetailsDataValues[i] == Arg_Details.CMVDetailValues[i], $"Invalid aggregated value for DetailsDataValues[{i}].");
    }

    [Fact]
    public void Test_CMVStatisticsCoordinator_ConstructAggregator_Summary_Successful()
    {
      var aggregator = _getCMVAggregator(Arg_Summary);

      Assert.True(aggregator.RequiresSerialisation, "Invalid aggregator value for RequiresSerialisation.");
      Assert.True(aggregator.SiteModelID == Arg_Summary.ProjectID, "Invalid aggregator value for SiteModelID.");
      Assert.True(Math.Abs(aggregator.CellSize - _siteModel.Grid.CellSize) < Consts.TOLERANCE_DIMENSION, "Invalid aggregator value for CellSize.");
      Assert.True(aggregator.OverrideMachineCMV == Arg_Summary.OverrideMachineCMV, "Invalid aggregator value for OverrideMachineCMV.");
      Assert.True(aggregator.OverridingMachineCMV == Arg_Summary.OverridingMachineCMV, "Invalid aggregator value for OverridingMachineCMV.");
    }

    [Fact]
    public void Test_CMVStatisticsCoordinator_ConstructComputor_Successful()
    {
      var aggregator = _getCMVAggregator(Arg_Details);
      var coordinator = _getCoordinator();
      var computor = coordinator.ConstructComputor(Arg_Details, aggregator);

      Assert.True(computor.RequestDescriptor == coordinator.RequestDescriptor, "Invalid computor value for RequestDescriptor.");
      Assert.True(computor.SiteModel == coordinator.SiteModel, "Invalid computor value for SiteModel.");
      Assert.True(computor.Aggregator.Equals(aggregator), "Invalid computor value for Aggregator.");
      Assert.True(computor.Filters.Filters.Length == Arg_Details.Filters.Filters.Length, "Invalid computor value for Filters length as different to Arg.");
      Assert.True(computor.Filters.Filters.Length == 1, "Invalid computor value for Filters length.");
      Assert.True(computor.IncludeSurveyedSurfaces, "Invalid computor value for IncludeSurveyedSurfaces.");
      Assert.True(computor.RequestedGridDataType == GridDataType.CCV, "Invalid computor value for RequestedGridDataType.");
    }

    [Fact]
    public void Test_CMVStatisticsCoordinator_ReadOutResults_Details_Successful()
    {
      var aggregator = _getCMVAggregator(Arg_Details);
      var coordinator = _getCoordinator();

      var response = new CMVStatisticsResponse();

      coordinator.ReadOutResults(aggregator, response);

      Assert.True(Math.Abs(response.CellSize - aggregator.CellSize) < Consts.TOLERANCE_DIMENSION, "CellSize invalid after result read-out.");

      Assert.True(response.Counts.Length == aggregator.Counts.Length, "Invalid read-out value for Counts.Length.");

      for (int i = 0; i < response.Counts.Length; i++)
        Assert.True(response.Counts[i] == aggregator.Counts[i], $"Invalid aggregated value for Counts[{i}].");
    }

    [Fact]
    public void Test_CMVStatisticsCoordinator_ReadOutResults_Summary_Successful()
    {
      var aggregator = _getCMVAggregator(Arg_Summary);
      var coordinator = _getCoordinator();

      var response = new CMVStatisticsResponse();

      coordinator.ReadOutResults(aggregator, response);

      Assert.True(Math.Abs(response.CellSize - aggregator.CellSize) < Consts.TOLERANCE_DIMENSION, "CellSize invalid after result read-out.");
      Assert.True(response.SummaryCellsScanned == aggregator.SummaryCellsScanned, "Invalid read-out value for SummaryCellsScanned.");
      Assert.True(response.LastTargetCMV == aggregator.LastTargetCMV, "Invalid read-out value for LastTargetCMV.");
      Assert.True(response.CellsScannedOverTarget == aggregator.CellsScannedOverTarget, "Invalid read-out value for CellsScannedOverTarget.");
      Assert.True(response.CellsScannedAtTarget == aggregator.CellsScannedAtTarget, "Invalid read-out value for CellsScannedAtTarget.");
      Assert.True(response.CellsScannedUnderTarget == aggregator.CellsScannedUnderTarget, "Invalid read-out value for CellsScannedUnderTarget.");
      Assert.True(response.IsTargetValueConstant == aggregator.IsTargetValueConstant, "Invalid read-out value for IsTargetValueConstant.");
      Assert.True(response.MissingTargetValue == aggregator.MissingTargetValue, "Invalid initial read-out for MissingTargetValue.");
    }
  }
}
