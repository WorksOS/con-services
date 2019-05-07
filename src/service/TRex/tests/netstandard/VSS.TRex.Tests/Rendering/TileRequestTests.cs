using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Cells;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
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
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting
      <TileRenderRequestComputeFunc, TileRenderRequestArgument, TileRenderResponse>();

    private void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeGridRouting
      <SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();

    private void AddDesignProfilerGridRouting() => IgniteMock.AddApplicationGridRouting
      <CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();

    private TileRenderRequestArgument SimpleTileRequestArgument(ISiteModel siteModel, DisplayMode displayMode, IPlanViewPalette palette = null)
    {
      return new TileRenderRequestArgument(siteModel.ID, displayMode, palette, siteModel.SiteModelExtent, true, 256, 256,
        new FilterSet(new CombinedFilter()), new DesignOffset());
    }

    private void BuildModelForSingleCellTileRender(out ISiteModel siteModel, float heightIncrement,
      uint cellX = SubGridTreeConsts.DefaultIndexOriginOffset, uint cellY = SubGridTreeConsts.DefaultIndexOriginOffset)
    {
      var baseTime = DateTime.UtcNow;
      var baseHeight = 1.0f;

      siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = baseHeight + x * heightIncrement,
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, cellX, cellY, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);
    }

    private void CheckSimpleRenderTileResponse(TileRenderResponse response)
    {
      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);
      response.Should().BeOfType<TileRenderResponse_Core2>();
      ((TileRenderResponse_Core2)response).TileBitmapData.Should().NotBeNull();

      // Convert the response into a bitmap
      var bmp = System.Drawing.Image.FromStream(new MemoryStream(((TileRenderResponse_Core2)response).TileBitmapData));
      bmp.Should().NotBeNull();
      bmp.Height.Should().Be(256);
      bmp.Width.Should().Be(256);
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
    public void Test_TileRenderRequest_EmptySiteModel_FullExtents(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new TileRenderRequest();

      var response = request.Execute(SimpleTileRequestArgument(siteModel, displayMode));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.Unknown);
      response.Should().BeOfType<TileRenderResponse_Core2>();
      ((TileRenderResponse_Core2) response).TileBitmapData.Should().BeNull();
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
    public void Test_TileRenderRequest_EmptySiteModel_FullExtents_WithColourPalette(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new TileRenderRequest();

      var palette = PVMPaletteFactory.GetPallete(siteModel, displayMode, siteModel.SiteModelExtent);

      var response = request.Execute(SimpleTileRequestArgument(siteModel, displayMode, palette));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.Unknown);
      response.Should().BeOfType<TileRenderResponse_Core2>();
      ((TileRenderResponse_Core2)response).TileBitmapData.Should().BeNull();
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
    public void Test_TileRenderRequest_SiteModelWithSingleCell_FullExtents(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      BuildModelForSingleCellTileRender(out var siteModel, 0.5f);
      
      var request = new TileRenderRequest();
      var response = request.Execute(SimpleTileRequestArgument(siteModel, displayMode));

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
    public void Test_TileRenderRequest_SiteModelWithSingleCell_FullExtents_WithColourPalette(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      BuildModelForSingleCellTileRender(out var siteModel, 0.5f);

      var palette = PVMPaletteFactory.GetPallete(siteModel, displayMode, siteModel.SiteModelExtent);

      var request = new TileRenderRequest();
      var response = request.Execute(SimpleTileRequestArgument(siteModel, displayMode, palette));

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
    public void Test_TileRenderRequest_SingleTAGFileSiteModel_FileExtents(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var request = new TileRenderRequest();
      var response = request.Execute(SimpleTileRequestArgument(siteModel, displayMode));

      CheckSimpleRenderTileResponse(response);

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
    public void Test_TileRenderRequest_SingleTAGFileSiteModel_FileExtents_WithColourPalette(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      var palette = PVMPaletteFactory.GetPallete(siteModel, displayMode, siteModel.SiteModelExtent);

      var request = new TileRenderRequest();
      var response = request.Execute(SimpleTileRequestArgument(siteModel, displayMode, palette));

      CheckSimpleRenderTileResponse(response);

      //File.WriteAllBytes($@"c:\temp\TRexTileRender-Unit-Test-{displayMode}.bmp", ((TileRenderResponse_Core2) response).TileBitmapData);
    }

    [Theory]
    [InlineData(false, -25)]
    [InlineData(true, -25)]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    public void Test_TileRenderRequest_SiteModelWithSingleCell_FullExtents_CutFill(bool usePalette, double offset)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      // A location on the bug36372.ttm surface - X=247500.0, Y=193350.0
      const double TTMLocationX = 247500.0;
      const double TTMLocationY = 193350.0;

      // Find the location of the cell in the site model for that location
      SubGridTree.CalculateIndexOfCellContainingPosition
        (TTMLocationX, TTMLocationY, SubGridTreeConsts.DefaultCellSize, SubGridTreeConsts.DefaultIndexOriginOffset, out uint cellX, out uint cellY);

      // Create the site model containing a single cell and add the design to it for the cut/fill
      BuildModelForSingleCellTileRender(out var siteModel, 0.5f, cellX, cellY);

      var palette = usePalette ? PVMPaletteFactory.GetPallete(siteModel, DisplayMode.CutFill, siteModel.SiteModelExtent) : null;

      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, "Bug36372.ttm", false);
      var referenceDesign = new DesignOffset(designUid, offset);

      var request = new TileRenderRequest();
      var arg = SimpleTileRequestArgument(siteModel, DisplayMode.CutFill, palette);

      // Add the cut/fill design reference to the request, and set the rendering extents to the cell in question,
      // with an additional 1 meter border around the cell
      arg.ReferenceDesign = referenceDesign;
      arg.Extents = siteModel.Grid.GetCellExtents(cellX, cellY);
      arg.Extents.Expand(1.0, 1.0);

      var response = request.Execute(arg);
      CheckSimpleRenderTileResponse(response);

      //The tile for 0 offset is red, for -25 it is blue
      //File.WriteAllBytes($@"c:\temp\TRexTileRender-Unit-Test-{DisplayMode.CutFill}.bmp", ((TileRenderResponse_Core2) response).TileBitmapData);
    }
  }
}
