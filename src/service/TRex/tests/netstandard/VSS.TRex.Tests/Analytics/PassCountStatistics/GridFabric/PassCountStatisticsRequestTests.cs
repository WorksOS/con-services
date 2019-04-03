using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.TRex.Analytics.PassCountStatistics;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Cells;
using VSS.TRex.Common.Records;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.PassCountStatistics.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(PassCountStatisticsRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(PassCountStatisticsRequest_ClusterCompute))]
  public class PassCountStatisticsRequestTests : BaseTests<PassCountStatisticsArgument, PassCountStatisticsResponse>, IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private PassCountStatisticsArgument SimplePassCountStatisticsArgument(ISiteModel siteModel, ushort targetMin, ushort targetMax)
    {
      return new PassCountStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        OverridingTargetPassCountRange = new PassCountRangeRecord(targetMin, targetMax),
        OverrideTargetPassCount = targetMin > 0 && targetMax > 0
      };
    }

    private void BuildModelForSingleCellPassCount(out ISiteModel siteModel, float heightIncrement)
    {
      var baseTime = DateTime.UtcNow;
      var baseHeight = 1.0f;

      siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = baseHeight + x * heightIncrement,
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);
    }

    [Fact]
    public void Test_SummaryPassCountStatistics_Creation()
    {
      var operation = new PassCountStatisticsOperation();

      operation.Should().NotBeNull();
    }

    [Fact]
    public void Test_SummaryPassCountStatistics_EmptySiteModel_FullExtents_NoPassCountTargetOverride()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var operation = new PassCountStatisticsOperation();

      var passCountSummaryResult = operation.Execute(SimplePassCountStatisticsArgument(siteModel, 0, 0));

      passCountSummaryResult.Should().NotBeNull();
      passCountSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public void Test_SummaryPassCountStatistics_SiteModelWithSingleCell_FullExtents_NoPassCountTargetOverride()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellPassCount(out var siteModel, 0.5f);
      var operation = new PassCountStatisticsOperation();

      var passCountSummaryResult = operation.Execute(SimplePassCountStatisticsArgument(siteModel, 0, 0));

      passCountSummaryResult.Should().NotBeNull();
      passCountSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      passCountSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.PartialResultMissingTarget);
    }

    [Fact]
    public void Test_SummaryPassCountStatistics_SiteModelWithSingleCell_FullExtents_NoPassCountTargetOverride_WithMachinePassCountTarget()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellPassCount(out var siteModel, 0.5f);
      siteModel.MachinesTargetValues[0].TargetPassCountStateEvents.PutValueAtDate(DateTime.MinValue, 10);

      var operation = new PassCountStatisticsOperation();

      var passCountSummaryResult = operation.Execute(SimplePassCountStatisticsArgument(siteModel, 0, 0));

      passCountSummaryResult.Should().NotBeNull();
      passCountSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      passCountSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoResult);
      passCountSummaryResult.BelowTargetPercent.Should().Be(0);
      passCountSummaryResult.AboveTargetPercent.Should().Be(0);
      passCountSummaryResult.WithinTargetPercent.Should().Be(100);
      passCountSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
      passCountSummaryResult.IsTargetPassCountConstant.Should().BeTrue();
      passCountSummaryResult.Counts.Should().BeNull();
      passCountSummaryResult.Percents.Should().BeNull();
    }

    [Theory]
    [InlineData(3, 5, 0.0, 0.0, 100.0)]
    [InlineData(15, 20, 100.0, 0.0, 0.0)]
    [InlineData(5, 10, 0.0, 100.0, 0.0)]
    public void Test_SummaryPassCountStatistics_SiteModelWithSingleCell_FullExtents_WithPassCountTargetOverrides
      (ushort minTarget, ushort maxTarget, double percentBelow, double percentWithin, double percentAbove)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellPassCount(out var siteModel, 0.5f);
      var operation = new PassCountStatisticsOperation();

      var passCountSummaryResult = operation.Execute(SimplePassCountStatisticsArgument(siteModel, minTarget, maxTarget));

      passCountSummaryResult.Should().NotBeNull();
      passCountSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      passCountSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoResult);
      passCountSummaryResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      passCountSummaryResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      passCountSummaryResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      passCountSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
      passCountSummaryResult.IsTargetPassCountConstant.Should().BeTrue();
      passCountSummaryResult.Counts.Should().BeNull();
      passCountSummaryResult.Percents.Should().BeNull();
    }

    [Theory]
    [InlineData(0, 0, 0.0, 0.0, 0.0)]
    public void Test_DetailedPassCountStatistics_SiteModelWithSingleCell_FullExtents
      (ushort minTarget, ushort maxTarget, double percentBelow, double percentWithin, double percentAbove)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellPassCount(out var siteModel, 0.5f);
      var operation = new PassCountStatisticsOperation();

      var arg = SimplePassCountStatisticsArgument(siteModel, minTarget, maxTarget);
      arg.PassCountDetailValues = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15};
      var passCountDetailResult = operation.Execute(arg);

      passCountDetailResult.Should().NotBeNull();

      // Checks counts and percentages
      passCountDetailResult.Counts.Sum().Should().Be(1);
      passCountDetailResult.Counts[9].Should().Be(1);
      passCountDetailResult.Percents.Sum().Should().BeApproximately(100.0, 0.000001);
      passCountDetailResult.Percents[9].Should().BeApproximately(100.0, 0.000001);

      // Check summary related fields are zero
      passCountDetailResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      passCountDetailResult.ReturnCode.Should().Be(MissingTargetDataResultType.PartialResultMissingTarget);
      passCountDetailResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      passCountDetailResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      passCountDetailResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      passCountDetailResult.TotalAreaCoveredSqMeters.Should().BeApproximately(0, 0.000001); // This being zero seems strange...
      passCountDetailResult.IsTargetPassCountConstant.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 0, 40.425531914893611, 22.391084093211752, 37.18338399189463, 342.29160000000007)]
    [InlineData(1, 3, 0.0, 62.816616008105377, 37.18338399189463, 342.29160000000007)]
    [InlineData(3, 5, 40.425531914893611, 59.574468085106382, 0.0, 342.29160000000007)]
    public void Test_SummaryPassCountStatistics_SiteModelWithSingleTAGFile_FullExtents_WithPassCountTargetOverrides
      (ushort minTarget, ushort maxTarget, double percentBelow, double percentWithin, double percentAbove, double totalArea)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var operation = new PassCountStatisticsOperation();

      var passCountSummaryResult = operation.Execute(SimplePassCountStatisticsArgument(siteModel, minTarget, maxTarget));

      passCountSummaryResult.Should().NotBeNull();
      passCountSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      passCountSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoResult);
      passCountSummaryResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      passCountSummaryResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      passCountSummaryResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      passCountSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(totalArea, 0.000001);
      passCountSummaryResult.IsTargetPassCountConstant.Should().BeTrue();
      passCountSummaryResult.Counts.Should().BeNull();
      passCountSummaryResult.Percents.Should().BeNull();
    }

    // Todo: Add additional tests for pass count detail

    [Theory]
    [InlineData(0, 0, 40.425531914893611, 22.391084093211752, 37.18338399189463, 342.29160000000007)]
    public void Test_DetailedPassCountStatistics_SiteModelWithSingleTAGFile_FullExtents
      (ushort minTarget, ushort maxTarget, double percentBelow, double percentWithin, double percentAbove, double totalArea)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var operation = new PassCountStatisticsOperation();

      var arg = SimplePassCountStatisticsArgument(siteModel, minTarget, maxTarget);
      arg.PassCountDetailValues = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15};
      var passCountDetailResult = operation.Execute(arg);

      passCountDetailResult.Should().NotBeNull();

      // Checks counts and percentages
      long[] expectedCounts = {755, 442, 663, 1038, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
      
      // Is sum of counts the same?
      passCountDetailResult.Counts.Sum().Should().Be(expectedCounts.Sum());
      // Are all counts the same and do percentages match?

      long totalCount = expectedCounts.Sum();
      for (int i = 0; i < expectedCounts.Length; i++)
      {
        expectedCounts[i].Should().Be(passCountDetailResult.Counts[i]);
        passCountDetailResult.Percents[i].Should().BeApproximately(100.0 * expectedCounts[i] / (1.0 * totalCount), 0.001);
      }

      // Check summary related fields are zero
      passCountDetailResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      passCountDetailResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoResult);
      passCountDetailResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      passCountDetailResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      passCountDetailResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      passCountDetailResult.TotalAreaCoveredSqMeters.Should().BeApproximately(totalArea, 0.000001);
      passCountDetailResult.IsTargetPassCountConstant.Should().BeTrue();
    }
  }
}
