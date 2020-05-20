using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Events;
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

    private void AddApplicationGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting<ProgressiveVolumesRequestComputeFunc_ApplicationService, ProgressiveVolumesRequestArgument, ProgressiveVolumesResponse>();

    private void AddClusterComputeGridRouting()
    {
      IgniteMock.Immutable.AddClusterComputeGridRouting<ProgressiveVolumesRequestComputeFunc_ClusterCompute, ProgressiveVolumesRequestArgument, ProgressiveVolumesResponse>();
    }

    private void AddDesignProfilerGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting
      <CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();


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

    private ProgressiveVolumesRequestArgument DefaultRequestArg(Guid projectUid)
    {
      return new ProgressiveVolumesRequestArgument
      {
        Interval = new TimeSpan(1, 0, 0, 0),
        StartDate = new DateTime(2000, 1, 1, 1, 1, 1),
        EndDate = new DateTime(2020, 1, 1, 1, 1, 1),
        ProjectID = projectUid,
        VolumeType = VolumeComputationType.Between2Filters,
        Filters = new FilterSet(new CombinedFilter()),
        BaseDesign = new DesignOffset(),
        TopDesign = new DesignOffset(),
        CutTolerance = 0.001,
        FillTolerance = 0.001
      };
    }

    private ProgressiveVolumesRequestArgument DefaultRequestArgFromModel(ISiteModel siteModel, VolumeComputationType volumeType)
    {
      var arg = DefaultRequestArg(siteModel.ID);

      var (startUtc, endUtc) = siteModel.GetDateRange();

      arg.VolumeType = volumeType;
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

    [Theory]
    [InlineData(VolumeComputationType.BetweenDesignAndFilter)]
    [InlineData(VolumeComputationType.BetweenFilterAndDesign)]
    public async Task FailWithNoDefinedBaseOrTopSurfaceDesign(VolumeComputationType volumeType)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var request = new ProgressiveVolumesRequest_ApplicationService();
      var response = await request.ExecuteAsync(DefaultRequestArgFromModel(siteModel, volumeType));

      response.Should().NotBeNull();

      response.Volumes.Should().BeNull();
      response.ResultStatus.Should().NotBe(RequestErrorStatus.OK);
    }

    private void CheckDefaultFilterToFilterSingleTAGFileResponse(ProgressiveVolumesResponse response)
    {
      //Was, response = {Cut:1.00113831634521, Fill:2.48526947021484, Cut Area:117.5652, FillArea: 202.9936, Total Area:353.0424, BoundingGrid:MinX: 537669.2, MaxX:537676.34, MinY:5427391.44, MaxY:5427514.52, MinZ: 1E+308, MaxZ:1E+308, BoundingLLH:MinX: 1E+308, MaxX:1E+308, MinY:1...
     // const double EPSILON = 0.000001;
      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);

      response.Volumes.Should().NotBeNull();

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
      var response = await request.ExecuteAsync(DefaultRequestArgFromModel(siteModel, VolumeComputationType.Between2Filters));

      response.Should().NotBeNull();

      CheckDefaultFilterToFilterSingleTAGFileResponse(response);

      response.Volumes.Length.Should().Be(10);
    }

    private void AddSimpleNEEToLLHConversionMock()
    {
      var csMock = new Mock<IConvertCoordinates>();
      csMock.Setup(x => x.NEEToLLH(It.IsAny<string>(), It.IsAny<XYZ[]>()))
        .Returns(() => {
          return Task.FromResult((RequestErrorStatus.OK, new XYZ[2]
          {
            new XYZ(0, 0, 1),
            new XYZ(1, 1, 0)
          }));
        });

      DIBuilder.Continue()
        .Add(x => x.AddSingleton(csMock.Object))
        .Complete();
    }

    [Fact]
    public async Task ApplicationService_DefaultFilterToFilter_Execute_SingleTAGFile()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddSimpleNEEToLLHConversionMock();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var request = new ProgressiveVolumesRequest_ApplicationService();
      var response = await request.ExecuteAsync(DefaultRequestArgFromModel(siteModel, VolumeComputationType.Between2Filters));

      response.Should().NotBeNull();

      CheckDefaultFilterToFilterSingleTAGFileResponse(response);

      response.Volumes.Length.Should().Be(10);
    }

    private void CheckDefaultSingleCellAtOriginResponse(ProgressiveVolumesResponse response)
    {
     // const double EPSILON = 0.000001;

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);

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

    private ISiteModel BuildModelForSingleCellProgressiveVolume(int numCellPasses, DateTime baseTime, TimeSpan timeIncrement, float baseHeight, float heightIncrement)
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, numCellPasses).Select(x =>
        new CellPass {InternalSiteModelMachineIndex = bulldozerMachineIndex, Time = baseTime + x * timeIncrement, Height = baseHeight + x * heightIncrement, PassType = PassType.Front}).ToList();

      // Ensure the machine the cell passes are being added to has start and stop evens bracketing the cell passes
      siteModel.MachinesTargetValues[bulldozerMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(baseTime, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(baseTime + (numCellPasses - 1) * timeIncrement, ProductionEventType.EndEvent);

      siteModel.MachinesTargetValues[bulldozerMachineIndex].SaveMachineEventsToPersistentStore(siteModel.PrimaryStorageProxy);

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Count());

      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }

    [Fact]
    public async Task ClusterCompute_DefaultFilterToFilter_Execute_SingleCell()
    {
      AddClusterComputeGridRouting();

      var baseTime = DateTime.UtcNow;
      var siteModel = BuildModelForSingleCellProgressiveVolume(10, baseTime, new TimeSpan(0, 1, 0), 1.0f, ELEVATION_INCREMENT_0_5);

      var request = new ProgressiveVolumesRequest_ClusterCompute();
      var response = await request.ExecuteAsync(DefaultRequestArgFromModel(siteModel, VolumeComputationType.Between2Filters));

      CheckDefaultSingleCellAtOriginResponse(response);
    }

    [Fact]
    public async Task ApplicationService_DefaultFilterToFilter_Execute_SingleCell_FillOnly()
    {
      const int numProgressiveVolumes = 10;

      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddSimpleNEEToLLHConversionMock();

      var baseTime = DateTime.UtcNow;
      var timeIncrement = new TimeSpan(0, 1, 0);
      var cellPassHeightIncrement = ELEVATION_INCREMENT_0_5;

      var siteModel = BuildModelForSingleCellProgressiveVolume(numProgressiveVolumes + 1, baseTime, timeIncrement, 1.0f, cellPassHeightIncrement);
      var (startUtc, endUtc) = siteModel.GetDateRange();

      startUtc.Should().Be(baseTime);
      endUtc.Should().Be(baseTime + numProgressiveVolumes * timeIncrement);

      var request = new ProgressiveVolumesRequest_ApplicationService();
      var arg = new ProgressiveVolumesRequestArgument
      {
        StartDate = startUtc,
        EndDate = endUtc,
        Interval = new TimeSpan((endUtc.Ticks - startUtc.Ticks) / numProgressiveVolumes),
        ProjectID = siteModel.ID,
        VolumeType = VolumeComputationType.Between2Filters,
        Filters = new FilterSet(new CombinedFilter()),
        BaseDesign = new DesignOffset(),
        TopDesign = new DesignOffset(),
        CutTolerance = 0.001,
        FillTolerance = 0.001
      };

      var response = await request.ExecuteAsync(arg);

      CheckDefaultSingleCellAtOriginResponse(response);

      response.Volumes.Length.Should().Be(numProgressiveVolumes);

      for (var i = 0; i < response.Volumes.Length - 1; i++)
      {
        response.Volumes[i].Date.Should().BeBefore(response.Volumes[i + 1].Date);
      }

      for (var i = 0; i < response.Volumes.Length; i++)
      {
        response.Volumes[i].Date.Should().Be(baseTime + (i + 1) * timeIncrement);
        response.Volumes[i].Volume.TotalCoverageArea.Should().Be( SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize);
        response.Volumes[i].Volume.Fill.Should().Be(cellPassHeightIncrement * SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize);
        response.Volumes[i].Volume.Cut.Should().Be(0);

        response.Volumes[i].Volume.FillArea.Should().Be(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize);
        response.Volumes[i].Volume.CutArea.Should().Be(0);
      }
    }

    [Fact]
    public async Task ApplicationService_DefaultFilterToFilter_Execute_SingleCell_CutOnly()
    {
      const int numProgressiveVolumes = 10;

      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddSimpleNEEToLLHConversionMock();

      var baseTime = DateTime.UtcNow;
      var timeIncrement = new TimeSpan(0, 1, 0);
      var cellPassHeightIncrement = -ELEVATION_INCREMENT_0_5;

      var siteModel = BuildModelForSingleCellProgressiveVolume(numProgressiveVolumes + 1, baseTime, timeIncrement, 1.0f, cellPassHeightIncrement);
      var (startUtc, endUtc) = siteModel.GetDateRange();

      startUtc.Should().Be(baseTime);
      endUtc.Should().Be(baseTime + numProgressiveVolumes * timeIncrement);

      var request = new ProgressiveVolumesRequest_ApplicationService();
      var arg = new ProgressiveVolumesRequestArgument
      {
        StartDate = startUtc,
        EndDate = endUtc,
        Interval = new TimeSpan((endUtc.Ticks - startUtc.Ticks) / numProgressiveVolumes),
        ProjectID = siteModel.ID,
        VolumeType = VolumeComputationType.Between2Filters,
        Filters = new FilterSet(new CombinedFilter()),
        BaseDesign = new DesignOffset(),
        TopDesign = new DesignOffset(),
        CutTolerance = 0.001,
        FillTolerance = 0.001
      };

      var response = await request.ExecuteAsync(arg);

      CheckDefaultSingleCellAtOriginResponse(response);

      response.Volumes.Length.Should().Be(numProgressiveVolumes);

      for (var i = 0; i < response.Volumes.Length - 1; i++)
      {
        response.Volumes[i].Date.Should().BeBefore(response.Volumes[i + 1].Date);
      }

      for (var i = 0; i < response.Volumes.Length; i++)
      {
        response.Volumes[i].Date.Should().Be(baseTime + (i + 1) * timeIncrement);
        response.Volumes[i].Volume.TotalCoverageArea.Should().Be(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize);
        response.Volumes[i].Volume.Fill.Should().Be(0);
        response.Volumes[i].Volume.Cut.Should().Be(- (cellPassHeightIncrement * SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize));

        response.Volumes[i].Volume.FillArea.Should().Be(0);
        response.Volumes[i].Volume.CutArea.Should().Be(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize);
      }
    }

    [Fact]
    public async Task ClusterCompute_DefaultFilterToSurface_Execute_SingleCell()
    {
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      var baseTime = DateTime.UtcNow;
      var timeIncrement = new TimeSpan(0, 1, 0);

      var siteModel = BuildModelForSingleCellProgressiveVolume(10, baseTime, timeIncrement, 1.0f, ELEVATION_INCREMENT_0_5);
      var request = new ProgressiveVolumesRequest_ClusterCompute();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 0.0f);

      var arg = DefaultRequestArgFromModel(siteModel, VolumeComputationType.BetweenFilterAndDesign);
      arg.TopDesign = new DesignOffset(designUid, 0);

      var response = await request.ExecuteAsync(arg);

      CheckDefaultSingleCellAtOriginResponse(response);

      response.Volumes.Length.Should().Be(11);
    }

    [Fact]
    public async Task ClusterCompute_SurfaceToDefaultFilter_Execute_SingleCell()
    {
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      var baseTime = DateTime.UtcNow;
      var timeIncrement = new TimeSpan(0, 1, 0);

      var siteModel = BuildModelForSingleCellProgressiveVolume(10, baseTime, timeIncrement, 1.0f, ELEVATION_INCREMENT_0_5);
      var request = new ProgressiveVolumesRequest_ClusterCompute();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 0.0f);

      var arg = DefaultRequestArgFromModel(siteModel, VolumeComputationType.BetweenDesignAndFilter);
      arg.BaseDesign = new DesignOffset(designUid, 0);

      var response = await request.ExecuteAsync(arg);

      CheckDefaultSingleCellAtOriginResponse(response);

      response.Volumes.Length.Should().Be(11);
    }
  }
}
