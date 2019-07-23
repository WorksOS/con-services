using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.TRex.Analytics.ElevationStatistics;
using VSS.TRex.Analytics.ElevationStatistics.GridFabric;
using VSS.TRex.Cells;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Analytics.ElevationStatistics.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(ElevationStatisticsRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(ElevationStatisticsRequest_ClusterCompute))]
  public class ElevationStatisticsRequestTests : BaseTests<ElevationStatisticsArgument, ElevationStatisticsResponse>, IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private const float ELEVATION_INCREMENT_1_0 = 1.0f;

    private ElevationStatisticsArgument SimpleElevationStatisticsArgument(ISiteModel siteModel)
    {
      return new ElevationStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter())
      };
    }

    private ISiteModel BuildModelForSingleCellElevation(float elevationIncrement)
    {
      var baseTime = DateTime.UtcNow;
      var baseElevation = 1.0F;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = (byte)(baseElevation + x * elevationIncrement),
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }

    private ISiteModel BuildModelForCellsElevation(float elevationIncrement)
    {
      var baseTime = DateTime.UtcNow;
      var baseElevation = 1.0F;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      CellPass[,][] cellPasses = new CellPass[32, 32][];

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        cellPasses[x, y] = Enumerable.Range(0, 1).Select(p =>
          new CellPass
          {
            InternalSiteModelMachineIndex = bulldozerMachineIndex,
            Time = baseTime.AddMinutes(p),
            Height = baseElevation + (x + y) * elevationIncrement, // incrementally increase height across the sub grid
            PassType = PassType.Front
          }).ToArray();
      });

      DITAGFileAndSubGridRequestsFixture.AddSingleSubGridWithPasses(siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses);

      return siteModel;
    }

    [Fact]
    public void Test_SummaryElevationStatistics_Creation()
    {
      var operation = new ElevationStatisticsOperation();

      operation.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_SummaryElevationStatistics_EmptySiteModel_FullExtents()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var operation = new ElevationStatisticsOperation();

      var elevationStatisticsResult = await operation.ExecuteAsync(SimpleElevationStatisticsArgument(siteModel));

      elevationStatisticsResult.Should().NotBeNull();
      elevationStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public async Task Test_SummaryElevationStatistics_SiteModelWithSingleCell_FullExtents()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellElevation(ELEVATION_INCREMENT_1_0);
      var operation = new ElevationStatisticsOperation();

      var elevationStatisticsResult = await operation.ExecuteAsync(SimpleElevationStatisticsArgument(siteModel));

      elevationStatisticsResult.Should().NotBeNull();
      elevationStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      elevationStatisticsResult.MinElevation.Should().Be(10.0);
      elevationStatisticsResult.MaxElevation.Should().Be(10.0);
      elevationStatisticsResult.CoverageArea.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
      elevationStatisticsResult.BoundingExtents.IsValidPlanExtent.Should().Be(true);
    }

    [Fact]
    public async Task Test_SummaryElevationStatistics_SiteModelWithMultipleCells_FullExtents()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForCellsElevation(ELEVATION_INCREMENT_1_0);

      var operation = new ElevationStatisticsOperation();

      var elevationStatisticsResult = await operation.ExecuteAsync(SimpleElevationStatisticsArgument(siteModel));

      elevationStatisticsResult.Should().NotBeNull();
      elevationStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      elevationStatisticsResult.MinElevation.Should().Be(1.0);
      elevationStatisticsResult.MaxElevation.Should().Be(63.0);
      elevationStatisticsResult.CoverageArea.Should().BeApproximately(1024 * SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
      elevationStatisticsResult.BoundingExtents.IsValidPlanExtent.Should().Be(true);
    }

  }
}
