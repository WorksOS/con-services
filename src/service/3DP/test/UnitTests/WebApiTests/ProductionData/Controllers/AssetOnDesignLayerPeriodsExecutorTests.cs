using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.ProductionData;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
#if RAPTOR
using VLPDDecls;
#endif

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class AssetOnDesignLayerPeriodsExecutorTests : MachineIdsBase
  {
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      Init();
    }

    [TestMethod]
    public async Task GetAssetOnDesignLayerPeriodsExecutor_TRex_Success()
    {
      var projectIds = new ProjectIDs(777, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = NULL_ASSETID;
      long layerId = 444;
      var machineDesignName = "The machine design name";
      var machineName = "MachineName2";
      var isJohnDoe = false;

      // for GetAssetOnDesignLayerPeriodsExecutor
      var expectedAssetOnDesignLayerPeriodsExecutionResult = new AssetOnDesignLayerPeriodsExecutionResult
      (
        new List<AssetOnDesignLayerPeriod>(1)
        {
          new AssetOnDesignLayerPeriod(NULL_ASSETID, NULL_RAPTOR_MACHINE_DESIGN_ID, layerId, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-40), assetUid, machineDesignName)
        }
      );
      tRexProxy.Setup(x => x.SendDataGetRequest<AssetOnDesignLayerPeriodsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedAssetOnDesignLayerPeriodsExecutionResult);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_LAYERS")).Returns(true);

      // for GetMachineIdsExecutor
      GetTRexMachineIdsMock(new List<MachineStatus>(1) { new MachineStatus(NULL_ASSETID, machineName, isJohnDoe, assetUid: assetUid) },
        projectIds.ProjectUid, configStore, true, false, tRexProxy);

      // GetAssetOnDesignLayerPeriodsExecutor will call GetMachineIdsExecutor
      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignLayerPeriodsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, 
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as AssetOnDesignLayerPeriodsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignLayerPeriods.Count, "Wrong layer count");
      Assert.AreEqual(assetUid, result.AssetOnDesignLayerPeriods[0].AssetUid, "Wrong machine Uid");
      Assert.AreEqual(assetId, result.AssetOnDesignLayerPeriods[0].AssetId, "Wrong legacyAssetId");
      Assert.AreEqual(NULL_RAPTOR_MACHINE_DESIGN_ID, result.AssetOnDesignLayerPeriods[0].OnMachineDesignId, "Wrong design OnMachineDesignId");
      Assert.AreEqual(machineDesignName, result.AssetOnDesignLayerPeriods[0].OnMachineDesignName, "Wrong design OnMachineDesignName");

      Assert.AreEqual(layerId, result.AssetOnDesignLayerPeriods[0].LayerId, "Wrong layer id");
    }
    
    [TestMethod]
    public async Task GetAssetOnDesignLayerPeriodsExecutor_TRex_MultiAssetUid()
    {
      var projectIds = new ProjectIDs(777, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid1Good = Guid.NewGuid();
      var assetId1Good = NULL_ASSETID;
      var assetId1Expected = assetId1Good;
      var assetUid2Good = Guid.NewGuid();
      var assetId2Invalid = 0;
      var assetId2Expected = NULL_ASSETID;
      var assetUid3Good = Guid.NewGuid();
      var assetId3Expected = NULL_ASSETID;
      long layerId = 444;
      var machineDesignName = "The machine Design name";
      
      // for GetAssetOnDesignLayerPeriodsExecutor
      var expectedAssetOnDesignLayerPeriodsExecutionResult = new AssetOnDesignLayerPeriodsExecutionResult
      (
        new List<AssetOnDesignLayerPeriod>(1)
        {
          new AssetOnDesignLayerPeriod(NULL_ASSETID, NULL_RAPTOR_MACHINE_DESIGN_ID, layerId, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-49), assetUid1Good, machineDesignName),
          new AssetOnDesignLayerPeriod(NULL_ASSETID, NULL_RAPTOR_MACHINE_DESIGN_ID, layerId, DateTime.UtcNow.AddDays(-48), DateTime.UtcNow.AddDays(-47), assetUid2Good, machineDesignName),
          new AssetOnDesignLayerPeriod(NULL_ASSETID, NULL_RAPTOR_MACHINE_DESIGN_ID, layerId, DateTime.UtcNow.AddDays(-46), DateTime.UtcNow.AddDays(-45), assetUid3Good, machineDesignName),
          new AssetOnDesignLayerPeriod(NULL_ASSETID, NULL_RAPTOR_MACHINE_DESIGN_ID, layerId, DateTime.UtcNow.AddDays(-44), DateTime.UtcNow.AddDays(-43), assetUid1Good, machineDesignName),
          new AssetOnDesignLayerPeriod(NULL_ASSETID, NULL_RAPTOR_MACHINE_DESIGN_ID, layerId, DateTime.UtcNow.AddDays(-42), DateTime.UtcNow.AddDays(-41), assetUid2Good, machineDesignName),
          new AssetOnDesignLayerPeriod(NULL_ASSETID, NULL_RAPTOR_MACHINE_DESIGN_ID, layerId, DateTime.UtcNow.AddDays(-40), DateTime.UtcNow.AddDays(-38), assetUid3Good, machineDesignName)
        }
      );
      tRexProxy.Setup(x => x.SendDataGetRequest<AssetOnDesignLayerPeriodsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedAssetOnDesignLayerPeriodsExecutionResult);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_LAYERS")).Returns(true);

      // for GetMachineIdsExecutor
      var machines = new List<MachineStatus>(3)
        {
          new MachineStatus(NULL_ASSETID, "MachineName0", false, assetUid: assetUid1Good),
          new MachineStatus(NULL_ASSETID, "MachineName1", false, assetUid: assetUid2Good),
          new MachineStatus(NULL_ASSETID, "MachineName2", false, assetUid: assetUid3Good)
        };
      GetTRexMachineIdsMock(machines, projectIds.ProjectUid, configStore, true, false, tRexProxy);
      
      // GetAssetOnDesignLayerPeriodsExecutor will call GetMachineIdsExecutor
      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignLayerPeriodsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, 
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as AssetOnDesignLayerPeriodsExecutionResult;
      Assert.AreEqual(6, result.AssetOnDesignLayerPeriods.Count, "Wrong design count");
      Assert.AreEqual(assetUid1Good, result.AssetOnDesignLayerPeriods[0].AssetUid, "Wrong asset1 Uid (0)");
      Assert.AreEqual(assetId1Expected, result.AssetOnDesignLayerPeriods[0].AssetId, "Wrong legacyAssetId1 (0)");
      Assert.AreEqual(layerId, result.AssetOnDesignLayerPeriods[0].LayerId, "Wrong layerid (0)");
      Assert.AreEqual(machineDesignName, result.AssetOnDesignLayerPeriods[0].OnMachineDesignName, "Wrong design OnMachineDesignName");
      Assert.AreEqual(NULL_RAPTOR_MACHINE_DESIGN_ID, result.AssetOnDesignLayerPeriods[0].OnMachineDesignId, "Wrong layer OnMachineDesignId");

      Assert.AreEqual(assetUid2Good, result.AssetOnDesignLayerPeriods[1].AssetUid, "Wrong asset1 Uid (1)");
      Assert.AreEqual(assetId2Expected, result.AssetOnDesignLayerPeriods[1].AssetId, "Wrong legacyAssetId1 (1)");
      Assert.AreEqual(layerId, result.AssetOnDesignLayerPeriods[1].LayerId, "Wrong layerid (1)");
      Assert.AreEqual(machineDesignName, result.AssetOnDesignLayerPeriods[1].OnMachineDesignName, "Wrong design OnMachineDesignName (1)");
      Assert.AreEqual(result.AssetOnDesignLayerPeriods[1].OnMachineDesignId, result.AssetOnDesignLayerPeriods[1].OnMachineDesignId, "Wrong layer OnMachineDesignId (1)");
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
    public async Task GetAssetOnDesignLayerPeriodsExecutor_TRexJohnDoe_Success()
    {
      var projectIds = new ProjectIDs(777, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = NULL_ASSETID;
      long layerId = 444;
      var machineDesignName = "The machine design name";
      var machineName = "MachineName2";
      var isJohnDoe = true;

      // for GetAssetOnDesignLayerPeriodsExecutor
      var expectedAssetOnDesignLayerPeriodsExecutionResult = new AssetOnDesignLayerPeriodsExecutionResult
      (
        new List<AssetOnDesignLayerPeriod>(1)
        {
          new AssetOnDesignLayerPeriod(NULL_ASSETID, NULL_RAPTOR_MACHINE_DESIGN_ID, layerId, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-40), assetUid, machineDesignName)
        }
      );
      tRexProxy.Setup(x => x.SendDataGetRequest<AssetOnDesignLayerPeriodsExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedAssetOnDesignLayerPeriodsExecutionResult);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_LAYERS")).Returns(true);

      // for GetMachineIdsExecutor
      GetTRexMachineIdsMock(new List<MachineStatus>(1) { new MachineStatus(NULL_ASSETID, machineName, isJohnDoe, assetUid: assetUid) },
        projectIds.ProjectUid, configStore, true, false, tRexProxy);

      // for GetMachineIdsExecutor, raptor call to assetMatch on JohnDoe assets
      GetRaptorMachineIdsMock(new[] { new TMachineDetail { Name = machineName, ID = assetId, IsJohnDoeMachine = isJohnDoe } },
        projectIds.ProjectId, raptorClient);

      // GetAssetOnDesignLayerPeriodsExecutor will call GetMachineIdsExecutor
      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignLayerPeriodsExecutor>(logger, configStore: configStore.Object,
          raptorClient: raptorClient.Object, trexCompactionDataProxy: tRexProxy.Object, 
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as AssetOnDesignLayerPeriodsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignLayerPeriods.Count, "Wrong layer count");
      Assert.AreEqual(assetUid, result.AssetOnDesignLayerPeriods[0].AssetUid, "Wrong machine Uid");
      Assert.AreEqual(assetId, result.AssetOnDesignLayerPeriods[0].AssetId, "Wrong legacyAssetId");
      Assert.AreEqual(NULL_RAPTOR_MACHINE_DESIGN_ID, result.AssetOnDesignLayerPeriods[0].OnMachineDesignId, "Wrong design OnMachineDesignId");
      Assert.AreEqual(machineDesignName, result.AssetOnDesignLayerPeriods[0].OnMachineDesignName, "Wrong design OnMachineDesignName");

      Assert.AreEqual(layerId, result.AssetOnDesignLayerPeriods[0].LayerId, "Wrong layer id");
    }

    [TestMethod]
    public async Task GetAssetOnDesignLayerPeriodsExecutor_Raptor_Success()
    {
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetId = 777;
      var assetUid = Guid.NewGuid();
      var designId = 888;
      var layerId = 999;
      var machineName = "MachineName2";
      var isJohnDoe = false;

      // for GetAssetOnDesignLayerPeriodsExecutor
      var expectedAssetOnDesignLayerPeriodsExecutionResult = new AssetOnDesignLayerPeriodsExecutionResult
      (
        new List<AssetOnDesignLayerPeriod>(1)
        {
          new AssetOnDesignLayerPeriod(assetId, designId, layerId, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-40))
        }
      );

      var tLayers = new[]
      {
        new TDesignLayer
        {
          FAssetID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].AssetId,
          FDesignID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].OnMachineDesignId,
          FLayerID = (int) expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].LayerId,
          FStartTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].StartDate,
          FEndTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].EndDate
        }
      };

      raptorClient
        .Setup(x => x.GetOnMachineLayers(projectIds.ProjectId, out tLayers))
        .Returns(0);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_LAYERS")).Returns(false);

      // for GetMachineIdsExecutor
      GetRaptorMachineIdsMock(new[] { new TMachineDetail { Name = machineName, ID = assetId, IsJohnDoeMachine = isJohnDoe } },
        projectIds.ProjectId, raptorClient);

      // for GetMachineIdsExecutor
      var assetMatches = new List<KeyValuePair<Guid, long>> { new KeyValuePair<Guid, long>(assetUid, assetId) };
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(false);

      // GetAssetOnDesignLayerPeriodsExecutor will call GetMachineIdsExecutor
      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignLayerPeriodsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as AssetOnDesignLayerPeriodsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignLayerPeriods.Count, "Wrong layer count");
      Assert.AreEqual(expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].AssetId, result.AssetOnDesignLayerPeriods[0].AssetId, "Wrong machine OnMachineDesignId");
      Assert.AreEqual(assetMatches[0].Key, result.AssetOnDesignLayerPeriods[0].AssetUid, "Wrong assetUid");

      Assert.AreEqual(expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].OnMachineDesignId, result.AssetOnDesignLayerPeriods[0].OnMachineDesignId, "Wrong design OnMachineDesignId");
      Assert.IsNull(result.AssetOnDesignLayerPeriods[0].OnMachineDesignName, "Wrong design OnMachineDesignName");
      Assert.AreEqual(expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].LayerId, result.AssetOnDesignLayerPeriods[0].LayerId, "Wrong layer OnMachineDesignId");
      Assert.AreEqual(expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].StartDate, result.AssetOnDesignLayerPeriods[0].StartDate, "Wrong startDate");
      Assert.AreEqual(expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].EndDate, result.AssetOnDesignLayerPeriods[0].EndDate, "Wrong endDate");
    }

    [TestMethod]
    public async Task GetAssetOnDesignLayerPeriodsExecutor_Raptor_MultiAssetUid()
    {
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid1Good = Guid.NewGuid();
      long assetId1Good = 777;
      var machineName1 = "MachineName2";
      var isJohnDoe1 = false;
      var assetUid1Expected = assetUid1Good;
      long assetId2Invalid = 0; // johnDoe?
      var assetUid2Good = Guid.NewGuid();
      Guid? assetUid2Expected = null;
      Guid? assetUid3NotFound = null;
      long assetId3NotFound = 888;
      Guid? assetUid3Expected = null;
      var designId = 888;
      var layerId = 999;

      // for GetAssetOnDesignLayerPeriodsExecutor
      var expectedAssetOnDesignLayerPeriodsExecutionResult = new AssetOnDesignLayerPeriodsExecutionResult
      (
        new List<AssetOnDesignLayerPeriod>(1)
        {
          new AssetOnDesignLayerPeriod(assetId1Good, designId, layerId, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-49)),
          new AssetOnDesignLayerPeriod(assetId2Invalid, designId, layerId, DateTime.UtcNow.AddDays(-48), DateTime.UtcNow.AddDays(-47)),
          new AssetOnDesignLayerPeriod(assetId3NotFound, designId, layerId, DateTime.UtcNow.AddDays(-46), DateTime.UtcNow.AddDays(-45)),
          new AssetOnDesignLayerPeriod(assetId1Good, designId, layerId, DateTime.UtcNow.AddDays(-44), DateTime.UtcNow.AddDays(-43)),
          new AssetOnDesignLayerPeriod(assetId2Invalid, designId, layerId, DateTime.UtcNow.AddDays(-42), DateTime.UtcNow.AddDays(-41)),
          new AssetOnDesignLayerPeriod(assetId3NotFound, designId, layerId, DateTime.UtcNow.AddDays(-40), DateTime.UtcNow.AddDays(-38))
        }
      );

      var tLayers = new[]
      {
        new TDesignLayer
        {
          FAssetID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].AssetId,
          FDesignID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].OnMachineDesignId,
          FLayerID = (int) expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].LayerId,
          FStartTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].StartDate,
          FEndTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].EndDate
        },
        new TDesignLayer
        {
          FAssetID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[1].AssetId,
          FDesignID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[1].OnMachineDesignId,
          FLayerID = (int) expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[1].LayerId,
          FStartTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[1].StartDate,
          FEndTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[1].EndDate
        },
        new TDesignLayer
        {
          FAssetID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[2].AssetId,
          FDesignID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[2].OnMachineDesignId,
          FLayerID = (int) expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[2].LayerId,
          FStartTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[2].StartDate,
          FEndTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[2].EndDate
        },
        new TDesignLayer
        {
          FAssetID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[3].AssetId,
          FDesignID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[3].OnMachineDesignId,
          FLayerID = (int) expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[3].LayerId,
          FStartTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[3].StartDate,
          FEndTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[3].EndDate
        },
        new TDesignLayer
        {
          FAssetID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[4].AssetId,
          FDesignID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[4].OnMachineDesignId,
          FLayerID = (int) expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[4].LayerId,
          FStartTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[4].StartDate,
          FEndTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[4].EndDate
        },
        new TDesignLayer
        {
          FAssetID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[5].AssetId,
          FDesignID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[5].OnMachineDesignId,
          FLayerID = (int) expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[5].LayerId,
          FStartTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[5].StartDate,
          FEndTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[5].EndDate
        }
      };

      raptorClient
        .Setup(x => x.GetOnMachineLayers(projectIds.ProjectId, out tLayers))
        .Returns(0);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_LAYERS")).Returns(false);

      // for GetMachineIdsExecutor
      GetRaptorMachineIdsMock(new[] { new TMachineDetail { Name = machineName1, ID = assetId1Good, IsJohnDoeMachine = isJohnDoe1 } },
        projectIds.ProjectId, raptorClient);

      // for GetMachineIdsExecutor
      var assets = new List<KeyValuePair<Guid, long>>
                   {
        new KeyValuePair<Guid, long>(assetUid1Good, assetId1Good),
        new KeyValuePair<Guid, long>(assetUid2Good, assetId2Invalid)
      };
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assets);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(false);

      // GetAssetOnDesignLayerPeriodsExecutor will call GetMachineIdsExecutor
      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignLayerPeriodsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as AssetOnDesignLayerPeriodsExecutionResult;
      Assert.AreEqual(6, result.AssetOnDesignLayerPeriods.Count, "Wrong design count");
      Assert.AreEqual(assetUid1Expected, result.AssetOnDesignLayerPeriods[0].AssetUid, "Wrong asset1 Uid (0)");
      Assert.AreEqual(assetId1Good, result.AssetOnDesignLayerPeriods[0].AssetId, "Wrong legacyAssetId1 (0)");
      Assert.AreEqual(designId, result.AssetOnDesignLayerPeriods[0].OnMachineDesignId, "Wrong onMachineDesignID (0)");
      Assert.IsNull(result.AssetOnDesignLayerPeriods[0].OnMachineDesignName, "Wrong onMachineDesignName (0)");
      Assert.AreEqual(assetUid2Expected, result.AssetOnDesignLayerPeriods[1].AssetUid, "Wrong asset2 Uid (1)");
      Assert.AreEqual(assetId2Invalid, result.AssetOnDesignLayerPeriods[1].AssetId, "Wrong legacyAssetId2 (1)");
      Assert.AreEqual(designId, result.AssetOnDesignLayerPeriods[1].OnMachineDesignId, "Wrong onMachineDesignID (1)");
      Assert.IsNull(result.AssetOnDesignLayerPeriods[1].OnMachineDesignName, "Wrong onMachineDesignName (1)");
      Assert.AreEqual(assetUid3Expected, result.AssetOnDesignLayerPeriods[2].AssetUid, "Wrong asset3 Uid (2)");
      Assert.AreEqual(assetId3NotFound, result.AssetOnDesignLayerPeriods[2].AssetId, "Wrong legacyAssetId3 (2)");

      Assert.AreEqual(assetUid1Expected, result.AssetOnDesignLayerPeriods[3].AssetUid, "Wrong asset1 Uid (2)");
      Assert.AreEqual(assetId1Good, result.AssetOnDesignLayerPeriods[3].AssetId, "Wrong legacyAssetId1 (2)");
      Assert.AreEqual(assetUid2Expected, result.AssetOnDesignLayerPeriods[4].AssetUid, "Wrong asset2 Uid (3)");
      Assert.AreEqual(assetId2Invalid, result.AssetOnDesignLayerPeriods[4].AssetId, "Wrong legacyAssetId2 (3)");
      Assert.AreEqual(assetUid3Expected, result.AssetOnDesignLayerPeriods[5].AssetUid, "Wrong asset3 Uid (5)");
      Assert.AreEqual(assetId3NotFound, result.AssetOnDesignLayerPeriods[5].AssetId, "Wrong legacyAssetId3 (5)");
    }

    [TestMethod]
    public async Task GetAssetOnDesignLayerPeriodsExecutor_RaptorJohnDoe_Success()
    {
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetId = 777;
      var assetUid = Guid.NewGuid();
      var designId = 888;
      var layerId = 999;
      var machineName = "MachineName2";
      var isJohnDoe = true;

      // for GetAssetOnDesignLayerPeriodsExecutor
      var expectedAssetOnDesignLayerPeriodsExecutionResult = new AssetOnDesignLayerPeriodsExecutionResult
      (
        new List<AssetOnDesignLayerPeriod>(1)
        {
          new AssetOnDesignLayerPeriod(assetId, designId, layerId, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-40))
        }
      );

      var tLayers = new[]
      {
        new TDesignLayer
        {
          FAssetID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].AssetId,
          FDesignID = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].OnMachineDesignId,
          FLayerID = (int) expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].LayerId,
          FStartTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].StartDate,
          FEndTime = expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].EndDate
        }
      };

      raptorClient
        .Setup(x => x.GetOnMachineLayers(projectIds.ProjectId, out tLayers))
        .Returns(0);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_LAYERS")).Returns(false);

      // for GetMachineIdsExecutor
      GetRaptorMachineIdsMock(new[] { new TMachineDetail { Name = machineName, ID = assetId, IsJohnDoeMachine = isJohnDoe } },
        projectIds.ProjectId, raptorClient);

      // GetMachineIdsExecutor trex call to assetMatch on JohnDoe assets
      GetTRexMachineIdsMock(new List<MachineStatus>(1) { new MachineStatus(NULL_ASSETID, machineName, isJohnDoe, assetUid: assetUid) },
        projectIds.ProjectUid, configStore, false, true, tRexProxy);


      // GetAssetOnDesignLayerPeriodsExecutor will call GetMachineIdsExecutor
      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignLayerPeriodsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as AssetOnDesignLayerPeriodsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignLayerPeriods.Count, "Wrong layer count");
      Assert.AreEqual(expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].AssetId, result.AssetOnDesignLayerPeriods[0].AssetId, "Wrong machine OnMachineDesignId");
      Assert.AreEqual(assetUid, result.AssetOnDesignLayerPeriods[0].AssetUid, "Wrong assetUid");

      Assert.AreEqual(expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].OnMachineDesignId, result.AssetOnDesignLayerPeriods[0].OnMachineDesignId, "Wrong design OnMachineDesignId");
      Assert.IsNull(result.AssetOnDesignLayerPeriods[0].OnMachineDesignName, "Wrong design OnMachineDesignName");
      Assert.AreEqual(layerId, result.AssetOnDesignLayerPeriods[0].LayerId, "Wrong layer OnMachineDesignId");
      Assert.AreEqual(expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].StartDate, result.AssetOnDesignLayerPeriods[0].StartDate, "Wrong startDate");
      Assert.AreEqual(expectedAssetOnDesignLayerPeriodsExecutionResult.AssetOnDesignLayerPeriods[0].EndDate, result.AssetOnDesignLayerPeriods[0].EndDate, "Wrong endDate");
    }
#endif

  }
}
