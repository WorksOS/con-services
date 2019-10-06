using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Analytics.PassCountStatistics;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Cells;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Records;
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
    private const float HEIGHT_INCREMENT_0_5 = 0.5f;

    private PassCountStatisticsArgument SimplePassCountStatisticsArgument(ISiteModel siteModel, ushort targetMin, ushort targetMax)
    {
      return new PassCountStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Overrides = new OverrideParameters
        { 
          OverridingTargetPassCountRange = new PassCountRangeRecord(targetMin, targetMax),
          OverrideTargetPassCount = targetMin > 0 && targetMax > 0
        }
      };
    }

    private ISiteModel BuildModelForSingleCellPassCount(float heightIncrement)
    {
      var baseTime = DateTime.UtcNow;
      var baseHeight = 1.0f;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
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

      return siteModel;
    }

    [Fact]
    public void Test_SummaryPassCountStatistics_Creation()
    {
      var operation = new PassCountStatisticsOperation();

      operation.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_SummaryPassCountStatistics_EmptySiteModel_FullExtents_NoPassCountTargetOverride()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var operation = new PassCountStatisticsOperation();

      var passCountSummaryResult = await operation.ExecuteAsync(SimplePassCountStatisticsArgument(siteModel, 0, 0));

      passCountSummaryResult.Should().NotBeNull();
      passCountSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public async Task Test_SummaryPassCountStatistics_SiteModelWithSingleCell_FullExtents_NoPassCountTargetOverride()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellPassCount(HEIGHT_INCREMENT_0_5);
      var operation = new PassCountStatisticsOperation();

      var passCountSummaryResult = await operation.ExecuteAsync(SimplePassCountStatisticsArgument(siteModel, 0, 0));

      passCountSummaryResult.Should().NotBeNull();
      passCountSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      passCountSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.PartialResultMissingTarget);
    }

    [Fact]
    public async Task Test_SummaryPassCountStatistics_SiteModelWithSingleCell_FullExtents_NoPassCountTargetOverride_WithMachinePassCountTarget()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellPassCount(HEIGHT_INCREMENT_0_5);
      siteModel.MachinesTargetValues[0].TargetPassCountStateEvents.PutValueAtDate(VSS.TRex.Common.Consts.MIN_DATETIME_AS_UTC, 10);

      var operation = new PassCountStatisticsOperation();

      var passCountSummaryResult = await operation.ExecuteAsync(SimplePassCountStatisticsArgument(siteModel, 0, 0));

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
    public async Task Test_SummaryPassCountStatistics_SiteModelWithSingleCell_FullExtents_WithPassCountTargetOverrides
      (ushort minTarget, ushort maxTarget, double percentBelow, double percentWithin, double percentAbove)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellPassCount(HEIGHT_INCREMENT_0_5);
      var operation = new PassCountStatisticsOperation();

      var passCountSummaryResult = await operation.ExecuteAsync(SimplePassCountStatisticsArgument(siteModel, minTarget, maxTarget));

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
    public async Task Test_DetailedPassCountStatistics_SiteModelWithSingleCell_FullExtents
      (ushort minTarget, ushort maxTarget, double percentBelow, double percentWithin, double percentAbove)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellPassCount(HEIGHT_INCREMENT_0_5);
      var operation = new PassCountStatisticsOperation();

      var arg = SimplePassCountStatisticsArgument(siteModel, minTarget, maxTarget);
      arg.PassCountDetailValues = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15};
      var passCountDetailResult = await operation.ExecuteAsync(arg);

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
    // Note: Leaving the old parameters for a bit in case this is a flapping test (in which case we will see these tests break
    // again and can compare the expected values against the comment out previous values.
    //    [InlineData(0, 0, 25.540275049115913, 2.226588081204977, 72.2331368696791, 353.04240000000004)]
    //    [InlineData(1, 3, 0.0, 27.766863130320889, 72.2331368696791, 353.04240000000004)]
    //    [InlineData(3, 5, 25.540275049115913, 16.699410609037326, 57.760314341846765, 353.04240000000004)]
    [InlineData(0, 0, 40.425531914893611, 22.391084093211752, 37.18338399189463, 342.29160000000007)]
    [InlineData(1, 3, 0.0, 62.816616008105377, 37.18338399189463, 342.29160000000007)]
    [InlineData(3, 5, 40.425531914893611, 59.574468085106382, 0.0, 342.29160000000007)]
    public async Task Test_SummaryPassCountStatistics_SiteModelWithSingleTAGFile_FullExtents_WithPassCountTargetOverrides
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

      var passCountSummaryResult = await operation.ExecuteAsync(SimplePassCountStatisticsArgument(siteModel, minTarget, maxTarget));

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
    //[InlineData(0, 0, 25.540275049115913, 2.226588081204977, 72.2331368696791, 353.04240000000004)]
    public async Task Test_DetailedPassCountStatistics_SiteModelWithSingleTAGFile_FullExtents
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
      var passCountDetailResult = await operation.ExecuteAsync(arg);

      passCountDetailResult.Should().NotBeNull();

      // Checks counts and percentages
      long[] expectedCounts = {755, 442, 663, 1038, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
//      long[] expectedCounts = { 93, 687, 68, 385, 57, 598, 65, 986, 52, 63, 0, 0, 0, 0, 0 };
      long expectedCountsSum = 0;
      for (int i = 0; i < expectedCounts.Length; i++)
        expectedCountsSum += (i + 1) * expectedCounts[i];

      // Is sum of counts the same?
      long passCountDetailResultSum = 0;
      for (int i = 0; i < passCountDetailResult.Counts.Length; i++)
        passCountDetailResultSum += (i + 1) * passCountDetailResult.Counts[i];

      passCountDetailResultSum.Should().Be(expectedCountsSum);

      // Are all counts the same and do percentages match?

      long totalCount = passCountDetailResult.Counts.Sum();
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
