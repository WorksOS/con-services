using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using FluentAssertions;
using VSS.TRex.Analytics.CutFillStatistics;
using VSS.TRex.Analytics.CutFillStatistics.GridFabric;
using VSS.TRex.Cells;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.TTM;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CutFillStatistics.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(CutFillStatisticsRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(CutFillStatisticsRequest_ClusterCompute))]
  public class CutFillStatisticsRequestTests : BaseTests<CutFillStatisticsArgument, CutFillStatisticsResponse>, IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private const float HEIGHT_INCREMENT_0_5 = 0.5f;

    private void AddDesignProfilerGridRouting()
    {
      IgniteMock.AddApplicationGridRouting<IComputeFunc<CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();
    }

    private CutFillStatisticsArgument SimpleCutFillStatisticsArgument(ISiteModel siteModel, Guid designUid, double offset)
    {
      return new CutFillStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesign = new DesignOffset(designUid, offset),
        Offsets = new double[0]
      };
    }

    private ISiteModel BuildModelForSingleCellCutFill(float heightIncrement)
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

    private ISiteModel BuildModelForSingleSubGridCutFill(float heightIncrement, float baseHeight=1.0f)
    {
      var baseTime = DateTime.UtcNow;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      CellPass[,][] cellPasses = new CellPass[32,32][];

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        cellPasses[x, y] = Enumerable.Range(0, 1).Select(p =>
          new CellPass
          {
            InternalSiteModelMachineIndex = bulldozerMachineIndex,
            Time = baseTime.AddMinutes(p),
            Height = baseHeight + (x + y) * heightIncrement, // incrementally increase height across the sub grid
            PassType = PassType.Front
          }).ToArray();
      });

      DITAGFileAndSubGridRequestsFixture.AddSingleSubGridWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      return siteModel;
    }

    [Fact]
    public void Creation()
    {
      var reqApplication = new CutFillStatisticsRequest_ApplicationService();
      reqApplication.Should().NotBeNull();

      var reqClusterCompute = new CutFillStatisticsRequest_ClusterCompute();
      reqClusterCompute.Should().NotBeNull();
    }

    [Fact]
    public async Task EmptySiteModel_FullExtents_NoDesign()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var operation = new CutFillStatisticsOperation();

      var argument = SimpleCutFillStatisticsArgument(siteModel, Guid.NewGuid(), 1.5);
      var result = await operation.ExecuteAsync(argument);

      result.Should().NotBeNull();
      result.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public async Task SiteModelWithSingleCell_FullExtents_NoDesign()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellCutFill(HEIGHT_INCREMENT_0_5);

      var operation = new CutFillStatisticsOperation();
      var argument = SimpleCutFillStatisticsArgument(siteModel, Guid.NewGuid(), 1.5);
      var result = await operation.ExecuteAsync(argument);

      result.Should().NotBeNull();
      result.ResultStatus.Should().Be(RequestErrorStatus.NoDesignProvided);
    }

    [Fact]
    public async Task SiteModelWithSingleCell_FullExtents_WithSingleFlatTriangleDesignAboutOrigin()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();
      AddDesignProfilerGridRouting();

      var siteModel = BuildModelForSingleCellCutFill(HEIGHT_INCREMENT_0_5);
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 1.0f);

      var operation = new CutFillStatisticsOperation();
      var argument = SimpleCutFillStatisticsArgument(siteModel, designUid, 0);
      argument.Offsets = new[] {0.5, 0.2, 0.1, 0.0, -0.1, -0.2, -0.5};

      var result = await operation.ExecuteAsync(argument);

      result.Should().NotBeNull();
      result.ResultStatus.Should().Be(RequestErrorStatus.OK);
      result.Counts[0].Should().Be(1);
      result.Percents[0].Should().Be(100);
    }

    [Fact]
    public async Task SiteModelWithSingleSubGrid_FullExtents_WithSingleFlatTriangleDesignAboutOrigin()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();
      AddDesignProfilerGridRouting();

      var siteModel = BuildModelForSingleSubGridCutFill(0.1f);
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 2.0f);

      var operation = new CutFillStatisticsOperation();
      var argument = SimpleCutFillStatisticsArgument(siteModel, designUid, 0);
      argument.Offsets = new[] { 1.0, 0.4, 0.2, 0.0, -0.2, -0.4, -1.0 };

      var result = await operation.ExecuteAsync(argument);

      result.Should().NotBeNull();
      result.ResultStatus.Should().Be(RequestErrorStatus.OK);

      result.Counts[0].Should().Be(693);
      result.Counts[1].Should().Be(105);
      result.Counts[2].Should().Be(27);
      result.Counts[3].Should().Be(33);
      result.Counts[4].Should().Be(24);
      result.Counts[5].Should().Be(20);
      result.Counts[6].Should().Be(1);

      long sum = result.Counts.Sum();

      for (int i = 0; i < result.Counts.Length; i++)
        result.Percents[i].Should().Be((double) result.Counts[i] / sum * 100);
    }

    [Theory(Skip="See BUG#85914")]
    [InlineData(0, 3)]//Difference between production data and design is 0.1
    [InlineData(-0.4, 2)]//Difference between production data and design is -0.3
    [InlineData(0.5, 5)]//Difference between production data and design is 0.6
    public async Task SiteModelWithSingleFlatSubGrid_FullExtents_WithSingleFlatTriangleDesignAboutOrigin(double offset, int indexWithData)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();
      AddDesignProfilerGridRouting();

      var siteModel = BuildModelForSingleSubGridCutFill(0.0f);
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 1.1f);

      var operation = new CutFillStatisticsOperation();
      var argument = SimpleCutFillStatisticsArgument(siteModel, designUid, offset);
      argument.Offsets = new[] { 1.0, 0.4, 0.2, 0.0, -0.2, -0.4, -1.0 };

      var result = await operation.ExecuteAsync(argument);

      result.Should().NotBeNull();
      result.ResultStatus.Should().Be(RequestErrorStatus.OK);

      for (int i = 0; i < 6; i++)
      {
        if (i == indexWithData)
          result.Counts[i].Should().Be(903);
        else
          result.Counts[i].Should().Be(0);
      }
  
      long sum = result.Counts.Sum();

      for (int i = 0; i < result.Counts.Length; i++)
        result.Percents[i].Should().Be((double)result.Counts[i] / sum * 100);
    }
  }
}
