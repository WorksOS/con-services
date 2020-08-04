using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using VSS.TRex.Cells;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Common;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.Events;
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
    private const float ELEVATION_INCREMENT_0_5 = 0.5f;

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
        BaseDesign = new DesignOffset(),
        TopDesign = new DesignOffset(),
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

    private void AddApplicationGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting<SimpleVolumesRequestComputeFunc_ApplicationService, SimpleVolumesRequestArgument, SimpleVolumesResponse>();

    private void AddClusterComputeGridRouting()
    {
      IgniteMock.Immutable.AddClusterComputeGridRouting<SimpleVolumesRequestComputeFunc_ClusterCompute, SimpleVolumesRequestArgument, SimpleVolumesResponse>();
    }

    private void AddDesignProfilerGridRouting()
    {
      IgniteMock.Immutable.AddApplicationGridRouting
        <CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();
    }

    [Fact]
    public async Task Test_SimpleVolumesRequest_ApplicationService_DefaultFilterToFilter_Execute_NoData()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var request = new SimpleVolumesRequest_ApplicationService();
      var response = await request.ExecuteAsync(SimpleDefaultRequestArg(Guid.NewGuid()));

      // This is a no data test, so the response will be null
      CheckResponseContainsNullValues(response);
    }

    [Fact]
    public async Task Test_SimpleVolumesRequest_ClusterCompute_DefaultFilterToFilter_Execute_NoData()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var request = new SimpleVolumesRequest_ClusterCompute();
      var response = await request.ExecuteAsync(SimpleDefaultRequestArg(Guid.NewGuid()));

      // This is a no data test, so the response will be null
      CheckResponseContainsNullValues(response);
    }

    private void CheckDefaultFilterToFilterSingleTAGFileResponse(SimpleVolumesResponse response)
    {
      //Was, response = {Cut:1.00113831634521, Fill:2.48526947021484, Cut Area:117.5652, FillArea: 202.9936, Total Area:353.0424, BoundingGrid:MinX: 537669.2, MaxX:537676.34, MinY:5427391.44, MaxY:5427514.52, MinZ: 1E+308, MaxZ:1E+308, BoundingLLH:MinX: 1E+308, MaxX:1E+308, MinY:1...
      const double EPSILON = 0.000001;
      response.Should().NotBeNull();
      response.Cut.Should().BeApproximately(0.99982155303955178, EPSILON);
      response.Fill.Should().BeApproximately(2.4776475891113323, EPSILON); 
      response.CutArea.Should().BeApproximately(113.86600000000001, EPSILON); 
      response.FillArea.Should().BeApproximately(200.56600000000006, EPSILON);
      response.TotalCoverageArea.Should().BeApproximately(353.0424, EPSILON);

      response.BoundingExtentGrid.MinX.Should().BeApproximately(537669.2, EPSILON);
      response.BoundingExtentGrid.MinY.Should().BeApproximately(5427391.44, EPSILON);
      response.BoundingExtentGrid.MaxX.Should().BeApproximately(537676.34, EPSILON);
      response.BoundingExtentGrid.MaxY.Should().BeApproximately(5427514.52, EPSILON);
      response.BoundingExtentGrid.MinZ.Should().Be(Consts.NullDouble);
      response.BoundingExtentGrid.MaxZ.Should().Be(Consts.NullDouble);
    }

    [Fact]
    public async Task Test_SimpleVolumesRequest_ClusterCompute_DefaultFilterToFilter_Execute_SingleTAGFile()
    {
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var request = new SimpleVolumesRequest_ClusterCompute();
      var response = await request.ExecuteAsync(SimpleDefaultRequestArg(siteModel.ID));

      CheckDefaultFilterToFilterSingleTAGFileResponse(response);
    }

    [Fact]
    public async Task Test_SimpleVolumesRequest_ApplicationService_DefaultFilterToFilter_Execute_SingleTAGFile()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var request = new SimpleVolumesRequest_ApplicationService();
      var response = await request.ExecuteAsync(SimpleDefaultRequestArg(siteModel.ID));

      CheckDefaultFilterToFilterSingleTAGFileResponse(response);
    }

    [Fact]
    public async Task Test_SimpleVolumesRequest_ApplicationService_FilterToFilter_MidTimeToLatest_Execute_SingleTAGFile()
    {
      const string expectedResponseText = "{\"Cut\":2.178961485290527,\"Fill\":0.5822854339599612,\"TotalCoverageArea\":279.75200000000007,\"CutArea\":164.84560000000002,\"FillArea\":72.71240000000002,\"BoundingExtentGrid\":{\"MinX\":537669.2000000001,\"MinY\":5427396.54,\"MaxX\":537675.6600000001,\"MaxY\":5427509.76,\"MinZ\":1E+308,\"MaxZ\":1E+308,\"Area\":731.4012000072782,\"CenterX\":537672.4300000002,\"CenterY\":5427453.15,\"CenterZ\":1E+308,\"IsMaximalPlanConverage\":false,\"IsValidHeightExtent\":false,\"IsValidPlanExtent\":true,\"LargestPlanDimension\":113.21999999973923,\"SizeX\":6.460000000079162,\"SizeY\":113.21999999973923,\"SizeZ\":0.0},\"BoundingExtentLLH\":{\"MinX\":1E+308,\"MinY\":1E+308,\"MaxX\":1E+308,\"MaxY\":1E+308,\"MinZ\":1E+308,\"MaxZ\":1E+308,\"Area\":0.0,\"CenterX\":1E+308,\"CenterY\":1E+308,\"CenterZ\":1E+308,\"IsMaximalPlanConverage\":false,\"IsValidHeightExtent\":false,\"IsValidPlanExtent\":false,\"LargestPlanDimension\":1E+308,\"SizeX\":0.0,\"SizeY\":0.0,\"SizeZ\":0.0},\"ResponseCode\":1,\"ClusterNode\":\"\",\"NumSubgridsProcessed\":0,\"NumSubgridsExamined\":0,\"NumProdDataSubGridsProcessed\":0,\"NumProdDataSubGridsExamined\":0,\"NumSurveyedSurfaceSubGridsProcessed\":0,\"NumSurveyedSurfaceSubGridsExamined\":0}";

      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var request = new SimpleVolumesRequest_ApplicationService();
      var arg = SimpleDefaultRequestArg(siteModel.ID);

      var (startUtc, endUtc) = siteModel.GetDateRange();

      arg.BaseFilter.AttributeFilter.HasTimeFilter = true;
      arg.BaseFilter.AttributeFilter.StartTime = DateTime.SpecifyKind(new DateTime((startUtc.Ticks + endUtc.Ticks) / 2), DateTimeKind.Utc);

      var response = await request.ExecuteAsync(arg);

      // var responseText = JsonConvert.SerializeObject(response);
      JsonConvert.DeserializeObject<SimpleVolumesResponse>(expectedResponseText).Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Test_SimpleVolumesRequest_ApplicationService_FilterToFilter_EarliestToMidTime_Execute_SingleTAGFile()
    {
      const string expectedResponseText = "{\"Cut\":0.6670116119384769,\"Fill\":2.2439066680908204,\"TotalCoverageArea\":279.75200000000007,\"CutArea\":78.72360000000002,\"FillArea\":175.24960000000004,\"BoundingExtentGrid\":{\"MinX\":537669.2000000001,\"MinY\":5427396.54,\"MaxX\":537675.6600000001,\"MaxY\":5427509.76,\"MinZ\":1E+308,\"MaxZ\":1E+308,\"Area\":731.4012000072782,\"CenterX\":537672.4300000002,\"CenterY\":5427453.15,\"CenterZ\":1E+308,\"IsMaximalPlanConverage\":false,\"IsValidHeightExtent\":false,\"IsValidPlanExtent\":true,\"LargestPlanDimension\":113.21999999973923,\"SizeX\":6.460000000079162,\"SizeY\":113.21999999973923,\"SizeZ\":0.0},\"BoundingExtentLLH\":{\"MinX\":1E+308,\"MinY\":1E+308,\"MaxX\":1E+308,\"MaxY\":1E+308,\"MinZ\":1E+308,\"MaxZ\":1E+308,\"Area\":0.0,\"CenterX\":1E+308,\"CenterY\":1E+308,\"CenterZ\":1E+308,\"IsMaximalPlanConverage\":false,\"IsValidHeightExtent\":false,\"IsValidPlanExtent\":false,\"LargestPlanDimension\":1E+308,\"SizeX\":0.0,\"SizeY\":0.0,\"SizeZ\":0.0},\"ResponseCode\":1,\"ClusterNode\":\"\",\"NumSubgridsProcessed\":0,\"NumSubgridsExamined\":0,\"NumProdDataSubGridsProcessed\":0,\"NumProdDataSubGridsExamined\":0,\"NumSurveyedSurfaceSubGridsProcessed\":0,\"NumSurveyedSurfaceSubGridsExamined\":0}";

      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var request = new SimpleVolumesRequest_ApplicationService();
      var arg = SimpleDefaultRequestArg(siteModel.ID);

      var (startUtc, endUtc) = siteModel.GetDateRange();

      arg.TopFilter.AttributeFilter.HasTimeFilter = true;
      arg.TopFilter.AttributeFilter.StartTime = DateTime.SpecifyKind(new DateTime((startUtc.Ticks + endUtc.Ticks) / 2), DateTimeKind.Utc);

      var response = await request.ExecuteAsync(arg);

      // var responseText = JsonConvert.SerializeObject(response);
      JsonConvert.DeserializeObject<SimpleVolumesResponse>(expectedResponseText).Should().BeEquivalentTo(response);
    }

    private SimpleVolumesRequestArgument RequestArgForSimpleRequestsWithIntermediaryFilter(ISiteModel siteModel, DateTime startUtc, DateTime endUtc)
    {
      return new SimpleVolumesRequestArgument
      {
        ProjectID = siteModel.ID,
        VolumeType = VolumeComputationType.Between2Filters,
        BaseFilter = new CombinedFilter
        {
          AttributeFilter =
          {
            ReturnEarliestFilteredCellPass = false,
            HasTimeFilter = true,
            StartTime = Consts.MIN_DATETIME_AS_UTC,
            EndTime = startUtc
          }
        },
        TopFilter = new CombinedFilter
        {
          AttributeFilter = new CellPassAttributeFilter
          {
            ReturnEarliestFilteredCellPass = false,
            HasTimeFilter = true,
            StartTime = startUtc,
            EndTime = endUtc
          }
        },
        BaseDesign = new DesignOffset(),
        TopDesign = new DesignOffset(),
        CutTolerance = 0.001,
        FillTolerance = 0.001
      };
    }

    [Fact]
    public async Task Test_SimpleVolumesRequest_ApplicationService_FilterToFilterWithIntermediary_Execute_SingleCell_ProjectExtent()
    {
      void CheckVolumesResponse(SimpleVolumesResponse response)
      {
        //Was, response = {Cut:1.00113831634521, Fill:2.48526947021484, Cut Area:117.5652, FillArea: 202.9936, Total Area:353.0424, BoundingGrid:MinX: 537669.2, MaxX:537676.34, MinY:5427391.44, MaxY:5427514.52, MinZ: 1E+308, MaxZ:1E+308, BoundingLLH:MinX: 1E+308, MaxX:1E+308, MinY:1...
        const double EPSILON = 0.000001;
        response.Should().NotBeNull();
        response.Cut.Should().BeApproximately(0.5202, EPSILON);
        response.Fill.Should().BeApproximately(0.0, EPSILON);
        response.CutArea.Should().BeApproximately(0.1156, EPSILON);
        response.FillArea.Should().BeApproximately(0.0, EPSILON);
        response.TotalCoverageArea.Should().BeApproximately(0.1156, EPSILON);

        response.BoundingExtentGrid.MinX.Should().BeApproximately(0, EPSILON);
        response.BoundingExtentGrid.MinY.Should().BeApproximately(0, EPSILON);
        response.BoundingExtentGrid.MaxX.Should().BeApproximately(0.34, EPSILON);
        response.BoundingExtentGrid.MaxY.Should().BeApproximately(0.34, EPSILON);
        response.BoundingExtentGrid.MinZ.Should().Be(Consts.NullDouble);
        response.BoundingExtentGrid.MaxZ.Should().Be(Consts.NullDouble);
      }

      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = BuildModelForSingleCellSummaryVolume(-ELEVATION_INCREMENT_0_5);

      var request = new SimpleVolumesRequest_ApplicationService();
      var (startUtc, endUtc) = siteModel.GetDateRange();
      var response = await request.ExecuteAsync(RequestArgForSimpleRequestsWithIntermediaryFilter(siteModel, startUtc, endUtc));

      CheckVolumesResponse(response);
    }

    [Fact]
    public async Task Test_SimpleVolumesRequest_ApplicationService_FilterToFilterWithIntermediary_Execute_SingleCell_IncludingProjectExtents()
    {
      void CheckVolumesResponse(SimpleVolumesResponse response)
      {
        //Was, response = {Cut:1.00113831634521, Fill:2.48526947021484, Cut Area:117.5652, FillArea: 202.9936, Total Area:353.0424, BoundingGrid:MinX: 537669.2, MaxX:537676.34, MinY:5427391.44, MaxY:5427514.52, MinZ: 1E+308, MaxZ:1E+308, BoundingLLH:MinX: 1E+308, MaxX:1E+308, MinY:1...
        const double EPSILON = 0.000001;
        response.Should().NotBeNull();
        response.Cut.Should().BeApproximately(0.5202, EPSILON);
        response.Fill.Should().BeApproximately(0.0, EPSILON);
        response.CutArea.Should().BeApproximately(0.1156, EPSILON);
        response.FillArea.Should().BeApproximately(0.0, EPSILON);
        response.TotalCoverageArea.Should().BeApproximately(0.1156, EPSILON);

        response.BoundingExtentGrid.MinX.Should().BeApproximately(0, EPSILON);
        response.BoundingExtentGrid.MinY.Should().BeApproximately(0, EPSILON);
        response.BoundingExtentGrid.MaxX.Should().BeApproximately(0.34, EPSILON);
        response.BoundingExtentGrid.MaxY.Should().BeApproximately(0.34, EPSILON);
        response.BoundingExtentGrid.MinZ.Should().Be(Consts.NullDouble);
        response.BoundingExtentGrid.MaxZ.Should().Be(Consts.NullDouble);
      }

      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = BuildModelForSingleCellSummaryVolume(-ELEVATION_INCREMENT_0_5);

      var request = new SimpleVolumesRequest_ApplicationService();
      var (startUtc, endUtc) = siteModel.GetDateRange();
      startUtc.AddDays(-1);
      endUtc.AddDays(1);
      var response = await request.ExecuteAsync(RequestArgForSimpleRequestsWithIntermediaryFilter(siteModel, startUtc, endUtc));

      CheckVolumesResponse(response);
    }

    [Fact]
    public async Task Test_SimpleVolumesRequest_ApplicationService_FilterToFilterWithIntermediary_Execute_SingleTAGFile()
    {
      void CheckVolumesResponse(SimpleVolumesResponse response)
      {
        const double EPSILON = 0.000001;
        response.Should().NotBeNull();
        response.Cut.Should().BeApproximately(0.99982155303955178, EPSILON);
        response.Fill.Should().BeApproximately(2.4776475891113323, EPSILON);
        response.CutArea.Should().BeApproximately(113.86600000000001, EPSILON);
        response.FillArea.Should().BeApproximately(200.56600000000006, EPSILON);
        response.TotalCoverageArea.Should().BeApproximately(353.0424, EPSILON);

        response.BoundingExtentGrid.MinX.Should().BeApproximately(537669.2, EPSILON);
        response.BoundingExtentGrid.MinY.Should().BeApproximately(5427391.44, EPSILON);
        response.BoundingExtentGrid.MaxX.Should().BeApproximately(537676.34, EPSILON);
        response.BoundingExtentGrid.MaxY.Should().BeApproximately(5427514.52, EPSILON);
        response.BoundingExtentGrid.MinZ.Should().Be(Consts.NullDouble);
        response.BoundingExtentGrid.MaxZ.Should().Be(Consts.NullDouble);
      }

      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var request = new SimpleVolumesRequest_ApplicationService();
      var (startUtc, endUtc) = siteModel.GetDateRange();
      var response = await request.ExecuteAsync(RequestArgForSimpleRequestsWithIntermediaryFilter(siteModel, startUtc, endUtc));

      CheckVolumesResponse(response);
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

    private ISiteModel BuildModelForSingleCellSummaryVolume(float heightIncrement)
    {
      var baseTime = DateTime.UtcNow;
      var baseHeight = 1.0f;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      const int numCellPasses = 10;
      var cellPasses = Enumerable.Range(0, numCellPasses).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = baseHeight + x * heightIncrement,
          PassType = PassType.Front
        });

      // Ensure the machine the cell passes are being added to has start and stop evens bracketing the cell passes
      siteModel.MachinesTargetValues[bulldozerMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(baseTime, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(baseTime.AddMinutes(numCellPasses - 1), ProductionEventType.EndEvent);

      siteModel.MachinesTargetValues[bulldozerMachineIndex].SaveMachineEventsToPersistentStore(siteModel.PrimaryStorageProxy);

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Count());

      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }

    [Fact]
    public async Task Test_SimpleVolumesRequest_ClusterCompute_DefaultFilterToFilter_Execute_SingleCell_WithFill()
    {
      AddClusterComputeGridRouting();

      var siteModel = BuildModelForSingleCellSummaryVolume(ELEVATION_INCREMENT_0_5);

      var request = new SimpleVolumesRequest_ClusterCompute();
      var response = await request.ExecuteAsync(SimpleDefaultRequestArg(siteModel.ID));

      CheckDefaultFilterToFilterSingleFillCellAtOriginResponse(response);
    }

    [Fact]
    public async Task Test_SimpleVolumesRequest_ApplicationService_DefaultFilterToFilter_Execute_SingleCell_WithFill()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = BuildModelForSingleCellSummaryVolume(ELEVATION_INCREMENT_0_5);

      var request = new SimpleVolumesRequest_ApplicationService();
      var response = await request.ExecuteAsync(SimpleDefaultRequestArg(siteModel.ID));

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
    public async Task Test_SimpleVolumesRequest_ClusterCompute_DefaultFilterToFilter_Execute_SingleCell_WithCut()
    {
      AddClusterComputeGridRouting();

      var siteModel = BuildModelForSingleCellSummaryVolume(-ELEVATION_INCREMENT_0_5);

      var request = new SimpleVolumesRequest_ClusterCompute();
      var response = await request.ExecuteAsync(SimpleDefaultRequestArg(siteModel.ID));

      CheckDefaultFilterToFilterSingleCutCellAtOriginResponse(response);
    }

    [Fact]
    public async Task Test_SimpleVolumesRequest_ApplicationService_DefaultFilterToFilter_Execute_SingleCell_WithCut()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = BuildModelForSingleCellSummaryVolume(-ELEVATION_INCREMENT_0_5);

      var request = new SimpleVolumesRequest_ApplicationService();
      var response = await request.ExecuteAsync(SimpleDefaultRequestArg(siteModel.ID));

      CheckDefaultFilterToFilterSingleCutCellAtOriginResponse(response);
    }

    [Fact]
    public async Task Test_SimpleVolumesRequest_ApplicationService_DefaultFilterToDesign_SingleCell()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      var siteModel = BuildModelForSingleCellSummaryVolume(-ELEVATION_INCREMENT_0_5);

      var topDesign = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 0);

      var request = new SimpleVolumesRequest_ApplicationService();
      var arg = SimpleDefaultRequestArg(siteModel.ID);
      arg.VolumeType = VolumeComputationType.BetweenFilterAndDesign;
      arg.TopDesign = new DesignOffset(topDesign, 0);

      var response = await request.ExecuteAsync(arg);

      const double EPSILON = 0.000001;

      response.Should().NotBeNull();
      response.Cut.Should().BeApproximately(1.0 * SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, EPSILON);
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
    public async Task Test_SimpleVolumesRequest_ApplicationService_DesignToDefaultFilter_SingleCell()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      var siteModel = BuildModelForSingleCellSummaryVolume(-ELEVATION_INCREMENT_0_5);

      var baseDesign = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref siteModel, 0);

      var request = new SimpleVolumesRequest_ApplicationService();
      var arg = SimpleDefaultRequestArg(siteModel.ID);
      arg.VolumeType = VolumeComputationType.BetweenDesignAndFilter;
      arg.BaseDesign = new DesignOffset(baseDesign, 0);

      var response = await request.ExecuteAsync(arg);

      const double EPSILON = 0.000001;

      response.Should().NotBeNull();
      response.Cut.Should().BeApproximately(3.5 * SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize, EPSILON);
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
  }
}
