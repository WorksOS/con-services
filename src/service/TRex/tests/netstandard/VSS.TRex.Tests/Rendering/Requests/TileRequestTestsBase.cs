using System;
using System.Drawing;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
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
using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;

namespace VSS.TRex.Tests.Rendering.Requests
{
  public class TileRequestTestsBase 
  {
    protected void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting
      <TileRenderRequestComputeFunc, TileRenderRequestArgument, TileRenderResponse>();

    protected void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeGridRouting
      <SubGridsRequestComputeFuncProgressive<SubGridsRequestArgument, SubGridRequestsResponse>, SubGridsRequestArgument, SubGridRequestsResponse>();

    protected void AddDesignProfilerGridRouting() => IgniteMock.AddApplicationGridRouting
      <CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();

    protected TileRenderRequestArgument SimpleTileRequestArgument(ISiteModel siteModel, DisplayMode displayMode, IPlanViewPalette palette = null, CellPassAttributeFilter attributeFilter = null)
    {
      var filter = new FilterSet(new CombinedFilter());

      if (attributeFilter != null)
        filter.Filters[0].AttributeFilter = attributeFilter;

      return new TileRenderRequestArgument(siteModel.ID, displayMode, palette, siteModel.SiteModelExtent, true, 256, 256, filter, new DesignOffset());
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
        var bmp = System.Drawing.Image.FromStream(new MemoryStream(((TileRenderResponse_Core2) response).TileBitmapData)) as Bitmap;
        bmp.Should().NotBeNull();
        bmp.Height.Should().Be(256);
        bmp.Width.Should().Be(256);

        if (fileName != "")
        {
          bmp.Save(fileName);
        }

        if (compareToFile != "")
        {
          var goodBmp = System.Drawing.Image.FromStream(new FileStream(compareToFile, FileMode.Open, FileAccess.Read, FileShare.Read)) as Bitmap;
          goodBmp.Height.Should().Be(bmp.Height);
          goodBmp.Width.Should().Be(bmp.Width);
          goodBmp.Size.Should().Be(bmp.Size);

          for (int i = 0; i <= bmp.Width - 1; i++)
          {
            for (int j = 0; j < bmp.Height - 1; j++)
            {
              goodBmp.GetPixel(i, j).Should().Be(bmp.GetPixel(i, j));
            }
          }
        }
      }
    }
  }
}
