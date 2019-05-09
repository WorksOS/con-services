using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Analytics.MDPStatistics;
using VSS.TRex.Analytics.MDPStatistics.GridFabric;
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

namespace VSS.TRex.Tests.Analytics.MDPStatistics.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(MDPStatisticsRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(MDPStatisticsRequest_ClusterCompute))]
  public class MDPStatisticsRequestTests : BaseTests<MDPStatisticsArgument, MDPStatisticsResponse>, IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private MDPStatisticsArgument SimpleMDPStatisticsArgument(ISiteModel siteModel, short target, double minPercentage, double maxPercentage)
    {
      return new MDPStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        OverridingMachineMDP = target,
        OverrideMachineMDP = target > 0,
        MDPPercentageRange = new MDPRangePercentageRecord(minPercentage, maxPercentage)
      };
    }

    private void BuildModelForSingleCellMDP(out ISiteModel siteModel, short mdpIncrement)
    {
      var baseTime = DateTime.UtcNow;
      short baseMDP = 10;

      siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          MDP = (short)(baseMDP + x * mdpIncrement),
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);
    }

    [Fact]
    public void Test_SummaryMDPStatistics_Creation()
    {
      var operation = new MDPStatisticsOperation();

      operation.Should().NotBeNull();
    }

    [Fact]
    public void Test_SummaryMDPStatistics_EmptySiteModel_FullExtents_NoMDPTargetOverride()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var operation = new MDPStatisticsOperation();

      var mdpSummaryResult = operation.Execute(SimpleMDPStatisticsArgument(siteModel, 0, 0.0, 0.0));

      mdpSummaryResult.Should().NotBeNull();
      mdpSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public void Test_SummaryMDPStatistics_SiteModelWithSingleCell_FullExtents_NoMDPTargetOverride()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellMDP(out var siteModel, 10);
      var operation = new MDPStatisticsOperation();

      var mdpSummaryResult = operation.Execute(SimpleMDPStatisticsArgument(siteModel, 0, 0.0, 0.0));

      mdpSummaryResult.Should().NotBeNull();
      mdpSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      mdpSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoResult);
      mdpSummaryResult.ConstantTargetMDP.Should().Be(CellPassConsts.NullMDP);
      mdpSummaryResult.IsTargetMDPConstant.Should().BeTrue();
      mdpSummaryResult.Counts.Should().BeNull();
      mdpSummaryResult.Percents.Should().BeNull();
      mdpSummaryResult.AboveTargetPercent.Should().Be(0);
      mdpSummaryResult.WithinTargetPercent.Should().Be(0);
      mdpSummaryResult.BelowTargetPercent.Should().Be(0);
      mdpSummaryResult.TotalAreaCoveredSqMeters.Should().Be(0);
    }

    [Fact]
    public void Test_SummaryMDPStatistics_SiteModelWithSingleCell_FullExtents_NoMDPTargetOverride_WithMachineMDPTarget()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellMDP(out var siteModel, 10);
      siteModel.MachinesTargetValues[0].TargetMDPStateEvents.PutValueAtDate(VSS.TRex.Common.Consts.MIN_DATETIME_AS_UTC, 50);

      var operation = new MDPStatisticsOperation();

      var mdpSummaryResult = operation.Execute(SimpleMDPStatisticsArgument(siteModel, 0, 0.0, 0.0));

      mdpSummaryResult.Should().NotBeNull();
      mdpSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      mdpSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoProblems);
      mdpSummaryResult.ConstantTargetMDP.Should().Be(CellPassConsts.NullMDP);
      mdpSummaryResult.IsTargetMDPConstant.Should().BeTrue();
      mdpSummaryResult.Counts.Should().BeNull();
      mdpSummaryResult.Percents.Should().BeNull();
      mdpSummaryResult.BelowTargetPercent.Should().Be(0);
      mdpSummaryResult.AboveTargetPercent.Should().Be(0);
      mdpSummaryResult.WithinTargetPercent.Should().Be(100);
      mdpSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
    }

    [Theory]
    [InlineData(10, 50, 80.0, 120.0, 0.0, 0.0, 100.0)]
    [InlineData(10, 90, 80.0, 120.0, 0.0, 100.0, 0.0)]
    [InlineData(5, 90, 80.0, 120.0, 100.0, 0.0, 0.0)]
    public void Test_SummaryMDPStatistics_SiteModelWithSingleCell_FullExtents_WithMDPTargetOverrides
      (short mdpIncrement, short target, double minPercentage, double maxPercentage, double percentBelow, double percentWithin, double percentAbove)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellMDP(out var siteModel, mdpIncrement);
      var operation = new MDPStatisticsOperation();

      var mdpSummaryResult = operation.Execute(SimpleMDPStatisticsArgument(siteModel, target, minPercentage, maxPercentage));

      mdpSummaryResult.Should().NotBeNull();
      mdpSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      mdpSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoProblems);
      mdpSummaryResult.ConstantTargetMDP.Should().Be(target);
      mdpSummaryResult.IsTargetMDPConstant.Should().BeTrue();
      mdpSummaryResult.Counts.Should().BeNull();
      mdpSummaryResult.Percents.Should().BeNull();
      mdpSummaryResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      mdpSummaryResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      mdpSummaryResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      mdpSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
    }

    [Theory]
    [InlineData(0, 0.0, 0.0, 0.0, 0.0, 0.0)]
    public void Test_DetailedMDPStatistics_SiteModelWithSingleCell_FullExtents
      (short target, double minPercentage, double maxPercentage, double percentBelow, double percentWithin, double percentAbove)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellMDP(out var siteModel, 10);
      var operation = new MDPStatisticsOperation();

      var arg = SimpleMDPStatisticsArgument(siteModel, target, minPercentage, maxPercentage);
      arg.MDPDetailValues = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150 };
      var mdpDetailResult = operation.Execute(arg);

      mdpDetailResult.Should().NotBeNull();

      // Checks counts and percentages
      mdpDetailResult.Counts.Sum().Should().Be(1);
      mdpDetailResult.Counts[9].Should().Be(1);
      mdpDetailResult.Percents.Sum().Should().BeApproximately(100.0, 0.000001);
      mdpDetailResult.Percents[9].Should().BeApproximately(100.0, 0.000001);

      // Check summary related fields are zero
      mdpDetailResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      mdpDetailResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoResult);
      mdpDetailResult.ConstantTargetMDP.Should().Be(CellPassConsts.NullMDP);
      mdpDetailResult.IsTargetMDPConstant.Should().BeTrue();
      mdpDetailResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      mdpDetailResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      mdpDetailResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      mdpDetailResult.TotalAreaCoveredSqMeters.Should().BeApproximately(0, 0.000001); // This being zero seems strange...
    }

    [Theory]
    [InlineData(0, 0.0, 0.0, 0.0, 0.0, 100.0)]
    [InlineData(800, 90.0, 110.0, 0.0, 0.0, 100.0)]
    [InlineData(1000, 70.0, 130.0, 0.0, 26.656274356975839, 73.343725643024158)]
    [InlineData(1700, 80.0, 120.0, 50.116913484021822, 49.883086515978178, 0.0)]
    [InlineData(2000, 80.0, 120.0, 100.0, 0.0, 0.0)]
    public void Test_SummaryMDPStatistics_SiteModelWithSingleTAGFile_FullExtents_WithCMVTargetOverrides
      (short target, double minPercentage, double maxPercentage, double percentBelow, double percentWithin, double percentAbove)
    {
      const short MACHINE_TARGET_MDP = 1500;

      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile-MDP.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var operation = new MDPStatisticsOperation();

      var mdpSummaryResult = operation.Execute(SimpleMDPStatisticsArgument(siteModel, target, minPercentage, maxPercentage));

      mdpSummaryResult.Should().NotBeNull();
      mdpSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      mdpSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoProblems);
      mdpSummaryResult.ConstantTargetMDP.Should().Be(target == 0 ? MACHINE_TARGET_MDP : target);
      mdpSummaryResult.IsTargetMDPConstant.Should().BeTrue();
      mdpSummaryResult.Counts.Should().BeNull();
      mdpSummaryResult.Percents.Should().BeNull();
      mdpSummaryResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      mdpSummaryResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      mdpSummaryResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      mdpSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(1283 * SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
    }

  }
}
