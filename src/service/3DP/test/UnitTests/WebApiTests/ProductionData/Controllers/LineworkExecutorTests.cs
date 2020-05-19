using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models.Files;
using VSS.Productivity3D.WebApi.Compaction.Controllers;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.TRex.Gateway.Common.Proxy;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  public class LineworkExecutorTests
  {
    private IServiceProvider serviceProvider;
    private ILoggerFactory logger;
    private IHeaderDictionary _customHeaders;
    private IConfigurationStore configStore;
    private ITRexCompactionDataProxy trexCompactionDataProxy;

    private WGS84LineworkBoundary[] TestResultBoundary()
    {
      return new[] {new WGS84LineworkBoundary {Boundary = new[] {new WGSPoint(0, 0), new WGSPoint(1, 0), new WGSPoint(0, 1)}, BoundaryName = "Test", BoundaryType = DXFLineWorkBoundaryType.GenericBoundary}};
    }

    public LineworkExecutorTests()
    {
      serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.WebApi.Tests.log")))
        .BuildServiceProvider();

      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      _customHeaders = new HeaderDictionary();

      var configStoreMock = new Mock<IConfigurationStore>();

      configStoreMock.Setup(x => x.GetValueBool("TREX_IS_AVAILABLE")).Returns(true);
      configStoreMock.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_LINEWORKFILE")).Returns(true);

      configStore = configStoreMock.Object;

      var mockTrexCompactionDataProxy = new Mock<ITRexCompactionDataProxy>();
      mockTrexCompactionDataProxy.Setup(x => 
          x.SendDataPostRequest<DxfLineworkFileResult, DXFBoundariesRequest>(It.IsAny<DXFBoundariesRequest>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>(), It.IsAny<bool>()))
      .Returns((DXFBoundariesRequest req, string route, IHeaderDictionary customHeaders, bool mutableGateway) =>
      {
        return Task.FromResult(new DxfLineworkFileResult(ContractExecutionStatesEnum.ExecutedSuccessfully, "Success", TestResultBoundary()));
      });
      trexCompactionDataProxy = mockTrexCompactionDataProxy.Object;
    }

    private MemoryStream CreateTestPolyline()
    {
      byte[] bytes = null;

      using (var ms = new MemoryStream())
      {
        using (var writer = new StreamWriter(ms))
        {
          writer.WriteLine("0");
          writer.WriteLine("SECTION");
          writer.WriteLine("2");
          writer.WriteLine("HEADER");
          writer.WriteLine("9");
          writer.WriteLine("  $MEASUREMENT");
          writer.WriteLine("70");
          writer.WriteLine("0");
          writer.WriteLine("0");
          writer.WriteLine("ENDSEC");
          writer.WriteLine("0");
          writer.WriteLine("SECTION");
          writer.WriteLine("2");
          writer.WriteLine("ENTITIES");
          writer.WriteLine("0");
          writer.WriteLine("LWPOLYLINE");
          writer.WriteLine("90");
          writer.WriteLine("3"); // 3 vertces
          writer.WriteLine("70");
          writer.WriteLine("1"); // Polyline is closed
          writer.WriteLine("10");
          writer.WriteLine("0");
          writer.WriteLine("20");
          writer.WriteLine("0");
          writer.WriteLine("10");
          writer.WriteLine("10");
          writer.WriteLine("20");
          writer.WriteLine("0");
          writer.WriteLine("10");
          writer.WriteLine("0");
          writer.WriteLine("20");
          writer.WriteLine("10");
          writer.WriteLine("0");
          writer.WriteLine("ENDSEC");
          writer.WriteLine("0");
          writer.WriteLine("EOF");
        }

        bytes = ms.ToArray();
      }
      return new MemoryStream(bytes);
    }

    [Fact]
    public void Creation()
    {
      var executor = new LineworkFileExecutor();
      executor.Should().NotBeNull();
    }

    [Fact]
    public async void Execute_ClosedOnly()
    {
      var mockCoordFile = new Mock<IFormFile>();
      mockCoordFile.Setup(x => x.Length).Returns(4);
      mockCoordFile.Setup(x => x.CopyTo(It.IsAny<MemoryStream>())).Callback((Stream ms) =>
      {
        ms.Write(new byte[] {1, 2, 3, 4});
      });

      var mockDXFFile = new Mock<IFormFile>();
      mockDXFFile.Setup(x => x.Length).Returns(CreateTestPolyline().Length);
      mockDXFFile.Setup(x => x.CopyTo(It.IsAny<MemoryStream>())).Callback((Stream ms) =>
      {
        CreateTestPolyline().CopyTo((MemoryStream)ms);
      });

      var requestDto = new DxfFileRequest
      {
        ConvertLineStringCoordsToPolygon = false, 
        DxfUnits = (int)DxfUnitsType.Meters,
        MaxBoundariesToProcess = 10,
        MaxVerticesPerBoundary = 1000,
        CoordinateSystemFile = mockCoordFile.Object,
        DxfFile = mockDXFFile.Object
      };

      var result = await RequestExecutorContainerFactory
        .Build<LineworkFileExecutor>(logger, configStore: configStore,
          trexCompactionDataProxy: trexCompactionDataProxy)
        .ProcessAsync(requestDto);

      result.Code.Should().Be(ContractExecutionStatesEnum.ExecutedSuccessfully);
      result.Message.Should().Be("Success");
      ((DxfLineworkFileResult) result).LineworkBoundaries.Should().BeEquivalentTo(TestResultBoundary());
    }
  }
}
