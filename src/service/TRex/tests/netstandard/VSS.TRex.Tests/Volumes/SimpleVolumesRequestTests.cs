using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.ComputeFuncs;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
  [UnitTestCoveredRequest(RequestType = typeof(SimpleVolumesRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(SimpleVolumesRequest_ClusterCompute))]
  public class SimpleVolumesRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Test_SimpleVolumesRequest_Creation1()
    {
      var request = new SimpleVolumesRequest_ApplicationService();

      Assert.NotNull(request);
    }

    [Fact]
    public void Test_SimpleVolumesRequest_Creation2()
    {
      var request = new SimpleVolumesRequest_ClusterCompute();

      Assert.NotNull(request);
    }

    private SimpleVolumesRequestArgument SimpleDefaultRequestArg(Guid ProjectUid)
    {
      return new SimpleVolumesRequestArgument
      {
        ProjectID = ProjectUid,
        VolumeType = VolumeComputationType.Between2Filters,
        BaseFilter = new CombinedFilter
        {
          AttributeFilter =
          {
            ReturnEarliestFilteredCellPass = true,
          }
        },
        TopFilter = new CombinedFilter(),
        BaseDesignID = Guid.Empty,
        TopDesignID = Guid.Empty,
        CutTolerance = 0.001,
        FillTolerance = 0.001
      };
    }

    private void CheckResponseContainsNullValues(SimpleVolumesResponse response)
    {
      response.Should().NotBeNull();
      response.Cut.Should().BeNull();
      response.Fill.Should().BeNull();
      response.CutArea.Should().BeNull();
      response.FillArea.Should().BeNull();
      response.TotalCoverageArea.Should().BeNull();
      response.BoundingExtentGrid.Should().BeEquivalentTo(BoundingWorldExtent3D.Null());
    }

    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<SimpleVolumesRequestComputeFunc_ApplicationService, SimpleVolumesRequestArgument, SimpleVolumesResponse>();

    private void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeGridRouting<SimpleVolumesRequestComputeFunc_ClusterCompute, SimpleVolumesRequestArgument, SimpleVolumesResponse>();

    [Fact]
    public void Test_SimpleVolumesRequest_ApplicationService_DefaultFilterToFilter_Execute_NoData()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var request = new SimpleVolumesRequest_ApplicationService();
      var response = request.Execute(SimpleDefaultRequestArg(Guid.NewGuid()));

      // This is a no data test, so the response will be null
      CheckResponseContainsNullValues(response);
    }

    [Fact]
    public void Test_SimpleVolumesRequest_ClusterCompute_DefaultFilterToFilter_Execute_NoData()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var request = new SimpleVolumesRequest_ClusterCompute();
      var response = request.Execute(SimpleDefaultRequestArg(Guid.NewGuid()));

      // This is a no data test, so the response will be null
      CheckResponseContainsNullValues(response);
    }

    private void CheckDefaultFilterToFilterSingleTAGFileResponse(SimpleVolumesResponse response)
    {
      //response = {Cut:1.00113831634521, Fill:2.48526947021484, Cut Area:117.5652, FillArea: 202.9936, Total Area:353.0424, BoundingGrid:MinX: 537669.2, MaxX:537676.34, MinY:5427391.44, MaxY:5427514.52, MinZ: 1E+308, MaxZ:1E+308, BoundingLLH:MinX: 1E+308, MaxX:1E+308, MinY:1...
      const double EPSILON = 0.000001;
      response.Should().NotBeNull();
      response.Cut.Should().BeApproximately(1.001138316, EPSILON);
      response.Fill.Should().BeApproximately(2.4852694702148, EPSILON); 
      response.CutArea.Should().BeApproximately(117.5652, EPSILON); 
      response.FillArea.Should().BeApproximately(202.9936, EPSILON);
      response.TotalCoverageArea.Should().BeApproximately(353.0424, EPSILON);

      response.BoundingExtentGrid.MinX.Should().BeApproximately(537669.2, EPSILON);
      response.BoundingExtentGrid.MinY.Should().BeApproximately(5427391.44, EPSILON);
      response.BoundingExtentGrid.MaxX.Should().BeApproximately(537676.34, EPSILON);
      response.BoundingExtentGrid.MaxY.Should().BeApproximately(5427514.52, EPSILON);
      response.BoundingExtentGrid.MinZ.Should().Be(Consts.NullDouble);
      response.BoundingExtentGrid.MaxZ.Should().Be(Consts.NullDouble);
    }

    [Fact]
    public void Test_SimpleVolumesRequest_ClusterCompute_DefaultFilterToFilter_Execute_SingleTAGFile()
    {
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var request = new SimpleVolumesRequest_ClusterCompute();
      var response = request.Execute(SimpleDefaultRequestArg(siteModel.ID));

      CheckDefaultFilterToFilterSingleTAGFileResponse(response);
    }

    [Fact]
    public void Test_SimpleVolumesRequest_ApplicationService_DefaultFilterToFilter_Execute_SingleTAGFile()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var request = new SimpleVolumesRequest_ApplicationService();
      var response = request.Execute(SimpleDefaultRequestArg(siteModel.ID));

      CheckDefaultFilterToFilterSingleTAGFileResponse(response);
    }

    public ISiteModel NewEmptyModel()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      _ = siteModel.Machines.CreateNew("Bulldozer", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      return siteModel;
    }

    private void CheckDefaultFilterToFilterSingleFillCellAtOriginResponse(SimpleVolumesResponse response)
    {
      const double EPSILON = 0.000001;

      response.Should().NotBeNull();
      response.Cut.Should().BeApproximately(0, EPSILON);
      response.Fill.Should().BeApproximately(4.5 * SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, EPSILON);
      response.CutArea.Should().BeApproximately(0, EPSILON);
      response.FillArea.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, EPSILON);
      response.TotalCoverageArea.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, EPSILON);

      response.BoundingExtentGrid.MinX.Should().BeApproximately(0, EPSILON);
      response.BoundingExtentGrid.MinY.Should().BeApproximately(0, EPSILON);
      response.BoundingExtentGrid.MaxX.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize, EPSILON);
      response.BoundingExtentGrid.MaxY.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize, EPSILON);
      response.BoundingExtentGrid.MinZ.Should().Be(Consts.NullDouble);
      response.BoundingExtentGrid.MaxZ.Should().Be(Consts.NullDouble);
    }

    private void BuildModelForSingleCellSmmaryVolume(out ISiteModel siteModel, float heightIncrement)
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
        });

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Count());
    }

    [Fact]
    public void Test_SimpleVolumesRequest_ClusterCompute_DefaultFilterToFilter_Execute_SingleCell_WithFill()
    {
      AddClusterComputeGridRouting();

      BuildModelForSingleCellSmmaryVolume(out var siteModel, 0.5f);

      var request = new SimpleVolumesRequest_ClusterCompute();
      var response = request.Execute(SimpleDefaultRequestArg(siteModel.ID));

      CheckDefaultFilterToFilterSingleFillCellAtOriginResponse(response);
    }

    [Fact]
    public void Test_SimpleVolumesRequest_ApplicationService_DefaultFilterToFilter_Execute_SingleCell_WithFill()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      BuildModelForSingleCellSmmaryVolume(out var siteModel, 0.5f);

      var request = new SimpleVolumesRequest_ApplicationService();
      var response = request.Execute(SimpleDefaultRequestArg(siteModel.ID));

      CheckDefaultFilterToFilterSingleFillCellAtOriginResponse(response);
    }

    private void CheckDefaultFilterToFilterSingleCutCellAtOriginResponse(SimpleVolumesResponse response)
    {
      const double EPSILON = 0.000001;

      response.Should().NotBeNull();
      response.Cut.Should().BeApproximately(4.50 * SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, EPSILON);
      response.Fill.Should().BeApproximately(0, EPSILON);
      response.CutArea.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, EPSILON);
      response.FillArea.Should().BeApproximately(0, EPSILON);
      response.TotalCoverageArea.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, EPSILON);

      response.BoundingExtentGrid.MinX.Should().BeApproximately(0, EPSILON);
      response.BoundingExtentGrid.MinY.Should().BeApproximately(0, EPSILON);
      response.BoundingExtentGrid.MaxX.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize, EPSILON);
      response.BoundingExtentGrid.MaxY.Should().BeApproximately(SubGridTreeConsts.DefaultCellSize, EPSILON);
      response.BoundingExtentGrid.MinZ.Should().Be(Consts.NullDouble);
      response.BoundingExtentGrid.MaxZ.Should().Be(Consts.NullDouble);
    }

    [Fact]
    public void Test_SimpleVolumesRequest_ClusterCompute_DefaultFilterToFilter_Execute_SingleCell_WithCut()
    {
      AddClusterComputeGridRouting();

      BuildModelForSingleCellSmmaryVolume(out var siteModel, -0.5f);

      var request = new SimpleVolumesRequest_ClusterCompute();
      var response = request.Execute(SimpleDefaultRequestArg(siteModel.ID));

      CheckDefaultFilterToFilterSingleCutCellAtOriginResponse(response);
    }

    [Fact]
    public void Test_SimpleVolumesRequest_ApplicationService_DefaultFilterToFilter_Execute_SingleCell_WithCut()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      BuildModelForSingleCellSmmaryVolume(out var siteModel, -0.5f);

      var request = new SimpleVolumesRequest_ApplicationService();
      var response = request.Execute(SimpleDefaultRequestArg(siteModel.ID));

      CheckDefaultFilterToFilterSingleCutCellAtOriginResponse(response);
    }
  }
}
