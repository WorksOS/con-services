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
using VSS.MasterData.Models.ResultHandling.Abstractions;
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

      var assetMatches = new List< KeyValuePair<Guid, long> >() { new KeyValuePair<Guid, long>(assetUid, assetId) };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);

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
      Assert.AreEqual(assetMatches[0].Value, result.MachineStatuses[0].AssetId,
        "Wrong legacyAssetId");
    }

#if RAPTOR
    [TestMethod]
    public async Task MachineIdsExecutor_TRexJohnDoe_Success()
    {
      var projectIds = new ProjectIDs(1, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;
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

      // to assetMatch on VSS assets
      var assetMatches = new List<KeyValuePair<Guid, long>>() { new KeyValuePair<Guid, long>(assetUid, assetId) };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);

      // raptor call to assetMatch on JohnDoe assets
      var tMachines = new[]
      {
        new TMachineDetail
        {
          Name = expectedTRexResult.MachineStatuses[0].MachineName,
          ID = assetId,
          IsJohnDoeMachine = true
        }
      };
      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetMachineIDs(projectIds.ProjectId))
        .Returns(tMachines);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(true);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object, raptorClient: raptorClient.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.MachineStatuses.Count, "Wrong machine count");
      Assert.AreEqual(expectedTRexResult.MachineStatuses[0].AssetUid, result.MachineStatuses[0].AssetUid,
        "Wrong machine Uid");
      Assert.AreEqual(expectedTRexResult.MachineStatuses[0].MachineName, result.MachineStatuses[0].MachineName,
        "Wrong machine name");
      Assert.AreEqual(assetId, result.MachineStatuses[0].AssetId,"Wrong legacyAssetId");
    }

    [TestMethod]
    public async Task MachineIdsExecutor_TRexJohnDoe_UnableToResolve()
    {
      var projectIds = new ProjectIDs(1, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetIdNoFound = -1;
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

      // raptor call to assetMatch on JohnDoe assets
      var tMachines = new TMachineDetail[0];
      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetMachineIDs(projectIds.ProjectId))
        .Returns(tMachines);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(true);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, raptorClient: raptorClient.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.MachineStatuses.Count, "Wrong machine count");
      Assert.AreEqual(expectedTRexResult.MachineStatuses[0].AssetUid, result.MachineStatuses[0].AssetUid,
        "Wrong machine Uid");
      Assert.AreEqual(expectedTRexResult.MachineStatuses[0].MachineName, result.MachineStatuses[0].MachineName,
        "Wrong machine name");
      Assert.AreEqual(assetIdNoFound, result.MachineStatuses[0].AssetId, "Wrong legacyAssetId");
    }

    [TestMethod]
    public async Task MachineIdsExecutor_TRexJohnDoeMix_Success()
    {
      var projectIds = new ProjectIDs(1, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var johnDoeAssetId0 = 111;
      var johnDoeAssetId1 = 333;
      var nonJohnDoeAssetId2 = 555;
      var expectedTRexResult = new MachineExecutionResult
      (
        new List<MachineStatus>(3)
        {
          new MachineStatus(-1, "MachineName2", true, "designName",
            10, DateTime.UtcNow.AddDays(-1), null, null, null, null, 
            Guid.NewGuid()),
          new MachineStatus(-1, "machine name1", true, "designName1",
            11, DateTime.UtcNow.AddDays(-1), null, null, null, null,
            Guid.NewGuid()),
          new MachineStatus(-1, "other Machine name", false, "designName2",
          12, DateTime.UtcNow.AddDays(-1), null, null, null, null,
          Guid.NewGuid())
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(),It.IsAny<IDictionary<string, string>>(),It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedTRexResult);

      // to assetMatch on VSS assets
      var assetMatches = new List<KeyValuePair<Guid, long>>()
      {
        new KeyValuePair<Guid, long>(expectedTRexResult.MachineStatuses[2].AssetUid.Value, nonJohnDoeAssetId2) ,
        new KeyValuePair<Guid, long>(Guid.NewGuid(), 111)
      };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);

      // raptor call to assetMatch on JohnDoe assets
      var tMachines = new[]
      {
        new TMachineDetail { Name = expectedTRexResult.MachineStatuses[1].MachineName, ID = johnDoeAssetId1, IsJohnDoeMachine = true },
        new TMachineDetail { Name = expectedTRexResult.MachineStatuses[0].MachineName.ToLower(), ID = johnDoeAssetId0, IsJohnDoeMachine = true },
        new TMachineDetail { Name = "blahblah", ID = 44, IsJohnDoeMachine = true },
        new TMachineDetail { Name = expectedTRexResult.MachineStatuses[0].MachineName, ID = johnDoeAssetId0, IsJohnDoeMachine = false }
      };
      var raptorClient = new Mock<IASNodeClient>();
      raptorClient.Setup(x => x.GetMachineIDs(projectIds.ProjectId)).Returns(tMachines);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(true);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetResolverProxy: assetProxy.Object, raptorClient: raptorClient.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(3, result.MachineStatuses.Count, "Wrong machine count");
      Assert.AreEqual(expectedTRexResult.MachineStatuses[0].AssetUid, result.MachineStatuses[0].AssetUid, "Wrong machine Uid for machine1");
      Assert.AreEqual(expectedTRexResult.MachineStatuses[0].MachineName, result.MachineStatuses[0].MachineName, "Wrong machine name for machine1");
      Assert.AreEqual(johnDoeAssetId0, result.MachineStatuses[0].AssetId, "Wrong legacyAssetId for machine1");
      Assert.AreEqual(expectedTRexResult.MachineStatuses[1].AssetUid, result.MachineStatuses[1].AssetUid, "Wrong machine Uid for machine2");
      Assert.AreEqual(expectedTRexResult.MachineStatuses[1].MachineName, result.MachineStatuses[1].MachineName, "Wrong machine name for machine2");
      Assert.AreEqual(johnDoeAssetId1, result.MachineStatuses[1].AssetId, "Wrong legacyAssetId for machine2");
      Assert.AreEqual(expectedTRexResult.MachineStatuses[2].AssetUid, result.MachineStatuses[2].AssetUid, "Wrong machine Uid for machine3");
      Assert.AreEqual(expectedTRexResult.MachineStatuses[2].MachineName, result.MachineStatuses[2].MachineName, "Wrong machine name for machine3");
      Assert.AreEqual(nonJohnDoeAssetId2, result.MachineStatuses[2].AssetId, "Wrong legacyAssetId for machine3");
    }
#endif

    [TestMethod]
    public async Task MachineIdsExecutor_TRex_MultiAssetUid()
    {
      var projectIds = new ProjectIDs(1, Guid.NewGuid());
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

      var assetMatches = new List<KeyValuePair<Guid, long>>()
      {
        new KeyValuePair<Guid, long>(assetUid1Good, assetId1Good),
        new KeyValuePair<Guid, long>(assetUid2Good, assetId2Invalid)
      };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<Guid>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);

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
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
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
        new TMachineDetail { Name = expectedResult.MachineStatuses[0].MachineName, ID = expectedResult.MachineStatuses[0].AssetId, IsJohnDoeMachine = false }
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetMachineIDs(projectIds.ProjectId))
        .Returns(tMachines);

      var assetMatches = new List<KeyValuePair<Guid, long>>() { new KeyValuePair<Guid, long>(assetUid, assetId) };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);

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
      Assert.AreEqual(assetMatches[0].Key, result.MachineStatuses[0].AssetUid, "Wrong assetUid");
    }

    [TestMethod]
    public async Task MachineIdsExecutor_RaptorJohnDoe_Success()
    {
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetId = 777;
      var assetUid = Guid.NewGuid();
      var expectedRaptorResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(assetId, "MachineName2", true, "designName",
            14, DateTime.UtcNow.AddDays(-1), null, null, null, null, null)
        }
      );

      var tMachines = new[]
      {
        new TMachineDetail { Name = expectedRaptorResult.MachineStatuses[0].MachineName, ID = expectedRaptorResult.MachineStatuses[0].AssetId, IsJohnDoeMachine = expectedRaptorResult.MachineStatuses[0].IsJohnDoe }
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetMachineIDs(projectIds.ProjectId))
        .Returns(tMachines);

      // trex call to assetMatch on JohnDoe assets
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

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(false);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.MachineStatuses.Count, "Wrong machine count");
      Assert.AreEqual(expectedRaptorResult.MachineStatuses[0].AssetId, result.MachineStatuses[0].AssetId, "Wrong machine Id");
      Assert.AreEqual(expectedRaptorResult.MachineStatuses[0].MachineName, result.MachineStatuses[0].MachineName,
        "Wrong machine name");
      Assert.AreEqual(expectedTRexResult.MachineStatuses[0].AssetUid, result.MachineStatuses[0].AssetUid, "Wrong assetUid");
    }

    [TestMethod]
    public async Task MachineIdsExecutor_RaptorJohnDoe_UnableToResolve()
    {
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetId = 777;
      var assetUid = Guid.NewGuid();
      var expectedRaptorResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(assetId, "MachineName2", true, "designName",
            14, DateTime.UtcNow.AddDays(-1), null, null, null, null, null)
        }
      );

      var tMachines = new[]
      {
        new TMachineDetail { Name = expectedRaptorResult.MachineStatuses[0].MachineName, ID = expectedRaptorResult.MachineStatuses[0].AssetId, IsJohnDoeMachine = expectedRaptorResult.MachineStatuses[0].IsJohnDoe }
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetMachineIDs(projectIds.ProjectId))
        .Returns(tMachines);

      // trex call to assetMatch on JohnDoe assets
      var expectedTRexResult = new MachineExecutionResult(new List<MachineStatus>());
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(),It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedTRexResult);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(false);

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineIdsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());
      var result = await executor.ProcessAsync(projectIds) as MachineExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.MachineStatuses.Count, "Wrong machine count");
      Assert.AreEqual(expectedRaptorResult.MachineStatuses[0].AssetId, result.MachineStatuses[0].AssetId, "Wrong machine Id");
      Assert.AreEqual(expectedRaptorResult.MachineStatuses[0].MachineName, result.MachineStatuses[0].MachineName,
        "Wrong machine name");
      Assert.IsNull(result.MachineStatuses[0].AssetUid, "Wrong assetUid");
    }

    [TestMethod]
    public async Task MachineIdsExecutor_RaptorJohnDoe_TRexCallUnavailable()
    {
      var projectIds = new ProjectIDs(999, Guid.NewGuid());
      var customerUid = Guid.NewGuid();
      var assetId = 777;
      var assetUid = Guid.NewGuid();
      var expectedRaptorResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(assetId, "MachineName2", true, "designName",
            14, DateTime.UtcNow.AddDays(-1), null, null, null, null, null)
        }
      );

      var tMachines = new[]
      {
        new TMachineDetail { Name = expectedRaptorResult.MachineStatuses[0].MachineName, ID = expectedRaptorResult.MachineStatuses[0].AssetId, IsJohnDoeMachine = expectedRaptorResult.MachineStatuses[0].IsJohnDoe }
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetMachineIDs(projectIds.ProjectId))
        .Returns(tMachines);

      // trex call to assetMatch on JohnDoe assets - trex throws exception
      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError));
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineExecutionResult>(projectIds.ProjectUid.ToString(),
          It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, string>>()))
         .ThrowsAsync(exception);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_MACHINES")).Returns(false);

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
      long assetId2Invalid = 0; // johnDoe?
      Guid? assetUid2ExpectedNull = null;
      var assetUid3Good = Guid.NewGuid();
      long assetId3Good = 888;
      var assetUid3Expected = assetUid3Good;
      var expectedResult = new MachineExecutionResult
      (
        new List<MachineStatus>(1)
        {
          new MachineStatus(assetId1Good, "MachineName0", false, "designName",14, DateTime.UtcNow.AddDays(-1), null, null, null, null, null),
          new MachineStatus(assetId2Invalid, "MachineName1", false, "designName",14, DateTime.UtcNow.AddDays(-1), null, null, null, null, null),
          new MachineStatus(assetId3Good, "MachineName2", false, "designName",14, DateTime.UtcNow.AddDays(-1), null, null, null, null, null)
        }
      );

      var tMachines = new[]
      {
        new TMachineDetail { Name = expectedResult.MachineStatuses[0].MachineName, ID = expectedResult.MachineStatuses[0].AssetId, IsJohnDoeMachine = false },
        new TMachineDetail { Name = expectedResult.MachineStatuses[1].MachineName, ID = expectedResult.MachineStatuses[1].AssetId, IsJohnDoeMachine = false },
        new TMachineDetail { Name = expectedResult.MachineStatuses[2].MachineName, ID = expectedResult.MachineStatuses[2].AssetId, IsJohnDoeMachine = false },
      };
      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetMachineIDs(projectIds.ProjectId))
        .Returns(tMachines);

      var assetMatches = new List<KeyValuePair<Guid, long>>()
      {
        new KeyValuePair<Guid, long>(assetUid1Good, assetId1Good),
        new KeyValuePair<Guid, long>(assetUid3Good, assetId3Good)
      };
      var assetProxy = new Mock<IAssetResolverProxy>();
      assetProxy.Setup(x => x.GetMatchingAssets(It.IsAny<List<long>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(assetMatches);

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
      Assert.AreEqual(assetUid2ExpectedNull, result.MachineStatuses[1].AssetUid, "Wrong assetUid (1)");
      Assert.AreEqual(assetId3Good, result.MachineStatuses[2].AssetId, "Wrong machine Id (2)");
      Assert.AreEqual(assetUid3Expected, result.MachineStatuses[2].AssetUid, "Wrong assetUid (2)");
    }
#endif

  }
}
