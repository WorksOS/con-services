using System;
using VSS.TRex.Analytics.CMVStatistics.Details;
using VSS.TRex.Analytics.CMVStatistics.GridFabric.Details;
using VSS.TRex.Common;
using VSS.TRex.Filters;
using VSS.TRex.Tests.netcore.Analytics.Common;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CMVStatistics
{
  public class CMVDetailsCoordinatorTests : BaseCoordinatorTests
  {
    private CMVDetailsArgument Arg => new CMVDetailsArgument()
    {
      ProjectID = _siteModel.ID,
      Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
      CMVDetailValues = new[] { 1, 5, 10, 15, 20, 25, 31 }
    };

    private CMVDetailsCoordinator _getCoordinator()
    {
      return new CMVDetailsCoordinator() { RequestDescriptor = Guid.NewGuid(), SiteModel = _siteModel };
    }

    private CMVDetailsAggregator _getCMVAggregator()
    {
      var coordinator = _getCoordinator();

      return coordinator.ConstructAggregator(Arg) as CMVDetailsAggregator;
    }

    [Fact]
    public void Test_CMVDetailsCoordinator_Creation()
    {
      var coordinator = new CMVDetailsCoordinator();

      Assert.True(coordinator.SiteModel == null, "Invalid initial value for SiteModel.");
      Assert.True(coordinator.RequestDescriptor == Guid.Empty, "Invalid initial value for RequestDescriptor.");
    }

    [Fact]
    public void Test_CMVDetailsCoordinator_ConstructAggregator_Successful()
    {
      var aggregator = _getCMVAggregator();

      Assert.True(aggregator.RequiresSerialisation, "Invalid aggregator value for RequiresSerialisation.");
      Assert.True(aggregator.SiteModelID == Arg.ProjectID, "Invalid aggregator value for SiteModelID.");
      Assert.True(Math.Abs(aggregator.CellSize - _siteModel.Grid.CellSize) < Consts.TOLERANCE_DIMENSION, "Invalid aggregator value for CellSize.");
      Assert.True(aggregator.DetailsDataValues.Length == Arg.CMVDetailValues.Length, "Invalid aggregator value for DetailsDataValues.Length.");

      for (int i = 0; i < aggregator.Counts.Length; i++)
        Assert.True(aggregator.DetailsDataValues[i] == Arg.CMVDetailValues[i], $"Invalid aggregated value for DetailsDataValues[{i}].");
    }

    [Fact]
    public void Test_CMVDetailsCoordinator_ConstructComputor_Successful()
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
    public void Test_CMVDetailsCoordinator_ReadOutResults_Successful()
    {
      var aggregator = _getCMVAggregator();
      var coordinator = _getCoordinator();

      var response = new CMVDetailsResponse();

      coordinator.ReadOutResults(aggregator, response);

      Assert.True(response.Counts.Length == aggregator.Counts.Length, "Invalid read-out value for Counts.Length.");

      for (int i = 0; i < response.Counts.Length; i++)
        Assert.True(response.Counts[i] == aggregator.Counts[i], $"Invalid aggregated value for Counts[{i}].");
    }
  }
}
