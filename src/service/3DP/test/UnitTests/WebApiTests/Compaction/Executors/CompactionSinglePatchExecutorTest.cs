using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProtoBuf;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using FluentAssertions;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Common.Models;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionSinglePatchExecutorTest
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

    //[TestMethod, Ignore("Temporary script to generate .proto file for PatchResult while development is ongoing.")]
    [TestMethod]
    public void GenerateProtoFile()
    {
      // After .proto file is created generate .cs client schema using:
      // $ protogen --proto_path=C:\temp PatchResult.proto --csharp_out=C:\temp
      // .\protogen --proto_path=.\ PatchSubgridsProtobufResult.proto --csharp_out=.\

      // var fileString = Serializer.GetProto<PatchSimpleListResult>();
      var fileString = Serializer.GetProto<PatchSubgridsProtobufResult>();
    }

    [DataTestMethod]
    [DataRow("ec520SerialNumber", "", "", 90, 180, 1, 200, 10, 210)]
    [DataRow("ec520SerialNumber", "sn941SerialNumber", "", -90, -180, 2000, 200, 2010, 210)]
    [DataRow("ec520SerialNumber", "", "", 90, -180, 1, 210, 11, 260)]
    public void SinglePatchRequest_Valid(string ecSerial, string radioSerial,
      string tccOrgUid,
      double machineLatitude, double machineLongitude,
      double bottomLeftX, double bottomLeftY, double topRightX, double topRightY)
    {
      var request = new PatchesRequest(ecSerial, radioSerial,
        tccOrgUid,
        machineLatitude, machineLongitude,
        new BoundingBox2DGrid(bottomLeftX, bottomLeftY, topRightX, topRightY));
      request.Validate();
    }
       

    [DataTestMethod]
    [DataRow("", "", "", 90, 180, 1, 200, 10, 210, "ECSerial required")]
    [DataRow("ec520SerialNumber", "", "", 91, 180, 1, 200, 10, 210, "Invalid Machine Location")]
    [DataRow("ec520SerialNumber", "", "", 91, -181, 1, 200, 10, 210, "Invalid Machine Location")]
    [DataRow("ec520SerialNumber", "", "", 0, -1, 1, 200, 10, 210, "Invalid Machine Location")]
    [DataRow("ec520SerialNumber", "", "", 90, -180, 11, 200, 10, 210, "Invalid bounding box: corners are not bottom left and top right.")]
    [DataRow("ec520SerialNumber", "", "", 90, -180, 1, 220, 10, 210, "Invalid bounding box: corners are not bottom left and top right.")]
    [DataRow("ec520SerialNumber", "", "", 90, -180, 1, 210, 11, 260.2, "Invalid bounding box: Must be 500m2 or less.")]
    public void SinglePatchRequest_Invalid(string ecSerial, string radioSerial,
      string tccOrgUid,
      double machineLatitude, double machineLongitude,
      double bottomLeftX, double bottomLeftY, double topRightX, double topRightY,
      string expectedMessage)
    {
      var request = new PatchesRequest(ecSerial, radioSerial,
        tccOrgUid,
        machineLatitude, machineLongitude,
        new BoundingBox2DGrid(bottomLeftX, bottomLeftY, topRightX, topRightY));
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());

      ex.Should().NotBeNull();
      ex.Code.Should().Be(HttpStatusCode.BadRequest);
      ex.GetResult.Code.Should().Be(ContractExecutionStatesEnum.ValidationError);
      ex.GetResult.Message.Should().Be(expectedMessage);
    }

    [TestMethod]
    public async Task SinglePatchRequest_Invalid_TRex_NoResult()
    {
      var projectId = 999;
      var projectUid = Guid.NewGuid();
      var request = new PatchesRequest("ec520SerialNumber", string.Empty, string.Empty,
        90, 180, new BoundingBox2DGrid(1, 200, 10, 210));
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_PATCHES")).Returns(true);

      var filterResult = new FilterResult();
      filterResult.PolygonGrid = new List<Point>
      {
        new Point(request.BoundingBox.BottomleftY, request.BoundingBox.BottomLeftX),
        new Point(request.BoundingBox.BottomleftY, request.BoundingBox.TopRightX),
        new Point(request.BoundingBox.TopRightY, request.BoundingBox.TopRightX),
        new Point(request.BoundingBox.TopRightY, request.BoundingBox.BottomLeftX)
      };
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
          $"SinglePatche request failed somehow. ProjectUid: {projectUid}"));

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataPostRequestWithStreamResponse(It.IsAny<PatchDataRequest>(), "/patches", It.IsAny<IDictionary<string, string>>()))
        .Throws(exception);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionSinglePatchExecutor>(_logger, configStore: mockConfigStore.Object,
          trexCompactionDataProxy: tRexProxy.Object);
      var result = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(patchRequest));
      result.Code.Should().Be(HttpStatusCode.InternalServerError);
      result.GetResult.Code.Should().Be(ContractExecutionStatesEnum.InternalProcessingError);
      result.GetResult.Message.Should().Be(exception.GetResult.Message);
    }

    [TestMethod]
    public async Task SinglePatchRequest_TRex_WithResult()
    {
      var projectId = 999;
      var projectUid = Guid.NewGuid();
      var request = new PatchesRequest("ec520SerialNumber", string.Empty, string.Empty,
        90, 180, new BoundingBox2DGrid(1, 200, 10, 210));
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_PATCHES")).Returns(true);

      var filterResult = new FilterResult();
      filterResult.PolygonGrid = new List<Point>
      {
        new Point(request.BoundingBox.BottomleftY, request.BoundingBox.BottomLeftX),
        new Point(request.BoundingBox.BottomleftY, request.BoundingBox.TopRightX),
        new Point(request.BoundingBox.TopRightY, request.BoundingBox.TopRightX),
        new Point(request.BoundingBox.TopRightY, request.BoundingBox.BottomLeftX)
      };
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

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      
      var subgridOriginX = 150;
      var subgridOriginY = 1400;
      var elevationOrigin = (float) 100.45;
      var nowTimeOrigin = new DateTimeOffset(DateTime.UtcNow.AddDays(-5).AddMinutes(100));
      var timeOrigin = (uint) (nowTimeOrigin).ToUnixTimeSeconds();
      var delta = (uint) 0;

      // elevation offsets are in mm
      var cells = new PatchCellHeightResult[cellSize * cellSize];
      for (uint c = delta; c < (cellSize * cellSize); c++)
        cells[c] = PatchCellHeightResult.Create((ushort)(c + 6), c + 3);

      var resultStream = WriteAsPerTRex(1, 1, subgridOriginX, subgridOriginY, elevationOrigin, timeOrigin, cells);
      tRexProxy.Setup(x => x.SendDataPostRequestWithStreamResponse(It.IsAny<PatchDataRequest>(), "/patches", It.IsAny<IDictionary<string, string>>()))
        .Returns(Task.FromResult<Stream>(resultStream));

      var executor = RequestExecutorContainerFactory
        .Build<CompactionSinglePatchExecutor>(_logger, configStore: mockConfigStore.Object,
          trexCompactionDataProxy: tRexProxy.Object);
      var result = await executor.ProcessAsync(patchRequest) as PatchSubgridsProtobufResult;
      result.Should().NotBeNull();
      result.Subgrids.Should().NotBeNull();
      result.Subgrids.Length.Should().Be(1);
      result.Subgrids[0].Cells.Length.Should().Be(cellSize * cellSize);
      result.Subgrids[0].ElevationOrigin.Should().Be(elevationOrigin);
      result.Subgrids[0].TimeOrigin.Should().Be(timeOrigin);
      result.Subgrids[0].Cells[0].ElevationOffset.Should().Be((ushort)(delta + 6 + 1)); // zero means no offset available
      result.Subgrids[0].Cells[0].TimeOffset.Should().Be(delta + 3);

      var doubleArrayResult = (new CompactionSinglePatchResult()).UnpackSubgrid(cellSize, result.Subgrids[0]);
      doubleArrayResult[0, 0].easting.Should().Be(subgridOriginX + (cellSize /2));
      doubleArrayResult[0, 0].northing.Should().Be(subgridOriginY + (cellSize / 2));
      doubleArrayResult[0, 0].elevation.Should().Be(Math.Round(elevationOrigin + (ushort)(delta + 6)/1000.0, 5));
      var actualDateTime = nowTimeOrigin.AddSeconds(delta + 3).DateTime;
      doubleArrayResult[0, 0].dateTime.Should().Be(new DateTime(actualDateTime.Year, actualDateTime.Month, actualDateTime.Day, actualDateTime.Hour, actualDateTime.Minute, actualDateTime.Second));
    }

    private MemoryStream WriteAsPerTRex(int totalPatchesRequired, int numSubgridsInPatch, 
      double subgridOriginX, double subgridOriginY, float elevationOrigin, uint timeOrigin, PatchCellHeightResult[] cells)
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
        writer.Write((ushort)cells[c].ElevationOffset);
        writer.Write((ushort)cells[c].TimeOffset);
      }
      resultStream.Position = 0;
      return resultStream;
    }
  }
}
