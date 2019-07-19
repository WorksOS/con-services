using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Analytics.CMVChangeStatistics;
using VSS.TRex.Analytics.CMVChangeStatistics.GridFabric;
using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CMVChangeStatistics.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(CMVChangeStatisticsRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(CMVChangeStatisticsRequest_ClusterCompute))]
  public class CMVChangeStatisticsRequestTests : BaseTests<CMVChangeStatisticsArgument, CMVChangeStatisticsResponse>, IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private const short LENGTH = 7;
    private const short CMV_INCREMENT = 10;

    private CMVChangeStatisticsArgument SimpleCMVChangeStatisticsArgument(ISiteModel siteModel, double[] cmvChangeValues)
    {
      return new CMVChangeStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        CMVChangeDetailsDataValues = cmvChangeValues
      };
    }

    private ISiteModel BuildModelForSingleCellCMV(short cmvIncrement, short targetCMV = CellPassConsts.NullCCV)
    {
      var baseTime = DateTime.UtcNow;
      short baseCMV = 10;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      if (targetCMV != CellPassConsts.NullCCV)
        siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetCCVStateEvents.PutValueAtDate(TRex.Common.Consts.MIN_DATETIME_AS_UTC, targetCMV);

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

      return siteModel;
    }

    private ISiteModel BuildModelForCellsCMV(short cmvIncrement, short targetCMV = CellPassConsts.NullCCV)
    {
      var baseTime = DateTime.UtcNow;
      short baseCMV = 10;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      if (targetCMV != CellPassConsts.NullCCV)
        siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetCCVStateEvents.PutValueAtDate(TRex.Common.Consts.MIN_DATETIME_AS_UTC, targetCMV);

      CellPass[,][] cellPasses = new CellPass[32, 32][];

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        cellPasses[x, y] = Enumerable.Range(0, 1).Select(p =>
          new CellPass
          {
            InternalSiteModelMachineIndex = bulldozerMachineIndex,
            Time = baseTime.AddMinutes(p),
            CCV = (short)(baseCMV + x * cmvIncrement), // incrementally increase height across the sub grid
            PassType = PassType.Front
          }).ToArray();
      });

      DITAGFileAndSubGridRequestsFixture.AddSingleSubGridWithPasses(siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      return siteModel;
    }

    [Fact]
    public void Test_CMVChangeStatisticsRequest_Creation()
    {
      var operation = new CMVChangeStatisticsOperation();

      operation.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_CMVChangeStatisticsRequest_EmptySiteModel_FullExtents()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var operation = new CMVChangeStatisticsOperation();

      var cmvChangeStatisticsResult = await operation.ExecuteAsync(SimpleCMVChangeStatisticsArgument(siteModel, new []{ -50.0, -20.0, -10.0, 0.0, 10.0, 20.0, 50.0 }));

      cmvChangeStatisticsResult.Should().NotBeNull();
      cmvChangeStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public async Task Test_CMVChangeStatisticsRequest_SiteModelWithSingleCell_FullExtents_NoTarget()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellCMV(CMV_INCREMENT);
      var operation = new CMVChangeStatisticsOperation();

      var cmvChangeStatisticsResult = await operation.ExecuteAsync(SimpleCMVChangeStatisticsArgument(siteModel, new[] { -50.0, -20.0, -10.0, 0.0, 10.0, 20.0, 50.0 }));

      cmvChangeStatisticsResult.Should().NotBeNull();
      cmvChangeStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      cmvChangeStatisticsResult.BelowTargetPercent.Should().Be(0);
      cmvChangeStatisticsResult.AboveTargetPercent.Should().Be(0);
      cmvChangeStatisticsResult.WithinTargetPercent.Should().Be(0);
      cmvChangeStatisticsResult.TotalAreaCoveredSqMeters.Should().Be(0);
      cmvChangeStatisticsResult.Counts.Should().NotBeNull();
      cmvChangeStatisticsResult.Counts.Length.Should().Be(LENGTH);

      for (var i = 0; i < cmvChangeStatisticsResult.Counts.Length; i++)
        cmvChangeStatisticsResult.Counts[i].Should().Be(0);

      cmvChangeStatisticsResult.Percents.Should().NotBeNull();
      cmvChangeStatisticsResult.Percents.Length.Should().Be(LENGTH);

      for (var i = 0; i < cmvChangeStatisticsResult.Percents.Length; i++)
        cmvChangeStatisticsResult.Percents[i].Should().Be(0);
    }

    [Fact]
    public async Task Test_SummaryCCAStatisticsRequest_SiteModelWithSingleCell_FullExtents()
    {
      const byte TARGET_CMV = 50;

      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellCMV(CMV_INCREMENT, TARGET_CMV);
      var operation = new CMVChangeStatisticsOperation();

      var cmvChangeStatisticsResult = await operation.ExecuteAsync(SimpleCMVChangeStatisticsArgument(siteModel, new[] { -50.0, -20.0, -10.0, 0.0, 10.0, 20.0, 50.0 }));

      cmvChangeStatisticsResult.Should().NotBeNull();
      cmvChangeStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      cmvChangeStatisticsResult.BelowTargetPercent.Should().Be(0);
      cmvChangeStatisticsResult.AboveTargetPercent.Should().Be(0);
      cmvChangeStatisticsResult.WithinTargetPercent.Should().Be(0);
      cmvChangeStatisticsResult.TotalAreaCoveredSqMeters.Should().Be(0);
      cmvChangeStatisticsResult.Counts.Should().NotBeNull();
      cmvChangeStatisticsResult.Counts.Length.Should().Be(LENGTH);

      for (var i = 0; i < cmvChangeStatisticsResult.Counts.Length; i++)
        cmvChangeStatisticsResult.Counts[i].Should().Be(0);

      cmvChangeStatisticsResult.Percents.Should().NotBeNull();
      cmvChangeStatisticsResult.Percents.Length.Should().Be(LENGTH);

      for (var i = 0; i < cmvChangeStatisticsResult.Percents.Length; i++)
        cmvChangeStatisticsResult.Percents[i].Should().Be(0);
    }

    [Fact]
    public async Task Test_CMVChangeStatisticsRequest_SiteModelWithMultipleCells_FullExtents()
    {
      const short NUMBER_OF_CELLS = 2177;

      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var operation = new CMVChangeStatisticsOperation();

      var cmvChangeStatisticsResult = await operation.ExecuteAsync(SimpleCMVChangeStatisticsArgument(siteModel, new[] { -50.0, -20.0, -10.0, 0.0, 10.0, 20.0, 50.0 }));

      cmvChangeStatisticsResult.Should().NotBeNull();
      cmvChangeStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      cmvChangeStatisticsResult.BelowTargetPercent.Should().Be(0);
      cmvChangeStatisticsResult.AboveTargetPercent.Should().Be(0);
      cmvChangeStatisticsResult.WithinTargetPercent.Should().Be(0);
      cmvChangeStatisticsResult.TotalAreaCoveredSqMeters.Should().BeApproximately(NUMBER_OF_CELLS * SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
      cmvChangeStatisticsResult.Counts.Should().NotBeNull();
      cmvChangeStatisticsResult.Counts.Length.Should().Be(LENGTH);

      for (var i = 0; i < cmvChangeStatisticsResult.Counts.Length; i++)
      {
        if (i == LENGTH - 1)
          cmvChangeStatisticsResult.Counts[i].Should().Be(NUMBER_OF_CELLS);
        else
          cmvChangeStatisticsResult.Counts[i].Should().Be(0);
      }

      cmvChangeStatisticsResult.Percents.Should().NotBeNull();
      cmvChangeStatisticsResult.Percents.Length.Should().Be(LENGTH);

      for (var i = 0; i < cmvChangeStatisticsResult.Percents.Length; i++)
      {
        if (i == LENGTH - 1)
          cmvChangeStatisticsResult.Percents[i].Should().Be(100);
        else
          cmvChangeStatisticsResult.Percents[i].Should().Be(0);
      }
    }
  }
}
