using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SVOICFilterSettings;
using SVOICLiftBuildSettings;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionCellDatumExecutorTests
  {
    private static IServiceProvider serviceProvider;
    private static ILoggerFactory logger;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      var serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, RaptorResult>();

      serviceProvider = serviceCollection.BuildServiceProvider();

      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
    }

    [TestMethod]
    public void CompactionCellDatumExecutorNoResult()
    {
      var request =
        CellDatumRequest.CreateCellDatumRequest(0, DisplayMode.CompactionCoverage, null, null, null, -1, null, null);

      TCellProductionData data = new TCellProductionData();

      var raptorClient = new Mock<IASNodeClient>();

      raptorClient.Setup(x => x.GetCellProductionData(
          request.ProjectId.Value,
          (int) RaptorConverters.convertDisplayMode(request.displayMode),
          request.gridPoint != null ? request.gridPoint.x : 0.0,
          request.gridPoint != null ? request.gridPoint.y : 0.0,
          It.IsAny<TWGS84Point>(),
          request.llPoint == null,
          It.IsAny<TICFilterSettings>(),
          It.IsAny<TICLiftBuildSettings>(),
          It.IsAny<TVLPDDesignDescriptor>(),
          out data))
        .Returns(false);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionCellDatumExecutor>(logger, raptorClient.Object);
      Assert.ThrowsException<ServiceException>(() => executor.Process(request));
    }

    [TestMethod]
    public void CompactionCellDatumExecutorSuccessNoNECoordinates()
    {
      var request =
        CellDatumRequest.CreateCellDatumRequest(0, DisplayMode.CCV, WGSPoint3D.CreatePoint(0.84, -1.75), null, null, -1, null, null);

      TCellProductionData data = new TCellProductionData
      {
        DisplayMode = (int) request.displayMode,
        Value = 500,
        ReturnCode = 0
      };

      var raptorClient = new Mock<IASNodeClient>();

      raptorClient.Setup(x => x.GetCellProductionData(
          request.ProjectId.Value,
          (int)RaptorConverters.convertDisplayMode(request.displayMode),
          request.gridPoint != null ? request.gridPoint.x : 0.0,
          request.gridPoint != null ? request.gridPoint.y : 0.0,
          It.IsAny<TWGS84Point>(),
          request.llPoint == null,
          It.IsAny<TICFilterSettings>(),
          It.IsAny<TICLiftBuildSettings>(),
          It.IsAny<TVLPDDesignDescriptor>(),
          out data))
        .Returns(true);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionCellDatumExecutor>(logger, raptorClient.Object);

      Assert.ThrowsException<ServiceException>(() => executor.Process(request), "On Cell Datum request. Failed to process coordinate conversion request.");
    }
  }
}
