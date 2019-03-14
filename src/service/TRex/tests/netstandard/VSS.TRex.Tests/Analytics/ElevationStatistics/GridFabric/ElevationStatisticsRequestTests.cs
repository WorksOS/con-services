using System;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Analytics.ElevationStatistics;
using VSS.TRex.Analytics.ElevationStatistics.GridFabric;
using VSS.TRex.Cells;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.TRex.Tests.Analytics.ElevationStatistics.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(ElevationStatisticsRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(ElevationStatisticsRequest_ClusterCompute))]
  public class ElevationStatisticsRequestTests : BaseTests<ElevationStatisticsArgument, ElevationStatisticsResponse>, IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private ISiteModel NewEmptyModel()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      _ = siteModel.Machines.CreateNew("Bulldozer", "", MachineType.Dozer, DeviceType.SNM940, false, Guid.NewGuid());
      return siteModel;
    }

    private ElevationStatisticsArgument SimpleElevationStatisticsArgument(ISiteModel siteModel)
    {
      return new ElevationStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter())
      };
    }

    private void BuildModelForCellsElevation(out ISiteModel siteModel, float elevationIncrement, int numberOfCells)
    {
      var baseTime = DateTime.UtcNow;
      var baseElevation = 1.0f;

      siteModel = NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      for (int i = 1; i <= numberOfCells; i++)
      {
        var cellPasses = Enumerable.Range(0, 10).Select(x =>
          new CellPass
          {
            InternalSiteModelMachineIndex = bulldozerMachineIndex,
            Time = baseTime.AddMinutes(x),
            Height = baseElevation + x * elevationIncrement + i - 1,
            PassType = PassType.Front
          }).ToArray();

        DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
          (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset + (uint)(i - 1), SubGridTreeConsts.DefaultIndexOriginOffset + (uint)(i - 1), cellPasses, i, cellPasses.Length);

        baseElevation = cellPasses[cellPasses.Length - 1].Height;
      }
    }

    [Fact]
    public void Test_SummaryElevationStatistics_Creation()
    {
      var operation = new ElevationStatisticsOperation();

      operation.Should().NotBeNull();
    }

    [Fact]
    public void Test_SummaryElevationStatistics_EmptySiteModel_FullExtents()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = NewEmptyModel();
      var operation = new ElevationStatisticsOperation();

      var elevationStatisticsResult = operation.Execute(SimpleElevationStatisticsArgument(siteModel));

      elevationStatisticsResult.Should().NotBeNull();
      elevationStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public void Test_SummaryElevationStatistics_SiteModelWithSingleCell_FullExtents()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForCellsElevation(out var siteModel, 1, 1);
      var operation = new ElevationStatisticsOperation();

      var elevationStatisticsResult = operation.Execute(SimpleElevationStatisticsArgument(siteModel));

      elevationStatisticsResult.Should().NotBeNull();
      elevationStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      elevationStatisticsResult.MinElevation.Should().Be(10.0);
      elevationStatisticsResult.MaxElevation.Should().Be(10.0);
      elevationStatisticsResult.CoverageArea.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
      elevationStatisticsResult.BoundingExtents.IsValidPlanExtent.Should().Be(true);
    }

    [Fact]
    public void Test_SummaryElevationStatistics_SiteModelWithMultipleCells_FullExtents()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForCellsElevation(out var siteModel, 1, 2);

      var operation = new ElevationStatisticsOperation();

      var elevationStatisticsResult = operation.Execute(SimpleElevationStatisticsArgument(siteModel));

      elevationStatisticsResult.Should().NotBeNull();
      elevationStatisticsResult.ResultStatus.Should().Be(RequestErrorStatus.OK);
      elevationStatisticsResult.MinElevation.Should().Be(10.0);
      elevationStatisticsResult.MaxElevation.Should().Be(20.0);
      elevationStatisticsResult.CoverageArea.Should().BeApproximately(2 * SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, 0.000001);
      elevationStatisticsResult.BoundingExtents.IsValidPlanExtent.Should().Be(true);
    }

  }
}
