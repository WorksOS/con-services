using System;
using System.Linq;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.TRex.Analytics.CCAStatistics;
using VSS.TRex.Analytics.CCAStatistics.GridFabric;
using VSS.TRex.Cells;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CCAStatistics.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(CCAStatisticsRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(CCAStatisticsRequest_ClusterCompute))]
  public class CCAStatisticsRequestTests : BaseTests<CCAStatisticsArgument, CCAStatisticsResponse>, IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private CCAStatisticsArgument SimpleCCAStatisticsArgument(ISiteModel siteModel)
    {
      return new CCAStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter())
      };
    }

    private void BuildModelForSingleCellCCA(out ISiteModel siteModel, byte ccaIncrement)
    {
      var baseTime = DateTime.UtcNow;
      byte baseCCA = 1;

      siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          CCA = (byte) (baseCCA + x * ccaIncrement),
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
    }

    [Fact]
    public void Test_SummaryCCAStatistics_Creation()
    {
      var operation = new CCAStatisticsOperation();

      operation.Should().NotBeNull();
    }

    [Fact]
    public void Test_SummaryCCAStatistics_EmptySiteModel_FullExtents()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var operation = new CCAStatisticsOperation();

      var ccaStatisticsResult = operation.Execute(SimpleCCAStatisticsArgument(siteModel));

      ccaStatisticsResult.Should().NotBeNull();
      ccaStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public void Test_SummaryCCAStatistics_SiteModelWithSingleCell_FullExtents()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellCCA(out var siteModel, 1);
      var operation = new CCAStatisticsOperation();

      var ccaStatisticsResult = operation.Execute(SimpleCCAStatisticsArgument(siteModel));

      ccaStatisticsResult.Should().NotBeNull();
      ccaStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      ccaStatisticsResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoProblems);
      ccaStatisticsResult.BelowTargetPercent.Should().Be(0);
      ccaStatisticsResult.AboveTargetPercent.Should().Be(0);
      ccaStatisticsResult.WithinTargetPercent.Should().Be(100);
      ccaStatisticsResult.TotalAreaCoveredSqMeters.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
      ccaStatisticsResult.ConstantTargetCCA.Should().Be(0);
      ccaStatisticsResult.IsTargetCCAConstant.Should().BeTrue();
      ccaStatisticsResult.Counts.Should().BeNull();
      ccaStatisticsResult.Percents.Should().BeNull();
    }
  }
}
