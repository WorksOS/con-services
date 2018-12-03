using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ASNode.UserPreferences;
using ASNodeDecls;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionReportStationOffsetExecutorTests
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
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();

      serviceCollection.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, RaptorResult>();

      serviceProvider = serviceCollection.BuildServiceProvider();

      logger = serviceProvider.GetRequiredService<ILoggerFactory>();
    }

    [TestMethod]
    public void CompactionReportStationOffsetExecutor_Raptor_NoResult()
    {
      var userPreferences = new UserPreferenceData{ Language = "en-US" };
      var request = CompactionReportStationOffsetRequest.CreateRequest(
        0, null, null, 0, null, true, true, true, true, true, true, null, null, 0, 0, 0, null, userPreferences, "New Zealand Standard Time");
      var filterSettings = RaptorConverters.ConvertFilter(request.FilterID, request.Filter, request.ProjectId);
      var cutfillDesignDescriptor = RaptorConverters.DesignDescriptor(request.DesignFile);
      var alignmentDescriptor = RaptorConverters.DesignDescriptor(request.AlignmentFile);
      var TASNodeUserPreference = ExportRequestHelper.ConvertUserPreferences(request.UserPreferences, request.ProjectTimezone);

      var options = RaptorConverters.convertOptions(null, request.LiftBuildSettings, 0,
        request.Filter?.LayerType ?? FilterLayerMethod.None, DisplayMode.Height, false);

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_STATIONOFFSET")).Returns("false");

      MemoryStream responseData;

      var raptorClient = new Mock<IASNodeClient>();

      var args = ASNode.StationOffsetReport.RPC.__Global.Construct_StationOffsetReport_Args(
        request.ProjectId ?? -1,
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
        .Build<CompactionReportStationOffsetExecutor>(logger, raptorClient.Object, configStore: mockConfigStore.Object);
      Assert.ThrowsException<ServiceException>(() => executor.Process(request));
    }

    [TestMethod]
    public void CompactionReportStationOffsetExecutor_TRex_NoResult()
    {
      var projectUid = Guid.NewGuid();
      var userPreferences = new UserPreferenceData { Language = "en-US" };
      var request = CompactionReportStationOffsetRequest.CreateRequest(
        0, projectUid, null, 0, null, true, true, true, true, true, true, null, null, 0, 0, 0, null, userPreferences, "New Zealand Standard Time");

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_STATIONOFFSET")).Returns("true");

      var exception = new ServiceException(HttpStatusCode.InternalServerError, 
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"StationOffset report has not been implemented in Trex yet. ProjectUid: { projectUid }"));
      var tRexProxy = new Mock<ITRexCompactionDataProxy>();
      tRexProxy.Setup(x => x.SendStationOffsetRequest(request, It.IsAny<IDictionary<string, string>>())).Throws(exception);
      var executor = RequestExecutorContainerFactory
        .Build<CompactionReportStationOffsetExecutor>(logger, null, configStore: mockConfigStore.Object, trexCompactionDataProxy: tRexProxy.Object);
      var result = Assert.ThrowsException<ServiceException>(() => executor.Process(request));
      Assert.AreEqual(HttpStatusCode.InternalServerError, result.Code);
      var contractResult = JsonConvert.DeserializeObject<ContractExecutionResult>(result.GetContent);
      Assert.AreEqual(ContractExecutionStatesEnum.InternalProcessingError, contractResult.Code);
      Assert.AreEqual($"StationOffset report has not been implemented in Trex yet. ProjectUid: {projectUid}", contractResult.Message);
    }

  }
}
