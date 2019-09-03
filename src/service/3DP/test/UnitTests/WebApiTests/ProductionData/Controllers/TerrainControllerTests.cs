using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class TerrainControllerTests
  {

    private static IServiceProvider serviceProvider;
    private static ILoggerFactory logger;
    private static Dictionary<string, string> _customHeaders;


    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      var serviceCollection = new ServiceCollection()
        .AddLogging()
        .AddSingleton(loggerFactory);

      serviceProvider = serviceCollection.BuildServiceProvider();
      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      _customHeaders = new Dictionary<string, string>();
    }


    [TestMethod]
    public void PD_GetTerrainTile_TRex_Fail()
    {
      var expectedResult = new QMTileResult
      (
        new byte[] { 0x41, 0x42, 0x42, 0x41} // may get compressed(gzip) later in part two
      );

      var resultStream = new MemoryStream(new byte[] 
        {
          0x41, 0x42, 0x42, 0x41
        });

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();

      var request = new QMTileRequest
                    {
        X = 0,
        Y = 0,
        Z = 0,
        Filter = new FilterResult(),
        ProjectUid = Guid.Empty	
      };

      // make bad call
      tRexProxy.Setup(x => x.SendDataPostRequestWithStreamResponse(
        It.Is<QMTileRequest>(r => r.ProjectUid == request.ProjectUid),
          It.Is<string>(s => s == "/terrainNoValid"),
          It.IsAny<IDictionary<string, string>>()))
       .Returns(Task.FromResult<Stream>(resultStream));

      var configStore = new Mock<IConfigurationStore>();

      var executor = RequestExecutorContainerFactory
        .Build<QMTilesExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);

      var result = executor.ProcessAsync(request).Result as QMTileResult;

      Assert.IsNull(result, "Result should be null");

      tRexProxy.Verify(m => m.SendDataPostRequestWithStreamResponse(
        It.Is<QMTileRequest>(r => r.ProjectUid == request.ProjectUid),
          It.Is<string>(s => s == "/terrain"),
          It.IsAny<IDictionary<string, string>>())
      , Times.Once);
      
    }

    [TestMethod]
    public void PD_GetTerrainTile_TRex_Success()
    {
      var expectedResult = new QMTileResult
      (
        new byte[] { 0x41, 0x42, 0x42, 0x41 } // may get compressed(gzip) later in part two
      );

      var resultStream = new MemoryStream(new byte[]
        {
          0x41, 0x42, 0x42, 0x41
        });

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();

      var request = new QMTileRequest
                    {
        X = 0,
        Y = 0,
        Z = 0,
        Filter = new FilterResult(),
        ProjectUid = Guid.NewGuid	()
      };


      tRexProxy.Setup(x => x.SendDataPostRequestWithStreamResponse(
        It.Is<QMTileRequest>(r => r.ProjectUid == request.ProjectUid),
          It.Is<string>(s => s == "/terrain"),
          It.IsAny<IDictionary<string, string>>()))
       .Returns(Task.FromResult<Stream>(resultStream));


      var configStore = new Mock<IConfigurationStore>();

      var executor = RequestExecutorContainerFactory
        .Build<QMTilesExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);


      var result = executor.ProcessAsync(request).Result as QMTileResult;

      Assert.IsNotNull(result, "Result should not be null");


      Assert.AreEqual(expectedResult.TileData.Length, result.TileData.Length, "QM Tile does not match");

      for (int i = 0; i < expectedResult.TileData.Length; i++)
      {
        Assert.IsTrue(expectedResult.TileData[i].Equals(result.TileData[i]), "QM Tile does not match");

      }


      tRexProxy.Verify(m => m.SendDataPostRequestWithStreamResponse(
        It.Is<QMTileRequest>(r => r.ProjectUid == request.ProjectUid),
          It.Is<string>(s => s == "/terrain"),
          It.IsAny<IDictionary<string, string>>())
      , Times.Once);

    }


  }
}
