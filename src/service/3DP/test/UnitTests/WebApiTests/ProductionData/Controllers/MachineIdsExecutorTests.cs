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
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.TRex.Gateway.Common.Abstractions;
#if RAPTOR
using VLPDDecls;
#endif

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class MachineIdsExecutorTests
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
    public async Task MachineIdsExecutor_TRex_Success()
    {
      var projectIds = new ProjectID() {ProjectUid = Guid.NewGuid()};
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;
      var expectedResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(-1, "MachineName2", false, "designName",
            14, DateTime.UtcNow.AddDays(-1), null, null, null, null, assetUid)
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(), 
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedResult);

      var assets = new List< KeyValuePair<Guid, long> >() { new KeyValuePair<Guid, long>(assetUid, assetId) };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(true);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.MachineStatuses.Count, "Wrong machine count");
      Assert.AreEqual(expectedResult.MachineStatuses[0].AssetUid, result.MachineStatuses[0].AssetUid,
        "Wrong machine Uid");
      Assert.AreEqual(expectedResult.MachineStatuses[0].MachineName, result.MachineStatuses[0].MachineName,
        "Wrong machine name");
      Assert.AreEqual(assets[0].Value, result.MachineStatuses[0].AssetId,
        "Wrong legacyAssetId");
    }

    [TestMethod]
    public void MachineIdsExecutor_TRex_NoProjectUid()
    {
      var projectIds = new ProjectID();

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(true);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);

      var ex = Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(projectIds)).Result;
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual("Failed to get/update data requested by GetMachineIdsExecutor", ex.GetResult.Message);
    }

    [TestMethod]
    public async Task MachineIdsExecutor_TRex_MultiAssetUid()
    {
      var projectIds = new ProjectID() {ProjectUid = Guid.NewGuid()};
      var customerUid = Guid.NewGuid();
      var assetUid1Good = Guid.NewGuid();
      var assetId1Good = 777;
      var assetId1Expected = assetId1Good;
      var assetUid2Good = Guid.NewGuid();
      var assetId2Invalid = 0;
      var assetId2Expected = -1;
      var assetUid3Good = Guid.NewGuid();
      var assetId3Expected = -1;
      var expectedResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(-1, "MachineName2", false, "designName",14, DateTime.UtcNow.AddDays(-1), null, null, null, null, assetUid1Good),
          new MachineStatus(-1, "MachineName3", false, "designName",14, DateTime.UtcNow.AddDays(-1), null, null, null, null, assetUid2Good),
          new MachineStatus(-1, "MachineName4", false, "designName",14, DateTime.UtcNow.AddDays(-1), null, null, null, null, assetUid3Good)
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(), 
          It.IsAny<IDictionary<string, string>>(),
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
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(true);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(3, result.MachineStatuses.Count, "Wrong design count");
      Assert.AreEqual(assetUid1Good, result.MachineStatuses[0].AssetUid, "Wrong asset1 Uid (0)");
      Assert.AreEqual(assetId1Expected, result.MachineStatuses[0].AssetId, "Wrong legacyAssetId1 (0)");
      Assert.AreEqual(assetUid2Good, result.MachineStatuses[1].AssetUid, "Wrong asset1 Uid (1)");
      Assert.AreEqual(assetId2Expected, result.MachineStatuses[1].AssetId, "Wrong legacyAssetId1 (1)");
      Assert.AreEqual(assetUid3Good, result.MachineStatuses[2].AssetUid, "Wrong asset2 Uid (2)");
      Assert.AreEqual(assetId3Expected, result.MachineStatuses[2].AssetId, "Wrong legacyAssetId2 (2)");
    }

#if RAPTOR

    [TestMethod]
    public async Task MachineIdsExecutor_Raptor_Success()
    {
      var projectIds = new ProjectID() {ProjectId = 999};
      var customerUid = Guid.NewGuid();
      var assetId = 777;
      var assetUid = Guid.NewGuid();
      var expectedResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(assetId, "MachineName2", false, "designName",
            14, DateTime.UtcNow.AddDays(-1), null, null, null, null, null)
        }
      );

      var tMachines = new[]
      {
        new TMachineDetail
        {
          Name = expectedResult.MachineStatuses[0].MachineName,
          ID = expectedResult.MachineStatuses[0].AssetId,
          IsJohnDoeMachine = false
        }
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetMachineIDs(projectIds.ProjectId.Value))
        .Returns(tMachines);

      var assets = new List<KeyValuePair<Guid, long>>() { new KeyValuePair<Guid, long>(assetUid, assetId) };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(false);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.MachineStatuses.Count, "Wrong machine count");
      Assert.AreEqual(expectedResult.MachineStatuses[0].AssetId, result.MachineStatuses[0].AssetId, "Wrong machine Id");
      Assert.AreEqual(expectedResult.MachineStatuses[0].MachineName, result.MachineStatuses[0].MachineName,
        "Wrong machine name");
      Assert.AreEqual(assets[0].Key, result.MachineStatuses[0].AssetUid, "Wrong assetUid");
    }

    [TestMethod]
    public async Task MachineIdsExecutor_Raptor_MultiAsset()
    {
      var projectIds = new ProjectID() { ProjectId = 999 };
      var customerUid = Guid.NewGuid();
      var assetUid1Good = Guid.NewGuid();
      long assetId1Good = 777;
      var assetUid1Expected = assetUid1Good;
      long assetId2Invalid = 0; // johnDoe?
      Guid? assetUid2Expected = null;
      var assetUid3Good = Guid.NewGuid();
      long assetId3Good = 888;
      var assetUid3Expected = assetUid3Good;
      var expectedResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(assetId1Good, "MachineName2", false, "designName",14, DateTime.UtcNow.AddDays(-1), null, null, null, null, null),
          new MachineStatus(assetId2Invalid, "MachineName2", false, "designName",14, DateTime.UtcNow.AddDays(-1), null, null, null, null, null),
          new MachineStatus(assetId3Good, "MachineName2", false, "designName",14, DateTime.UtcNow.AddDays(-1), null, null, null, null, null)
        }
      );

      var tMachines = new[]
      {
        new TMachineDetail
        {
          Name = expectedResult.MachineStatuses[0].MachineName,
          ID = expectedResult.MachineStatuses[0].AssetId,
          IsJohnDoeMachine = false
        },
        new TMachineDetail
        {
          Name = expectedResult.MachineStatuses[1].MachineName,
          ID = expectedResult.MachineStatuses[1].AssetId,
          IsJohnDoeMachine = false
        },
        new TMachineDetail
        {
          Name = expectedResult.MachineStatuses[2].MachineName,
          ID = expectedResult.MachineStatuses[2].AssetId,
          IsJohnDoeMachine = false
        },
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetMachineIDs(projectIds.ProjectId.Value))
        .Returns(tMachines);

      var assets = new List<KeyValuePair<Guid, long>>()
      {
        new KeyValuePair<Guid, long>(assetUid1Good, assetId1Good),
        new KeyValuePair<Guid, long>(assetUid3Good, assetId3Good)
      };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(false);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(3, result.MachineStatuses.Count, "Wrong machine count");
      Assert.AreEqual(assetId1Good, result.MachineStatuses[0].AssetId, "Wrong machine Id (0)");
      Assert.AreEqual(assetUid1Expected, result.MachineStatuses[0].AssetUid, "Wrong assetUid (0)");
      Assert.AreEqual(assetId2Invalid, result.MachineStatuses[1].AssetId, "Wrong machine Id (1)");
      Assert.AreEqual(assetUid2Expected, result.MachineStatuses[1].AssetUid, "Wrong assetUid (1)");
      Assert.AreEqual(assetId3Good, result.MachineStatuses[2].AssetId, "Wrong machine Id (2)");
      Assert.AreEqual(assetUid3Expected, result.MachineStatuses[2].AssetUid, "Wrong assetUid (2)");
    }
#endif

  }
}
