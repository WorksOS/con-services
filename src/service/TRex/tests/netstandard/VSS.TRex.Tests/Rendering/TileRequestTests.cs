using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Cells;
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
      _ = siteModel.Machines.CreateNew("Bulldozer", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      return siteModel;
    }

    private void AddApplicationGridRouting() => DITAGFileAndSubGridRequestsWithIgniteFixture.AddApplicationGridRouting<TileRenderRequestComputeFunc, TileRenderRequestArgument, TileRenderResponse>();

    private void AddClusterComputeGridRouting() => DITAGFileAndSubGridRequestsWithIgniteFixture.AddClusterComputeGridRouting<SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();

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

    [Fact]
    public void Test_TileRenderRequest_Creation()
    {
      var request = new TileRenderRequest();

      request.Should().NotBeNull();
    }

    private void BuildModelForSingleCellTileRender(out ISiteModel siteModel, float heightIncrement)
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

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
    }

    private void CheckSimpleRenderTileReponse(TileRenderResponse response)
    {
      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);
      response.Should().BeOfType<TileRenderResponse_Core2>();
      ((TileRenderResponse_Core2)response).TileBitmapData.Should().NotBeNull();

      // Convert the response into a bitmap
      var bmp = System.Drawing.Bitmap.FromStream(new MemoryStream(((TileRenderResponse_Core2)response).TileBitmapData));
      bmp.Should().NotBeNull();
      bmp.Height.Should().Be(256);
      bmp.Width.Should().Be(256);
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
    //[InlineData(DisplayMode.CutFill)]
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
    //[InlineData(DisplayMode.CutFill)]
    public void Test_TileRenderRequest_SiteModelWithSingleCell_FullExtents(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      BuildModelForSingleCellTileRender(out var siteModel, 0.5f);

      var request = new TileRenderRequest();
      var response = request.Execute(SimpleTileRequestArgument(siteModel, displayMode));

      CheckSimpleRenderTileReponse(response);
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
    //[InlineData(DisplayMode.CutFill)]
    public void Test_TileRenderRequest_SingleTAGFileSiteModel_FileExtents(DisplayMode displayMode)
    {
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();

      var corePath = Path.Combine("TestData", "Rendering");
      var tagFiles = new[]
      {
        Path.Combine(corePath, "TestTAGFile.tag"),
      };

      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);
      var request = new TileRenderRequest();
      var response = request.Execute(SimpleTileRequestArgument(siteModel, displayMode));

      CheckSimpleRenderTileReponse(response);

      //File.WriteAllBytes($@"c:\temp\TRexTileRender-Unit-Test-{displayMode}.bmp", ((TileRenderResponse_Core2) response).TileBitmapData);
    }
  }
}
