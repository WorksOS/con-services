using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Analytics.SpeedStatistics;
using VSS.TRex.Analytics.SpeedStatistics.GridFabric;
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

namespace VSS.TRex.Tests.Analytics.SpeedStatistics.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(SpeedStatisticsRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(SpeedStatisticsRequest_ClusterCompute))]
  public class SpeedStatisticsRequestTests : BaseTests<SpeedStatisticsArgument, SpeedStatisticsResponse>, IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private SpeedStatisticsArgument SimpleSpeedStatisticsArgument(ISiteModel siteModel, ushort minSpeed, ushort maxSpeed)
    {
      return new SpeedStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        Overrides = new OverrideParameters
        { TargetMachineSpeed = new MachineSpeedExtendedRecord(minSpeed, maxSpeed) }
      };
    }

    private ISiteModel BuildModelForSingleCellSpeed(ushort mdpIncrement)
    {
      var baseTime = DateTime.UtcNow;
      ushort baseMDP = 10;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          MachineSpeed = (ushort) (baseMDP + x * mdpIncrement),
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }

    [Fact]
    public void Test_SummarySpeedStatistics_Creation()
    {
      var operation = new SpeedStatisticsOperation();

      operation.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_SummarySpeedStatistics_EmptySiteModel_FullExtents_NoSpeedTargetOverride()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var operation = new SpeedStatisticsOperation();

      var mdpSummaryResult = await operation.ExecuteAsync(SimpleSpeedStatisticsArgument(siteModel, 0, 0));

      mdpSummaryResult.Should().NotBeNull();
      mdpSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public async Task Test_SummarySpeedStatistics_SiteModelWithSingleCell_FullExtents_NoSpeedTargetRange()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellSpeed(10);
      
      var operation = new SpeedStatisticsOperation();

      var speedSummaryResult = await operation.ExecuteAsync(SimpleSpeedStatisticsArgument(siteModel, CellPassConsts.NullMachineSpeed, CellPassConsts.NullMachineSpeed));

      speedSummaryResult.Should().NotBeNull();
      speedSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      speedSummaryResult.Counts.Should().BeNull();
      speedSummaryResult.Percents.Should().BeNull();
      speedSummaryResult.AboveTargetPercent.Should().Be(0.0);
      speedSummaryResult.WithinTargetPercent.Should().Be(0.0);
      speedSummaryResult.BelowTargetPercent.Should().Be(100.0);
      speedSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
    }

    [Theory]
    [InlineData(50, 70, 0.0, 100.0, 0.0)]
    [InlineData(70, 90, 100.0, 0.0, 0.0)]
    [InlineData(30, 50, 0.0, 0.0, 100.0)]
    public async Task Test_SummarySpeedStatistics_SiteModelWithSingleCell_FullExtents_WithSpeedTargetRange
      (ushort minTarget, ushort maxTarget, double percentBelow, double percentWithin, double percentAbove)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellSpeed(5);
      var operation = new SpeedStatisticsOperation();

      var speedSummaryResult = await operation.ExecuteAsync(SimpleSpeedStatisticsArgument(siteModel, minTarget, maxTarget));

      speedSummaryResult.Should().NotBeNull();
      speedSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      speedSummaryResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      speedSummaryResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      speedSummaryResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      speedSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
      speedSummaryResult.Counts.Should().BeNull();
      speedSummaryResult.Percents.Should().BeNull();
    }

    [Theory]
    [InlineData(0, 0, 0.0, 0.0, 100.0)]
    [InlineData(5, 10, 0.0, 0.0, 100.0)]
    [InlineData(20, 40, 0.0, 0.0, 100.0)]
    [InlineData(55, 65, 0.0, 0.06548788474132286, 99.934512115258684)]
    public async Task Test_SummarySpeedStatistics_SiteModelWithSingleTAGFile_FullExtents_WithSpeedTargetRange
      (ushort minTarget, ushort maxTarget, double percentBelow, double percentWithin, double percentAbove)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var operation = new SpeedStatisticsOperation();

      var cmvSummaryResult = await operation.ExecuteAsync(SimpleSpeedStatisticsArgument(siteModel, minTarget, maxTarget));

      cmvSummaryResult.Should().NotBeNull();
      cmvSummaryResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      cmvSummaryResult.Counts.Should().BeNull();
      cmvSummaryResult.Percents.Should().BeNull();
      cmvSummaryResult.BelowTargetPercent.Should().BeApproximately(percentBelow, 0.001);
      cmvSummaryResult.AboveTargetPercent.Should().BeApproximately(percentAbove, 0.001);
      cmvSummaryResult.WithinTargetPercent.Should().BeApproximately(percentWithin, 0.001);
      cmvSummaryResult.TotalAreaCoveredSqMeters.Should().BeApproximately(3054 * SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
    }
  }
}
