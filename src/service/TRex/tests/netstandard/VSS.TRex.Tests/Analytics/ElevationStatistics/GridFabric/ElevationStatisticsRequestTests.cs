using System;
using System.Linq;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.TRex.Analytics.ElevationStatistics;
using VSS.TRex.Analytics.ElevationStatistics.GridFabric;
using VSS.TRex.Cells;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.ElevationStatistics.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(ElevationStatisticsRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(ElevationStatisticsRequest_ClusterCompute))]
  public class ElevationStatisticsRequestTests : BaseTests<ElevationStatisticsArgument, ElevationStatisticsResponse>, IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private ElevationStatisticsArgument SimpleElevationStatisticsArgument(ISiteModel siteModel)
    {
      return new ElevationStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter())
      };
    }

    private void BuildModelForSingleCellElevation(out ISiteModel siteModel, float elevationIncrement)
    {
      var baseTime = DateTime.UtcNow;
      byte baseElevation = 1;

      siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = (byte)(baseElevation + x * elevationIncrement),
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);
    }

    [Fact]
    public void Test_SummaryElevationStatistics_Creation()
    {
      var operation = new ElevationStatisticsOperation();

      operation.Should().NotBeNull();
    }

    [Fact]
    public void Test_SummaryElevationStatistics_EmptySiteModel_FullExtents()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var operation = new ElevationStatisticsOperation();

      var elevationStatisticsResult = operation.Execute(SimpleElevationStatisticsArgument(siteModel));

      elevationStatisticsResult.Should().NotBeNull();
      elevationStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public void Test_SummaryElevationStatistics_SiteModelWithSingleCell_FullExtents()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellElevation(out var siteModel, 1);
      var operation = new ElevationStatisticsOperation();

      var elevationStatisticsResult = operation.Execute(SimpleElevationStatisticsArgument(siteModel));

      elevationStatisticsResult.Should().NotBeNull();
      elevationStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      elevationStatisticsResult.MinElevation.Should().Be(10);
      elevationStatisticsResult.MaxElevation.Should().Be(10);
      elevationStatisticsResult.CoverageArea.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
      elevationStatisticsResult.BoundingExtents.IsValidPlanExtent.Should().Be(true);
    }
  }
}
