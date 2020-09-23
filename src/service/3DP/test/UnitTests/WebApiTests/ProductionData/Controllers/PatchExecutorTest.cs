using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;
using Point = VSS.MasterData.Models.Models.Point;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class PatchExecutorTest
  {
    private static IServiceProvider _serviceProvider;
    private static ILoggerFactory _logger;
    private const ushort cellSize = 32;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      _serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.WebApi.Tests.log")))
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .BuildServiceProvider();

      _logger = _serviceProvider.GetRequiredService<ILoggerFactory>();
    }


    [TestMethod]
    public async Task PatchRequest_Invalid_TRex_NoResult()
    {
      var projectId = 999;
      var projectUid = Guid.NewGuid();
      var bBox = new BoundingBox2DGrid(1, 200, 10, 210);
      var mockConfigStore = new Mock<IConfigurationStore>();

      var filterResult = new FilterResult();
      filterResult.SetBoundary(new List<Point>()
      {
        new Point(bBox.BottomleftY, bBox.BottomLeftX),
        new Point(bBox.BottomleftY, bBox.TopRightX),
        new Point(bBox.TopRightY, bBox.TopRightX),
        new Point(bBox.TopRightY, bBox.BottomLeftX)
      });
      var patchRequest = new PatchRequest(
        projectId,
        projectUid,
        new Guid(),
        DisplayMode.Height,
        null,
        new LiftBuildSettings(),
        false,
        VolumesType.None,
        VelociraptorConstants.VOLUME_CHANGE_TOLERANCE,
        null, filterResult, null, FilterLayerMethod.AutoMapReset,
        0, 1000, true);
      patchRequest.Validate();

      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          $"Patch request failed somehow. ProjectUid: {projectUid}"));

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataPostRequestWithStreamResponse(It.IsAny<PatchDataRequest>(), "/patches", It.IsAny<IHeaderDictionary>()))
        .Throws(exception);

      var executor = RequestExecutorContainerFactory
        .Build<PatchExecutor>(_logger, mockConfigStore.Object,
          trexCompactionDataProxy: tRexProxy.Object);
      var result = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(patchRequest));
      result.Code.Should().Be(HttpStatusCode.InternalServerError);
      result.GetResult.Code.Should().Be(ContractExecutionStatesEnum.InternalProcessingError);
      result.GetResult.Message.Should().Be(exception.GetResult.Message);
    }

    [TestMethod]
    public async Task PatchRequest_TRex_WithResult()
    {
      var projectId = 999;
      var projectUid = Guid.NewGuid();
      var request = new PatchesRequest("ec520SerialNumber",
        90, 180, new BoundingBox2DGrid(1, 200, 10, 210));
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_PATCHES")).Returns(true);

      var filterResult = new FilterResult();
      filterResult.SetBoundary(new List<Point>
      {
        new Point(request.BoundingBox.BottomleftY, request.BoundingBox.BottomLeftX),
        new Point(request.BoundingBox.BottomleftY, request.BoundingBox.TopRightX),
        new Point(request.BoundingBox.TopRightY, request.BoundingBox.TopRightX),
        new Point(request.BoundingBox.TopRightY, request.BoundingBox.BottomLeftX)
      });
  

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();

      var subGridOriginX = 150.45;
      var subGridOriginY = 1400.677;
      var palette = new List<ColorPalette>();
      uint paletteColor = 44444;
      var elevationOrigin = (float)100.450;
      var nowTimeOrigin = new DateTimeOffset(DateTime.UtcNow.AddDays(-5).AddMinutes(100));
      var timeOrigin = (uint)(nowTimeOrigin).ToUnixTimeSeconds();
      var delta = (uint)0;

      // elevation offsets are in mm
      var elevationOffsets = new ushort[cellSize * cellSize];
      var timeOffsets = new uint[cellSize * cellSize];
      for (var c = delta; c < (cellSize * cellSize); c++)
      {
        elevationOffsets[c] = (ushort)(c + 6);
        timeOffsets[c] = c + 3;
        if (((int) c % 10) == 0)
          palette.Add(new ColorPalette(paletteColor++, elevationOrigin + (elevationOffsets[c] / 1000.0)));
      }

      var patchRequest = new PatchRequest(
        projectId,
        projectUid,
        new Guid(),
        DisplayMode.Height,
        palette,
        new LiftBuildSettings(),
        true,
        VolumesType.None,
        VelociraptorConstants.VOLUME_CHANGE_TOLERANCE,
        null, filterResult, null, FilterLayerMethod.AutoMapReset,
        0, 1000, false);
      patchRequest.Validate();

      var resultStream = WriteAsPerTRex(1, 1, subGridOriginX, subGridOriginY, elevationOrigin, timeOrigin, elevationOffsets, timeOffsets);
      tRexProxy.Setup(x => x.SendDataPostRequestWithStreamResponse(It.IsAny<PatchDataRequest>(), "/patches", It.IsAny<IHeaderDictionary>()))
        .Returns(Task.FromResult<Stream>(resultStream));

      var executor = RequestExecutorContainerFactory
        .Build<PatchExecutor>(_logger, mockConfigStore.Object,
          trexCompactionDataProxy: tRexProxy.Object);
      var result = await executor.ProcessAsync(patchRequest) as PatchResultRenderedColors;
      result.Should().NotBeNull();
      result.Subgrids.Should().NotBeNull();
      result.Subgrids.Length.Should().Be(1);

      var subGrid = (PatchSubgridResult)result.Subgrids[0];
      subGrid.CellOriginX.Should().Be((int)subGridOriginX);
      subGrid.CellOriginY.Should().Be((int)subGridOriginY);
      subGrid.ElevationOrigin.Should().Be(elevationOrigin);
      subGrid.Cells[0,0].Elevation.Should().Be((float)(elevationOrigin + ((delta + 6.0) / 1000.0)));
      subGrid.Cells[0,0].Color.Should().Be(0);
      subGrid.Cells[0,1].Elevation.Should().Be((float)(elevationOrigin + ((delta + 7.0) / 1000.0)));
      subGrid.Cells[0,1].Color.Should().Be(44444);
      subGrid.Cells[1,0].Elevation.Should().Be((float)(elevationOrigin + ((delta + 32.0 + 6.0) / 1000.0)));
      subGrid.Cells[1,0].Color.Should().Be(44447);
    }

    private MemoryStream WriteAsPerTRex(int totalPatchesRequired, int numSubgridsInPatch,
      double subgridOriginX, double subgridOriginY, float elevationOrigin, uint timeOrigin, ushort[] elevationOffsets, uint[] timeOffsets)
    {
      var resultStream = new MemoryStream();
      var writer = new BinaryWriter(resultStream);
      writer.Write((int)totalPatchesRequired);
      writer.Write((int)numSubgridsInPatch);
      writer.Write((double)cellSize);

      writer.Write((double)subgridOriginX);
      writer.Write((double)subgridOriginY);
      writer.Write((Boolean)false); // isValid cells

      writer.Write((float)elevationOrigin);
      writer.Write((byte)2);
      writer.Write((uint)timeOrigin);
      writer.Write((byte)2);
      for (uint c = 0; c < (cellSize * cellSize); c++)
      {
        writer.Write((ushort)elevationOffsets[c]);
        writer.Write((ushort)timeOffsets[c]);
      }
      resultStream.Position = 0;
      return resultStream;
    }
  }
}
