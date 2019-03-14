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
    [Ignore] // todo resolve once Merino AssetService is available
    public async Task GetAssetOnDesignPeriodsExecutor_TRex_Success()
    {
      var projectIds = new ProjectID(){ProjectUid = Guid.NewGuid() };
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;
      var expectedResult =  new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design1", 1, assetId, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-40), assetUid )
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(), It.IsAny<string>(),
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
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].AssetUid, result.AssetOnDesignPeriods[0].AssetUid, "Wrong asset Uid");
      Assert.AreEqual(assets[0].LegacyAssetID, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].MachineId, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].StartDate, result.AssetOnDesignPeriods[0].StartDate, "Wrong StartDate");

      // there's some funny giggery-pokery in the executor to join up any dis-jointed periods
      Assert.IsTrue(result.AssetOnDesignPeriods[0].EndDate > expectedResult.AssetOnDesignPeriods[0].EndDate, "Wrong EndDate");
    }

    [TestMethod]
    [Ignore] // todo resolve once Merino AssetService is available
    public async Task GetAssetOnDesignPeriodsExecutor_TRex_Success_2designs()
    {
      var projectIds = new ProjectID() { ProjectUid = Guid.NewGuid() };
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;
      var expectedResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design1", 1, assetId, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-40), assetUid ),
          new AssetOnDesignPeriod("The NameOf Design2", 2, assetId, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-1), assetUid )
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(), It.IsAny<string>(),
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
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, assetProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(2, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].AssetUid, result.AssetOnDesignPeriods[0].AssetUid, "Wrong asset Uid");
      Assert.AreEqual(assets[0].LegacyAssetID, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].MachineId, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].Id, result.AssetOnDesignPeriods[0].Id, "Wrong DesignId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].StartDate, result.AssetOnDesignPeriods[0].StartDate, "Wrong StartDate");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[1].StartDate, result.AssetOnDesignPeriods[0].EndDate, "Wrong EndDate");

      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[1].AssetUid, result.AssetOnDesignPeriods[1].AssetUid, "Wrong asset Uid");
      Assert.AreEqual(assets[0].LegacyAssetID, result.AssetOnDesignPeriods[1].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[1].MachineId, result.AssetOnDesignPeriods[1].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[1].Id, result.AssetOnDesignPeriods[1].Id, "Wrong DesignId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[1].StartDate, result.AssetOnDesignPeriods[1].StartDate, "Wrong StartDate");
      // the final design change is considered current
      Assert.IsTrue(result.AssetOnDesignPeriods[1].EndDate > expectedResult.AssetOnDesignPeriods[1].EndDate, "Wrong EndDate");
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

      var ex = Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(projectIds)).Result;
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual("Failed to get/update data requested by GetAssetOnDesignPeriodsExecutor", ex.GetResult.Message);
    }

#if RAPTOR
    [TestMethod]
    [Ignore] // todo resolve once Merino AssetService is available
    public async Task GetAssetOnDesignPeriodsExecutor_Raptor_Success()
    {
      var projectIds = new ProjectID() { ProjectId = 999 };
      var customerUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();
      var assetId = 777;
      var designId = 888;
      var expectedResult = new MachineDesignsExecutionResult
      (
        new List<AssetOnDesignPeriod>
        {
          new AssetOnDesignPeriod("The NameOf Design", designId, assetId, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-1), assetUid )
        }
      );

      TDesignName[] assetOnDesigns = new TDesignName[1]{new TDesignName
      {
        FID = (int) expectedResult.AssetOnDesignPeriods[0].Id,
        FName = expectedResult.AssetOnDesignPeriods[0].Name,
        FStartDate = expectedResult.AssetOnDesignPeriods[0].StartDate,
        FEndDate = expectedResult.AssetOnDesignPeriods[0].EndDate,
        FMachineID = expectedResult.AssetOnDesignPeriods[0].MachineId
      }};

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetOnMachineDesignEvents(projectIds.ProjectId.Value))
        .Returns(assetOnDesigns);

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
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns("false");

      var executor = RequestExecutorContainerFactory
        .Build<GetAssetOnDesignPeriodsExecutor>(logger, raptorClient.Object, configStore: configStore.Object,
          assetProxy: assetProxy.Object,
          customHeaders: _customHeaders, customerUid: customerUid.ToString());

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.AssetOnDesignPeriods.Count, "Wrong design count");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].AssetUid, result.AssetOnDesignPeriods[0].AssetUid, "Wrong asset Uid");
      Assert.AreEqual(assets[0].LegacyAssetID, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].MachineId, result.AssetOnDesignPeriods[0].MachineId, "Wrong legacyAssetId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].Id, result.AssetOnDesignPeriods[0].Id, "Wrong DesignId");
      Assert.AreEqual(expectedResult.AssetOnDesignPeriods[0].StartDate, result.AssetOnDesignPeriods[0].StartDate, "Wrong StartDate");

      // the final design change is considered current
      Assert.IsTrue(result.AssetOnDesignPeriods[0].EndDate > expectedResult.AssetOnDesignPeriods[0].EndDate, "Wrong EndDate");
    }
#endif

  }
}
