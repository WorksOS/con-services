using System;
using System.Linq;
using System.Threading.Tasks;
using CoreX.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.ComputeFuncs;
using VSS.TRex.CellDatum.GridFabric.Requests;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.CellDatum.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(CellPassesRequest_ClusterCompute))]
  [UnitTestCoveredRequest(RequestType = typeof(CellPassesRequest_ApplicationService))]
  public class CellPassesRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private const int expectedCount = 20;

    private void AddApplicationGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting<CellPassesRequestComputeFunc_ApplicationService, CellPassesRequestArgument_ApplicationService, CellPassesResponse>();

    private void AddClusterComputeGridRouting()
    {
      IgniteMock.Immutable.AddClusterComputeSpatialAffinityGridRouting<CellPassesRequestComputeFunc_ClusterCompute, CellPassesRequestArgument_ClusterCompute, CellPassesResponse>();
      IgniteMock.Immutable.AddClusterComputeGridRouting<SubGridProgressiveResponseRequestComputeFunc, ISubGridProgressiveResponseRequestComputeFuncArgument, bool>();
    }

    private static CellPassesRequestArgument_ClusterCompute CreateRequestArgument(ISiteModel siteModel)
    {
      //The single cell is at world origin
      var coords = new XYZ(0.1, 0.1);
      siteModel.Grid.CalculateIndexOfCellContainingPosition(coords.X, coords.Y, out var otgCellX, out var otgCellY);

      var response = new CellPassesRequestArgument_ClusterCompute()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        NEECoords = coords,
        OTGCellX = otgCellX,
        OTGCellY = otgCellY,
      };
       
      return response;
    }


    private ISiteModel BuildTestSiteModel(DateTime baseTime, int count = 10)
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;
      siteModel.MachinesTargetValues[bulldozerMachineIndex].VibrationStateEvents.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, VibrationState.On);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].GPSAccuracyAndToleranceStateEvents.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, new GPSAccuracyAndTolerance(GPSAccuracy.Fine, 20));
      
      var cellPasses = Enumerable.Range(1, count).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = 1.0f + x * 0.5f,
          PassType = PassType.Front,
          CCV =  (short)(10 + 10 * x),
          MachineSpeed =  (ushort)(650 + x),
          MDP = (short)(20 + 20 * x),
          MaterialTemperature =(ushort)(1000 + x)
        }).ToArray();

      for (var i=0; i<cellPasses.Length; i++)
      {
        siteModel.MachinesTargetValues[bulldozerMachineIndex].LayerIDStateEvents.PutValueAtDate(cellPasses[i].Time, (ushort)(i%5));
      }

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);
      return siteModel;
    }

    private (CellPassesRequest_ApplicationService, FilterSet, Guid) BuildTestDataAndSetAreaFilter()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var baseTime = DateTime.UtcNow;
      var siteModel = BuildTestSiteModel(baseTime, count: expectedCount);

      var request = new CellPassesRequest_ApplicationService();

      var filter = new FilterSet(new CombinedFilter());

      var fence = new Fence();
      fence.Points.Add(new FencePoint(-115.01912, 36.207522, 0.0));
      fence.Points.Add(new FencePoint(-115.018673, 36.207501, 0.0));
      fence.Points.Add(new FencePoint(-115.018887, 36.207213, 0.0));
      fence.Points.Add(new FencePoint(-115.01932, 36.207325, 0.0));

      filter.Filters[0].SpatialFilter.Fence = fence;
      filter.Filters[0].SpatialFilter.IsSpatial = true;

      // Mocked ConvertCoordinates expected result.
      var neeCoords = new XYZ[fence.Points.Count];
      neeCoords[0].X = 0;
      neeCoords[0].Y = 0;
      neeCoords[1].X = 0;
      neeCoords[1].Y = 1;
      neeCoords[2].X = 1;
      neeCoords[2].Y = 1;
      neeCoords[3].X = 1;
      neeCoords[3].Y = 0;

      var expectedCoordinateConversionResult = neeCoords.ToCoreX_XYZ();

      var convertCoordinatesMock = new Mock<ICoreXWrapper>();

      convertCoordinatesMock.Setup(x => x.LLHToNEE(It.IsAny<string>(), It.IsAny<CoreX.Models.XYZ[]>(), It.IsAny<CoreX.Types.InputAs>()))
        .Returns(expectedCoordinateConversionResult);

      DIBuilder.Continue().Add(x => x.AddSingleton(convertCoordinatesMock.Object)).Complete();

      return (request, filter, siteModel.ID);
    }

    [Fact]
    public void Test_CellPassesRequest_ClusterCompute_Creation()
    {
      var request = new CellPassesRequest_ClusterCompute();
      request.Should().NotBeNull();
    }

    [Fact]
    public async Task Test_CellPassesRequest_ClusterCompute_Execute_EmptySiteModel()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var request = new CellPassesRequest_ClusterCompute();
      var arg = CreateRequestArgument(siteModel);
      var response = await request.ExecuteAsync(arg, new SubGridSpatialAffinityKey());

      response.Should().NotBeNull();
      response.ReturnCode.Should().Be(CellPassesReturnCode.NoDataFound);
    }

    [Fact]
    public async Task Test_CellPassesRequest_ClusterCompute_ExecuteData()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var baseTime = DateTime.UtcNow;
      var siteModel = BuildTestSiteModel(baseTime, count: expectedCount);

      var request = new CellPassesRequest_ClusterCompute();
      var arg = CreateRequestArgument(siteModel);
      var response = await request.ExecuteAsync(arg, new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER_TICKS, arg.ProjectID, arg.OTGCellX, arg.OTGCellY));

      response.Should().NotBeNull();
      response.ReturnCode.Should().Be(CellPassesReturnCode.DataFound);
      response.CellPasses.Count.Should().Be(expectedCount);

      for (var idx = 0; idx < expectedCount; idx++)
      {
        var mockModifier = idx + 1;
        var expectedTime = baseTime.AddMinutes(mockModifier);
        var expectedHeight = 1.0f + mockModifier * 0.5f;
        var expectedCcv = (short) (10 + 10 * mockModifier);
        var expectedMachineSpeed = (ushort) (650 + mockModifier);
        var expectedMdp = (short) (20 + 20 * mockModifier);
        var expectedLayerID = (ushort) (idx%5);

        var cellPass = response.CellPasses[idx];
        cellPass.LastPassValidCCV.Should().Be(expectedCcv);
        cellPass.LastPassTime.Should().Be(expectedTime);
        cellPass.Height.Should().Be(expectedHeight);
        cellPass.MachineSpeed.Should().Be(expectedMachineSpeed);
        cellPass.LastPassValidMDP.Should().Be(expectedMdp);
        cellPass.LayerID.Should().Be(expectedLayerID);

        cellPass.GPSAccuracy.Should().Be(GPSAccuracy.Fine);
        cellPass.GPSTolerance.Should().Be(20);
      }
    }

    [Fact]
    public async Task Test_CellPassesRequest_ContractResponseMapping()
    {
      const int expectedCount = 15;
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var baseTime = DateTime.UtcNow;
      var siteModel = BuildTestSiteModel(baseTime, count: expectedCount);

      var executor = new CellPassesExecutor();
      var coords = new Point(0.1, 0.1);

      var request = new CellPassesTRexRequest(siteModel.ID, coords, null, null, null);

      var response = await executor.ProcessAsync(request) as CellPassesV2Result;
      response.Should().NotBeNull();
      response?.Code.Should().Be((int) CellPassesReturnCode.DataFound);
      response?.Layers.Length.Should().Be(1);
      response?.Layers[0].PassData.Length.Should().Be(expectedCount);

      for (var idx = 0; idx < expectedCount; idx++)
      {
        var mockModifier = idx + 1;
        var expectedTime = baseTime.AddMinutes(mockModifier);
        var expectedHeight = 1.0f + mockModifier * 0.5f;
        var expectedCcv = (short) (10 + 10 * mockModifier);
        var expectedMdp = (short) (20 + 20 * mockModifier);

        var cellPass = response.Layers[0].PassData[idx];
        cellPass.FilteredPass.Time.Should().Be(expectedTime);
        cellPass.FilteredPass.Height.Should().Be(expectedHeight);
        cellPass.FilteredPass.Ccv.Should().Be(expectedCcv);
        cellPass.FilteredPass.Mdp.Should().Be(expectedMdp);
      }
    }

    [Fact]
    public async Task Test_CellPassesRequest_ApplicationService_ExecuteData()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var baseTime = DateTime.UtcNow;
      var siteModel = BuildTestSiteModel(baseTime, count: expectedCount);

      var request = new CellPassesRequest_ApplicationService();

      var arg = new CellPassesRequestArgument_ApplicationService(siteModel.ID, true, new XYZ(0.1, 0.1, 0), new FilterSet(new CombinedFilter()));
      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      response.ReturnCode.Should().Be(CellPassesReturnCode.DataFound);
      response.CellPasses.Count.Should().Be(expectedCount);

      for (var idx = 0; idx < expectedCount; idx++)
      {
        var mockModifier = idx + 1;
        var expectedTime = baseTime.AddMinutes(mockModifier);
        var expectedHeight = 1.0f + mockModifier * 0.5f;
        var expectedCcv = (short)(10 + 10 * mockModifier);
        var expectedMachineSpeed = (ushort)(650 + mockModifier);
        var expectedMdp = (short)(20 + 20 * mockModifier);

        var cellPass = response.CellPasses[idx];
        cellPass.LastPassValidCCV.Should().Be(expectedCcv);
        cellPass.LastPassTime.Should().Be(expectedTime);
        cellPass.Height.Should().Be(expectedHeight);
        cellPass.MachineSpeed.Should().Be(expectedMachineSpeed);
        cellPass.LastPassValidMDP.Should().Be(expectedMdp);

        cellPass.GPSAccuracy.Should().Be(GPSAccuracy.Fine);
        cellPass.GPSTolerance.Should().Be(20);
      }
    }
    [Fact]
    public async Task Test_CellPassesRequest_ApplicationService_AreaFilter_DataFound()
    {
      var (request, filter, siteModelID) = BuildTestDataAndSetAreaFilter();

      var arg = new CellPassesRequestArgument_ApplicationService(siteModelID, true, new XYZ(0.1, 0.25, 0), filter);

      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      response.ReturnCode.Should().Be(CellPassesReturnCode.DataFound);
      response.CellPasses.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task Test_CellPassesRequest_ApplicationService_AreaFilter_NoDataFound()
    {
      var (request, filter, siteModelID) = BuildTestDataAndSetAreaFilter();

      var arg = new CellPassesRequestArgument_ApplicationService(siteModelID, true, new XYZ(1.1, 1.1, 0), filter);

      var response = await request.ExecuteAsync(arg);

      response.Should().NotBeNull();
      response.ReturnCode.Should().Be(CellPassesReturnCode.NoDataFound);
      response.CellPasses.Should().HaveCount(0);
    }
  }
}

