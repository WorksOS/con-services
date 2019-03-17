using System;
using System.IO;
using System.Linq;
using Apache.Ignite.Core.Compute;
using FluentAssertions;
using VSS.TRex.Analytics.CutFillStatistics;
using VSS.TRex.Analytics.CutFillStatistics.GridFabric;
using VSS.TRex.Cells;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.TTM;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.Analytics.Common;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.TRex.Tests.Analytics.CutFillStatistics.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(CutFillStatisticsRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(CutFillStatisticsRequest_ClusterCompute))]
  public class CutFillStatisticsRequestTests : BaseTests<CutFillStatisticsArgument, CutFillStatisticsResponse>, IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private ISiteModel NewEmptyModel()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      _ = siteModel.Machines.CreateNew("Bulldozer", "", MachineType.Dozer, DeviceType.SNM940, false, Guid.NewGuid());
      return siteModel;
    }

    private void AddDesignProfilerGridRouting()
    {
      IgniteMock.AddApplicationGridRouting<IComputeFunc<CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();
    }

    private CutFillStatisticsArgument SimpleCutFillStatisticsArgument(ISiteModel siteModel, Guid DesignUid)
    {
      return new CutFillStatisticsArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        DesignID = DesignUid,
        Offsets = new double[0]
      };
    }

    private void BuildModelForSingleCellCutFill(out ISiteModel siteModel, float heightIncrement)
    {
      var baseTime = DateTime.UtcNow;
      var baseHeight = 1.0f;

      siteModel = NewEmptyModel();
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
    }

    private void BuildModelForSingleSubGridCutFill(out ISiteModel siteModel, float heightIncrement)
    {
      var baseTime = DateTime.UtcNow;
      var baseHeight = 1.0f;

      siteModel = NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var _siteModel = siteModel;

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
    public void EmptySiteModel_FullExtents_NoDesign()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = NewEmptyModel();
      var operation = new CutFillStatisticsOperation();

      var argument = SimpleCutFillStatisticsArgument(siteModel, Guid.NewGuid());
      var result = operation.Execute(argument);

      result.Should().NotBeNull();
      result.ResultStatus.Should().Be(RequestErrorStatus.FailedToRequestDatamodelStatistics);
    }

    [Fact]
    public void SiteModelWithSingleCell_FullExtents_NoDesign()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellCutFill(out var siteModel, 0.5f);

      var operation = new CutFillStatisticsOperation();
      var argument = SimpleCutFillStatisticsArgument(siteModel, Guid.NewGuid());
      var result = operation.Execute(argument);

      result.Should().NotBeNull();
      result.ResultStatus.Should().Be(RequestErrorStatus.NoDesignProvided);
    }

    private Guid ConstructSingleFlatTriangleDesignAboutOrigin(ref ISiteModel siteModel, float elevation)
    {
      // Make the mutable TIN containing the triangle and register it to the site model
      VSS.TRex.Designs.TTM.TrimbleTINModel tin = new TrimbleTINModel();
      tin.Vertices.InitPointSearch(-100, -100, 100, 100, 3);
      tin.Triangles.AddTriangle(tin.Vertices.AddPoint(-25, -25, elevation),
        tin.Vertices.AddPoint(25, -25, elevation),
        tin.Vertices.AddPoint(0, 25, elevation));

      var tempFileName = Path.GetTempFileName() + ".ttm";
      tin.SaveToFile(tempFileName, true);

      return DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel
        (ref siteModel, Path.GetDirectoryName(tempFileName), Path.GetFileName(tempFileName), true);
    }

    [Fact]
    public void SiteModelWithSingleCell_FullExtents_WithSingleFlatTriangleDesignAboutOrigin()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();
      AddDesignProfilerGridRouting();

      BuildModelForSingleCellCutFill(out var siteModel, 0.5f);
      var designUid = ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 1.0f);

      var operation = new CutFillStatisticsOperation();
      var argument = SimpleCutFillStatisticsArgument(siteModel, designUid);
      argument.Offsets = new[] {0.5, 0.2, 0.1, 0.0, -0.1, -0.2, -0.5};

      var result = operation.Execute(argument);

      result.Should().NotBeNull();
      result.ResultStatus.Should().Be(RequestErrorStatus.OK);
      result.Counts[0].Should().Be(1);
      result.Percents[0].Should().Be(100);
    }

    [Fact]
    public void SiteModelWithSingleSubGrid_FullExtents_WithSingleFlatTriangleDesignAboutOrigin()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();
      AddDesignProfilerGridRouting();

      BuildModelForSingleSubGridCutFill(out var siteModel, 0.1f);
      var designUid = ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 2.0f);

      var operation = new CutFillStatisticsOperation();
      var argument = SimpleCutFillStatisticsArgument(siteModel, designUid);
      argument.Offsets = new[] { 1.0, 0.4, 0.2, 0.0, -0.2, -0.4, -1.0 };

      var result = operation.Execute(argument);

      result.Should().NotBeNull();
      result.ResultStatus.Should().Be(RequestErrorStatus.OK);

      result.Counts[0].Should().Be(693);
      result.Counts[1].Should().Be(105);
      result.Counts[2].Should().Be(27 );
      result.Counts[3].Should().Be(33 );
      result.Counts[4].Should().Be(24 );
      result.Counts[5].Should().Be(20 );
      result.Counts[6].Should().Be(1);

      long sum = result.Counts.Sum();

      for (int i = 0; i < result.Counts.Length; i++)
        result.Percents[i].Should().Be((double)result.Counts[i] / sum * 100);
    }
  }
}
