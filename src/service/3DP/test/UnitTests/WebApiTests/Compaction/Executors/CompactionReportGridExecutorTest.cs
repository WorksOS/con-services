using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
#if RAPTOR
using ASNodeRPC;
using SVOICFilterSettings;
using SVOICLiftBuildSettings;
using SVOICOptionsDecls;
using VLPDDecls;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionReportGridExecutorTest
  {
    private static IServiceProvider _serviceProvider;
    private static ILoggerFactory _logger;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      _serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.WebApi.Tests.log")))
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
#if RAPTOR
        .AddTransient<IErrorCodesProvider, RaptorResult>()
#endif
        .BuildServiceProvider();

      _logger = _serviceProvider.GetRequiredService<ILoggerFactory>();
    }
#if RAPTOR
    [TestMethod]
    public void CompactionReportGridExecutor_Raptor_NoResult()
    {
      var request = CompactionReportGridRequest.CreateCompactionReportGridRequest(
        0, null, null, -1, null, false, false, false, false, false, false, null, 0.0, GridReportOption.Automatic, 0.0, 0.0, 0.0, 0.0, 0.0);

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_STATIONOFFSET")).Returns(false);

      MemoryStream responseData;

      var raptorClient = new Mock<IASNodeClient>();

      var args = ASNode.GridReport.RPC.__Global.Construct_GridReport_Args(
        request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
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
        .Build<CompactionReportGridExecutor>(_logger, raptorClient.Object, configStore: mockConfigStore.Object);
      Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));
    }
#endif
    [TestMethod]
    public async Task CompactionReportGridExecutor_TRex_NoResult()
    {
      var projectUid = Guid.NewGuid();
      var request = CompactionReportGridRequest.CreateCompactionReportGridRequest(
        0, projectUid, null, -1, null, true, false, false, false, false, false,
        null, 4.0, GridReportOption.Automatic,
        0.0, 0.0, 1.0, 2.0, 0.0);

      var mockConfigStore = new Mock<IConfigurationStore>();
#if RAPTOR
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_GRIDREPORT")).Returns(true);
#endif

      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          $"Grid report failed somehow. ProjectUid: {projectUid}"));

      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataPostRequestWithStreamResponse(It.IsAny<CompactionReportGridTRexRequest>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
        .Throws(exception);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionReportGridExecutor>(_logger, configStore: mockConfigStore.Object,
          trexCompactionDataProxy: tRexProxy.Object);
      var result = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));
      Assert.AreEqual(HttpStatusCode.InternalServerError, result.Code);
      Assert.AreEqual(ContractExecutionStatesEnum.InternalProcessingError, result.GetResult.Code);
      Assert.AreEqual(exception.GetResult.Message, result.GetResult.Message);
    }
#if RAPTOR
    [Ignore]
    [TestMethod]
    public async Task CompactionReportGridExecutorSuccess()
    {
      var request = CompactionReportGridRequest.CreateCompactionReportGridRequest(
        0, null, null, -1, null, false, false, false, false, false, false, null, 0.0, GridReportOption.Automatic, 0.0, 0.0, 0.0, 0.0, 0.0);

      MemoryStream responseData = new MemoryStream();

      var raptorClient = new Mock<IASNodeClient>();

      var args = ASNode.GridReport.RPC.__Global.Construct_GridReport_Args(
        request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
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
        .Build<CompactionReportGridExecutor>(_logger, raptorClient.Object);

      var result = await executor.ProcessAsync(request) as CompactionReportResult;

      Assert.IsNotNull(result, "Result should not be null");

      var reportDataAsJson = JsonConvert.SerializeObject(result.ReportData);

      Assert.AreEqual(responseData.ToString(), reportDataAsJson, "Wrong Data");

    }
#endif
  }
}
