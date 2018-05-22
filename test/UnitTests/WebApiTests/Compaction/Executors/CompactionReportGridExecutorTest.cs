using System;
using System.IO;
using ASNodeRPC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using SVOICFilterSettings;
using SVOICLiftBuildSettings;
using SVOICOptionsDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionReportGridExecutorTest
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
    public void CompactionReportGridExecutorNoResult()
    {
      var request = CompactionReportGridRequest.CreateCompactionReportGridRequest(
        0, null, -1, null, false, false, false, false, false, false, null, 0.0, GridReportOption.Automatic, 0.0, 0.0, 0.0, 0.0, 0.0);

      MemoryStream responseData = new MemoryStream();

      var raptorClient = new Mock<IASNodeClient>();

      var args = ASNode.GridReport.RPC.__Global.Construct_GridReport_Args(
        request.ProjectId ?? -1,
        (int)CompactionReportType.Grid,
        It.IsAny<TASNodeRequestDescriptor>(),
        It.IsAny<TVLPDDesignDescriptor>(),
        request.GridInterval,
        request.ReportElevation,
        request.ReportCutFill,
        request.ReportCMV,
        request.ReportMDP,
        request.ReportPassCount,
        request.ReportTemperature,
        (int)request.GridReportOption,
        request.StartNorthing,
        request.StartEasting,
        request.EndNorthing,
        request.EndEasting,
        request.Azimuth,
        It.IsAny<TICFilterSettings>(),
        It.IsAny<TICLiftBuildSettings>(),
        It.IsAny<TSVOICOptions>()
      );

      raptorClient.
        Setup(x => x.GetReportGrid(args, out responseData)).
        Returns(0); // icsrrUnknownError

      var executor = RequestExecutorContainerFactory
        .Build<CompactionReportGridExecutor>(logger, raptorClient.Object);
      Assert.ThrowsException<ServiceException>(() => executor.Process(request));
    }

    [Ignore]
    [TestMethod]
    public void CompactionReportGridExecutorSuccess()
    {
      var request = CompactionReportGridRequest.CreateCompactionReportGridRequest(
        0, null, -1, null, false, false, false, false, false, false, null, 0.0, GridReportOption.Automatic, 0.0, 0.0, 0.0, 0.0, 0.0);

      MemoryStream responseData = new MemoryStream();

      var raptorClient = new Mock<IASNodeClient>();

      var args = ASNode.GridReport.RPC.__Global.Construct_GridReport_Args(
        request.ProjectId ?? -1,
        (int)CompactionReportType.Grid,
        It.IsAny<TASNodeRequestDescriptor>(),
        It.IsAny<TVLPDDesignDescriptor>(),
        request.GridInterval,
        request.ReportElevation,
        request.ReportCutFill,
        request.ReportCMV,
        request.ReportMDP,
        request.ReportPassCount,
        request.ReportTemperature,
        (int)request.GridReportOption,
        request.StartNorthing,
        request.StartEasting,
        request.EndNorthing,
        request.EndEasting,
        request.Azimuth,
        It.IsAny<TICFilterSettings>(),
        It.IsAny<TICLiftBuildSettings>(),
        It.IsAny<TSVOICOptions>()
      );

      raptorClient.
        Setup(x => x.GetReportGrid(args, out responseData)).
        Returns(1); // icsrrNoError

      var executor = RequestExecutorContainerFactory
        .Build<CompactionReportGridExecutor>(logger, raptorClient.Object);

      var result = executor.Process(request) as CompactionReportResult;

      Assert.IsNotNull(result, "Result should not be null");

      var reportDataAsJson = JsonConvert.SerializeObject(result.ReportData);

      Assert.AreEqual(responseData.ToString(), reportDataAsJson, "Wrong Data");

    }
  }
}
