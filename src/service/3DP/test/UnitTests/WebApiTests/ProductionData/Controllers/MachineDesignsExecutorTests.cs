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
    private const int NULL_RAPTOR_MACHINE_DESIGN_ID = -1;

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
      var projectIds = new ProjectIDs(99, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;

      // for GetAssetOnDesignPeriodsExecutor
      var expectedGetAssetOnDesignPeriodsExecutorResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, -1, DateTime.UtcNow.AddDays(-50),
            DateTime.UtcNow.AddDays(-40), assetUid)
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedGetAssetOnDesignPeriodsExecutorResult);
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(true);

      // for GetMachineIdsExecutor
      var expectedMachineExecutionResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(-1, "MachineName2", false, "designName",
            14, DateTime.UtcNow.AddDays(-1), null, null, null, null, assetUid)
        }
      );
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedMachineExecutionResult);

      // for GetMachineIdsExecutor
      var assetMatches = new List<KeyValuePair<Guid, long>>() {new KeyValuePair<Guid, long>(assetUid, assetId)};
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(true);

      // GetAssetOnDesignPeriodsExecutor will call GetMachineIdsExecutor
      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].AssetUid, result.AssetOnDesignPeriods[0].AssetUid,
        "Wrong asset Uid");
      Assert.AreEqual(assetId, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(assetMatches[0].Value, result.AssetOnDesignPeriods[0].MachineId,
        "Wrong legacyAssetId");
      Assert.AreEqual(expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].StartDate, result.AssetOnDesignPeriods[0].StartDate,
        "Wrong StartDate");
      Assert.AreEqual(NULL_RAPTOR_MACHINE_DESIGN_ID, result.AssetOnDesignPeriods[0].OnMachineDesignId,"Wrong onMachineDesignId");
      Assert.AreEqual(expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignName, result.AssetOnDesignPeriods[0].OnMachineDesignName, "Wrong onMachineDesignName");

      // there's some funny giggery-pokery in the executor to join up any dis-jointed periods
      Assert.IsTrue(result.AssetOnDesignPeriods[0].EndDate > expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].EndDate,
        "Wrong EndDate");
    }

    [TestMethod]
    public async Task GetAssetOnDesignPeriodsExecutor_TRex_Success_2designs()
    {
      var projectIds = new ProjectIDs(99, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;

      // for GetAssetOnDesignPeriodsExecutor
      var expectedGetAssetOnDesignPeriodsExecutorResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, -1, DateTime.UtcNow.AddDays(-50),
            DateTime.UtcNow.AddDays(-40), assetUid),
          new AssetOnDesignPeriod("The NameOf Design2", NULL_RAPTOR_MACHINE_DESIGN_ID, -1, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-1),
            assetUid)
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedGetAssetOnDesignPeriodsExecutorResult);
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(true);

      // for GetMachineIdsExecutor
      var expectedMachineExecutionResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(-1, "MachineName2", false, "designName",
            14, DateTime.UtcNow.AddDays(-1), null, null, null, null, assetUid)
        }
      );
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedMachineExecutionResult);

      // for GetMachineIdsExecutor
      var assetMatches = new List<KeyValuePair<Guid, long>>() {new KeyValuePair<Guid, long>(assetUid, assetId)};
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(true);

      // GetAssetOnDesignPeriodsExecutor will call GetMachineIdsExecutor
      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(2, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].AssetUid, result.AssetOnDesignPeriods[0].AssetUid,
        "Wrong asset Uid");
      Assert.AreEqual(assetMatches[0].Value, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(assetId, result.AssetOnDesignPeriods[0].MachineId,
        "Wrong legacyAssetId");
      Assert.AreEqual(NULL_RAPTOR_MACHINE_DESIGN_ID, result.AssetOnDesignPeriods[0].OnMachineDesignId, "Wrong DesignId");
      Assert.AreEqual(expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignName, result.AssetOnDesignPeriods[0].OnMachineDesignName, "Wrong DesignName");
      Assert.AreEqual(expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].StartDate, result.AssetOnDesignPeriods[0].StartDate,
        "Wrong StartDate");
      Assert.AreEqual(expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[1].StartDate, result.AssetOnDesignPeriods[0].EndDate,
        "Wrong EndDate");

      Assert.AreEqual(expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[1].AssetUid, result.AssetOnDesignPeriods[1].AssetUid,
        "Wrong asset Uid");
      Assert.AreEqual(assetMatches[0].Value, result.AssetOnDesignPeriods[1].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(assetId, result.AssetOnDesignPeriods[1].MachineId,
        "Wrong legacyAssetId");
      Assert.AreEqual(NULL_RAPTOR_MACHINE_DESIGN_ID, result.AssetOnDesignPeriods[1].OnMachineDesignId, "Wrong DesignId");
      Assert.AreEqual(expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[1].OnMachineDesignName, result.AssetOnDesignPeriods[1].OnMachineDesignName, "Wrong DesignName");
      Assert.AreEqual(expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[1].StartDate, result.AssetOnDesignPeriods[1].StartDate,
        "Wrong StartDate");
      // the final design change is considered current
      Assert.IsTrue(result.AssetOnDesignPeriods[1].EndDate > expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[1].EndDate,
        "Wrong EndDate");
    }

    [TestMethod]
    public async Task GetAssetOnDesignPeriodsExecutor_TRex_MultiAssetUid()
    {
      var projectIds = new ProjectIDs(99, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid1Good = Guid.NewGuid();
      var assetId1Good = 777;
      var assetId1Expected = assetId1Good;
      var assetUid2Good = Guid.NewGuid();
      var assetId2Invalid = 0;
      var assetId2Expected = -1;
      var assetUid3Good = Guid.NewGuid();
      var assetId3Expected = -1;

      // for GetAssetOnDesignPeriodsExecutor
      var expectedGetAssetOnDesignPeriodsExecutorResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, -1, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-4),
            assetUid1Good),
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, -1, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-2),
            assetUid2Good),
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, -1, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1),
            assetUid3Good),
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, -1, DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-9), assetUid1Good),
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, -1, DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-9), assetUid2Good),
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, -1, DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-9), assetUid3Good)
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedGetAssetOnDesignPeriodsExecutorResult);
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(true);

      // for GetMachineIdsExecutor
      var expectedMachineExecutionResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(-1, "MachineName1", false, "designName",
            14, DateTime.UtcNow.AddDays(-1), null, null, null, null, assetUid1Good),
          new MachineStatus(-1, "MachineName2", false, "designName",
            14, DateTime.UtcNow.AddDays(-1), null, null, null, null, assetUid2Good),
          new MachineStatus(-1, "MachineName3", false, "designName",
            14, DateTime.UtcNow.AddDays(-1), null, null, null, null, assetUid3Good)
        }
      );
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedMachineExecutionResult);

      // for GetMachineIdsExecutor
      var assetMatches = new List<KeyValuePair<Guid, long>>()
      {
        new KeyValuePair<Guid, long>(assetUid1Good, assetId1Good),
        new KeyValuePair<Guid, long>(assetUid2Good, assetId2Invalid)
      };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(true);

      // GetAssetOnDesignPeriodsExecutor will call GetMachineIdsExecutor
      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(6, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(assetUid1Good, result.AssetOnDesignPeriods[0].AssetUid, "Wrong asset1 Uid (0)");
      Assert.AreEqual(assetId1Expected, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId1 (0)");
      Assert.AreEqual(NULL_RAPTOR_MACHINE_DESIGN_ID, result.AssetOnDesignPeriods[0].OnMachineDesignId, "Wrong onMachineDesignId (0)");
      Assert.AreEqual(expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignName, result.AssetOnDesignPeriods[0].OnMachineDesignName, "Wrong onMachineDesignName (0)");
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


#if RAPTOR
    [TestMethod]
    public async Task GetAssetOnDesignPeriodsExecutor_TRexJohnDoe_Success()
    {
      var projectIds = new ProjectIDs(99, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;

      // for GetAssetOnDesignPeriodsExecutor
      var expectedGetAssetOnDesignPeriodsExecutorResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, -1, DateTime.UtcNow.AddDays(-50),
            DateTime.UtcNow.AddDays(-40), assetUid)
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedGetAssetOnDesignPeriodsExecutorResult);
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(true);

      // for GetMachineIdsExecutor
      var expectedMachineExecutionResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(-1, "MachineName2", true, "designName",
            14, DateTime.UtcNow.AddDays(-1), null, null, null, null, assetUid)
        }
      );
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedMachineExecutionResult);

      // for GetMachineIdsExecutor
      // for GetMachineIdsExecutor, raptor call to assetMatch on JohnDoe assets
      var tMachines = new[]
      {
        new TMachineDetail
        {
          Name = expectedMachineExecutionResult.MachineStatuses[0].MachineName,
          ID = assetId,
          IsJohnDoeMachine = true
        }
      };
      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetMachineIDs(projectIds.ProjectId))
        .Returns(tMachines);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(true);

      // GetAssetOnDesignPeriodsExecutor will call GetMachineIdsExecutor
      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, configStore: configStore.Object,
          raptorClient: raptorClient.Object, trexCompactionDataProxy: tRexProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].AssetUid, result.AssetOnDesignPeriods[0].AssetUid,
        "Wrong asset Uid");
      Assert.AreEqual(assetId, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].StartDate, result.AssetOnDesignPeriods[0].StartDate,
        "Wrong StartDate");
      Assert.AreEqual(NULL_RAPTOR_MACHINE_DESIGN_ID, result.AssetOnDesignPeriods[0].OnMachineDesignId, "Wrong onMachineDesignId");
      Assert.AreEqual(expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignName, result.AssetOnDesignPeriods[0].OnMachineDesignName, "Wrong onMachineDesignName");

      // there's some funny giggery-pokery in the executor to join up any dis-jointed periods
      Assert.IsTrue(result.AssetOnDesignPeriods[0].EndDate > expectedGetAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].EndDate,
        "Wrong EndDate");
    }

    [TestMethod]
    public async Task GetAssetOnDesignPeriodsExecutor_Raptor_Success()
    {
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;
      var designId = 888;

      // GetAssetOnDesignPeriodsExecutor
      var expectedAssetOnDesignPeriodsExecutorResult = new MachineDesignsExecutionResult
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
          FID = (int) expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignId,
          FName = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignName,
          FStartDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].StartDate,
          FEndDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].EndDate,
          FMachineID = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].MachineId
        }
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetOnMachineDesignEvents(projectIds.ProjectId))
        .Returns(tMachineDesigns);
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(false);

      // for GetMachineIdsExecutor
      var tMachines = new[]
      {
        new TMachineDetail { Name = "blahblah", ID = assetId, IsJohnDoeMachine = false }
      };
      raptorClient
        .Setup(x => x.GetMachineIDs(projectIds.ProjectId))
        .Returns(tMachines);

      // for GetMachineIdsExecutor
      var assetMatches = new List<KeyValuePair<Guid, long>>() {new KeyValuePair<Guid, long>(assetUid, assetId)};
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);
     
      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(assetMatches[0].Key, result.AssetOnDesignPeriods[0].AssetUid, "Wrong assetUid");
      Assert.AreEqual(expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].MachineId, result.AssetOnDesignPeriods[0].MachineId,
        "Wrong legacyAssetId");
      Assert.AreEqual(expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignId, result.AssetOnDesignPeriods[0].OnMachineDesignId, "Wrong DesignId");
      Assert.AreEqual(expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignName, result.AssetOnDesignPeriods[0].OnMachineDesignName, "Wrong DesignName");
      Assert.AreEqual(expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].StartDate, result.AssetOnDesignPeriods[0].StartDate,
        "Wrong StartDate");

      // the final design change is considered current
      Assert.IsTrue(result.AssetOnDesignPeriods[0].EndDate > expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].EndDate,
        "Wrong EndDate");
    }

    [TestMethod]
    public async Task GetAssetOnDesignPeriodsExecutor_Raptor_MultiAssetUid()
    {
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
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

      // GetAssetOnDesignPeriodsExecutor
      var expectedAssetOnDesignPeriodsExecutorResult = new MachineDesignsExecutionResult
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
          FID = (int) expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignId,
          FName = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignName,
          FStartDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].StartDate,
          FEndDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].EndDate,
          FMachineID = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].MachineId
        },
        new TDesignName
        {
          FID = (int) expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[1].OnMachineDesignId,
          FName = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[1].OnMachineDesignName,
          FStartDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[1].StartDate,
          FEndDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[1].EndDate,
          FMachineID = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[1].MachineId
        },
        new TDesignName
        {
          FID = (int) expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[2].OnMachineDesignId,
          FName = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[2].OnMachineDesignName,
          FStartDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[2].StartDate,
          FEndDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[2].EndDate,
          FMachineID = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[2].MachineId
        },
        new TDesignName
        {
          FID = (int) expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[3].OnMachineDesignId,
          FName = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[3].OnMachineDesignName,
          FStartDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[3].StartDate,
          FEndDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[3].EndDate,
          FMachineID = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[3].MachineId
        },
        new TDesignName
        {
          FID = (int) expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[4].OnMachineDesignId,
          FName = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[4].OnMachineDesignName,
          FStartDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[4].StartDate,
          FEndDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[4].EndDate,
          FMachineID = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[4].MachineId
        },
        new TDesignName
        {
          FID = (int) expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[5].OnMachineDesignId,
          FName = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[5].OnMachineDesignName,
          FStartDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[5].StartDate,
          FEndDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[5].EndDate,
          FMachineID = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[5].MachineId
        }
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetOnMachineDesignEvents(projectIds.ProjectId))
        .Returns(tMachineDesigns);

      // for GetMachineIdsExecutor
      var tMachines = new[]
      {
        new TMachineDetail { Name = "blahblah1", ID = assetId1Good, IsJohnDoeMachine = false },
        new TMachineDetail { Name = "blahblah2", ID = assetId3Good, IsJohnDoeMachine = false }
      };
      raptorClient
        .Setup(x => x.GetMachineIDs(projectIds.ProjectId))
        .Returns(tMachines);
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(false);

      // for GetMachineIdsExecutor
      var assetMatches = new List<KeyValuePair<Guid, long>>()
      {
        new KeyValuePair<Guid, long>(assetUid1Good, assetId1Good),
        new KeyValuePair<Guid, long>(assetUid3Good, assetId3Good)
      };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(6, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(assetUid1Expected, result.AssetOnDesignPeriods[0].AssetUid, "Wrong asset1 Uid (0)");
      Assert.AreEqual(assetId1Good, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId1 (0)");
      Assert.AreEqual(expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignId, result.AssetOnDesignPeriods[0].OnMachineDesignId, "Wrong onMachineDesignId (0)");
      Assert.AreEqual(expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignName, result.AssetOnDesignPeriods[0].OnMachineDesignName, "Wrong onMachineDesignName (0)");
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

    [TestMethod]
    public async Task GetAssetOnDesignPeriodsExecutor_RaptorJohnDoe_Success()
    {
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;
      var designId = 888;

      // GetAssetOnDesignPeriodsExecutor
      var expectedAssetOnDesignPeriodsExecutorResult = new MachineDesignsExecutionResult
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
          FID = (int) expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignId,
          FName = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignName,
          FStartDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].StartDate,
          FEndDate = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].EndDate,
          FMachineID = expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].MachineId
        }
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetOnMachineDesignEvents(projectIds.ProjectId))
        .Returns(tMachineDesigns);
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(false);

      // for GetMachineIdsExecutor
      var tMachines = new[]
      {
        new TMachineDetail { Name = "MachineName2", ID = assetId, IsJohnDoeMachine = true }
      };
      raptorClient
        .Setup(x => x.GetMachineIDs(projectIds.ProjectId))
        .Returns(tMachines);

      // GetMachineIdsExecutor trex call to assetMatch on JohnDoe assets
      var expectedTRexResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(-1, "MachineName2", true, "designName",
            14, DateTime.UtcNow.AddDays(-1), null, null, null, null, assetUid)
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedTRexResult);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(false);

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(expectedTRexResult.MachineStatuses[0].AssetUid, result.AssetOnDesignPeriods[0].AssetUid, "Wrong assetUid");
      Assert.AreEqual(expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].MachineId, result.AssetOnDesignPeriods[0].MachineId,
        "Wrong legacyAssetId");
      Assert.AreEqual(expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignId, result.AssetOnDesignPeriods[0].OnMachineDesignId, "Wrong DesignId");
      Assert.AreEqual(expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].OnMachineDesignName, result.AssetOnDesignPeriods[0].OnMachineDesignName, "Wrong DesignName");
      Assert.AreEqual(expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].StartDate, result.AssetOnDesignPeriods[0].StartDate,
        "Wrong StartDate");

      // the final design change is considered current
      Assert.IsTrue(result.AssetOnDesignPeriods[0].EndDate > expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].EndDate,
        "Wrong EndDate");
    }
#endif

  }
}
