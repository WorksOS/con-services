using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.Productivity3D.Common.Executors;
using System.IO;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class TerrainControllerTests
  {

    private static IServiceProvider serviceProvider;
    private static ILoggerFactory logger;
    private static Dictionary<string, string> _customHeaders;
 //   private const int NULL_RAPTOR_MACHINE_DESIGN_ID = -1;

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
    public void PD_GetTerrainTile_TRex_Success()
    {
      var projectIds = new ProjectID() { ProjectUid = Guid.NewGuid(), ProjectId = 1 };
      var callerId = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;

      var expectedResult = new QMTileResult
      (
        new byte[] { 0x41, 0x42, 0x42, 0x41} // may get compressed(gzip) later in part two
      );

      var resultStream = new MemoryStream(new byte[] 
        {
          0x41, 0x42, 0x42, 0x41
        });

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();

      var request = new QMTileRequest()
      {
        X = 0,
        Y = 0,
        Z = 0,
        CallId = new Guid(),
        Filter1 = new FilterResult(),
        FilterId1 = 1,
        ProjectId = 1,
        ProjectUid = projectIds.ProjectUid
      };

      //var request2 = CastRequestObjectTo<QMTileRequest>(request);

      tRexProxy.Setup(x => x.SendDataPostRequestWithStreamResponse<QMTileRequest>(
        It.Is<QMTileRequest>(r => r.CallId == request.CallId),
          It.Is<string>(s => s == "/terrain"),
          It.IsAny<IDictionary<string, string>>()))
       .Returns(Task.FromResult<Stream>(resultStream));


      // Send request to TRex webapi endpoint
      //    var fileResult = trexCompactionDataProxy.SendDataPostRequestWithStreamResponse(request, "/terrain", customHeaders).Result;



      //    Task<Stream> SendDataPostRequestWithStreamResponse<TRequest>(TRequest dataRequest, string route,
      // IDictionary<string, string> customHeaders = null);

      //  var assets = new List<KeyValuePair<Guid, long>>() { new KeyValuePair<Guid, long>(assetUid, assetId) };
      //  var assetProxy = new Mock<IAssetResolverProxy>();
      //      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
      //        .ReturnsAsync(assets);

      var configStore = new Mock<IConfigurationStore>();
      //configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(true);

      /*
      var executor = RequestExecutorContainerFactory
        .Build<QMTilesExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
          */

      var executor = RequestExecutorContainerFactory
        .Build<QMTilesExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);


      //var qmTileResult = RequestExecutorContainerFactory.Build<QMTilesExecutor>(logger,
      //configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders).Process(request) as QMTileResult;


      var result = executor.ProcessAsync(request).Result as QMTileResult;

      Assert.IsNotNull(result, "Result should not be null");


      Assert.AreEqual(expectedResult.TileData.Length, result.TileData.Length, "QM Tile does not match");

      for (int i = 0; i < expectedResult.TileData.Length; i++)
      {
        Assert.IsTrue(expectedResult.TileData[i].Equals(result.TileData[i]), "QM Tile does not match");

      }


//      Assert.IsTrue(expectedResult.Equals(result.TileData), "QM Tile does not match");


      tRexProxy.Verify(m => m.SendDataPostRequestWithStreamResponse(
        It.Is<QMTileRequest>(r => r.CallId == request.CallId),
          It.Is<string>(s => s == "/terrain"),
          It.IsAny<IDictionary<string, string>>())
      , Times.Once);
      
    }



  }
}
