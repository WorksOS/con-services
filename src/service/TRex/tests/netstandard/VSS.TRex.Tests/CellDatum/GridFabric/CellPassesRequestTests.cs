using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.ComputeFuncs;
using VSS.TRex.CellDatum.GridFabric.Requests;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.Cells;
using VSS.TRex.Common;
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
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<CellPassesRequestComputeFunc_ApplicationService, CellPassesRequestArgument_ApplicationService, CellPassesResponse>();

    private void AddClusterComputeGridRouting()
    {
      IgniteMock.AddClusterComputeSpatialAffinityGridRouting<CellPassesRequestComputeFunc_ClusterCompute, CellPassesRequestArgument_ClusterCompute, CellPassesResponse>();
      IgniteMock.AddClusterComputeGridRouting<SubGridProgressiveResponseRequestComputeFunc, ISubGridProgressiveResponseRequestComputeFuncArgument, bool>();
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

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);
      return siteModel;
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
      const int expectedCount = 20;
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

        var cellPass = response.CellPasses[idx];
        cellPass.LastPassValidCCV.Should().Be(expectedCcv);
        cellPass.LastPassTime.Should().Be(expectedTime);
        cellPass.Height.Should().Be(expectedHeight);
        cellPass.MachineSpeed.Should().Be(expectedMachineSpeed);
        cellPass.LastPassValidMDP.Should().Be(expectedMdp);
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
  }
}
