using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using SkiaSharp;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.ComputeFuncs;
using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGrids.Responses;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;

namespace VSS.TRex.Tests.Rendering.Requests
{
  public class TileRequestTestsBase 
  {
    protected void AddApplicationGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting
      <TileRenderRequestComputeFunc, TileRenderRequestArgument, TileRenderResponse>();

    protected void AddClusterComputeGridRouting()
    {
      IgniteMock.Immutable.AddClusterComputeGridRouting<SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();
      IgniteMock.Immutable.AddClusterComputeGridRouting<SubGridProgressiveResponseRequestComputeFunc, ISubGridProgressiveResponseRequestComputeFuncArgument, bool>();
    }

    protected void AddDesignProfilerGridRouting()
    {
      IgniteMock.Immutable.AddApplicationGridRouting
        <CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();
      IgniteMock.Immutable.AddApplicationGridRouting
        <SurfaceElevationPatchComputeFunc, ISurfaceElevationPatchArgument, ISerialisedByteArrayWrapper>();
      IgniteMock.Immutable.AddApplicationGridRouting
        <AlignmentDesignFilterBoundaryComputeFunc, AlignmentDesignFilterBoundaryArgument, AlignmentDesignFilterBoundaryResponse>();
    }

    protected TileRenderRequestArgument SimpleTileRequestArgument(ISiteModel siteModel, DisplayMode displayMode, IPlanViewPalette palette = null, CellPassAttributeFilter attributeFilter = null, VolumeComputationType volumeType = VolumeComputationType.None)
    {
      var filter = displayMode == DisplayMode.CutFill ? new FilterSet(new CombinedFilter(), new CombinedFilter()) : new FilterSet(new CombinedFilter());

      if (attributeFilter != null)
        filter.Filters[0].AttributeFilter = attributeFilter;

      return new TileRenderRequestArgument(siteModel.ID, displayMode, palette, siteModel.SiteModelExtent, true, 256, 256, filter, new DesignOffset(), volumeType);
    }

    protected ISiteModel BuildModelForSingleCellTileRender(float heightIncrement,
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

    protected void CheckSimpleRenderTileResponse(TileRenderResponse response, DisplayMode? displayMode = null, string fileName = "", string compareToFile = "")
    {
      response.Should().NotBeNull();
      response.Should().BeOfType<TileRenderResponse>();

      if (displayMode != null && (displayMode == DisplayMode.CCA || displayMode == DisplayMode.CCASummary))
      {
        response.ResultStatus.Should().Be(RequestErrorStatus.FailedToGetCCAMinimumPassesValue);
        ((TileRenderResponse)response).TileBitmapData.Should().BeNull();
      }
      else
      {
        response.ResultStatus.Should().Be(RequestErrorStatus.OK);
        ((TileRenderResponse)response).TileBitmapData.Should().NotBeNull();

        // Convert the response into a bitmap
        var bmp = SKBitmap.Decode(((TileRenderResponse)response).TileBitmapData);

        bmp.Should().NotBeNull();
        bmp.Height.Should().Be(256);
        bmp.Width.Should().Be(256);
        
        if (!string.IsNullOrEmpty(fileName))
        {
          using var image = SKImage.FromBitmap(bmp);
          using var data = image.Encode(SKEncodedImageFormat.Png, 100);
          using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
          data.SaveTo(stream);
        }
        else
        {
          // If the comparison file does not exist then create it to provide a base comparison moving forward.
          if (!string.IsNullOrEmpty(compareToFile) && !File.Exists(compareToFile))
          {
            using var image = SKImage.FromBitmap(bmp);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = new FileStream(compareToFile, FileMode.Create, FileAccess.Write, FileShare.None);
            data.SaveTo(stream);
          }
        }

        if (!string.IsNullOrEmpty(compareToFile))
        {
          var goodBmp = SKBitmap.Decode(compareToFile);
          goodBmp.Height.Should().Be(bmp.Height);
          goodBmp.Width.Should().Be(bmp.Width);

          for (var i = 0; i < bmp.Width; i++)
          {
            for (var j = 0; j < bmp.Height; j++)
            {
              if (goodBmp.GetPixel(i, j) != bmp.GetPixel(i, j))
              {
                j = j;
              }
              goodBmp.GetPixel(i, j).Should().Be(bmp.GetPixel(i, j));
            }
          }
        }
      }
    }
  }
}
