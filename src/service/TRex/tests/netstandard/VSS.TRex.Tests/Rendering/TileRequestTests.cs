using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Cells;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.ComputeFuncs;
using VSS.TRex.Rendering.GridFabric.Requests;
using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Rendering
{
  public class TileRequestTests : IClassFixture<DIRenderingFixture>
  {
    private ISiteModel NewEmptyModel()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      siteModel.Machines.CreateNew("Bulldozer", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      return siteModel;
    }

    private void AddApplicationGridRouting() => DITAGFileAndSubGridRequestsWithIgniteFixture.AddApplicationGridRouting
      <TileRenderRequestComputeFunc, TileRenderRequestArgument, TileRenderResponse>();

    private void AddClusterComputeGridRouting() => DITAGFileAndSubGridRequestsWithIgniteFixture.AddClusterComputeGridRouting
      <SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();

    private void AddDesignProfilerGridRouting() => DITAGFileAndSubGridRequestsWithIgniteFixture.AddApplicationGridRouting
      <CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();

    private TileRenderRequestArgument SimpleTileRequestArgument(ISiteModel siteModel, DisplayMode displayMode)
    {
      return new TileRenderRequestArgument
      {
        CoordsAreGrid = true,
        Extents = siteModel.SiteModelExtent,
        TRexNodeID = "UnitTest_TileRenderRequestNode",
        Mode = displayMode,
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        PixelsX = 256,
        PixelsY = 256
      };
    }

    private void BuildModelForSingleCellTileRender(out ISiteModel siteModel, float heightIncrement,
      uint cellX = SubGridTreeConsts.DefaultIndexOriginOffset, uint cellY = SubGridTreeConsts.DefaultIndexOriginOffset)
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

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, cellX, cellY, cellPasses, 1, cellPasses.Length);
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
    [InlineData(DisplayMode.CCA)]
    [InlineData(DisplayMode.CCASummary)]
    [InlineData(DisplayMode.MDP)]
    [InlineData(DisplayMode.MachineSpeed)]
    [InlineData(DisplayMode.TargetSpeedSummary)]
    [InlineData(DisplayMode.TemperatureSummary)]
    public void Test_TileRenderRequest_EmptySiteModel_FullExtents(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var siteModel = NewEmptyModel();
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
    [InlineData(DisplayMode.CCA)]
    [InlineData(DisplayMode.CCASummary)]
    [InlineData(DisplayMode.MDP)]
    [InlineData(DisplayMode.MachineSpeed)]
    [InlineData(DisplayMode.TargetSpeedSummary)]
    [InlineData(DisplayMode.TemperatureSummary)]
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
    [InlineData(DisplayMode.CCA)]
    [InlineData(DisplayMode.CCASummary)]
    [InlineData(DisplayMode.MDP)]
    [InlineData(DisplayMode.MachineSpeed)]
    [InlineData(DisplayMode.TargetSpeedSummary)]
    [InlineData(DisplayMode.TemperatureSummary)]
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

    [Fact]
    public void Test_TileRenderRequest_SiteModelWithSingleCell_FullExtents_CutFill()
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

      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, "bug36372.ttm");
      var request = new TileRenderRequest();
      var arg = SimpleTileRequestArgument(siteModel, DisplayMode.CutFill);

      // Add the cut/fill design reference to the request, and set the rendering extents to the cell in question,
      // with an additional 1 meter border around the cell
      arg.ReferenceDesignUID = designUid;
      arg.Extents = siteModel.Grid.GetCellExtents(cellX, cellY);
      arg.Extents.Expand(1.0, 1.0);

      var response = request.Execute(arg);
      CheckSimpleRenderTileResponse(response);

      // File.WriteAllBytes($@"c:\temp\TRexTileRender-Unit-Test-{DisplayMode.CutFill}.bmp", ((TileRenderResponse_Core2) response).TileBitmapData);
    }
  }
}
