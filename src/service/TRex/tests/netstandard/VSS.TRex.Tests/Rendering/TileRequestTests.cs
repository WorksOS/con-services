using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Cells;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.ComputeFuncs;
using VSS.TRex.Rendering.GridFabric.Requests;
using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Rendering
{
  [UnitTestCoveredRequest(RequestType = typeof(TileRenderRequest))]
  public class TileRequestTests : IClassFixture<DIRenderingFixture>
  {
    private const float HEIGHT_INCREMENT_0_5 = 0.5f;

    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting
      <TileRenderRequestComputeFunc, TileRenderRequestArgument, TileRenderResponse>();

    private void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeGridRouting
      <SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();

    private void AddDesignProfilerGridRouting() => IgniteMock.AddApplicationGridRouting
      <CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();

    private TileRenderRequestArgument SimpleTileRequestArgument(ISiteModel siteModel, DisplayMode displayMode, IPlanViewPalette palette = null, CellPassAttributeFilter attributeFilter = null)
    {
      var filter = new FilterSet(new CombinedFilter());

      if (attributeFilter != null)
        filter.Filters[0].AttributeFilter = attributeFilter;

      return new TileRenderRequestArgument(siteModel.ID, displayMode, palette, siteModel.SiteModelExtent, true, 256, 256, filter, new DesignOffset());
    }

    private ISiteModel BuildModelForSingleCellTileRender(float heightIncrement,
      int cellX = SubGridTreeConsts.DefaultIndexOriginOffset, int cellY = SubGridTreeConsts.DefaultIndexOriginOffset)
    {
      var baseTime = DateTime.UtcNow;
      var baseHeight = 1.0f;
      byte baseCCA = 1;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetCCAStateEvents.PutValueAtDate(VSS.TRex.Common.Consts.MIN_DATETIME_AS_UTC, 5);

      var referenceDate = DateTime.UtcNow;
      var startReportPeriod1 = referenceDate.AddMinutes(-60);
      var endReportPeriod1 = referenceDate.AddMinutes(-30);

      siteModel.MachinesTargetValues[bulldozerMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startReportPeriod1, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endReportPeriod1, ProductionEventType.EndEvent);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].LayerIDStateEvents.PutValueAtDate(endReportPeriod1, 1);

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = baseHeight + x * heightIncrement,
          CCA = (byte)(baseCCA + x),
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, cellX, cellY, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }

    private void CheckSimpleRenderTileResponse(TileRenderResponse response, DisplayMode? displayMode = null)
    {
      response.Should().NotBeNull();
      response.Should().BeOfType<TileRenderResponse_Core2>();

      if (displayMode != null && (displayMode == DisplayMode.CCA || displayMode == DisplayMode.CCASummary))
      {
        response.ResultStatus.Should().Be(RequestErrorStatus.FailedToGetCCAMinimumPassesValue);
        ((TileRenderResponse_Core2)response).TileBitmapData.Should().BeNull();
      }
      else
      {
        response.ResultStatus.Should().Be(RequestErrorStatus.OK);
        ((TileRenderResponse_Core2)response).TileBitmapData.Should().NotBeNull();

        // Convert the response into a bitmap
        var bmp = System.Drawing.Image.FromStream(new MemoryStream(((TileRenderResponse_Core2)response).TileBitmapData));
        bmp.Should().NotBeNull();
        bmp.Height.Should().Be(256);
        bmp.Width.Should().Be(256);
      }
    }

    [Fact]
    public void Test_TileRenderRequest_Creation()
    {
      var request = new TileRenderRequest();

      request.Should().NotBeNull();
    }

    [Theory]
    [InlineData(DisplayMode.Height)]
    [InlineData(DisplayMode.CCV)]
    [InlineData(DisplayMode.CCVPercentSummary)]
    [InlineData(DisplayMode.CCA)]
    [InlineData(DisplayMode.CCASummary)]
    [InlineData(DisplayMode.MDP)]
    [InlineData(DisplayMode.MDPPercentSummary)]
    [InlineData(DisplayMode.MachineSpeed)]
    [InlineData(DisplayMode.TargetSpeedSummary)]
    [InlineData(DisplayMode.TemperatureDetail)]
    [InlineData(DisplayMode.TemperatureSummary)]
    [InlineData(DisplayMode.PassCount)]
    [InlineData(DisplayMode.PassCountSummary)]
    public async Task Test_TileRenderRequest_EmptySiteModel_FullExtents(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new TileRenderRequest();

      var response = await request.ExecuteAsync(SimpleTileRequestArgument(siteModel, displayMode));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.InvalidCoordinateRange);
      response.Should().BeOfType<TileRenderResponse_Core2>();
      ((TileRenderResponse_Core2) response).TileBitmapData.Should().NotBeNull();
    }

    [Theory]
    [InlineData(DisplayMode.Height)]
    [InlineData(DisplayMode.CCV)]
    [InlineData(DisplayMode.CCVPercentSummary)]
    [InlineData(DisplayMode.CCA)]
    [InlineData(DisplayMode.CCASummary)]
    [InlineData(DisplayMode.MDP)]
    [InlineData(DisplayMode.MDPPercentSummary)]
    [InlineData(DisplayMode.MachineSpeed)]
    [InlineData(DisplayMode.TargetSpeedSummary)]
    [InlineData(DisplayMode.TemperatureDetail)]
    [InlineData(DisplayMode.TemperatureSummary)]
    [InlineData(DisplayMode.PassCount)]
    [InlineData(DisplayMode.PassCountSummary)]
    public async Task Test_TileRenderRequest_EmptySiteModel_FullExtents_WithColourPalette(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new TileRenderRequest();

      var palette = PVMPaletteFactory.GetPalette(siteModel, displayMode, siteModel.SiteModelExtent);

      var response = await request.ExecuteAsync(SimpleTileRequestArgument(siteModel, displayMode, palette));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.InvalidCoordinateRange);
      response.Should().BeOfType<TileRenderResponse_Core2>();
      ((TileRenderResponse_Core2)response).TileBitmapData.Should().NotBeNull();
    }

    [Theory]
    [InlineData(DisplayMode.Height)]
    [InlineData(DisplayMode.CCV)]
    [InlineData(DisplayMode.CCVPercentSummary)]
    [InlineData(DisplayMode.CCA)]
    [InlineData(DisplayMode.CCASummary)]
    [InlineData(DisplayMode.MDP)]
    [InlineData(DisplayMode.MDPPercentSummary)]
    [InlineData(DisplayMode.MachineSpeed)]
    [InlineData(DisplayMode.TargetSpeedSummary)]
    [InlineData(DisplayMode.TemperatureDetail)]
    [InlineData(DisplayMode.TemperatureSummary)]
    [InlineData(DisplayMode.PassCount)]
    [InlineData(DisplayMode.PassCountSummary)]
    public async Task Test_TileRenderRequest_SiteModelWithSingleCell_FullExtents(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = BuildModelForSingleCellTileRender(HEIGHT_INCREMENT_0_5);
      
      var request = new TileRenderRequest();
      var filter = new CellPassAttributeFilter() { MachinesList = new[] { siteModel.Machines[0].ID }, LayerID = 1 };
      var response = await request.ExecuteAsync(SimpleTileRequestArgument(siteModel, displayMode, null, filter));

      CheckSimpleRenderTileResponse(response);
    }

    [Theory]
    [InlineData(DisplayMode.Height)]
    [InlineData(DisplayMode.CCV)]
    [InlineData(DisplayMode.CCVPercentSummary)]
    [InlineData(DisplayMode.CCA)]
    [InlineData(DisplayMode.CCASummary)]
    [InlineData(DisplayMode.MDP)]
    [InlineData(DisplayMode.MDPPercentSummary)]
    [InlineData(DisplayMode.MachineSpeed)]
    [InlineData(DisplayMode.TargetSpeedSummary)]
    [InlineData(DisplayMode.TemperatureDetail)]
    [InlineData(DisplayMode.TemperatureSummary)]
    [InlineData(DisplayMode.PassCount)]
    [InlineData(DisplayMode.PassCountSummary)]
    public async Task Test_TileRenderRequest_SiteModelWithSingleCell_FullExtents_WithColourPalette(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = BuildModelForSingleCellTileRender(HEIGHT_INCREMENT_0_5);

      var palette = PVMPaletteFactory.GetPalette(siteModel, displayMode, siteModel.SiteModelExtent);

      var request = new TileRenderRequest();
      var response = await request.ExecuteAsync(SimpleTileRequestArgument(siteModel, displayMode, palette));

      CheckSimpleRenderTileResponse(response);
    }

    [Theory]
    [InlineData(DisplayMode.Height)]
    [InlineData(DisplayMode.CCV)]
    [InlineData(DisplayMode.CCVPercentSummary)]
    [InlineData(DisplayMode.CCA)]
    [InlineData(DisplayMode.CCASummary)]
    [InlineData(DisplayMode.MDP)]
    [InlineData(DisplayMode.MDPPercentSummary)]
    [InlineData(DisplayMode.MachineSpeed)]
    [InlineData(DisplayMode.TargetSpeedSummary)]
    [InlineData(DisplayMode.TemperatureDetail)]
    [InlineData(DisplayMode.TemperatureSummary)]
    [InlineData(DisplayMode.PassCount)]
    [InlineData(DisplayMode.PassCountSummary)]
    public async Task Test_TileRenderRequest_SingleTAGFileSiteModel_FileExtents(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var request = new TileRenderRequest();
      var response = await request.ExecuteAsync(SimpleTileRequestArgument(siteModel, displayMode));

      CheckSimpleRenderTileResponse(response, displayMode);

      //File.WriteAllBytes($@"c:\temp\TRexTileRender-Unit-Test-{displayMode}.bmp", ((TileRenderResponse_Core2) response).TileBitmapData);
    }

    [Theory]
    [InlineData(DisplayMode.Height)]
    [InlineData(DisplayMode.CCV)]
    [InlineData(DisplayMode.CCVPercentSummary)]
    [InlineData(DisplayMode.CCA)]
    [InlineData(DisplayMode.CCASummary)]
    [InlineData(DisplayMode.MDP)]
    [InlineData(DisplayMode.MDPPercentSummary)]
    [InlineData(DisplayMode.MachineSpeed)]
    [InlineData(DisplayMode.TargetSpeedSummary)]
    [InlineData(DisplayMode.TemperatureDetail)]
    [InlineData(DisplayMode.TemperatureSummary)]
    [InlineData(DisplayMode.PassCount)]
    [InlineData(DisplayMode.PassCountSummary)]
    public async Task Test_TileRenderRequest_NoSubGridData_EmptyTile(DisplayMode displayMode)
    {
      // See BUG# 86870
      // Test setup: Setup some production data in different sub grids
      // Request a tile in a sub grid with no data
      // Tile should return quickly, with an empty tile (previously it would sit for 2 minutes to timeout, and return null).
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var baseTime = DateTime.UtcNow;
      var baseHeight = 1.0f;
      byte baseCCA = 1;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetCCAStateEvents.PutValueAtDate(VSS.TRex.Common.Consts.MIN_DATETIME_AS_UTC, 5);

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = baseHeight + x * HEIGHT_INCREMENT_0_5,
          CCA = (byte)(baseCCA + x),
          PassType = PassType.Front
        }).ToArray();

      // And offset to make sure we span multiple sub grids (if the tile request is in the same sub grid, even if empty, the bug was not applicable)
      const int offset = (2 * SubGridTreeConsts.CellsPerSubGrid) + 1;

      // Create some cell passes in 4 corners, but in seperate sub grids 
      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, SubGridTreeConsts.DefaultIndexOriginOffset + offset, SubGridTreeConsts.DefaultIndexOriginOffset + offset, cellPasses);
      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, SubGridTreeConsts.DefaultIndexOriginOffset + offset, SubGridTreeConsts.DefaultIndexOriginOffset - offset, cellPasses);
      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, SubGridTreeConsts.DefaultIndexOriginOffset - offset, SubGridTreeConsts.DefaultIndexOriginOffset - offset, cellPasses);
      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, SubGridTreeConsts.DefaultIndexOriginOffset - offset, SubGridTreeConsts.DefaultIndexOriginOffset + offset, cellPasses);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      // Now if we get a single cell pass in the center, which has no data we should get an empty tile (not a null tile)
      var tileExtents = siteModel.Grid.GetCellExtents(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset);

      var request = new TileRenderRequest();
      var arg =  new TileRenderRequestArgument(siteModel.ID, displayMode, null, tileExtents, true, 256, 256, new FilterSet(new CombinedFilter()), new DesignOffset());

      var startTime = DateTime.UtcNow;
      var response = await request.ExecuteAsync(arg);
      var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

      // 30 seconds should be ample time, even on a slow computer - but well under the 2 minute timeout which is enforced by the pipeline processor.
      duration.Should().BeLessThan(30 * 1000, "Empty tile should return quickly - see BUG# 86870"); 

      // And the tile should NOT be null
      CheckSimpleRenderTileResponse(response, displayMode);

//      File.WriteAllBytes($@"c:\temp\TRexTileRender-Unit-Test-{displayMode}.bmp", ((TileRenderResponse_Core2) response).TileBitmapData);
    }

    [Theory]
    [InlineData(DisplayMode.Height)]
    [InlineData(DisplayMode.CCV)]
    [InlineData(DisplayMode.CCVPercentSummary)]
    [InlineData(DisplayMode.CCA)]
    [InlineData(DisplayMode.CCASummary)]
    [InlineData(DisplayMode.MDP)]
    [InlineData(DisplayMode.MDPPercentSummary)]
    [InlineData(DisplayMode.MachineSpeed)]
    [InlineData(DisplayMode.TargetSpeedSummary)]
    [InlineData(DisplayMode.TemperatureDetail)]
    [InlineData(DisplayMode.TemperatureSummary)]
    [InlineData(DisplayMode.PassCount)]
    [InlineData(DisplayMode.PassCountSummary)]
    public async Task Test_TileRenderRequest_SingleTAGFileSiteModel_FileExtents_WithColourPalette(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var palette = PVMPaletteFactory.GetPalette(siteModel, displayMode, siteModel.SiteModelExtent);

      var request = new TileRenderRequest();
      var response = await request.ExecuteAsync(SimpleTileRequestArgument(siteModel, displayMode, palette));

      CheckSimpleRenderTileResponse(response);

      //File.WriteAllBytes($@"c:\temp\TRexTileRender-Unit-Test-{displayMode}.bmp", ((TileRenderResponse_Core2) response).TileBitmapData);
    }

    [Theory]
    [InlineData(false, -25)]
    [InlineData(true, -25)]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    public async Task Test_TileRenderRequest_SiteModelWithSingleCell_FullExtents_CutFill(bool usePalette, double offset)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      // A location on the bug36372.ttm surface - X=247500.0, Y=193350.0
      const double TTMLocationX = 247500.0;
      const double TTMLocationY = 193350.0;

      // Find the location of the cell in the site model for that location
      SubGridTree.CalculateIndexOfCellContainingPosition
        (TTMLocationX, TTMLocationY, SubGridTreeConsts.DefaultCellSize, SubGridTreeConsts.DefaultIndexOriginOffset, out int cellX, out int cellY);

      // Create the site model containing a single cell and add the design to it for the cut/fill
      var siteModel = BuildModelForSingleCellTileRender(HEIGHT_INCREMENT_0_5, cellX, cellY);

      var palette = usePalette ? PVMPaletteFactory.GetPalette(siteModel, DisplayMode.CutFill, siteModel.SiteModelExtent) : null;

      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, "Bug36372.ttm", false);
      var referenceDesign = new DesignOffset(designUid, offset);

      var request = new TileRenderRequest();
      var arg = SimpleTileRequestArgument(siteModel, DisplayMode.CutFill, palette);

      // Add the cut/fill design reference to the request, and set the rendering extents to the cell in question,
      // with an additional 1 meter border around the cell
      arg.ReferenceDesign = referenceDesign;
      arg.Extents = siteModel.Grid.GetCellExtents(cellX, cellY);
      arg.Extents.Expand(1.0, 1.0);

      var response = await request.ExecuteAsync(arg);
      CheckSimpleRenderTileResponse(response);

      //The tile for 0 offset is red, for -25 it is blue
      //File.WriteAllBytes($@"c:\temp\TRexTileRender-Unit-Test-{DisplayMode.CutFill}.bmp", ((TileRenderResponse_Core2) response).TileBitmapData);
    }
  }
}
