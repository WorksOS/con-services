using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.ComputeFuncs;
using VSS.TRex.CellDatum.GridFabric.Requests;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.LiftAnalysis
{
  public class LiftAnalysisTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private const byte LAYER_ID1 = 111;
    private const byte LAYER_ID2 = 222;

    private void AddApplicationGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting<CellPassesRequestComputeFunc_ApplicationService, CellPassesRequestArgument_ApplicationService, CellPassesResponse>();

    private void AddClusterComputeGridRouting()
    {
      IgniteMock.Immutable.AddClusterComputeSpatialAffinityGridRouting<CellPassesRequestComputeFunc_ClusterCompute, CellPassesRequestArgument_ClusterCompute, CellPassesResponse>();
      IgniteMock.Immutable.AddClusterComputeGridRouting<SubGridProgressiveResponseRequestComputeFunc, ISubGridProgressiveResponseRequestComputeFuncArgument, bool>();
    }

    private ISiteModel BuildModelForSingleCellLiftAnalysis(DateTime baseTime)
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;
      siteModel.MachinesTargetValues[bulldozerMachineIndex].VibrationStateEvents.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, VibrationState.On);

      siteModel.MachinesTargetValues[bulldozerMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(baseTime, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].LayerIDStateEvents.PutValueAtDate(baseTime, LAYER_ID1);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].LayerIDStateEvents.PutValueAtDate(baseTime.AddMinutes(5).AddSeconds(1), LAYER_ID2);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(baseTime.AddMinutes(11), ProductionEventType.EndEvent);

      var cellPasses = Enumerable.Range(1, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = 1.0f + x * 0.5f,
          PassType = PassType.Front,
          CCV = (short)(10 + 10 * x)
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }

    private static CellPassesRequestArgument_ClusterCompute CreateRequestArgument(ISiteModel siteModel)
    {
      //The single cell is at world origin
      var coords = new XYZ(0.1, 0.1);
      siteModel.Grid.CalculateIndexOfCellContainingPosition(coords.X, coords.Y, out var otgCellX, out var otgCellY);

      var request = new CellPassesRequestArgument_ClusterCompute()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        NEECoords = coords,
        OTGCellX = otgCellX,
        OTGCellY = otgCellY,
      };

      return request;
    }


    [Fact]
    public async Task Test_CellPassesRequest_NoLiftAnalysisOrFilter()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var baseTime = DateTime.UtcNow;
      var siteModel = BuildModelForSingleCellLiftAnalysis(baseTime);

      var request = new CellPassesRequest_ClusterCompute();
      var arg = CreateRequestArgument(siteModel);
      var response = await request.ExecuteAsync(arg, new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER_TICKS, arg.ProjectID, arg.OTGCellX, arg.OTGCellY));

      //No lift analysis means the cell passes should be treated as one layer
      response.Should().NotBeNull();
      response.ReturnCode.Should().Be(CellPassesReturnCode.DataFound);
      response.CellPasses.Count.Should().Be(10);

      for (var idx = 0; idx < 10; idx++)
      {
        var mockModifier = idx + 1;
        var expectedTime = baseTime.AddMinutes(mockModifier);
        var expectedHeight = 1.0f + mockModifier * 0.5f;
        var expectedCcv = (short)(10 + 10 * mockModifier);

        var cellPass = response.CellPasses[idx];
        cellPass.LastPassValidCCV.Should().Be(expectedCcv);
        cellPass.LastPassTime.Should().Be(expectedTime);
        cellPass.Height.Should().Be(expectedHeight);
        cellPass.LayersCount.Should().Be(1);
      }
    }

    [Fact]
    public async Task Test_CellPassesRequest_WithLiftAnalysis()
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var baseTime = DateTime.UtcNow;
      var siteModel = BuildModelForSingleCellLiftAnalysis(baseTime);

      var request = new CellPassesRequest_ClusterCompute();
      var arg = CreateRequestArgument(siteModel);
      arg.LiftParams = new LiftParameters { LiftDetectionType = LiftDetectionType.Tagfile };
      var response = await request.ExecuteAsync(arg, new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER_TICKS, arg.ProjectID, arg.OTGCellX, arg.OTGCellY));

      //There should be two layers with 5 cell passes in each.
      response.Should().NotBeNull();
      response.ReturnCode.Should().Be(CellPassesReturnCode.DataFound);
      response.CellPasses.Count.Should().Be(10);

      for (var idx = 0; idx < 10; idx++)
      {
        var mockModifier = idx + 1;
        var expectedTime = baseTime.AddMinutes(mockModifier);
        var expectedHeight = 1.0f + mockModifier * 0.5f;
        var expectedCcv = (short)(10 + 10 * mockModifier);
        var expectedLayer = idx < 5 ? 1 : 2;

        var cellPass = response.CellPasses[idx];
        cellPass.LastPassValidCCV.Should().Be(expectedCcv);
        cellPass.LastPassTime.Should().Be(expectedTime);
        cellPass.Height.Should().Be(expectedHeight);
        cellPass.LayersCount.Should().Be(expectedLayer);
      }
    }

    [Theory]
    [InlineData(LAYER_ID1)]
    [InlineData(LAYER_ID2)]
    public async Task Test_CellPassesRequest_WithLiftFilter(int layerId)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var baseTime = DateTime.UtcNow;
      var siteModel = BuildModelForSingleCellLiftAnalysis(baseTime);

      var request = new CellPassesRequest_ClusterCompute();
      var arg = CreateRequestArgument(siteModel);
      var attributeFilter = new CellPassAttributeFilter { HasLayerStateFilter  = true, LayerState = LayerState.On, HasLayerIDFilter = true, LayerID = layerId };
      arg.Filters = new FilterSet(new CombinedFilter { AttributeFilter = attributeFilter, SpatialFilter = new CellSpatialFilter() });
      var response = await request.ExecuteAsync(arg, new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER_TICKS, arg.ProjectID, arg.OTGCellX, arg.OTGCellY));

      //There should be one layer with 5 cell passes. The other layer and its cell passes should be excluded.
      response.Should().NotBeNull();
      response.ReturnCode.Should().Be(CellPassesReturnCode.DataFound);
      response.CellPasses.Count.Should().Be(5);

      for (var idx = 0; idx < 5; idx++)
      {
        var mockModifier = idx + (layerId == LAYER_ID1 ? 1 : 6);
        var expectedTime = baseTime.AddMinutes(mockModifier);
        var expectedHeight = 1.0f + mockModifier * 0.5f;
        var expectedCcv = (short)(10 + 10 * mockModifier);

        var cellPass = response.CellPasses[idx];
        cellPass.LastPassValidCCV.Should().Be(expectedCcv);
        cellPass.LastPassTime.Should().Be(expectedTime);
        cellPass.Height.Should().Be(expectedHeight);
        cellPass.LayersCount.Should().Be(1);
      }
    }
  }
}
