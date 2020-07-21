using System;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.Executors;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using System.Drawing;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Cells;
using VSS.TRex.Types;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.Responses;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.GridFabric;

namespace VSS.TRex.Tests.Rendering
{
  public class RenderOverlayTileTests : IClassFixture<DIRenderingFixture>
  {
    private const float HEIGHT_INCREMENT_0_5 = 0.5f;

    protected void AddClusterComputeGridRouting()
    {
      IgniteMock.Immutable.AddClusterComputeGridRouting<SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();
      IgniteMock.Immutable.AddClusterComputeGridRouting<SubGridProgressiveResponseRequestComputeFunc, ISubGridProgressiveResponseRequestComputeFuncArgument, bool>();
    }

    protected void AddDesignProfilerGridRouting()
    {
      IgniteMock.Immutable.AddApplicationGridRouting<CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();
      IgniteMock.Immutable.AddApplicationGridRouting<SurfaceElevationPatchComputeFunc, ISurfaceElevationPatchArgument, ISerialisedByteArrayWrapper>();
    }

    [Fact]
    public void Test_RenderOverlayTile_Creation()
    {
      var render = new RenderOverlayTile(Guid.NewGuid(),
        DisplayMode.Height,
        new XYZ(0, 0),
        new XYZ(100, 100),
        true, // CoordsAreGrid
        100, //PixelsX
        100, // PixelsY
        null, // Filters
        new DesignOffset(), // DesignDescriptor.Null(),
        null,
        Color.Black,
        string.Empty,
        new LiftParameters());

      render.Should().NotBeNull();
    }

    protected ISiteModel BuildModelForSingleCellTileRender(float heightIncrement,
int cellX = SubGridTreeConsts.DefaultIndexOriginOffset, int cellY = SubGridTreeConsts.DefaultIndexOriginOffset)
    {
      var baseTime = DateTime.UtcNow;
      var baseHeight = 1.0f;

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
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses(siteModel, cellX, cellY, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }

    [Fact]
    public async Task Test_RenderOverlayTile_SurveyedSurface_ElevationOnly_Rotated()
    {
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      // Render a surveyed surface area of 100x100 meters in a tile 150x150 meters with a single cell with 
      // production data placed at the origin

      // A location on the bug36372.ttm surface - X=247500.0, Y=193350.0
      const double LOCATION_X = 00.0;
      const double LOCATION_Y = 0.0;

      // Find the location of the cell in the site model for that location
      SubGridTree.CalculateIndexOfCellContainingPosition
        (LOCATION_X, LOCATION_Y, SubGridTreeConsts.DefaultCellSize, SubGridTreeConsts.DefaultIndexOriginOffset, out var cellX, out var cellY);

      // Create the site model containing a single cell and add the surveyed surface to it 
      var siteModel = BuildModelForSingleCellTileRender(HEIGHT_INCREMENT_0_5, cellX, cellY);

      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructFlatSurveyedSurfaceEncompassingExtent(ref siteModel,
        new BoundingWorldExtent3D(0, 0, 100, 100), 100, DateTime.UtcNow);
      var palette = PVMPaletteFactory.GetPalette(siteModel, DisplayMode.Height, siteModel.SiteModelExtent);

      var render = new RenderOverlayTile(siteModel.ID,
                                         DisplayMode.Height,
                                         new XYZ(0, 0),
                                         new XYZ(150, 150),
                                         true, // CoordsAreGrid
                                         100, //PixelsX
                                         100, // PixelsY
                                         new FilterSet( new CombinedFilter() ),
                                         new DesignOffset(),
                                         palette,
                                         Color.Black,
                                         string.Empty,
                                         new LiftParameters());

      var result = await render.ExecuteAsync();
      result.Should().NotBeNull();



      /*

      var request = new TileRenderRequest();
      var arg = SimpleTileRequestArgument(siteModel, DisplayMode.Height, palette);
      arg.Extents = new TRex.Geometry.BoundingWorldExtent3D(0, 0, 150, 150);

      var response = await request.ExecuteAsync(arg);

      const string FILE_NAME = "SimpleSurveyedSurface.bmp";
      var path = Path.Combine("TestData", "RenderedTiles", "SurveyedSurface", FILE_NAME);

      var saveFileName = @$"c:\temp\{FILE_NAME}";

      CheckSimpleRenderTileResponse(response, DisplayMode.CutFill, saveFileName, path);
      */
    }
  }
}
