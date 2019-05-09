using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Analytics.CMVStatistics;
using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Records;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CMVStatistics.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(CMVStatisticsRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(CMVStatisticsRequest_ClusterCompute))]
  public class CMVStatisticsRequestTests : BaseTests<CMVStatisticsArgument, CMVStatisticsResponse>, IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private CMVStatisticsArgument SimpleCMVStatisticsArgument(ISiteModel siteModel, short target, double minPercentage, double maxPercentage)
    {
      return new CMVStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        OverridingMachineCMV = target,
        OverrideMachineCMV = target > 0,
        CMVPercentageRange = new CMVRangePercentageRecord(minPercentage, maxPercentage)
      };
    }

    private void BuildModelForSingleCellCMV(out ISiteModel siteModel, short cmvIncrement)
    {
      var baseTime = DateTime.UtcNow;
      short baseCMV = 10;

      siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          CCV = (short) (baseCMV + x * cmvIncrement),
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);
    }

    [Fact]
    public void Test_SummaryCMVStatistics_Creation()
    {
      var operation = new CMVStatisticsOperation();

      operation.Should().NotBeNull();
    }

    [Fact]
    public void Test_SummaryCMVStatistics_EmptySiteModel_FullExtents_NoCMVTargetOverride()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var operation = new CMVStatisticsOperation();

      var cmvSummaryResult = operation.Execute(SimpleCMVStatisticsArgument(siteModel, 0, 0.0, 0.0));

      cmvSummaryResult.Should().NotBeNull();
      cmvSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public void Test_SummaryCMVStatistics_SiteModelWithSingleCell_FullExtents_NoCMVTargetOverride()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellCMV(out var siteModel, 10);
      var operation = new CMVStatisticsOperation();

      var cmvSummaryResult = operation.Execute(SimpleCMVStatisticsArgument(siteModel, 0, 0.0, 0.0));

      cmvSummaryResult.Should().NotBeNull();
      cmvSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      cmvSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoResult);
      cmvSummaryResult.ConstantTargetCMV.Should().Be(CellPassConsts.NullCCV);
      cmvSummaryResult.IsTargetCMVConstant.Should().BeTrue();
      cmvSummaryResult.Counts.Should().BeNull();
      cmvSummaryResult.Percents.Should().BeNull();
      cmvSummaryResult.AboveTargetPercent.Should().Be(0);
      cmvSummaryResult.WithinTargetPercent.Should().Be(0);
      cmvSummaryResult.BelowTargetPercent.Should().Be(0);
      cmvSummaryResult.TotalAreaCoveredSqMeters.Should().Be(0);
    }

    [Fact]
    public void Test_SummaryCMVStatistics_SiteModelWithSingleCell_FullExtents_NoCMVTargetOverride_WithMachineCMVTarget()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellCMV(out var siteModel, 10);
      siteModel.MachinesTargetValues[0].TargetCCVStateEvents.PutValueAtDate(VSS.TRex.Common.Consts.MIN_DATETIME_AS_UTC, 50);

      var operation = new CMVStatisticsOperation();

      var cmvSummaryResult = operation.Execute(SimpleCMVStatisticsArgument(siteModel, 0, 0.0, 0.0));

      cmvSummaryResult.Should().NotBeNull();
      cmvSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      cmvSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoProblems);
      cmvSummaryResult.ConstantTargetCMV.Should().Be(CellPassConsts.NullCCV);
      cmvSummaryResult.IsTargetCMVConstant.Should().BeTrue();
      cmvSummaryResult.Counts.Should().BeNull();
      cmvSummaryResult.Percents.Should().BeNull();
      cmvSummaryResult.BelowTargetPercent.Should().Be(0);
      cmvSummaryResult.AboveTargetPercent.Should().Be(0);
      cmvSummaryResult.WithinTargetPercent.Should().Be(100);
      cmvSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
    }

    [Theory]
    [InlineData(10, 50, 80.0, 120.0, 0.0, 0.0, 100.0)]
    [InlineData(10, 90, 80.0, 120.0, 0.0, 100.0, 0.0)]
    [InlineData(5, 90, 80.0, 120.0, 100.0, 0.0, 0.0)]
    public void Test_SummaryCMVStatistics_SiteModelWithSingleCell_FullExtents_WithCMVTargetOverrides
      (short cmvIncrement, short target, double minPercentage, double maxPercentage, double percentBelow, double percentWithin, double percentAbove)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellCMV(out var siteModel, cmvIncrement);
      var operation = new CMVStatisticsOperation();

      var cmvSummaryResult = operation.Execute(SimpleCMVStatisticsArgument(siteModel, target, minPercentage, maxPercentage));

      cmvSummaryResult.Should().NotBeNull();
      cmvSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      cmvSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoProblems);
      cmvSummaryResult.ConstantTargetCMV.Should().Be(target);
      cmvSummaryResult.IsTargetCMVConstant.Should().BeTrue();
      cmvSummaryResult.Counts.Should().BeNull();
      cmvSummaryResult.Percents.Should().BeNull();
      cmvSummaryResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      cmvSummaryResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      cmvSummaryResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      cmvSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
    }

    [Theory]
    [InlineData(0, 0.0, 0.0, 0.0, 0.0, 0.0)]
    public void Test_DetailedCMVStatistics_SiteModelWithSingleCell_FullExtents
      (short target, double minPercentage, double maxPercentage, double percentBelow, double percentWithin, double percentAbove)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellCMV(out var siteModel, 10);
      var operation = new CMVStatisticsOperation();

      var arg = SimpleCMVStatisticsArgument(siteModel, target, minPercentage, maxPercentage);
      arg.CMVDetailValues = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150 };
      var cmvDetailResult = operation.Execute(arg);

      cmvDetailResult.Should().NotBeNull();

      // Checks counts and percentages
      cmvDetailResult.Counts.Sum().Should().Be(1);
      cmvDetailResult.Counts[9].Should().Be(1);
      cmvDetailResult.Percents.Sum().Should().BeApproximately(100.0, 0.000001);
      cmvDetailResult.Percents[9].Should().BeApproximately(100.0, 0.000001);

      // Check summary related fields are zero
      cmvDetailResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      cmvDetailResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoResult);
      cmvDetailResult.ConstantTargetCMV.Should().Be(CellPassConsts.NullCCV);
      cmvDetailResult.IsTargetCMVConstant.Should().BeTrue();
      cmvDetailResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      cmvDetailResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      cmvDetailResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      cmvDetailResult.TotalAreaCoveredSqMeters.Should().BeApproximately(0, 0.000001); // This being zero seems strange...
    }

    [Theory]
    [InlineData(0, 0.0, 0.0, 0.0, 1.056499770326137, 98.943500229673859)]
    [InlineData(200, 90.0, 110.0, 1.056499770326137, 0.0, 98.943500229673859)]
    [InlineData(400, 70.0, 130.0, 1.056499770326137, 90.261828203950387, 8.6816720257234739)]
    [InlineData(500, 80.0, 120.0, 17.638952687184197, 81.350482315112544, 1.0105649977032614)]
    [InlineData(600, 80.0, 120.0, 73.449701423977956, 26.366559485530544, 0.18373909049150206)]
    public void Test_SummaryCMVStatistics_SiteModelWithSingleTAGFile_FullExtents_WithCMVTargetOverrides
      (short target, double minPercentage, double maxPercentage, double percentBelow, double percentWithin, double percentAbove)
    {
      const short MACHINE_TARGET_CMV = 300;

      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var operation = new CMVStatisticsOperation();

      var cmvSummaryResult = operation.Execute(SimpleCMVStatisticsArgument(siteModel, target, minPercentage, maxPercentage));

      cmvSummaryResult.Should().NotBeNull();
      cmvSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      cmvSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoProblems);
      cmvSummaryResult.ConstantTargetCMV.Should().Be(target == 0 ? MACHINE_TARGET_CMV : target);
      cmvSummaryResult.IsTargetCMVConstant.Should().BeTrue();
      cmvSummaryResult.Counts.Should().BeNull();
      cmvSummaryResult.Percents.Should().BeNull();
      cmvSummaryResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      cmvSummaryResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      cmvSummaryResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      cmvSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(2177 * SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
    }

  }
}
