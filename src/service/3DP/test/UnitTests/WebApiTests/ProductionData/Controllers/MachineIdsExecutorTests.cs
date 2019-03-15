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
    [Ignore] // todo resolve once Merino AssetService is available
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
          It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedResult);

      var assets = new List<AssetData>(1)
      {
        new AssetData(customerUid, assetUid, "Asset Name", assetId,
          "serial Number", "CAT", "D7", "Asset Type",
          "", 1, 1977)
      };

      var assetProxy = new Mock<IAssetServiceProxy>();
      assetProxy.Setup(x => x.GetAssetsV1( It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINES")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.MachineStatuses.Count, "Wrong machine count");
      Assert.AreEqual(expectedResult.MachineStatuses[0].AssetUid, result.MachineStatuses[0].AssetUid,
        "Wrong machine Uid");
      Assert.AreEqual(expectedResult.MachineStatuses[0].MachineName, result.MachineStatuses[0].MachineName,
        "Wrong machine name");
      Assert.AreEqual(assets[0].LegacyAssetID, result.MachineStatuses[0].AssetId,
        "Wrong legacyAssetId");
    }

    [TestMethod]
    public void MachineIdsExecutor_TRex_NoProjectUid()
    {
      var projectIds = new ProjectID();

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINES")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);

      var ex = Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(projectIds)).Result;
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual("Failed to get/update data requested by GetMachineIdsExecutor", ex.GetResult.Message);
    }

#if RAPTOR

    [TestMethod]
    [Ignore] // todo resolve once Merino AssetService is available
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
            14, DateTime.UtcNow.AddDays(-1), null, null, null, null, assetUid)
        }
      );

      TMachineDetail[] machines = new TMachineDetail[1]
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
        .Returns(machines);

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
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINES")).Returns("false");

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          assetProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.MachineStatuses.Count, "Wrong machine count");
      Assert.AreEqual(expectedResult.MachineStatuses[0].AssetId, result.MachineStatuses[0].AssetId, "Wrong machine Id");
      Assert.AreEqual(expectedResult.MachineStatuses[0].AssetUid, result.MachineStatuses[0].AssetUid,
        "Wrong machine Uid");
      Assert.AreEqual(expectedResult.MachineStatuses[0].MachineName, result.MachineStatuses[0].MachineName,
        "Wrong machine name");
      Assert.AreEqual(assets[0].LegacyAssetID, result.MachineStatuses[0].AssetId,
        "Wrong legacyAssetId");
    }
#endif

  }
}
