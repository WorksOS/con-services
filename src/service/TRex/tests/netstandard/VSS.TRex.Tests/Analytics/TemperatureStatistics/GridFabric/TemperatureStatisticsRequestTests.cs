using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Analytics.TemperatureStatistics;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Records;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.TemperatureStatistics.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(TemperatureStatisticsRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(TemperatureStatisticsRequest_ClusterCompute))]
  public class TemperatureStatisticsRequestTests : BaseTests<TemperatureStatisticsArgument, TemperatureStatisticsResponse>, IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private const ushort TEMPERATURE_INCREMENT = 10;
    private const ushort MACHINE_TARGET_MIN = 820;
    private const ushort MACHINE_TARGET_MAX = 1150;

    private TemperatureStatisticsArgument SimpleTemperatureStatisticsArgument(ISiteModel siteModel, ushort targetMin, ushort targetMax)
    {
      return new TemperatureStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Overrides = new OverrideParameters
        { 
          OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord(targetMin, targetMax),
          OverrideTemperatureWarningLevels = targetMin > 0 && targetMax > 0
        }
      };
    }

    private ISiteModel BuildModelForSingleCellTemperature(ushort temperatureIncrement)
    {
      var baseTime = DateTime.UtcNow;
      ushort baseTemperature = 10;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          MaterialTemperature = (ushort) (baseTemperature + x * temperatureIncrement),
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }

    [Fact]
    public void Test_SummaryTemperatureStatistics_Creation()
    {
      var operation = new TemperatureStatisticsOperation();

      operation.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_SummaryTemperatureStatistics_EmptySiteModel_FullExtents_NoTemperatureTargetOverride()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var operation = new TemperatureStatisticsOperation();

      var temperatureSummaryResult = await operation.ExecuteAsync(SimpleTemperatureStatisticsArgument(siteModel, 0, 0));

      temperatureSummaryResult.Should().NotBeNull();
      temperatureSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public async Task Test_SummaryTemperatureStatistics_SiteModelWithSingleCell_FullExtents_NoTemperatureTargetOverride()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellTemperature(TEMPERATURE_INCREMENT);
      var operation = new TemperatureStatisticsOperation();

      var temperatureSummaryResult = await operation.ExecuteAsync(SimpleTemperatureStatisticsArgument(siteModel, 0, 0));

      temperatureSummaryResult.Should().NotBeNull();
      temperatureSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      temperatureSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoResult);
      temperatureSummaryResult.MinimumTemperature.Should().Be(CellPassConsts.NullMaterialTemperatureValue);
      temperatureSummaryResult.MaximumTemperature.Should().Be(CellPassConsts.NullMaterialTemperatureValue);
      temperatureSummaryResult.IsTargetTemperatureConstant.Should().BeTrue();
      temperatureSummaryResult.Counts.Should().BeNull();
      temperatureSummaryResult.Percents.Should().BeNull();
      temperatureSummaryResult.AboveTargetPercent.Should().Be(0.0);
      temperatureSummaryResult.WithinTargetPercent.Should().Be(0.0);
      temperatureSummaryResult.BelowTargetPercent.Should().Be(0.0);
      temperatureSummaryResult.TotalAreaCoveredSqMeters.Should().Be(0.0);

    }

    [Fact]
    public async Task Test_SummaryTemperatureStatistics_SiteModelWithSingleCell_FullExtents_NoTemperatureTargetOverride_WithMachineTemperatureTarget()
    {
      const ushort TARGET_TEMPERATURE_MIN = 25;
      const ushort TARGET_TEMPERATURE_MAX = 80;

      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellTemperature(TEMPERATURE_INCREMENT);
      siteModel.MachinesTargetValues[0].TargetMinMaterialTemperature.PutValueAtDate(VSS.TRex.Common.Consts.MIN_DATETIME_AS_UTC, TARGET_TEMPERATURE_MIN);
      siteModel.MachinesTargetValues[0].TargetMaxMaterialTemperature.PutValueAtDate(VSS.TRex.Common.Consts.MIN_DATETIME_AS_UTC, TARGET_TEMPERATURE_MAX);

      var operation = new TemperatureStatisticsOperation();

      var temperatureSummaryResult = await operation.ExecuteAsync(SimpleTemperatureStatisticsArgument(siteModel, 0, 0));

      temperatureSummaryResult.Should().NotBeNull();
      temperatureSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      temperatureSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoProblems);
      temperatureSummaryResult.MinimumTemperature.Should().Be(TARGET_TEMPERATURE_MIN);
      temperatureSummaryResult.MaximumTemperature.Should().Be(TARGET_TEMPERATURE_MAX);
      temperatureSummaryResult.IsTargetTemperatureConstant.Should().BeTrue();
      temperatureSummaryResult.Counts.Should().BeNull();
      temperatureSummaryResult.Percents.Should().BeNull();
      temperatureSummaryResult.BelowTargetPercent.Should().Be(0.0);
      temperatureSummaryResult.AboveTargetPercent.Should().Be(100.0);
      temperatureSummaryResult.WithinTargetPercent.Should().Be(0.0);
      temperatureSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
    }

    [Theory]
    [InlineData(50, 80, 0.0, 0.0, 100.0)]
    [InlineData(80, 120, 0.0, 100.0, 0.0)]
    [InlineData(110, 150, 100.0, 0.0, 0.0)]
    public async Task Test_SummaryTemperatureStatistics_SiteModelWithSingleCell_FullExtents_WithTemperatureTargetOverrides
      (ushort minTarget, ushort maxTarget, double percentBelow, double percentWithin, double percentAbove)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellTemperature(TEMPERATURE_INCREMENT);
      var operation = new TemperatureStatisticsOperation();

      var temperatureSummaryResult = await operation.ExecuteAsync(SimpleTemperatureStatisticsArgument(siteModel, minTarget, maxTarget));

      temperatureSummaryResult.Should().NotBeNull();
      temperatureSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      temperatureSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoProblems);
      temperatureSummaryResult.MinimumTemperature.Should().Be(minTarget);
      temperatureSummaryResult.MaximumTemperature.Should().Be(maxTarget);
      temperatureSummaryResult.IsTargetTemperatureConstant.Should().BeTrue();
      temperatureSummaryResult.Counts.Should().BeNull();
      temperatureSummaryResult.Percents.Should().BeNull();
      temperatureSummaryResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      temperatureSummaryResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      temperatureSummaryResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      temperatureSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
    }

    [Theory]
    [InlineData(0, 0, 0.0, 0.0, 0.0)]
    public async Task Test_DetailedTemperatureStatistics_SiteModelWithSingleCell_FullExtents
      (ushort minTarget, ushort maxTarget, double percentBelow, double percentWithin, double percentAbove)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellTemperature(TEMPERATURE_INCREMENT);
      var operation = new TemperatureStatisticsOperation();

      var arg = SimpleTemperatureStatisticsArgument(siteModel, minTarget, maxTarget);
      arg.TemperatureDetailValues = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150 };
      var temperatureDetailResult = await operation.ExecuteAsync(arg);

      temperatureDetailResult.Should().NotBeNull();

      // Checks counts and percentages
      temperatureDetailResult.Counts.Sum().Should().Be(1);
      temperatureDetailResult.Counts[9].Should().Be(1);
      temperatureDetailResult.Percents.Sum().Should().BeApproximately(100.0, 0.000001);
      temperatureDetailResult.Percents[9].Should().BeApproximately(100.0, 0.000001);

      // Check summary related fields are zero
      temperatureDetailResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      temperatureDetailResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoResult);
      temperatureDetailResult.MinimumTemperature.Should().Be(CellPassConsts.NullMaterialTemperatureValue);
      temperatureDetailResult.MaximumTemperature.Should().Be(CellPassConsts.NullMaterialTemperatureValue);
      temperatureDetailResult.IsTargetTemperatureConstant.Should().BeTrue();
      temperatureDetailResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      temperatureDetailResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      temperatureDetailResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      temperatureDetailResult.TotalAreaCoveredSqMeters.Should().BeApproximately(0, 0.000001); // This being zero seems strange...
    }

    [Theory]
    [InlineData(0, 0, 8.8081204977079253, 80.582842174197779, 10.609037328094303)]
    [InlineData(500, 800, 7.8912901113294032, 0.49115913555992141, 91.617550753110677)]
    [InlineData(850, 950, 9.0700720366732153, 6.8107400130975773, 84.119187950229218)]
    [InlineData(1300, 1500, 100, 0.0, 0.0)]
    public async Task Test_SummaryTemperatureStatistics_SiteModelWithSingleTAGFile_FullExtents_WithTemperatureTargetOverrides
      (ushort minTarget, ushort maxTarget, double percentBelow, double percentWithin, double percentAbove)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var operation = new TemperatureStatisticsOperation();

      var temperatureSummaryResult = await operation.ExecuteAsync(SimpleTemperatureStatisticsArgument(siteModel, minTarget, maxTarget));

      temperatureSummaryResult.Should().NotBeNull();
      temperatureSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      temperatureSummaryResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoProblems);
      temperatureSummaryResult.MinimumTemperature.Should().Be(minTarget == 0 ? MACHINE_TARGET_MIN : minTarget);
      temperatureSummaryResult.MaximumTemperature.Should().Be(maxTarget == 0 ? MACHINE_TARGET_MAX : maxTarget);
      temperatureSummaryResult.IsTargetTemperatureConstant.Should().BeTrue();
      temperatureSummaryResult.Counts.Should().BeNull();
      temperatureSummaryResult.Percents.Should().BeNull();
      temperatureSummaryResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      temperatureSummaryResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      temperatureSummaryResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      temperatureSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(3054 * SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);

    }

    [Theory]
    [InlineData(0, 0, 8.8081204977079253, 80.582842174197779, 10.609037328094303)]
    public async Task Test_DetailedTemperatureStatistics_SiteModelWithSingleTAGFile_FullExtents
      (ushort minTarget, ushort maxTarget, double percentBelow, double percentWithin, double percentAbove)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var operation = new TemperatureStatisticsOperation();

      var arg = SimpleTemperatureStatisticsArgument(siteModel, minTarget, maxTarget);
      arg.TemperatureDetailValues = new[] { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500 };
      var temperatureDetailResult = await operation.ExecuteAsync(arg);

      temperatureDetailResult.Should().NotBeNull();

      // Checks counts and percentages
      long[] expectedCounts = { 0, 0, 231, 10, 7, 0, 8, 105, 275, 957, 1461, 0, 0, 0, 0 };
      long expectedCountsSum = 0;
      for (int i = 0; i < expectedCounts.Length; i++)
        expectedCountsSum += (i + 1) * expectedCounts[i];

      // Is sum of counts the same?
      long TemperatureDetailResultSum = 0;
      for (int i = 0; i < temperatureDetailResult.Counts.Length; i++)
        TemperatureDetailResultSum += (i + 1) * temperatureDetailResult.Counts[i];

      TemperatureDetailResultSum.Should().Be(expectedCountsSum);

      // Are all counts the same and do percentages match?

      long totalCount = temperatureDetailResult.Counts.Sum();
      for (int i = 0; i < expectedCounts.Length; i++)
      {
        expectedCounts[i].Should().Be(temperatureDetailResult.Counts[i]);
        temperatureDetailResult.Percents[i].Should().BeApproximately(100.0 * expectedCounts[i] / (1.0 * totalCount), 0.001);
      }

      // Check summary related fields are zero
      temperatureDetailResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      temperatureDetailResult.ReturnCode.Should().Be(MissingTargetDataResultType.NoProblems);
      temperatureDetailResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      temperatureDetailResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      temperatureDetailResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      temperatureDetailResult.TotalAreaCoveredSqMeters.Should().BeApproximately(3054 * SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
      temperatureDetailResult.MinimumTemperature.Should().Be(MACHINE_TARGET_MIN);
      temperatureDetailResult.MaximumTemperature.Should().Be(MACHINE_TARGET_MAX);
      temperatureDetailResult.IsTargetTemperatureConstant.Should().BeTrue();
    }
  }
}
