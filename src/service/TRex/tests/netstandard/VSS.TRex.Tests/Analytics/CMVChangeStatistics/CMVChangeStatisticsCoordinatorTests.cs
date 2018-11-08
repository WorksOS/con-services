using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Analytics.CMVChangeStatistics;
using VSS.TRex.Analytics.CMVChangeStatistics.GridFabric;
using VSS.TRex.Common;
using VSS.TRex.Filters;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CMVChangeStatistics
{
  public class CMVChangeStatisticsCoordinatorTests : BaseCoordinatorTests
  {
    private CMVChangeStatisticsArgument Arg => new CMVChangeStatisticsArgument()
    {
      ProjectID = _siteModel.ID,
      Filters = new FilterSet(new CombinedFilter()),
      CMVChangeDetailsDatalValues = new[] { -50.0, -20.0, -10.0, 0.0, 10.0, 20.0, 50.0 }
    };

    private CMVChangeStatisticsCoordinator _getCoordinator()
    {
      return new CMVChangeStatisticsCoordinator() { RequestDescriptor = Guid.NewGuid(), SiteModel = _siteModel };
    }

    private CMVChangeStatisticsAggregator _getCMVAggregator()
    {
      var coordinator = _getCoordinator();

      return coordinator.ConstructAggregator(Arg) as CMVChangeStatisticsAggregator;
    }

    [Fact]
    public void Test_CMVChangeStatisticsCoordinator_Creation()
    {
      var coordinator = new CMVChangeStatisticsCoordinator();

      Assert.True(coordinator.SiteModel == null, "Invalid initial value for SiteModel.");
      Assert.True(coordinator.RequestDescriptor == Guid.Empty, "Invalid initial value for RequestDescriptor.");
    }

    [Fact]
    public void Test_CMVChangeStatisticsCoordinator_ConstructAggregator_Successful()
    {
      var aggregator = _getCMVAggregator();

      Assert.True(aggregator.RequiresSerialisation, "Invalid aggregator value for RequiresSerialisation.");
      Assert.True(aggregator.SiteModelID == Arg.ProjectID, "Invalid aggregator value for SiteModelID.");
      Assert.True(Math.Abs(aggregator.CellSize - _siteModel.Grid.CellSize) < Consts.TOLERANCE_DIMENSION, "Invalid aggregator value for CellSize.");

      Assert.True(aggregator.CMVChangeDetailsDataValues.Length == Arg.CMVChangeDetailsDatalValues.Length, "Invalid aggregator value for DetailsDataValues.Length.");

      for (int i = 0; i < aggregator.Counts.Length; i++)
        Assert.True(Math.Abs(aggregator.CMVChangeDetailsDataValues[i] - Arg.CMVChangeDetailsDatalValues[i]) < Consts.TOLERANCE_DIMENSION, $"Invalid aggregated value for DetailsDataValues[{i}].");
    }

    [Fact]
    public void Test_CMVChangeStatisticsCoordinator_ConstructComputor_Successful()
    {
      var aggregator = _getCMVAggregator();
      var coordinator = _getCoordinator();
      var computor = coordinator.ConstructComputor(Arg, aggregator);

      Assert.True(computor.RequestDescriptor == coordinator.RequestDescriptor, "Invalid computor value for RequestDescriptor.");
      Assert.True(computor.SiteModel == coordinator.SiteModel, "Invalid computor value for SiteModel.");
      Assert.True(computor.Aggregator.Equals(aggregator), "Invalid computor value for Aggregator.");
      Assert.True(computor.Filters.Filters.Length == Arg.Filters.Filters.Length, "Invalid computor value for Filters length as different to Arg.");
      Assert.True(computor.Filters.Filters.Length == 1, "Invalid computor value for Filters length.");
      Assert.True(computor.IncludeSurveyedSurfaces, "Invalid computor value for IncludeSurveyedSurfaces.");
      Assert.True(computor.RequestedGridDataType == GridDataType.CCV, "Invalid computor value for RequestedGridDataType.");
    }

    [Fact]
    public void Test_CMVChangeStatisticsCoordinator_ReadOutResults_Successful()
    {
      var aggregator = _getCMVAggregator();
      var coordinator = _getCoordinator();

      var response = new CMVChangeStatisticsResponse();

      coordinator.ReadOutResults(aggregator, response);

      Assert.True(Math.Abs(response.CellSize - aggregator.CellSize) < Consts.TOLERANCE_DIMENSION, "CellSize invalid after result read-out.");

      Assert.True(Math.Abs(response.SummaryProcessedArea - aggregator.SummaryProcessedArea) < Consts.TOLERANCE_DIMENSION, "Invalid read-out value for SummaryProcessedArea.");

      Assert.True(response.Counts.Length == aggregator.Counts.Length, "Invalid read-out value for Counts.Length.");

      for (int i = 0; i < response.Counts.Length; i++)
        Assert.True(response.Counts[i] == aggregator.Counts[i], $"Invalid aggregated value for Counts[{i}].");
    }
  }
}
