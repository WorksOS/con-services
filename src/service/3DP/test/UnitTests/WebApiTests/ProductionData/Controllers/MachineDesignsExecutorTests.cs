using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
  public class MachineDesignsExecutorTests : MachineIdsBase
  {
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      Init();
    }

    [TestMethod]
    public async Task GetAssetOnDesignPeriodsExecutor_TRex_Success()
    {
      var projectIds = new ProjectIDs(99, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;
      var machineName = "MachineName2";
      var isJohnDoe = false;

      // for GetAssetOnDesignPeriodsExecutor
      var expectedGetAssetOnDesignPeriodsExecutorResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, NULL_ASSETID, DateTime.UtcNow.AddDays(-50),
            DateTime.UtcNow.AddDays(-40), assetUid)
        }
      );
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedGetAssetOnDesignPeriodsExecutorResult);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(true);

      // for GetMachineIdsExecutor
      GetTRexMachineIdsMock(new List<MachineStatus>(1) { new MachineStatus(NULL_ASSETID, machineName, isJohnDoe, assetUid: assetUid) },
        projectIds.ProjectUid, configStore, true, false, tRexProxy);

      // for GetMachineIdsExecutor
      var assetMatches = new List<KeyValuePair<Guid, long>>() {new KeyValuePair<Guid, long>(assetUid, assetId)};
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);

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
      var machineName = "MachineName2";
      var isJohnDoe = false;

      // for GetAssetOnDesignPeriodsExecutor
      var expectedGetAssetOnDesignPeriodsExecutorResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, NULL_ASSETID, DateTime.UtcNow.AddDays(-50),
            DateTime.UtcNow.AddDays(-40), assetUid),
          new AssetOnDesignPeriod("The NameOf Design2", NULL_RAPTOR_MACHINE_DESIGN_ID, NULL_ASSETID, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-1),
            assetUid)
        }
      );
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedGetAssetOnDesignPeriodsExecutorResult);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(true);

      // for GetMachineIdsExecutor
      GetTRexMachineIdsMock(new List<MachineStatus>(1) { new MachineStatus(NULL_ASSETID, machineName, isJohnDoe, assetUid: assetUid) },
        projectIds.ProjectUid, configStore, true, false, tRexProxy);

      // for GetMachineIdsExecutor
      var assetMatches = new List<KeyValuePair<Guid, long>>() {new KeyValuePair<Guid, long>(assetUid, assetId)};
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);

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
      var assetId2Expected = NULL_ASSETID;
      var assetUid3Good = Guid.NewGuid();
      var assetId3Expected = NULL_ASSETID;

      // for GetAssetOnDesignPeriodsExecutor
      var expectedGetAssetOnDesignPeriodsExecutorResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, NULL_ASSETID, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-4),
            assetUid1Good),
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, NULL_ASSETID, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-2),
            assetUid2Good),
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, NULL_ASSETID, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1),
            assetUid3Good),
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, NULL_ASSETID, DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-9), assetUid1Good),
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, NULL_ASSETID, DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-9), assetUid2Good),
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, NULL_ASSETID, DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-9), assetUid3Good)
        }
      );
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedGetAssetOnDesignPeriodsExecutorResult);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(true);

      // for GetMachineIdsExecutor
      var machines = new List<MachineStatus>(3)
      {
        new MachineStatus(NULL_ASSETID, "MachineName1", false, assetUid: assetUid1Good),
        new MachineStatus(NULL_ASSETID, "MachineName2", false, assetUid: assetUid2Good),
        new MachineStatus(NULL_ASSETID, "MachineName3", false, assetUid: assetUid3Good)
      };
      GetTRexMachineIdsMock(machines, projectIds.ProjectUid, configStore, true, false, tRexProxy);

      // for GetMachineIdsExecutor
      var assetMatches = new List<KeyValuePair<Guid, long>>()
      {
        new KeyValuePair<Guid, long>(assetUid1Good, assetId1Good),
        new KeyValuePair<Guid, long>(assetUid2Good, assetId2Invalid)
      };
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);

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
      var machineName = "MachineName2";
      var isJohnDoe = true;

      // for GetAssetOnDesignPeriodsExecutor
      var expectedGetAssetOnDesignPeriodsExecutorResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design1", NULL_RAPTOR_MACHINE_DESIGN_ID, NULL_ASSETID, DateTime.UtcNow.AddDays(-50),
            DateTime.UtcNow.AddDays(-40), assetUid)
        }
      );
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedGetAssetOnDesignPeriodsExecutorResult);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(true);

      // for GetMachineIdsExecutor
      GetTRexMachineIdsMock(new List<MachineStatus>(1) { new MachineStatus(NULL_ASSETID, machineName, isJohnDoe, assetUid: assetUid) },
        projectIds.ProjectUid, configStore, true, false, tRexProxy);

      // for GetMachineIdsExecutor, raptor call to assetMatch on JohnDoe assets
      GetRaptorMachineIdsMock(new [] { new TMachineDetail{Name = machineName, ID = assetId, IsJohnDoeMachine = isJohnDoe}},
        projectIds.ProjectId, raptorClient);

      // GetAssetOnDesignPeriodsExecutor will call GetMachineIdsExecutor
      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, configStore: configStore.Object,
          raptorClient: raptorClient.Object, trexCompactionDataProxy: tRexProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(assetUid, result.AssetOnDesignPeriods[0].AssetUid,
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
      var machineName = "MachineName2";
      var isJohnDoe = false;

      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(false);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(false);
      configStore.Setup(x => x.GetValueBool("TREX_IS_AVAILABLE")).Returns(false);

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

      raptorClient
        .Setup(x => x.GetOnMachineDesignEvents(projectIds.ProjectId))
        .Returns(tMachineDesigns);

      // for GetMachineIdsExecutor
      GetRaptorMachineIdsMock(new[] { new TMachineDetail { Name = machineName, ID = assetId, IsJohnDoeMachine = isJohnDoe } },
        projectIds.ProjectId, raptorClient);

      // for GetMachineIdsExecutor
      var assetMatches = new List<KeyValuePair<Guid, long>>() {new KeyValuePair<Guid, long>(assetUid, assetId)};
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

      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(false);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(false);
      configStore.Setup(x => x.GetValueBool("TREX_IS_AVAILABLE")).Returns(false);

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

      raptorClient
        .Setup(x => x.GetOnMachineDesignEvents(projectIds.ProjectId))
        .Returns(tMachineDesigns);

      // for GetMachineIdsExecutor
      var tMachines = new[]
      {
        new TMachineDetail { Name = "blahblah1", ID = assetId1Good, IsJohnDoeMachine = false },
        new TMachineDetail { Name = "blahblah2", ID = assetId3Good, IsJohnDoeMachine = false }
      };
      GetRaptorMachineIdsMock(tMachines, projectIds.ProjectId, raptorClient);

      // for GetMachineIdsExecutor
      var assetMatches = new List<KeyValuePair<Guid, long>>()
      {
        new KeyValuePair<Guid, long>(assetUid1Good, assetId1Good),
        new KeyValuePair<Guid, long>(assetUid3Good, assetId3Good)
      };
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
      var machineName = "MachineName2";
      var isJohnDoe = true;
      var designId = 888;
      var machineDesignName = "The machine design name";

      // GetAssetOnDesignPeriodsExecutor
      var expectedAssetOnDesignPeriodsExecutorResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod(machineDesignName, designId, assetId, DateTime.UtcNow.AddDays(-5),
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

      raptorClient
        .Setup(x => x.GetOnMachineDesignEvents(projectIds.ProjectId))
        .Returns(tMachineDesigns);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns(false);

      // for GetMachineIdsExecutor
      GetRaptorMachineIdsMock(new[] { new TMachineDetail { Name = machineName, ID = assetId, IsJohnDoeMachine = isJohnDoe } },
        projectIds.ProjectId, raptorClient);

      // GetMachineIdsExecutor trex call to assetMatch on JohnDoe assets
      GetTRexMachineIdsMock(new List<MachineStatus>(1) { new MachineStatus(NULL_ASSETID, machineName, isJohnDoe, assetUid: assetUid) },
        projectIds.ProjectUid, configStore, false, true, tRexProxy);

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(assetUid, result.AssetOnDesignPeriods[0].AssetUid, "Wrong assetUid");
      Assert.AreEqual(assetId, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(designId, result.AssetOnDesignPeriods[0].OnMachineDesignId, "Wrong DesignId");
      Assert.AreEqual(machineDesignName, result.AssetOnDesignPeriods[0].OnMachineDesignName, "Wrong DesignName");
      Assert.AreEqual(expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].StartDate, result.AssetOnDesignPeriods[0].StartDate,
        "Wrong StartDate");

      // the final design change is considered current
      Assert.IsTrue(result.AssetOnDesignPeriods[0].EndDate > expectedAssetOnDesignPeriodsExecutorResult.AssetOnDesignPeriods[0].EndDate,
        "Wrong EndDate");
    }
#endif

  }
}
