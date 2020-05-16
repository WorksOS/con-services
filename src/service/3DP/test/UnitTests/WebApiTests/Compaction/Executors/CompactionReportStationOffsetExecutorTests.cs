using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
#if RAPTOR
using ASNodeDecls;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionReportStationOffsetExecutorTests
  {
    private static IServiceProvider _serviceProvider;
    private static ILoggerFactory _logger;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      _serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.WebApi.Tests.log")))
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
#if RAPTOR
        .AddTransient<IErrorCodesProvider, RaptorResult>()
#endif
        .BuildServiceProvider();

      _logger = _serviceProvider.GetRequiredService<ILoggerFactory>();
    }
#if RAPTOR
    [TestMethod]
    public void CompactionReportStationOffsetExecutor_Raptor_NoResult()
    {
      var userPreferences = new UserPreferenceData { Language = "en-US" };
      var request = CompactionReportStationOffsetRequest.CreateRequest(
        33, null, null, 0, null, true, true, true, true, true, true, null, null, 0, 0, 0, null, userPreferences, "New Zealand Standard Time");
      var filterSettings = RaptorConverters.ConvertFilter(request.Filter);
      var cutfillDesignDescriptor = RaptorConverters.DesignDescriptor(request.DesignFile);
      var alignmentDescriptor = RaptorConverters.DesignDescriptor(request.AlignmentFile);
      var TASNodeUserPreference = ExportRequestHelper.ConvertToRaptorUserPreferences(request.UserPreferences, request.ProjectTimezone);

      var options = RaptorConverters.convertOptions(null, request.LiftBuildSettings, 0,
        request.Filter?.LayerType ?? FilterLayerMethod.None, DisplayMode.Height, false);

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_STATIONOFFSET")).Returns(false);

      MemoryStream responseData;

      var raptorClient = new Mock<IASNodeClient>();

      var args = ASNode.StationOffsetReport.RPC.__Global.Construct_StationOffsetReport_Args(
        request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
        (int)CompactionReportType.StationOffset,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtProdDataReport),
        TASNodeUserPreference,
        alignmentDescriptor,
        cutfillDesignDescriptor,
        request.StartStation,
        request.EndStation,
        request.Offsets,
        request.CrossSectionInterval,
        request.ReportElevation,
        request.ReportCutFill,
        request.ReportCMV,
        request.ReportMDP,
        request.ReportPassCount,
        request.ReportTemperature,
        (int)GridReportOption.Unused,
        0, 0, 0, 0, 0, 0, 0, // Northings, Eastings and Direction values are not used on Station Offset report.
        filterSettings,
        RaptorConverters.ConvertLift(request.LiftBuildSettings, filterSettings.LayerMethod),
        options
      );

      raptorClient.Setup(x => x.GetReportStationOffset(args, out responseData)).Returns(0); // icsrrUnknownError
      var executor = RequestExecutorContainerFactory
        .Build<CompactionReportStationOffsetExecutor>(_logger, raptorClient.Object, configStore: mockConfigStore.Object);
      Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));
    }
#endif
    [TestMethod]
    public async Task CompactionReportStationOffsetExecutor_TRex_NoResult()
    {
      var projectUid = Guid.NewGuid();
      var alignmentDesignUid = Guid.NewGuid();
      var userPreferences = new UserPreferenceData { Language = "en-US" };
      var request = CompactionReportStationOffsetRequest.CreateRequest(
        0, projectUid, null, 0, null, false, true, true, true, true, false,
        null, new DesignDescriptor(-1,
          FileDescriptor.CreateFileDescriptor(string.Empty, string.Empty, projectUid.ToString(),
            "theFilename.svl"), -1, alignmentDesignUid), 0, 0, 0, null, userPreferences, "New Zealand Standard Time");

      var mockConfigStore = new Mock<IConfigurationStore>();
#if RAPTOR
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_STATIONOFFSET")).Returns(true);
#endif

      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"StationOffset report failed somehow. ProjectUid: { projectUid }"));
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendDataPostRequestWithStreamResponse(It.IsAny<CompactionReportStationOffsetTRexRequest>(), It.IsAny<string>(), It.IsAny<IHeaderDictionary>())).Throws(exception);
      var executor = RequestExecutorContainerFactory
        .Build<CompactionReportStationOffsetExecutor>(_logger, configStore: mockConfigStore.Object, trexCompactionDataProxy: tRexProxy.Object);
      var result = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));
      Assert.AreEqual(HttpStatusCode.InternalServerError, result.Code);
      Assert.AreEqual(ContractExecutionStatesEnum.InternalProcessingError, result.GetResult.Code);
      Assert.AreEqual(exception.GetResult.Message, result.GetResult.Message);
    }

  }
}
