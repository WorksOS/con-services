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
    public async Task MachineDesignsExecutor_TRex_Success()
    {
      var projectIds = new ProjectID(){ProjectUid = Guid.NewGuid() };
      var assetUid1 = Guid.NewGuid();
      var expectedResult =  new MachineDesignsExecutionResult
      (
        new List<DesignName>
        {
          new DesignName("The NameOf Design1", -1, -1, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-40), assetUid1 )
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(), It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedResult);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineDesignsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.Designs.Count, "Wrong design count");
      Assert.AreEqual(expectedResult.Designs[0].MachineUid, result.Designs[0].MachineUid, "Wrong machine Uid");
      Assert.AreEqual(expectedResult.Designs[0].StartDate, result.Designs[0].StartDate, "Wrong StartDate");

      // there's some funny giggery-pokery in the executor to join up any dis-jointed periods
      Assert.IsTrue(result.Designs[0].EndDate > expectedResult.Designs[0].EndDate, "Wrong EndDate");
    }

    [TestMethod]
    public async Task MachineDesignsExecutor_TRex_Success_2designs()
    {
      var projectIds = new ProjectID() { ProjectUid = Guid.NewGuid() };
      var assetUid1 = Guid.NewGuid();
      var expectedResult = new MachineDesignsExecutionResult
      (
        new List<DesignName>
        {
          new DesignName("The NameOf Design1", -1, -1, DateTime.UtcNow.AddDays(-50), DateTime.UtcNow.AddDays(-40), assetUid1 ),
          new DesignName("The NameOf Design2", -1, -1, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-1), assetUid1 )
        }
      );
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(), It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(expectedResult);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineDesignsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);

      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(2, result.Designs.Count, "Wrong design count");
      Assert.AreEqual(expectedResult.Designs[0].MachineUid, result.Designs[0].MachineUid, "Wrong machine Uid");
      Assert.AreEqual(expectedResult.Designs[0].StartDate, result.Designs[0].StartDate, "Wrong StartDate");
      Assert.AreEqual(expectedResult.Designs[1].EndDate, result.Designs[0].EndDate, "Wrong EndDate");

      Assert.AreEqual(expectedResult.Designs[1].MachineUid, result.Designs[1].MachineUid, "Wrong machine Uid");
      Assert.AreEqual(expectedResult.Designs[1].StartDate, result.Designs[1].StartDate, "Wrong StartDate");
      // there's some funny giggery-pokery in the executor to join up any dis-jointed periods
      Assert.IsTrue(result.Designs[1].EndDate > expectedResult.Designs[1].EndDate, "Wrong EndDate");
    }

    [TestMethod]
    public void MachineDesignsExecutor_TRex_NoProjectUid()
    {
      var projectIds = new ProjectID();
      
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineDesignsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);

      var ex = Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(projectIds)).Result;
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual("Failed to get/update data requested by GetMachineDesignsExecutor", ex.GetResult.Message);
    }

#if RAPTOR
    [TestMethod]
    public async Task MachineDesignsExecutor_Raptor_Success()
    {
      var projectIds = new ProjectID() { ProjectId = 999 };
      var assetId = 777;
      var designId = 888;
      var expectedResult = new MachineDesignsExecutionResult
      (
        new List<DesignName>
        {
          new DesignName("The NameOf Design", assetId, designId, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-1), null )
        }
      );

      TDesignName[] designs = new TDesignName[1]{new TDesignName
      {
        FID = (int) expectedResult.Designs[0].Id,
        FName = expectedResult.Designs[0].Name,
        FStartDate = expectedResult.Designs[0].StartDate,
        FEndDate = expectedResult.Designs[0].EndDate,
        FMachineID = expectedResult.Designs[0].MachineId
      }};

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetOnMachineDesignEvents(projectIds.ProjectId.Value))
        .Returns(designs);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")).Returns("false");

      var executor = RequestExecutorContainerFactory
        .Build<GetMachineDesignsExecutor>(logger, raptorClient.Object, configStore: configStore.Object);
      var result = await executor.ProcessAsync(projectIds) as MachineDesignsExecutionResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(1, result.Designs.Count, "Wrong design count");
      Assert.AreEqual(expectedResult.Designs[0].MachineId, result.Designs[0].MachineId, "Wrong machine Id");
      Assert.AreEqual(expectedResult.Designs[0].Id, result.Designs[0].Id, "Wrong design Id");
      Assert.AreEqual(expectedResult.Designs[0].StartDate, result.Designs[0].StartDate, "Wrong StartDate");

      // there's some funny giggery-pokery in the executor to join up any dis-jointed periods
      Assert.IsTrue(result.Designs[0].EndDate > expectedResult.Designs[0].EndDate, "Wrong EndDate");
    }
#endif

  }
}
