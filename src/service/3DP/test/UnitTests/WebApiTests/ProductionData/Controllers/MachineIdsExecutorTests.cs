using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.ProductionData;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
#if RAPTOR
using VLPDDecls;
#endif

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class MachineIdsExecutorTests : MachineIdsBase
  {
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      Init();
    }

    [TestMethod]
    [DataRow(345, "87e6bd66-54d8-4651-8907-88b15d81b2d7")]
    public void ProjectIDsRequest_Success(int projectId, string projectUid)
    {
      var request = new ProjectIDs(projectId, Guid.Parse(projectUid));
      request.Validate();
    }

    [TestMethod]
    [DataRow(-1, "87e6bd66-54d8-4651-8907-88b15d81b2d7", "ProjectId must be provided")]
    [DataRow(0, "87e6bd66-54d8-4651-8907-88b15d81b2d7", "ProjectId must be provided")]
    [DataRow(1, "00000000-0000-0000-0000-000000000000", "ProjectUid must be provided")]
    public void ProjectIDsRequest_Invalid(int projectId, string projectUid, string errorMessage)
    {
      var request = new ProjectIDs(projectId, Guid.Parse(projectUid));
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.IsNotNull(ex, "should be exception");
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code, "Invalid HttpStatusCode.");
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code, "Invalid executionState.");
      Assert.AreEqual(errorMessage, ex.GetResult.Message, "Invalid error message.");
    }

    [TestMethod]
    public async Task MachineIdsExecutor_TRex_Success()
    {
      var projectIds = new ProjectIDs(1, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = NULL_ASSETID;
      var machineName = "MachineName";
      var isJohnDoe = false;
 
      GetTRexMachineIdsMock(new List<MachineStatus>(1) { new MachineStatus(NULL_ASSETID, machineName, isJohnDoe, assetUid: assetUid) }, 
        projectIds.ProjectUid, configStore, true, false, tRexProxy);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, 
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.MachineStatuses.Count, "Wrong asset count");
      Assert.AreEqual(assetUid, result.MachineStatuses[0].AssetUid, "Wrong asset Uid");
      Assert.AreEqual(assetId, result.MachineStatuses[0].AssetId, "Wrong asset Id");
      Assert.AreEqual(machineName, result.MachineStatuses[0].MachineName, "Wrong machine name");
    }

#if RAPTOR
    [TestMethod]
    public async Task MachineIdsExecutor_TRexJohnDoe_Success()
    {
      var projectIds = new ProjectIDs(1, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = NULL_ASSETID;
      var machineName = "MachineName";
      var isJohnDoe = true;

      GetTRexMachineIdsMock(new List<MachineStatus>(1) { new MachineStatus(NULL_ASSETID, machineName, isJohnDoe, assetUid: assetUid) },
        projectIds.ProjectUid, configStore, true, false, tRexProxy);

      // to assetMatch on VSS assets
      var assetMatches = new List<KeyValuePair<Guid, long>> { new KeyValuePair<Guid, long>(assetUid, assetId) };
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);

      // raptor call to assetMatch on JohnDoe assets
      GetRaptorMachineIdsMock(new[] { new TMachineDetail { Name = machineName, ID = assetId, IsJohnDoeMachine = isJohnDoe } },
        projectIds.ProjectId, raptorClient);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object, raptorClient: raptorClient.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.MachineStatuses.Count, "Wrong asset count");
      Assert.AreEqual(assetUid, result.MachineStatuses[0].AssetUid, "Wrong asset Uid");
      Assert.AreEqual(assetId, result.MachineStatuses[0].AssetId, "Wrong asset Id");
      Assert.AreEqual(machineName, result.MachineStatuses[0].MachineName, "Wrong machine name");
    }

    [TestMethod]
    public async Task MachineIdsExecutor_TRexJohnDoe_UnableToResolve()
    {
      var projectIds = new ProjectIDs(1, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      //var assetId = 777;
      var machineName = "MachineName";
      var isJohnDoe = true;

      GetTRexMachineIdsMock(new List<MachineStatus>(1) { new MachineStatus(NULL_ASSETID, machineName, isJohnDoe, assetUid: assetUid) },
        projectIds.ProjectUid, configStore, true, false, tRexProxy);

      // raptor call to assetMatch on JohnDoe assets
      GetRaptorMachineIdsMock(new TMachineDetail[0], projectIds.ProjectId, raptorClient);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, raptorClient: raptorClient.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.MachineStatuses.Count, "Wrong asset count");
      Assert.AreEqual(assetUid, result.MachineStatuses[0].AssetUid, "Wrong asset Uid");
      Assert.AreEqual(NULL_ASSETID, result.MachineStatuses[0].AssetId, "Wrong asset Id");
      Assert.AreEqual(machineName, result.MachineStatuses[0].MachineName, "Wrong machine name");
    }

    [TestMethod]
    public async Task MachineIdsExecutor_TRexJohnDoeMix_Success()
    {
      var projectIds = new ProjectIDs(1, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var johnDoeAssetId0 = 111;
      var johnDoeAssetId1 = 333;
      var nonJohnDoeAssetId2 = 555;

      var machines = new List<MachineStatus>(3)
      {
        new MachineStatus(NULL_ASSETID, "MachineName2", true, assetUid: Guid.NewGuid()),
        new MachineStatus(NULL_ASSETID, "machine name1", true, assetUid: Guid.NewGuid()),
        new MachineStatus(NULL_ASSETID, "other Machine name", false, assetUid: Guid.NewGuid())
      };
      GetTRexMachineIdsMock(machines, projectIds.ProjectUid, configStore, true, false, tRexProxy);

      // to assetMatch on VSS assets
      var assetMatches = new List<KeyValuePair<Guid, long>>
                         {
        new KeyValuePair<Guid, long>(machines[2].AssetUid.Value, nonJohnDoeAssetId2) ,
        new KeyValuePair<Guid, long>(Guid.NewGuid(), 111)
      };
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);

      // raptor call to assetMatch on JohnDoe assets
      var tMachines = new[]
      {
        new TMachineDetail { Name = machines[1].MachineName, ID = johnDoeAssetId1, IsJohnDoeMachine = true },
        new TMachineDetail { Name = machines[0].MachineName.ToLower(), ID = johnDoeAssetId0, IsJohnDoeMachine = true },
        new TMachineDetail { Name = "blahblah", ID = 44, IsJohnDoeMachine = true },
        new TMachineDetail { Name = machines[0].MachineName, ID = johnDoeAssetId0, IsJohnDoeMachine = false }
      };
      GetRaptorMachineIdsMock(tMachines, projectIds.ProjectId, raptorClient);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object, raptorClient: raptorClient.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(3, result.MachineStatuses.Count, "Wrong machine count");
      Assert.AreEqual(machines[0].AssetUid, result.MachineStatuses[0].AssetUid, "Wrong machine Uid for machine1");
      Assert.AreEqual(machines[0].MachineName, result.MachineStatuses[0].MachineName, "Wrong machine name for machine1");
      Assert.AreEqual(johnDoeAssetId0, result.MachineStatuses[0].AssetId, "Wrong legacyAssetId for machine1");
      Assert.AreEqual(machines[1].AssetUid, result.MachineStatuses[1].AssetUid, "Wrong machine Uid for machine2");
      Assert.AreEqual(machines[1].MachineName, result.MachineStatuses[1].MachineName, "Wrong machine name for machine2");
      Assert.AreEqual(johnDoeAssetId1, result.MachineStatuses[1].AssetId, "Wrong legacyAssetId for machine2");
      Assert.AreEqual(machines[2].AssetUid, result.MachineStatuses[2].AssetUid, "Wrong machine Uid for machine3");
      Assert.AreEqual(machines[2].MachineName, result.MachineStatuses[2].MachineName, "Wrong machine name for machine3");
      Assert.AreEqual(nonJohnDoeAssetId2, result.MachineStatuses[2].AssetId, "Wrong legacyAssetId for machine3");
    }
#endif

    [TestMethod]
    public async Task MachineIdsExecutor_TRex_MultiAssetUid()
    {
      var projectIds = new ProjectIDs(1, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid1Good = Guid.NewGuid();
      var assetId1Good = NULL_ASSETID;
      var assetId1Expected = assetId1Good;
      var assetUid2Good = Guid.NewGuid();
      var assetId2Invalid = 0;
      var assetId2Expected = NULL_ASSETID;
      var assetUid3Good = Guid.NewGuid();
      var assetId3Expected = NULL_ASSETID;

      var machines = new List<MachineStatus>(3)
        {
          new MachineStatus(NULL_ASSETID, "MachineName2", false, assetUid: assetUid1Good),
          new MachineStatus(NULL_ASSETID, "MachineName3", false, assetUid: assetUid2Good),
          new MachineStatus(NULL_ASSETID, "MachineName4", false, assetUid: assetUid3Good)
        };
      GetTRexMachineIdsMock(machines, projectIds.ProjectUid, configStore, true, false, tRexProxy);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, 
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
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetId = 777;
      var assetUid = Guid.NewGuid();
      var machineName = "MachineName2";
      var isJohnDoe = false;

      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(false);
      
      GetRaptorMachineIdsMock(new[] { new TMachineDetail { Name = machineName, ID = assetId, IsJohnDoeMachine = isJohnDoe } },
        projectIds.ProjectId, raptorClient);

      var assetMatches = new List<KeyValuePair<Guid, long>> { new KeyValuePair<Guid, long>(assetUid, assetId) };
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          assetResolverProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.MachineStatuses.Count, "Wrong machine count");
      Assert.AreEqual(assetId, result.MachineStatuses[0].AssetId, "Wrong machine Id");
      Assert.AreEqual(assetUid, result.MachineStatuses[0].AssetUid, "Wrong assetUid");
      Assert.AreEqual(machineName, result.MachineStatuses[0].MachineName, "Wrong machine name");
    }

    [TestMethod]
    public async Task MachineIdsExecutor_RaptorJohnDoe_Success()
    {
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetId = 777;
      var assetUid = Guid.NewGuid();
      var machineName = "MachineName";
      var isJohnDoe = true;

      GetRaptorMachineIdsMock(new[] { new TMachineDetail { Name = machineName, ID = assetId, IsJohnDoeMachine = isJohnDoe } },
        projectIds.ProjectId, raptorClient);

      // trex call to assetMatch on JohnDoe assets
      GetTRexMachineIdsMock(new List<MachineStatus>(1) { new MachineStatus(NULL_ASSETID, machineName, isJohnDoe, assetUid: assetUid) },
        projectIds.ProjectUid, configStore, false, true, tRexProxy);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.MachineStatuses.Count, "Wrong machine count");
      Assert.AreEqual(assetId, result.MachineStatuses[0].AssetId, "Wrong machine Id");
      Assert.AreEqual(assetUid, result.MachineStatuses[0].AssetUid, "Wrong assetUid");
      Assert.AreEqual(machineName, result.MachineStatuses[0].MachineName, "Wrong machine name");
    }

    [TestMethod]
    public async Task MachineIdsExecutor_RaptorJohnDoe_UnableToResolve()
    {
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetId = 777;
      var machineName = "MachineName2";
      var isJohnDoe = true;

      GetRaptorMachineIdsMock(new[] { new TMachineDetail { Name = machineName, ID = assetId, IsJohnDoeMachine = isJohnDoe } },
        projectIds.ProjectId, raptorClient);

      // trex call to assetMatch on JohnDoe assets
      GetTRexMachineIdsMock(new List<MachineStatus>(0), projectIds.ProjectUid, configStore, false, false, tRexProxy);
      
      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.MachineStatuses.Count, "Wrong machine count");
      Assert.AreEqual(assetId, result.MachineStatuses[0].AssetId, "Wrong machine Id");
      Assert.IsNull(result.MachineStatuses[0].AssetUid, "Wrong assetUid");
      Assert.AreEqual(machineName, result.MachineStatuses[0].MachineName, "Wrong machine name");
    }

    [TestMethod]
    public async Task MachineIdsExecutor_RaptorJohnDoe_TRexCallUnavailable()
    {
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetId = 777;
      var machineName = "MachineName2";
      var isJohnDoe = true;

      GetRaptorMachineIdsMock(new[] { new TMachineDetail { Name = machineName, ID = assetId, IsJohnDoeMachine = isJohnDoe } },
        projectIds.ProjectId, raptorClient);

      // trex call to assetMatch on JohnDoe assets - trex throws exception
      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError));
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, string>>()))
         .ThrowsAsync(exception);
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(false);
      configStore.Setup(x => x.GetValueBool("TREX_IS_AVAILABLE")).Returns(true);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(projectIds));
    }

    [TestMethod]
    public async Task MachineIdsExecutor_Raptor_MultiAsset()
    {
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid1Good = Guid.NewGuid();
      long assetId1Good = 777;
      var assetUid1Expected = assetUid1Good;
      var machineName1 = "MachineName1";
      long assetId2Invalid = 0; // johnDoe?
      Guid? assetUid2ExpectedNull = null;
      var machineName2 = "MachineName2";
      var assetUid3Good = Guid.NewGuid();
      long assetId3Good = 888;
      var assetUid3Expected = assetUid3Good;
      var machineName3 = "MachineName3";

      var tMachines = new[]
      {
        new TMachineDetail { Name = machineName1, ID = assetId1Good, IsJohnDoeMachine = false },
        new TMachineDetail { Name = machineName2, ID = assetId2Invalid, IsJohnDoeMachine = false },
        new TMachineDetail { Name = machineName3, ID = assetId3Good, IsJohnDoeMachine = false }
      };
      GetRaptorMachineIdsMock(tMachines, projectIds.ProjectId, raptorClient);

      var assetMatches = new List<KeyValuePair<Guid, long>>
                         {
        new KeyValuePair<Guid, long>(assetUid1Good, assetId1Good),
        new KeyValuePair<Guid, long>(assetUid3Good, assetId3Good)
      };
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);

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
      Assert.AreEqual(assetUid2ExpectedNull, result.MachineStatuses[1].AssetUid, "Wrong assetUid (1)");
      Assert.AreEqual(assetId3Good, result.MachineStatuses[2].AssetId, "Wrong machine Id (2)");
      Assert.AreEqual(assetUid3Expected, result.MachineStatuses[2].AssetUid, "Wrong assetUid (2)");
    }
#endif

  }
}
