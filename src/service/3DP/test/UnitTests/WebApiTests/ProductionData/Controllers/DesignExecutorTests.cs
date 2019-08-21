using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Serilog;
#if RAPTOR
using DesignProfiler.ComputeDesignBoundary.RPC;
using DesignProfilerDecls;
using VLPDDecls;
#endif
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models.MapHandling;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class DesignExecutorTests
  {
    private const int PROJECT_ID = 123;
    private const double TOLERANCE = 1.2;
    private const int NUMBER_OF_COORDINATES = 7;

    private const string joString = "{\"type\": \"FeatureCollection\",\"features\": [{\"type\": \"Feature\",\"geometry\": {\"type\": \"Polygon\",\"coordinates\": [[[-115.020639,36.207504],[-115.020068,36.207317],[-115.0195,36.207209],[-115.019495,36.207203],[-115.01949,36.207198],[-115.020362,36.207487],[-115.020639,36.207504]]]},\"properties\": {\"name\": \"Large Sites Road - Trimble Road.TTM\"}}]}";

    private static IServiceProvider serviceProvider;
    private static ILoggerFactory logger;
    private static Dictionary<string, string> _customHeaders;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      serviceProvider = new ServiceCollection()
                        .AddLogging()
                        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.WebApi.Tests.log")))
                        .BuildServiceProvider();

      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      _customHeaders = new Dictionary<string, string>();
    }

    [TestMethod]
    public void DesignExecutor_DesignBoundariesRequest_Success()
    {
      var request = new DesignBoundariesRequest(PROJECT_ID, Guid.NewGuid(), TOLERANCE);
      request.Validate();
    }

    [TestMethod]
    public void DesignExecutor_DesignBoundariesRequest_ProjectUIDFailure()
    {
      var request = new DesignBoundariesRequest(PROJECT_ID, Guid.Empty, TOLERANCE);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());

      ex.Should().NotBeNull();
      ex.Code.Should().Be(HttpStatusCode.BadRequest);
      ex.GetResult.Code.Should().Be(ContractExecutionStatesEnum.ValidationError);
      ex.GetResult.Message.Should().Be("Invalid project UID.");
    }

    [TestMethod]
    public void DesignExecutor_DesignBoundariesRequest_ProjectIDFailure()
    {
      var request = new DesignBoundariesRequest(0, Guid.NewGuid(), TOLERANCE);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());

      ex.Should().NotBeNull();
      ex.Code.Should().Be(HttpStatusCode.BadRequest);
      ex.GetResult.Code.Should().Be(ContractExecutionStatesEnum.ValidationError);
      ex.GetResult.Message.Should().Be("Invalid project ID");
    }

    [TestMethod]
    public void DesignExecutor_TRex_Success()
    {
      var request = new DesignBoundariesRequest(PROJECT_ID, Guid.NewGuid(), TOLERANCE);
      
      var expectedResult = new DesignBoundaryResult(JsonConvert.DeserializeObject<GeoJson>(joString));

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<DesignBoundaryResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedResult);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_DESIGN_BOUNDARY")).Returns(true);

      var executor = RequestExecutorContainerFactory
        .Build<DesignExecutor>(logger, configStore: configStore.Object, fileList: new List<FileData> { new FileData() },
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);

      var result = executor.ProcessAsync(request).Result as DesignResult;
      
      result.Should().NotBeNull();
      result.DesignBoundaries.Should().NotBeNull();
      result.DesignBoundaries.Length.Should().Be(1);

      var geoJSon = result.DesignBoundaries[0].ToObject<GeoJson>();

      geoJSon.Features.Count.Should().Be(1);
      geoJSon.Features.Count.Should().Be(expectedResult.GeoJSON.Features.Count);

      geoJSon.Features[0].Geometry.Coordinates.Count.Should().Be(1);
      geoJSon.Features[0].Geometry.Coordinates.Count.Should().Be(expectedResult.GeoJSON.Features[0].Geometry.Coordinates.Count);

      geoJSon.Features[0].Geometry.Coordinates[0].Count.Should().Be(NUMBER_OF_COORDINATES);
      geoJSon.Features[0].Geometry.Coordinates[0].Count.Should().Be(expectedResult.GeoJSON.Features[0].Geometry.Coordinates[0].Count);
      
      for (var i = 0; i < geoJSon.Features[0].Geometry.Coordinates[0].Count; i++)
      {
        var coordinate = geoJSon.Features[0].Geometry.Coordinates[0][i];
        var resultCoordinate = expectedResult.GeoJSON.Features[0].Geometry.Coordinates[0][i];
        
        coordinate[0].Should().Be(resultCoordinate[0]);
        coordinate[1].Should().Be(resultCoordinate[1]);
      }
    }

#if RAPTOR
    [TestMethod]
    public void DesignExecutor_Raptor_Success()
    {
      var request = new DesignBoundariesRequest(PROJECT_ID, Guid.NewGuid(), TOLERANCE);

      var expectedResultStream = new MemoryStream();

      var writer = new StreamWriter(expectedResultStream);
      try
      {
        writer.Write(joString);
        writer.Flush();
        expectedResultStream.Position = 0;

        var expectedResult = TDesignProfilerRequestResult.dppiOK;

        var configStore = new Mock<IConfigurationStore>();
        configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_DESIGN_BOUNDARY")).Returns(false);
        configStore.Setup(x => x.GetValueString("TCCFILESPACEID")).Returns(Guid.NewGuid().ToString());
        configStore.Setup(x => x.GetValueString("TCCFILESPACENAME")).Returns("Test-Dev");

        var raptorClient = new Mock<IASNodeClient>();
        raptorClient
          .Setup(x => x.GetDesignBoundary(It.IsAny<TDesignProfilerServiceRPCVerb_CalculateDesignBoundary_Args>(),
            out expectedResultStream,
            out expectedResult)
          ).Returns(true);

        var executor = RequestExecutorContainerFactory
          .Build<DesignExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
            fileList: new List<FileData> { new FileData() }, customHeaders: _customHeaders);

        var result = executor.ProcessAsync(request).Result as DesignResult;

        result.Should().NotBeNull();
        result.DesignBoundaries.Should().NotBeNull();
        result.DesignBoundaries.Length.Should().Be(1);

        var geoJSon = result.DesignBoundaries[0].ToObject<GeoJson>();
        var expectedGeoJSon = JsonConvert.DeserializeObject<GeoJson>(joString);

        geoJSon.Features.Count.Should().Be(1);
        geoJSon.Features.Count.Should().Be(expectedGeoJSon.Features.Count);

        geoJSon.Features[0].Geometry.Coordinates.Count.Should().Be(1);
        geoJSon.Features[0].Geometry.Coordinates.Count.Should().Be(expectedGeoJSon.Features[0].Geometry.Coordinates.Count);

        geoJSon.Features[0].Geometry.Coordinates[0].Count.Should().Be(NUMBER_OF_COORDINATES);
        geoJSon.Features[0].Geometry.Coordinates[0].Count.Should().Be(expectedGeoJSon.Features[0].Geometry.Coordinates[0].Count);
        
        for (var i = 0; i < geoJSon.Features[0].Geometry.Coordinates[0].Count; i++)
        {
          var coordinate = geoJSon.Features[0].Geometry.Coordinates[0][i];
          var resultCoordinate = expectedGeoJSon.Features[0].Geometry.Coordinates[0][i];

          coordinate[0].Should().Be(resultCoordinate[0]);
          coordinate[1].Should().Be(resultCoordinate[1]);
        }
      }
      finally
      {
        writer.Dispose();
        expectedResultStream.Dispose();
      }
    }
#endif
  }
}
