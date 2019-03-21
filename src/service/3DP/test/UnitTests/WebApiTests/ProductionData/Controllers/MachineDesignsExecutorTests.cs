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
  public class MachineDesignsExecutorTests
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
    public async Task GetAssetOnDesignPeriodsExecutor_TRex_Success()
    {
      var projectIds = new ProjectID() {ProjectUid = Guid.NewGuid()};
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;
      var expectedResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design1", 1, -1, DateTime.UtcNow.AddDays(-50),
            DateTime.UtcNow.AddDays(-40), assetUid)
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedResult);

      var assets = new List<KeyValuePair<Guid, long>>() {new KeyValuePair<Guid, long>(assetUid, assetId)};
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].AssetUid, result.AssetOnDesignPeriods[0].AssetUid,
        "Wrong asset Uid");
      Assert.AreEqual(assets[0].Value, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].MachineId, result.AssetOnDesignPeriods[0].MachineId,
        "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].StartDate, result.AssetOnDesignPeriods[0].StartDate,
        "Wrong StartDate");

      // there's some funny giggery-pokery in the executor to join up any dis-jointed periods
      Assert.IsTrue(result.AssetOnDesignPeriods[0].EndDate > expectedResult.AssetOnDesignPeriods[0].EndDate,
        "Wrong EndDate");
    }

    [TestMethod]
    public async Task GetAssetOnDesignPeriodsExecutor_TRex_Success_2designs()
    {
      var projectIds = new ProjectID() {ProjectUid = Guid.NewGuid()};
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;
      var expectedResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design1", 1, -1, DateTime.UtcNow.AddDays(-50),
            DateTime.UtcNow.AddDays(-40), assetUid),
          new AssetOnDesignPeriod("The NameOf Design2", 2, -1, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-1),
            assetUid)
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedResult);

      var assets = new List<KeyValuePair<Guid, long>>() {new KeyValuePair<Guid, long>(assetUid, assetId)};
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(2, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].AssetUid, result.AssetOnDesignPeriods[0].AssetUid,
        "Wrong asset Uid");
      Assert.AreEqual(assets[0].Value, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].MachineId, result.AssetOnDesignPeriods[0].MachineId,
        "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].Id, result.AssetOnDesignPeriods[0].Id, "Wrong DesignId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].StartDate, result.AssetOnDesignPeriods[0].StartDate,
        "Wrong StartDate");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[1].StartDate, result.AssetOnDesignPeriods[0].EndDate,
        "Wrong EndDate");

      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[1].AssetUid, result.AssetOnDesignPeriods[1].AssetUid,
        "Wrong asset Uid");
      Assert.AreEqual(assets[0].Value, result.AssetOnDesignPeriods[1].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[1].MachineId, result.AssetOnDesignPeriods[1].MachineId,
        "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[1].Id, result.AssetOnDesignPeriods[1].Id, "Wrong DesignId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[1].StartDate, result.AssetOnDesignPeriods[1].StartDate,
        "Wrong StartDate");
      // the final design change is considered current
      Assert.IsTrue(result.AssetOnDesignPeriods[1].EndDate > expectedResult.AssetOnDesignPeriods[1].EndDate,
        "Wrong EndDate");
    }

    [TestMethod]
    public async Task GetAssetOnDesignPeriodsExecutor_TRex_MultiAssetUid()
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
      var expectedResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design1", 1, -1, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-4),
            assetUid1Good),
          new AssetOnDesignPeriod("The NameOf Design1", 1, -1, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-2),
            assetUid2Good),
          new AssetOnDesignPeriod("The NameOf Design1", 1, -1, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1),
            assetUid3Good),
          new AssetOnDesignPeriod("The NameOf Design1", 1, -1, DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-9), assetUid1Good),
          new AssetOnDesignPeriod("The NameOf Design1", 1, -1, DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-9), assetUid2Good),
          new AssetOnDesignPeriod("The NameOf Design1", 1, -1, DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-9), assetUid3Good)
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(),
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
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(6, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(assetUid1Good, result.AssetOnDesignPeriods[0].AssetUid, "Wrong asset1 Uid (0)");
      Assert.AreEqual(assetId1Expected, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId1 (0)");
      Assert.AreEqual(assetUid1Good, result.AssetOnDesignPeriods[1].AssetUid, "Wrong asset1 Uid (1)");
      Assert.AreEqual(assetId1Expected, result.AssetOnDesignPeriods[1].MachineId, "Wrong legacyAssetId1 (1)");

      Assert.AreEqual(assetUid2Good, result.AssetOnDesignPeriods[2].AssetUid, "Wrong asset2 Uid (2)");
      Assert.AreEqual(assetId2Expected, result.AssetOnDesignPeriods[2].MachineId, "Wrong legacyAssetId2 (2)");
      Assert.AreEqual(assetUid2Good, result.AssetOnDesignPeriods[3].AssetUid, "Wrong asset2 Uid (3)");
      Assert.AreEqual(assetId2Expected, result.AssetOnDesignPeriods[3].MachineId, "Wrong legacyAssetId2 (3)");

      Assert.AreEqual(assetUid3Good, result.AssetOnDesignPeriods[4].AssetUid, "Wrong asset3 Uid (4)");
      Assert.AreEqual(assetId3Expected, result.AssetOnDesignPeriods[4].MachineId, "Wrong legacyAssetId3 (4)");
      Assert.AreEqual(assetUid3Good, result.AssetOnDesignPeriods[5].AssetUid, "Wrong asset3 Uid (5)");
      Assert.AreEqual(assetId3Expected, result.AssetOnDesignPeriods[5].MachineId, "Wrong legacyAssetId3 (5)");
    }


    [TestMethod]
    public void GetAssetOnDesignPeriodsExecutor_TRex_NoProjectUid()
    {
      var projectIds = new ProjectID();

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);

      var ex = Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(projectIds))
        .Result;
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual("Failed to get/update data requested by GetAssetOnDesignPeriodsExecutor", ex.GetResult.Message);
    }

#if RAPTOR
    [TestMethod]
    public async Task GetAssetOnDesignPeriodsExecutor_Raptor_Success()
    {
      var projectIds = new ProjectID() {ProjectId = 999};
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;
      var designId = 888;
      var expectedResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design", designId, assetId, DateTime.UtcNow.AddDays(-5),
            DateTime.UtcNow.AddDays(-1), null)
        }
      );

      var tMachineDesigns = new[]
      {
        new TDesignName
        {
          FID = (int) expectedResult.AssetOnDesignPeriods[0].Id,
          FName = expectedResult.AssetOnDesignPeriods[0].Name,
          FStartDate = expectedResult.AssetOnDesignPeriods[0].StartDate,
          FEndDate = expectedResult.AssetOnDesignPeriods[0].EndDate,
          FMachineID = expectedResult.AssetOnDesignPeriods[0].MachineId
        }
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetOnMachineDesignEvents(projectIds.ProjectId.Value))
        .Returns(tMachineDesigns);

      var assets = new List<KeyValuePair<Guid, long>>() {new KeyValuePair<Guid, long>(assetUid, assetId)};
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns("false");

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(assets[0].Key, result.AssetOnDesignPeriods[0].AssetUid, "Wrong assetUid");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].MachineId, result.AssetOnDesignPeriods[0].MachineId,
        "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].Id, result.AssetOnDesignPeriods[0].Id, "Wrong DesignId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].StartDate, result.AssetOnDesignPeriods[0].StartDate,
        "Wrong StartDate");

      // the final design change is considered current
      Assert.IsTrue(result.AssetOnDesignPeriods[0].EndDate > expectedResult.AssetOnDesignPeriods[0].EndDate,
        "Wrong EndDate");
    }

    [TestMethod]
    public async Task GetAssetOnDesignPeriodsExecutor_Raptor_MultiAssetUid()
    {
      var projectIds = new ProjectID() {ProjectId = 999};
      var customerUid = Guid.NewGuid();
      var assetUid1Good = Guid.NewGuid();
      long assetId1Good = 777;
      var assetUid1Expected = assetUid1Good;
      long assetId2Invalid = 0; // johnDoe?
      Guid? assetUid2Expected = null;
      var assetUid3Good = Guid.NewGuid();
      long assetId3Good = 888;
      var assetUid3Expected = assetUid3Good;
      var designId = 888;
      var expectedResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design1", designId, assetId1Good, DateTime.UtcNow.AddDays(-5),
            DateTime.UtcNow.AddDays(-4), null),
          new AssetOnDesignPeriod("The NameOf Design1", designId, assetId2Invalid, DateTime.UtcNow.AddDays(-3),
            DateTime.UtcNow.AddDays(-2), null),
          new AssetOnDesignPeriod("The NameOf Design1", designId, assetId3Good, DateTime.UtcNow.AddDays(-2),
            DateTime.UtcNow.AddDays(-1), null),
          new AssetOnDesignPeriod("The NameOf Design1", designId, assetId1Good, DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-9), null),
          new AssetOnDesignPeriod("The NameOf Design1", designId, assetId2Invalid, DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-9), null),
          new AssetOnDesignPeriod("The NameOf Design1", designId, assetId3Good, DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-9), null)
        }
      );

      var tMachineDesigns = new[]
      {
        new TDesignName
        {
          FID = (int) expectedResult.AssetOnDesignPeriods[0].Id,
          FName = expectedResult.AssetOnDesignPeriods[0].Name,
          FStartDate = expectedResult.AssetOnDesignPeriods[0].StartDate,
          FEndDate = expectedResult.AssetOnDesignPeriods[0].EndDate,
          FMachineID = expectedResult.AssetOnDesignPeriods[0].MachineId
        },
        new TDesignName
        {
          FID = (int) expectedResult.AssetOnDesignPeriods[1].Id,
          FName = expectedResult.AssetOnDesignPeriods[1].Name,
          FStartDate = expectedResult.AssetOnDesignPeriods[1].StartDate,
          FEndDate = expectedResult.AssetOnDesignPeriods[1].EndDate,
          FMachineID = expectedResult.AssetOnDesignPeriods[1].MachineId
        },
        new TDesignName
        {
          FID = (int) expectedResult.AssetOnDesignPeriods[2].Id,
          FName = expectedResult.AssetOnDesignPeriods[2].Name,
          FStartDate = expectedResult.AssetOnDesignPeriods[2].StartDate,
          FEndDate = expectedResult.AssetOnDesignPeriods[2].EndDate,
          FMachineID = expectedResult.AssetOnDesignPeriods[2].MachineId
        },
        new TDesignName
        {
          FID = (int) expectedResult.AssetOnDesignPeriods[3].Id,
          FName = expectedResult.AssetOnDesignPeriods[3].Name,
          FStartDate = expectedResult.AssetOnDesignPeriods[3].StartDate,
          FEndDate = expectedResult.AssetOnDesignPeriods[3].EndDate,
          FMachineID = expectedResult.AssetOnDesignPeriods[3].MachineId
        },
        new TDesignName
        {
          FID = (int) expectedResult.AssetOnDesignPeriods[4].Id,
          FName = expectedResult.AssetOnDesignPeriods[4].Name,
          FStartDate = expectedResult.AssetOnDesignPeriods[4].StartDate,
          FEndDate = expectedResult.AssetOnDesignPeriods[4].EndDate,
          FMachineID = expectedResult.AssetOnDesignPeriods[4].MachineId
        },
        new TDesignName
        {
          FID = (int) expectedResult.AssetOnDesignPeriods[5].Id,
          FName = expectedResult.AssetOnDesignPeriods[5].Name,
          FStartDate = expectedResult.AssetOnDesignPeriods[5].StartDate,
          FEndDate = expectedResult.AssetOnDesignPeriods[5].EndDate,
          FMachineID = expectedResult.AssetOnDesignPeriods[5].MachineId
        }
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetOnMachineDesignEvents(projectIds.ProjectId.Value))
        .Returns(tMachineDesigns);

      var assets = new List<KeyValuePair<Guid, long>>()
      {
        new KeyValuePair<Guid, long>(assetUid1Good, assetId1Good),
        new KeyValuePair<Guid, long>(assetUid3Good, assetId3Good)
      };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns("false");

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(6, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(assetUid1Expected, result.AssetOnDesignPeriods[0].AssetUid, "Wrong asset1 Uid (0)");
      Assert.AreEqual(assetId1Good, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId1 (0)");
      Assert.AreEqual(assetUid1Expected, result.AssetOnDesignPeriods[1].AssetUid, "Wrong asset1 Uid (1)");
      Assert.AreEqual(assetId1Good, result.AssetOnDesignPeriods[1].MachineId, "Wrong legacyAssetId1 (1)");

      Assert.AreEqual(assetUid2Expected, result.AssetOnDesignPeriods[2].AssetUid, "Wrong asset2 Uid (2)");
      Assert.AreEqual(assetId2Invalid, result.AssetOnDesignPeriods[2].MachineId, "Wrong legacyAssetId2 (2)");
      Assert.AreEqual(assetUid2Expected, result.AssetOnDesignPeriods[3].AssetUid, "Wrong asset2 Uid (3)");
      Assert.AreEqual(assetId2Invalid, result.AssetOnDesignPeriods[3].MachineId, "Wrong legacyAssetId2 (3)");

      Assert.AreEqual(assetUid3Expected, result.AssetOnDesignPeriods[4].AssetUid, "Wrong asset3 Uid (4)");
      Assert.AreEqual(assetId3Good, result.AssetOnDesignPeriods[4].MachineId, "Wrong legacyAssetId3 (4)");
      Assert.AreEqual(assetUid3Expected, result.AssetOnDesignPeriods[5].AssetUid, "Wrong asset3 Uid (5)");
      Assert.AreEqual(assetId3Good, result.AssetOnDesignPeriods[5].MachineId, "Wrong legacyAssetId3 (5)");
    }
#endif

  }
}
