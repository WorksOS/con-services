using System;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Analytics.CCAStatistics;
using VSS.TRex.Analytics.CCAStatistics.GridFabric;
using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
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
    private const byte CCA_INCREMENT = 1;

    private CCAStatisticsArgument SimpleCCAStatisticsArgument(ISiteModel siteModel)
    {
      return new CCAStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter())
      };
    }

    private ISiteModel BuildModelForSingleCellCCA(byte ccaIncrement, byte targetCCA = CellPassConsts.NullCCATarget)
    {
      var baseTime = DateTime.UtcNow;
      byte baseCCA = 1;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      if (targetCCA != CellPassConsts.NullCCATarget)
        siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetCCAStateEvents.PutValueAtDate(VSS.TRex.Common.Consts.MIN_DATETIME_AS_UTC, targetCCA);

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
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
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
    public void Test_SummaryCCAStatistics_SiteModelWithSingleCell_FullExtents_NoTarget()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellCCA(CCA_INCREMENT);
      var operation = new CCAStatisticsOperation();

      var ccaStatisticsResult = operation.Execute(SimpleCCAStatisticsArgument(siteModel));

      ccaStatisticsResult.Should().NotBeNull();
      ccaStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      ccaStatisticsResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoResult);
      ccaStatisticsResult.BelowTargetPercent.Should().Be(0);
      ccaStatisticsResult.AboveTargetPercent.Should().Be(0);
      ccaStatisticsResult.WithinTargetPercent.Should().Be(0);
      ccaStatisticsResult.TotalAreaCoveredSqMeters.Should().Be(0);
      ccaStatisticsResult.ConstantTargetCCA.Should().Be(CellPassConsts.NullCCATarget);
      ccaStatisticsResult.IsTargetCCAConstant.Should().BeTrue();
      ccaStatisticsResult.Counts.Should().BeNull();
      ccaStatisticsResult.Percents.Should().BeNull();
    }

    [Fact]
    public void Test_SummaryCCAStatistics_SiteModelWithSingleCell_FullExtents()
    {
      const byte TARGET_CCA = 5;

      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellCCA(CCA_INCREMENT, TARGET_CCA);
      var operation = new CCAStatisticsOperation();

      var ccaStatisticsResult = operation.Execute(SimpleCCAStatisticsArgument(siteModel));

      ccaStatisticsResult.Should().NotBeNull();
      ccaStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      ccaStatisticsResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoProblems);
      ccaStatisticsResult.BelowTargetPercent.Should().Be(0);
      ccaStatisticsResult.AboveTargetPercent.Should().Be(0);
      ccaStatisticsResult.WithinTargetPercent.Should().Be(100);
      ccaStatisticsResult.TotalAreaCoveredSqMeters.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
      ccaStatisticsResult.ConstantTargetCCA.Should().Be(TARGET_CCA);
      ccaStatisticsResult.IsTargetCCAConstant.Should().BeTrue();
      ccaStatisticsResult.Counts.Should().BeNull();
      ccaStatisticsResult.Percents.Should().BeNull();
    }
  }
}
