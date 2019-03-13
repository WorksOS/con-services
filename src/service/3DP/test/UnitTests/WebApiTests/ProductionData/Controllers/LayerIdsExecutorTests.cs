using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.AssetService.Proxy;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
#if RAPTOR
using VLPDDecls;
#endif

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class LayerIdsExecutorTests
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
    public async Task LayerIdsExecutor_TRex_Success()
    {
      var projectIds = new ProjectID() {ProjectUid = Guid.NewGuid()};
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;
      long layerId = 444;
      var expectedResult = new LayerIdsExecutionResult
      (
        new List<LayerIdDetails>(1)
        {
          new LayerIdDetails(-1, -1, layerId, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-40), assetUid)
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<LayerIdsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedResult);

      var assets = new List<AssetData>(1)
      {
        new AssetData(customerUid, assetUid, "Asset Name", assetId,
          "serial Number", "CAT", "D7", "Asset Type",
          "", 1, 1977)
      };

      var assetProxy = new Mock<IAssetServiceProxy>();
      assetProxy.Setup(x => x.GetAssetsV1(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_LAYERS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetLayerIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as LayerIdsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.Layers.Count, "Wrong layer count");
      Assert.AreEqual(expectedResult.Layers[0].AssetUid, result.Layers[0].AssetUid,
        "Wrong machine Uid");
      Assert.AreEqual(assets[0].LegacyAssetID, result.Layers[0].AssetId,
        "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.Layers[0].DesignId, result.Layers[0].DesignId, "Wrong design Id");
      Assert.AreEqual(expectedResult.Layers[0].LayerId, result.Layers[0].LayerId, "Wrong layer id");
    }

    [TestMethod]
    public void LayerIdsExecutor_TRex_NoProjectUid()
    {
      var projectIds = new ProjectID();

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_LAYERS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetLayerIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);

      var ex = Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(projectIds)).Result;
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual("Failed to get/update data requested by GetLayerIdsExecutor", ex.GetResult.Message);
    }

#if RAPTOR

    [TestMethod]
    public async Task LayerIdsExecutor_Raptor_Success()
    {
      var projectIds = new ProjectID() {ProjectId = 999};
      var customerUid = Guid.NewGuid();
      var assetId = 777;
      var assetUid = Guid.NewGuid();
      var designId = 888;
      var layerId = 999;
      var expectedResult = new LayerIdsExecutionResult
      (
        new List<LayerIdDetails>(1)
        {
          new LayerIdDetails(assetId, designId, layerId, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-40),assetUid)
        }
      );

      TDesignLayer[] tLayers = new TDesignLayer[1]
      {
        new TDesignLayer
        {
          FAssetID = expectedResult.Layers[0].AssetId,
          FDesignID = expectedResult.Layers[0].DesignId,
          FLayerID = (int) expectedResult.Layers[0].LayerId,
          FStartTime = expectedResult.Layers[0].StartDate,
          FEndTime = expectedResult.Layers[0].EndDate,
        }
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetOnMachineLayers(projectIds.ProjectId.Value, out tLayers))
        .Returns(0);

      var assets = new List<AssetData>(1)
      {
        new AssetData(customerUid, assetUid, "Asset Name", assetId,
          "serial Number", "CAT", "D7", "Asset Type",
          "", 1, 1977)
      };

      var assetProxy = new Mock<IAssetServiceProxy>();
      assetProxy.Setup(x => x.GetAssetsV1(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_LAYERS")).Returns("false");

      var executor = RequestExecutorContainerFactory
        .Build<GetLayerIdsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          assetProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as LayerIdsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.Layers.Count, "Wrong layer count");
      Assert.AreEqual(expectedResult.Layers[0].AssetId, result.Layers[0].AssetId, "Wrong machine Id");
      Assert.AreEqual(expectedResult.Layers[0].AssetUid, result.Layers[0].AssetUid, "Wrong asset Uid");
      Assert.AreEqual(assets[0].LegacyAssetID, result.Layers[0].AssetId, "Wrong legacyAssetId");

      Assert.AreEqual(expectedResult.Layers[0].DesignId, result.Layers[0].DesignId, "Wrong design Id");
      Assert.AreEqual(expectedResult.Layers[0].LayerId, result.Layers[0].LayerId, "Wrong layer Id");
      Assert.AreEqual(expectedResult.Layers[0].StartDate, result.Layers[0].StartDate, "Wrong startDate");
      Assert.AreEqual(expectedResult.Layers[0].EndDate, result.Layers[0].EndDate, "Wrong endDate");
    }
#endif

  }
}
