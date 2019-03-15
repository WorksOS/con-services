using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
#if RAPTOR
using BoundingExtents;
using SVOICStatistics;
using VLPDDecls;
#endif

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class ProjectStatisticsExecutorTests
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
    public void ProjectStatisticsExecutor_TRexValidation_Success()
    {
      var excludedSurveyedSurfaceUids = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
      var request = new ProjectStatisticsTRexRequest(Guid.NewGuid(), excludedSurveyedSurfaceUids);
      request.Validate();
    }

    [TestMethod]
    public void ProjectStatisticsExecutor_TRexValidation_ProjectUidFailure()
    {
      var excludedSurveyedSurfaceUids = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
      var request = new ProjectStatisticsTRexRequest(Guid.Empty, excludedSurveyedSurfaceUids);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.IsNotNull(ex, "should be exception");
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code, "Invalid HttpStatusCode.");
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code, "Invalid executionState.");
      Assert.AreEqual("Invalid project UID.", ex.GetResult.Message, "Invalid error message.");
    }

    [TestMethod]
    public void ProjectStatisticsExecutor_TRexValidation_SSUidFailure()
    {
      var excludedSurveyedSurfaceUids = new Guid[] { Guid.Empty, Guid.NewGuid(), Guid.NewGuid() };
      var request = new ProjectStatisticsTRexRequest(Guid.NewGuid(), excludedSurveyedSurfaceUids);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.IsNotNull(ex, "should be exception");
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code, "Invalid HttpStatusCode.");
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code, "Invalid executionState.");
      Assert.AreEqual("Excluded Surface Uid is invalid", ex.GetResult.Message, "Invalid error message.");
    }

    [TestMethod]
    public void ProjectStatisticsExecutor_TRex_Success()
    {
      var excludedSurveyedSurfaceUids = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
      var excludedSurveyedSurfaceIds = new long[] { 444, 777, 888 };
      var request = new ProjectStatisticsMultiRequest(Guid.NewGuid(), 123, excludedSurveyedSurfaceUids, excludedSurveyedSurfaceIds);

      var expectedResult = new ProjectStatisticsResult(ContractExecutionStatesEnum.ExecutedSuccessfully)
      {
        startTime = DateTime.UtcNow.AddDays(-5),
        endTime = DateTime.UtcNow.AddDays(-1),
        cellSize = 32.5,
        indexOriginOffset = 10,
        extents = BoundingBox3DGrid.CreatBoundingBox3DGrid(10, 500, 0, 20, 510, 0)
      };
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataPostRequest<ProjectStatisticsResult, ProjectStatisticsTRexRequest>(It.IsAny<ProjectStatisticsTRexRequest>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), false))
        .ReturnsAsync((expectedResult));


      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_PROJECTSTATISTICS")).Returns("true");

      var executor = RequestExecutorContainerFactory
        .Build<ProjectStatisticsExecutor>(logger, configStore: configStore.Object,
          trexCompactionDataProxy: tRexProxy.Object, customHeaders: _customHeaders);

      var result = executor.ProcessAsync(request).Result as ProjectStatisticsResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(expectedResult.startTime, result.startTime, "Wrong startTime");
      Assert.AreEqual(expectedResult.extents.MaxX, result.extents.MaxX, "Wrong MaxX extent");
    }

#if RAPTOR

    [TestMethod]
    public void ProjectStatisticsExecutor_RaptorValidation_Success()
    {
      var excludedSurveyedSurfaceIds = new long[] { 555, 666, 777 };
      var request = new ProjectStatisticsRequest(4545, excludedSurveyedSurfaceIds);
      request.Validate();
    }

    [TestMethod]
    public void ProjectStatisticsExecutor_RaptorValidation_ProjectIdFailure()
    {
      var excludedSurveyedSurfaceIds = new long[] { 555, 666, 777 };
      var request = new ProjectStatisticsRequest(0, excludedSurveyedSurfaceIds);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.IsNotNull(ex, "should be exception");
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code, "Invalid HttpStatusCode.");
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code, "Invalid executionState.");
      Assert.AreEqual("Invalid project ID", ex.GetResult.Message, "Invalid error message.");
    }

    [TestMethod]
    public void ProjectStatisticsExecutor_RaptorValidation_SSIdFailure()
    {
      var excludedSurveyedSurfaceIds = new long[] { 0, 666, 777 };
      var request = new ProjectStatisticsRequest(5656, excludedSurveyedSurfaceIds);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.IsNotNull(ex, "should be exception");
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code, "Invalid HttpStatusCode.");
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, ex.GetResult.Code, "Invalid executionState.");
      Assert.AreEqual("Excluded Surface Id is invalid", ex.GetResult.Message, "Invalid error message.");
    }

    [TestMethod]
    public void ProjectStatisticsExecutor_Raptor_Success()
    {
      var excludedSurveyedSurfaceUids = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
      var excludedSurveyedSurfaceIds = new long[] { 444, 777, 888 };
      var request = new ProjectStatisticsMultiRequest(Guid.NewGuid(), 123, excludedSurveyedSurfaceUids, excludedSurveyedSurfaceIds);

      var statistics = new TICDataModelStatistics()
      {
        StartTime = DateTime.UtcNow.AddDays(-5),
        EndTime = DateTime.UtcNow.AddDays(-1),
        CellSize = 32.5,
        IndexOriginOffset = 10,
        Extents = new T3DBoundingWorldExtent(10, 500, 0, 20, 510, 0)
      };

      var raptorClient = new Mock<IASNodeClient>();
      raptorClient
        .Setup(x => x.GetDataModelStatistics(request.ProjectId, It.IsAny<TSurveyedSurfaceID[]>(), out statistics))
        .Returns(true);

      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_PROJECTSTATISTICS")).Returns("false");

      var executor = RequestExecutorContainerFactory
        .Build<ProjectStatisticsExecutor>(logger, raptorClient.Object, configStore: configStore.Object);
      var result = executor.ProcessAsync(request).Result as ProjectStatisticsResult;
      Assert.IsNotNull(result, "Result should not be null");
      Assert.AreEqual(statistics.CellSize, result.cellSize, "Wrong CellSize");
      Assert.AreEqual(statistics.Extents.MaxX, result.extents.MaxX, "Wrong MaxX extent");
    }
#endif

  }
}
