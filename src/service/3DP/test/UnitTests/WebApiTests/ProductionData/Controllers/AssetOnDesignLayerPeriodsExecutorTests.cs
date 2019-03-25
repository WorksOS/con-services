using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
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
  public class AssetOnDesignLayerPeriodsExecutorTests
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
    public async Task GetAssetOnDesignLayerPeriodsExecutor_TRex_Success()
    {
      var projectIds = new ProjectID() {ProjectUid = Guid.NewGuid()};
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;
      long layerId = 444;
      var expectedResult = new AssetOnDesignLayerPeriodsExecutionResult
      (
        new List<AssetOnDesignLayerPeriod>(1)
        {
          new AssetOnDesignLayerPeriod(-1, -1, layerId, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-40), assetUid)
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<AssetOnDesignLayerPeriodsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedResult);

      var assets = new List<KeyValuePair<Guid, long>>() { new KeyValuePair<Guid, long>(assetUid, assetId) };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_LAYERS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignLayerPeriodsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as AssetOnDesignLayerPeriodsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignLayerPeriods.Count, "Wrong layer count");
      Assert.AreEqual(expectedResult.AssetOnDesignLayerPeriods[0].AssetUid, result.AssetOnDesignLayerPeriods[0].AssetUid,
        "Wrong machine Uid");
      Assert.AreEqual(assets[0].Value, result.AssetOnDesignLayerPeriods[0].AssetId,
        "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignLayerPeriods[0].DesignId, result.AssetOnDesignLayerPeriods[0].DesignId, "Wrong design Id");
      Assert.AreEqual(expectedResult.AssetOnDesignLayerPeriods[0].LayerId, result.AssetOnDesignLayerPeriods[0].LayerId, "Wrong layer id");
    }

    [TestMethod]
    public void GetAssetOnDesignLayerPeriodsExecutor_TRex_NoProjectUid()
    {
      var projectIds = new ProjectID();

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_LAYERS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignLayerPeriodsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);

      var ex = Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(projectIds)).Result;
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual("Failed to get/update data requested by GetAssetOnDesignLayerPeriodsExecutor", ex.GetResult.Message);
    }

    [TestMethod]
    public async Task GetAssetOnDesignLayerPeriodsExecutor_TRex_MultiAssetUid()
    {
      var projectIds = new ProjectID() { ProjectUid = Guid.NewGuid() };
      var customerUid = Guid.NewGuid();
      var assetUid1Good = Guid.NewGuid();
      var assetId1Good = 777;
      var assetId1Expected = assetId1Good;
      var assetUid2Good = Guid.NewGuid();
      var assetId2Invalid = 0;
      var assetId2Expected = -1;
      var assetUid3Good = Guid.NewGuid();
      var assetId3Expected = -1;
      long layerId = 444;
      var expectedResult = new AssetOnDesignLayerPeriodsExecutionResult
      (
        new List<AssetOnDesignLayerPeriod>(1)
        {
          new AssetOnDesignLayerPeriod(-1, -1, layerId, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-49), assetUid1Good),
          new AssetOnDesignLayerPeriod(-1, -1, layerId, DateTime.UtcNow.AddDays(-48), DateTime.UtcNow.AddDays(-47), assetUid2Good),
          new AssetOnDesignLayerPeriod(-1, -1, layerId, DateTime.UtcNow.AddDays(-46), DateTime.UtcNow.AddDays(-45), assetUid3Good),
          new AssetOnDesignLayerPeriod(-1, -1, layerId, DateTime.UtcNow.AddDays(-44), DateTime.UtcNow.AddDays(-43), assetUid1Good),
          new AssetOnDesignLayerPeriod(-1, -1, layerId, DateTime.UtcNow.AddDays(-42), DateTime.UtcNow.AddDays(-41), assetUid2Good),
          new AssetOnDesignLayerPeriod(-1, -1, layerId, DateTime.UtcNow.AddDays(-40), DateTime.UtcNow.AddDays(-38), assetUid3Good)
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<AssetOnDesignLayerPeriodsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedResult);

      var assets = new List<KeyValuePair<Guid, long>>()
      {
        new KeyValuePair<Guid, long>(assetUid1Good, assetId1Good),
        new KeyValuePair<Guid, long>(assetUid2Good, assetId2Invalid)
      };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_LAYERS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignLayerPeriodsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as AssetOnDesignLayerPeriodsExecutionResult;
      Assert.AreEqual(6, result.AssetOnDesignLayerPeriods.Count, "Wrong design count");
      Assert.AreEqual(assetUid1Good, result.AssetOnDesignLayerPeriods[0].AssetUid, "Wrong asset1 Uid (0)");
      Assert.AreEqual(assetId1Expected, result.AssetOnDesignLayerPeriods[0].AssetId, "Wrong legacyAssetId1 (0)");
      Assert.AreEqual(assetUid2Good, result.AssetOnDesignLayerPeriods[1].AssetUid, "Wrong asset1 Uid (1)");
      Assert.AreEqual(assetId2Expected, result.AssetOnDesignLayerPeriods[1].AssetId, "Wrong legacyAssetId1 (1)");
      Assert.AreEqual(assetUid3Good, result.AssetOnDesignLayerPeriods[2].AssetUid, "Wrong asset2 Uid (2)");
      Assert.AreEqual(assetId3Expected, result.AssetOnDesignLayerPeriods[2].AssetId, "Wrong legacyAssetId2 (2)");

      Assert.AreEqual(assetUid1Good, result.AssetOnDesignLayerPeriods[3].AssetUid, "Wrong asset2 Uid (3)");
      Assert.AreEqual(assetId1Expected, result.AssetOnDesignLayerPeriods[3].AssetId, "Wrong legacyAssetId2 (3)");
      Assert.AreEqual(assetUid2Good, result.AssetOnDesignLayerPeriods[4].AssetUid, "Wrong asset3 Uid (4)");
      Assert.AreEqual(assetId2Expected, result.AssetOnDesignLayerPeriods[4].AssetId, "Wrong legacyAssetId3 (4)");
      Assert.AreEqual(assetUid3Good, result.AssetOnDesignLayerPeriods[5].AssetUid, "Wrong asset3 Uid (5)");
      Assert.AreEqual(assetId3Expected, result.AssetOnDesignLayerPeriods[5].AssetId, "Wrong legacyAssetId3 (5)");
    }

#if RAPTOR

    [TestMethod]
    public async Task GetAssetOnDesignLayerPeriodsExecutor_Raptor_Success()
    {
      var projectIds = new ProjectID() {ProjectId = 999};
      var customerUid = Guid.NewGuid();
      var assetId = 777;
      var assetUid = Guid.NewGuid();
      var designId = 888;
      var layerId = 999;
      var expectedResult = new AssetOnDesignLayerPeriodsExecutionResult
      (
        new List<AssetOnDesignLayerPeriod>(1)
        {
          new AssetOnDesignLayerPeriod(assetId, designId, layerId, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-40),null)
        }
      );

      var tLayers = new[]
      {
        new TDesignLayer
        {
          FAssetID = expectedResult.AssetOnDesignLayerPeriods[0].AssetId,
          FDesignID = expectedResult.AssetOnDesignLayerPeriods[0].DesignId,
          FLayerID = (int) expectedResult.AssetOnDesignLayerPeriods[0].LayerId,
          FStartTime = expectedResult.AssetOnDesignLayerPeriods[0].StartDate,
          FEndTime = expectedResult.AssetOnDesignLayerPeriods[0].EndDate,
        }
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetOnMachineLayers(projectIds.ProjectId.Value, out tLayers))
        .Returns(0);

      var assets = new List<KeyValuePair<Guid, long>>() { new KeyValuePair<Guid, long>(assetUid, assetId) };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_LAYERS")).Returns("false");

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignLayerPeriodsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as AssetOnDesignLayerPeriodsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignLayerPeriods.Count, "Wrong layer count");
      Assert.AreEqual(expectedResult.AssetOnDesignLayerPeriods[0].AssetId, result.AssetOnDesignLayerPeriods[0].AssetId, "Wrong machine Id");
      Assert.AreEqual(assets[0].Key, result.AssetOnDesignLayerPeriods[0].AssetUid, "Wrong assetUid");

      Assert.AreEqual(expectedResult.AssetOnDesignLayerPeriods[0].DesignId, result.AssetOnDesignLayerPeriods[0].DesignId, "Wrong design Id");
      Assert.AreEqual(expectedResult.AssetOnDesignLayerPeriods[0].LayerId, result.AssetOnDesignLayerPeriods[0].LayerId, "Wrong layer Id");
      Assert.AreEqual(expectedResult.AssetOnDesignLayerPeriods[0].StartDate, result.AssetOnDesignLayerPeriods[0].StartDate, "Wrong startDate");
      Assert.AreEqual(expectedResult.AssetOnDesignLayerPeriods[0].EndDate, result.AssetOnDesignLayerPeriods[0].EndDate, "Wrong endDate");
    }

    [TestMethod]
    public async Task GetAssetOnDesignLayerPeriodsExecutor_Raptor_MultiAssetUid()
    {
      var projectIds = new ProjectID() { ProjectId = 999 };
      var customerUid = Guid.NewGuid();
      var assetUid1Good = Guid.NewGuid();
      long assetId1Good = 777;
      var assetUid1Expected = assetUid1Good;
      long assetId2Invalid = 0; // johnDoe?
      var assetUid2Good = Guid.NewGuid();
      Guid? assetUid2Expected = null;
      Guid? assetUid3NotFound = null;
      long assetId3NotFound = 888;
      Guid? assetUid3Expected = null;
      var designId = 888;
      var layerId = 999;
      var expectedResult = new AssetOnDesignLayerPeriodsExecutionResult
      (
        new List<AssetOnDesignLayerPeriod>(1)
        {
          new AssetOnDesignLayerPeriod(assetId1Good, -1, layerId, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-49), null),
          new AssetOnDesignLayerPeriod(assetId2Invalid, -1, layerId, DateTime.UtcNow.AddDays(-48), DateTime.UtcNow.AddDays(-47), null),
          new AssetOnDesignLayerPeriod(assetId3NotFound, -1, layerId, DateTime.UtcNow.AddDays(-46), DateTime.UtcNow.AddDays(-45), null),
          new AssetOnDesignLayerPeriod(assetId1Good, -1, layerId, DateTime.UtcNow.AddDays(-44), DateTime.UtcNow.AddDays(-43), null),
          new AssetOnDesignLayerPeriod(assetId2Invalid, -1, layerId, DateTime.UtcNow.AddDays(-42), DateTime.UtcNow.AddDays(-41), null),
          new AssetOnDesignLayerPeriod(assetId3NotFound, -1, layerId, DateTime.UtcNow.AddDays(-40), DateTime.UtcNow.AddDays(-38), null)
        }
      );

      var tLayers = new[]
      {
        new TDesignLayer
        {
          FAssetID = expectedResult.AssetOnDesignLayerPeriods[0].AssetId,
          FDesignID = expectedResult.AssetOnDesignLayerPeriods[0].DesignId,
          FLayerID = (int) expectedResult.AssetOnDesignLayerPeriods[0].LayerId,
          FStartTime = expectedResult.AssetOnDesignLayerPeriods[0].StartDate,
          FEndTime = expectedResult.AssetOnDesignLayerPeriods[0].EndDate,
        },
        new TDesignLayer
        {
          FAssetID = expectedResult.AssetOnDesignLayerPeriods[1].AssetId,
          FDesignID = expectedResult.AssetOnDesignLayerPeriods[1].DesignId,
          FLayerID = (int) expectedResult.AssetOnDesignLayerPeriods[1].LayerId,
          FStartTime = expectedResult.AssetOnDesignLayerPeriods[1].StartDate,
          FEndTime = expectedResult.AssetOnDesignLayerPeriods[1].EndDate,
        },
        new TDesignLayer
        {
          FAssetID = expectedResult.AssetOnDesignLayerPeriods[2].AssetId,
          FDesignID = expectedResult.AssetOnDesignLayerPeriods[2].DesignId,
          FLayerID = (int) expectedResult.AssetOnDesignLayerPeriods[2].LayerId,
          FStartTime = expectedResult.AssetOnDesignLayerPeriods[2].StartDate,
          FEndTime = expectedResult.AssetOnDesignLayerPeriods[2].EndDate,
        },
        new TDesignLayer
        {
          FAssetID = expectedResult.AssetOnDesignLayerPeriods[3].AssetId,
          FDesignID = expectedResult.AssetOnDesignLayerPeriods[3].DesignId,
          FLayerID = (int) expectedResult.AssetOnDesignLayerPeriods[3].LayerId,
          FStartTime = expectedResult.AssetOnDesignLayerPeriods[3].StartDate,
          FEndTime = expectedResult.AssetOnDesignLayerPeriods[3].EndDate,
        },
        new TDesignLayer
        {
          FAssetID = expectedResult.AssetOnDesignLayerPeriods[4].AssetId,
          FDesignID = expectedResult.AssetOnDesignLayerPeriods[4].DesignId,
          FLayerID = (int) expectedResult.AssetOnDesignLayerPeriods[4].LayerId,
          FStartTime = expectedResult.AssetOnDesignLayerPeriods[4].StartDate,
          FEndTime = expectedResult.AssetOnDesignLayerPeriods[4].EndDate,
        },
        new TDesignLayer
        {
          FAssetID = expectedResult.AssetOnDesignLayerPeriods[5].AssetId,
          FDesignID = expectedResult.AssetOnDesignLayerPeriods[5].DesignId,
          FLayerID = (int) expectedResult.AssetOnDesignLayerPeriods[5].LayerId,
          FStartTime = expectedResult.AssetOnDesignLayerPeriods[5].StartDate,
          FEndTime = expectedResult.AssetOnDesignLayerPeriods[5].EndDate,
        }
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetOnMachineLayers(projectIds.ProjectId.Value, out tLayers))
        .Returns(0);

      var assets = new List<KeyValuePair<Guid, long>>()
      {
        new KeyValuePair<Guid, long>(assetUid1Good, assetId1Good),
        new KeyValuePair<Guid, long>(assetUid2Good, assetId2Invalid)
      };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_LAYERS")).Returns("false");

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignLayerPeriodsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as AssetOnDesignLayerPeriodsExecutionResult;
      Assert.AreEqual(6, result.AssetOnDesignLayerPeriods.Count, "Wrong design count");
      Assert.AreEqual(assetUid1Expected, result.AssetOnDesignLayerPeriods[0].AssetUid, "Wrong asset1 Uid (0)");
      Assert.AreEqual(assetId1Good, result.AssetOnDesignLayerPeriods[0].AssetId, "Wrong legacyAssetId1 (0)");
      Assert.AreEqual(assetUid2Expected, result.AssetOnDesignLayerPeriods[1].AssetUid, "Wrong asset2 Uid (1)");
      Assert.AreEqual(assetId2Invalid, result.AssetOnDesignLayerPeriods[1].AssetId, "Wrong legacyAssetId2 (1)");
      Assert.AreEqual(assetUid3Expected, result.AssetOnDesignLayerPeriods[2].AssetUid, "Wrong asset3 Uid (2)");
      Assert.AreEqual(assetId3NotFound, result.AssetOnDesignLayerPeriods[2].AssetId, "Wrong legacyAssetId3 (2)");

      Assert.AreEqual(assetUid1Expected, result.AssetOnDesignLayerPeriods[3].AssetUid, "Wrong asset1 Uid (2)");
      Assert.AreEqual(assetId1Good, result.AssetOnDesignLayerPeriods[3].AssetId, "Wrong legacyAssetId1 (2)");
      Assert.AreEqual(assetUid2Expected, result.AssetOnDesignLayerPeriods[4].AssetUid, "Wrong asset2 Uid (3)");
      Assert.AreEqual(assetId2Invalid, result.AssetOnDesignLayerPeriods[4].AssetId, "Wrong legacyAssetId2 (3)");
      Assert.AreEqual(assetUid3Expected, result.AssetOnDesignLayerPeriods[5].AssetUid, "Wrong asset3 Uid (5)");
      Assert.AreEqual(assetId3NotFound, result.AssetOnDesignLayerPeriods[5].AssetId, "Wrong legacyAssetId3 (5)");
    }
#endif

  }
}
