using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.ComputeFuncs;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
  [UnitTestCoveredRequest(RequestType = typeof(ProgressiveVolumesRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(ProgressiveVolumesRequest_ClusterCompute))]
  public class ProgressiveVolumesRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private const float ELEVATION_INCREMENT_0_5 = 0.5f;

    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<ProgressiveVolumesRequestComputeFunc_ApplicationService, ProgressiveVolumesRequestArgument, ProgressiveVolumesResponse>();

    private void AddClusterComputeGridRouting()
    {
      IgniteMock.AddClusterComputeGridRouting<ProgressiveVolumesRequestComputeFunc_ClusterCompute, ProgressiveVolumesRequestArgument, ProgressiveVolumesResponse>();
    }

    [Fact]
    public void Creation1()
    {
      var request = new ProgressiveVolumesRequest_ApplicationService();

      Assert.NotNull(request);
    }

    [Fact]
    public void Creation2()
    {
      var request = new ProgressiveVolumesRequest_ClusterCompute();

      Assert.NotNull(request);
    }

    private ProgressiveVolumesRequestArgument DefaultRequestArg(Guid ProjectUid)
    {
      return new ProgressiveVolumesRequestArgument
      {
        Interval = new TimeSpan(1, 0, 0, 0),
        StartDate = new DateTime(2000, 1, 1, 1, 1, 1),
        EndDate = new DateTime(2020, 1, 1, 1, 1, 1),
        ProjectID = ProjectUid,
        VolumeType = VolumeComputationType.Between2Filters,
        Filters = new FilterSet(new CombinedFilter()),
        BaseDesign = new DesignOffset(),
        TopDesign = new DesignOffset(),
        CutTolerance = 0.001,
        FillTolerance = 0.001
      };
    }

    private ProgressiveVolumesRequestArgument DefaultRequestArgFromModel(ISiteModel siteModel)
    {
      var arg = DefaultRequestArg(siteModel.ID);

      var (startUtc, endUtc) = siteModel.GetDateRange();

      arg.StartDate = startUtc;
      arg.EndDate = endUtc;
      arg.Interval = new TimeSpan((arg.EndDate.Ticks - arg.StartDate.Ticks) / 10);

      return arg;
    }

    private void CheckResponseContainsNullValues(ProgressiveVolumesResponse response)
    {
      response.Should().NotBeNull();

      if (response.Volumes == null)
      {
        return;
      }

      foreach (var volume in response.Volumes)
      {
        volume.Volume.BoundingExtentGrid.Should().BeEquivalentTo(BoundingWorldExtent3D.Null());
        volume.Volume.BoundingExtentLLH.Should().BeEquivalentTo(BoundingWorldExtent3D.Null());
      }
    }

    [Fact]
    public async Task ApplicationService_DefaultFilterToFilter_Execute_NoData()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var request = new ProgressiveVolumesRequest_ApplicationService();
      var response = await request.ExecuteAsync(DefaultRequestArg(Guid.NewGuid()));

      // This is a no data test, so the response will be null
      CheckResponseContainsNullValues(response);
    }
    
    [Fact]
    public async Task ClusterCompute_DefaultFilterToFilter_Execute_NoData()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var request = new ProgressiveVolumesRequest_ClusterCompute();
      var response = await request.ExecuteAsync(DefaultRequestArg(Guid.NewGuid()));

      // This is a no data test, so the response will be null
      CheckResponseContainsNullValues(response);
    }

    private void CheckDefaultFilterToFilterSingleTAGFileResponse(ProgressiveVolumesResponse response)
    {
      //Was, response = {Cut:1.00113831634521, Fill:2.48526947021484, Cut Area:117.5652, FillArea: 202.9936, Total Area:353.0424, BoundingGrid:MinX: 537669.2, MaxX:537676.34, MinY:5427391.44, MaxY:5427514.52, MinZ: 1E+308, MaxZ:1E+308, BoundingLLH:MinX: 1E+308, MaxX:1E+308, MinY:1...
      const double EPSILON = 0.000001;
      response.Should().NotBeNull();

      // todo: Complete this
/*      response.BoundingExtentGrid.MinX.Should().BeApproximately(537669.2, EPSILON);
      response.BoundingExtentGrid.MinY.Should().BeApproximately(5427391.44, EPSILON);
      response.BoundingExtentGrid.MaxX.Should().BeApproximately(537676.34, EPSILON);
      response.BoundingExtentGrid.MaxY.Should().BeApproximately(5427514.52, EPSILON);
      response.BoundingExtentGrid.MinZ.Should().Be(Consts.NullDouble);
      response.BoundingExtentGrid.MaxZ.Should().Be(Consts.NullDouble);*/
    }

    [Fact]
    public async Task ClusterCompute_DefaultFilterToFilter_Execute_SingleTAGFile()
    {
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var request = new ProgressiveVolumesRequest_ClusterCompute();
      var response = await request.ExecuteAsync(DefaultRequestArgFromModel(siteModel));

      CheckDefaultFilterToFilterSingleTAGFileResponse(response);
    }

    [Fact]
    public async Task ApplicationService_DefaultFilterToFilter_Execute_SingleTAGFile()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var request = new ProgressiveVolumesRequest_ApplicationService();
      var response = await request.ExecuteAsync(DefaultRequestArgFromModel(siteModel));

      CheckDefaultFilterToFilterSingleTAGFileResponse(response);
    }

    private void CheckDefaultSingleCellAtOriginResponse(ProgressiveVolumesResponse response)
    {
      const double EPSILON = 0.000001;

      response.Should().NotBeNull();

      // todo: Complete this
/*
      response.BoundingExtentGrid.MinX.Should().BeApproximately(0, EPSILON);
      response.BoundingExtentGrid.MinY.Should().BeApproximately(0, EPSILON);
      response.BoundingExtentGrid.MaxX.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize, EPSILON);
      response.BoundingExtentGrid.MaxY.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize, EPSILON);
      response.BoundingExtentGrid.MinZ.Should().Be(Consts.NullDouble);
      response.BoundingExtentGrid.MaxZ.Should().Be(Consts.NullDouble);
      */
    }

    private ISiteModel BuildModelForSingleCellSummaryVolume(float heightIncrement)
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
        });

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Count());
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }

    [Fact]
    public async Task ClusterCompute_DefaultFilterToFilter_Execute_SingleCell()
    {
      AddClusterComputeGridRouting();

      var siteModel = BuildModelForSingleCellSummaryVolume(ELEVATION_INCREMENT_0_5);

      var request = new ProgressiveVolumesRequest_ClusterCompute();
      var response = await request.ExecuteAsync(DefaultRequestArgFromModel(siteModel));

      CheckDefaultSingleCellAtOriginResponse(response);
    }

    [Fact]
    public async Task ApplicationService_DefaultFilterToFilter_Execute_SingleCell()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = BuildModelForSingleCellSummaryVolume(ELEVATION_INCREMENT_0_5);

      var request = new ProgressiveVolumesRequest_ApplicationService();
      var response = await request.ExecuteAsync(DefaultRequestArgFromModel(siteModel));

      CheckDefaultSingleCellAtOriginResponse(response);
    }
  }
}
