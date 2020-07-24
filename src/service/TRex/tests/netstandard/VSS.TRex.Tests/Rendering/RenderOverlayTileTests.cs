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
using VSS.TRex.Rendering.Abstractions;
using System.IO;
using VSS.TRex.DI;
using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.Common.Utilities;
using Moq;
using CoreX.Interfaces;
using Microsoft.Extensions.DependencyInjection;

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

    protected void CheckSimpleRenderTileResponse(IBitmap bitmap, string fileName = "", string compareToFile = "")
    {

      // Get the rendering factory from the DI context
      var renderingFactory = DIContext.Obtain<IRenderingFactory>();
      var response = renderingFactory.CreateTileRenderResponse(bitmap?.GetBitmap()) as TileRenderResponse;

      var bmp = Image.FromStream(new MemoryStream(((TileRenderResponse_Core2)response).TileBitmapData)) as Bitmap;

      // Convert the response into a bitmap
      bmp.Should().NotBeNull();
//      bmp.Height.Should().Be(256);
//      bmp.Width.Should().Be(256);

      if (!string.IsNullOrEmpty(fileName))
      {
        bmp.Save(fileName);
      }
      else
      {
        // If the comparison file does not exist then create it to provide a base comparison moving forward.
        if (!string.IsNullOrEmpty(compareToFile) && !File.Exists(compareToFile))
        {
          bmp.Save(compareToFile);
        }
      }

      if (!string.IsNullOrEmpty(compareToFile))
      {
        var goodBmp = Image.FromStream(new FileStream(compareToFile, FileMode.Open, FileAccess.Read, FileShare.Read)) as Bitmap;
        goodBmp.Height.Should().Be(bmp.Height);
        goodBmp.Width.Should().Be(bmp.Width);
        goodBmp.Size.Should().Be(bmp.Size);

        for (var i = 0; i <= bmp.Width - 1; i++)
        {
          for (var j = 0; j < bmp.Height - 1; j++)
          {
            goodBmp.GetPixel(i, j).Should().Be(bmp.GetPixel(i, j));
          }
        }
      }
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

    protected ISiteModel BuildModelForSingleCellTileRender(float heightIncrement, int cellX = SubGridTreeConsts.DefaultIndexOriginOffset, int cellY = SubGridTreeConsts.DefaultIndexOriginOffset)
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

    [Theory]
    [InlineData(360, 360, 5, 0, 0, 50)]
    [InlineData(0, 360, 5, 0, 0, 256)]
    [InlineData(0, 360, 5, 50.0, 50.0, 256)]
    [InlineData(0, 360, 5, 0, 0, 50)]
    [InlineData(0, 360, 5, 50.0, 50.0, 50)]
    public async Task Test_RenderOverlayTile_SurveyedSurface_ElevationOnly_Rotated(int initialRotation, int maxRotation, int rotationIncrement, double rotateAboutX, double rotateAboutY, ushort imagePixelSize)
    {
      AddClusterComputeGridRouting();
      AddDesignProfilerGridRouting();

      // Render a surveyed surface area of 100x100 meters in a tile 150x150 meters with a single cell with 
      // production data placed at the origin

      // A location on the bug36372.ttm surface - X=247500.0, Y=193350.0
      const double LOCATION_X = 0.0;
      const double LOCATION_Y = 0.0;

      // Find the location of the cell in the site model for that location
      SubGridTree.CalculateIndexOfCellContainingPosition
        (LOCATION_X, LOCATION_Y, SubGridTreeConsts.DefaultCellSize, SubGridTreeConsts.DefaultIndexOriginOffset, out var cellX, out var cellY);

      // Create the site model containing a single cell and add the surveyed surface to it 
      var siteModel = BuildModelForSingleCellTileRender(HEIGHT_INCREMENT_0_5, cellX, cellY);

      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSurveyedSurfaceEncompassingExtent(ref siteModel,
        new BoundingWorldExtent3D(0, 0, 100, 100), DateTime.UtcNow, new double[] { 100, 101, 102, 103 });
      var palette = PVMPaletteFactory.GetPalette(siteModel, DisplayMode.Height, siteModel.SiteModelExtent);

      var rotationDegrees = initialRotation;

      while (rotationDegrees <= maxRotation)
      {
        // Rotate the 'top right'/rotatedPoint by x degress
        var rot = MathUtilities.DegreesToRadians(rotationDegrees);
        GeometryHelper.RotatePointAbout(rot, 0, 0, out var rotatedBottomLeftPointX, out var rotatedBottomLeftPointY, rotateAboutX, rotateAboutY);
        GeometryHelper.RotatePointAbout(rot, 100, 100, out var rotatedTopRightPointX, out var rotatedTopRightPointY, rotateAboutX, rotateAboutY);
        GeometryHelper.RotatePointAbout(rot, 0, 100, out var rotatedTopLeftPointX, out var rotatedTopLeftPointY, rotateAboutX, rotateAboutY);
        GeometryHelper.RotatePointAbout(rot, 100, 0, out var rotatedBottomRightPointX, out var rotatedBottomRightPointY, rotateAboutX, rotateAboutY);

        var mockConvertCoordinates = new Mock<IConvertCoordinates>();
        mockConvertCoordinates.Setup(x => x.LLHToNEE(It.IsAny<string>(), It.IsAny<CoreX.Models.XYZ[]>(), It.IsAny<CoreX.Types.InputAs>())).Returns(new CoreX.Models.XYZ[]
        {
          new CoreX.Models.XYZ(rotatedBottomLeftPointX, rotatedBottomLeftPointY,  0.0),
          new CoreX.Models.XYZ(rotatedTopRightPointX, rotatedTopRightPointY, 0.0),
          new CoreX.Models.XYZ(rotatedTopLeftPointX, rotatedTopLeftPointY, 0.0),
          new CoreX.Models.XYZ(rotatedBottomRightPointX, rotatedBottomRightPointY, 0.0)
        }
        );

        DIBuilder.Continue().Add(x => x.AddSingleton(mockConvertCoordinates.Object)).Complete();
        var render = new RenderOverlayTile(siteModel.ID,
                                           DisplayMode.Height,
                                           new XYZ(0, 0),
                                           new XYZ(100, 100),
                                           false, // Coords are LLH for rotated, grid otherwise - the mocked conversion above will return the true rotated grid coordinates
                                           imagePixelSize, //PixelsX
                                           imagePixelSize, // PixelsY
                                           new FilterSet(new CombinedFilter()),
                                           new DesignOffset(),
                                           palette,
                                           Color.Black,
                                           string.Empty,
                                           new LiftParameters());

        var result = await render.ExecuteAsync();
        result.Should().NotBeNull();

        var filename = $"RotatedOverlayTileWithSurveyedSurface({imagePixelSize} pixels, rotate about {rotateAboutX},{rotateAboutY} by {rotationDegrees} degrees).bmp";
        var path = Path.Combine("TestData", "RenderedTiles", "SurveyedSurface", filename);
        var saveFileName = @$"c:\temp\{filename}";
        CheckSimpleRenderTileResponse(result, saveFileName, "");

        rotationDegrees += rotationIncrement;
      }
    }
  }
}
