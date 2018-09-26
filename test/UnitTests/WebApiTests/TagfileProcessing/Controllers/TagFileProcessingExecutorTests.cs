using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ShineOn.Rtl;
using TAGProcServiceDecls;
using VLPDDecls;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.TagfileProcessing.Controllers
{
  [TestClass]
  public class TagFileProcessingExecutorTests
  {
    private static IServiceProvider _serviceProvider;
    private static ILoggerFactory _logger;
    private static Dictionary<string, string> _customHeaders;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      var serviceCollection = new ServiceCollection()
        .AddLogging()
        .AddSingleton(loggerFactory)
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, RaptorResult>();

      _serviceProvider = serviceCollection.BuildServiceProvider();

      _logger = _serviceProvider.GetRequiredService<ILoggerFactory>();
      _customHeaders = new Dictionary<string, string>();
    }

    [TestMethod]
    public async Task DirectTagFileSubmitter_RaptorAndTRex_Successfull()
    {
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      long legacyProjectId = 1;
      var request = new CompactionTagFileRequest()
      {
        ProjectId = legacyProjectId,
        ProjectUid = null,
        FileName = "Machine Name--whatever --161230235959",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      // create the Raptor mocks with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>()));

      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Returns(TTAGProcServerProcessResult.tpsprOK);

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY")).Returns("true");
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprOK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, mockTransferProxy.Object, mockTRexTagFileProxy.Object, null,
          _customHeaders);

      var result = await submitter.ProcessAsync(request).ConfigureAwait(false);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage);
    }

    [TestMethod]
    public async Task DirectTagFileSubmitter_TRex_Unsuccessfull()
    {
      // trex result is ignored in overall result to final return is the Raptor success

      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      long legacyProjectId = 1;
      var request = new CompactionTagFileRequest()
      {
        ProjectId = legacyProjectId,
        ProjectUid = null,
        FileName = "Machine Name--whatever --161230235959",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      // create the Raptor mocks with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>()));

      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Returns(TTAGProcServerProcessResult.tpsprOK);

      // create the Trex mocks with UNsuccessful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY")).Returns("true");
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(
          new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprFailedEventDateValidation));
      mockTRexTagFileProxy.Setup(s => s.SendTagFileDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, mockTransferProxy.Object, mockTRexTagFileProxy.Object, null,
          _customHeaders);

      var result = await submitter.ProcessAsync(request).ConfigureAwait(false);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage);
    }

    [TestMethod]
    public async Task DirectTagFileSubmitter_Raptor_Unsuccessfull()
    {
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      long legacyProjectId = 1;
      var request = new CompactionTagFileRequest()
      {
        ProjectId = legacyProjectId,
        ProjectUid = null,
        FileName = "Machine Name--whatever --161230235959",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      // create the Raptor mocks with UNsuccessful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>()));

      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()
          ))
        .Returns(TTAGProcServerProcessResult.tpsprFileReaderCorruptedTAGFileData);

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY")).Returns("true");
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprOK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, mockTransferProxy.Object, mockTRexTagFileProxy.Object, null,
          _customHeaders);

      var result = await submitter.ProcessAsync(request).ConfigureAwait(false);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Code == (int) TTAGProcServerProcessResult.tpsprFileReaderCorruptedTAGFileData);
    }

    [TestMethod]
    public async Task DirectTagFileSubmitter_TRex_ThrowsException()
    {
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      long legacyProjectId = 1;
      var request = new CompactionTagFileRequest()
      {
        ProjectId = legacyProjectId,
        ProjectUid = null,
        FileName = "Machine Name--whatever --161230235959",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      // create the Raptor mocks with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>()));

      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Returns(TTAGProcServerProcessResult.tpsprOK);

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY")).Returns("true");
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ThrowsAsync(new NotImplementedException());

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, mockTransferProxy.Object, mockTRexTagFileProxy.Object, null,
          _customHeaders);

      var result = await submitter.ProcessAsync(request).ConfigureAwait(false);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage);
    }

    [TestMethod]
    public async Task DirectTagFileSubmitter_Raptor_ThrowsException()
    {
      long legacyProjectId = 1; //  todo projectID validation rejects  0 and -1. Also projectId or UID MUST have a valid value;
                                //   VelociraptorConstants.NO_PROJECT_ID = -1
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      var request = new CompactionTagFileRequest()
      {
        ProjectId = legacyProjectId, 
        ProjectUid = null,
        FileName = "Machine Name--whatever --161230235959",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      // create the Raptor mocks with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>()));

      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Throws<NotImplementedException>();

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY")).Returns("true");
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprOK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, mockTransferProxy.Object, mockTRexTagFileProxy.Object, null,
          _customHeaders);

      var result = await submitter.ProcessAsync(request).ConfigureAwait(false);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Code == (int) TTAGProcServerProcessResult.tpsprUnknown);
    }

    [TestMethod]
    public async Task NonDirectTagFileSubmitter_RaptorAndTRex_Successful()
    {
      var projectUid = Guid.NewGuid();
      var resolvedLegacyProjectId = 544;
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      var request = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended
      (
        new CompactionTagFileRequest()
        {
          ProjectId = resolvedLegacyProjectId,
          ProjectUid = projectUid,
          FileName = "Machine Name--whatever --161230235959",
          Data = tagFileContent,
          OrgId = string.Empty
        },
        CreateAFence()
      );

      // create the mock PDSClient with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Returns(TTAGProcServerProcessResult.tpsprOK);

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY")).Returns("true");
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprOK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileNonDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, null, mockTRexTagFileProxy.Object, null, _customHeaders);

      var result = await submitter.ProcessAsync(request).ConfigureAwait(false);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage);
    }

    [TestMethod]
    public async Task NonDirectTagFileSubmitter_TRex_UnSuccessful()
    {
      var projectUid = Guid.NewGuid();
      var resolvedLegacyProjectId = 544;
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      var request = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended
      (
        new CompactionTagFileRequest()
        {
          ProjectId = resolvedLegacyProjectId,
          ProjectUid = projectUid,
          FileName = "Machine Name--whatever --161230235959",
          Data = tagFileContent,
          OrgId = string.Empty
        },
        CreateAFence()
      );

      // create the mock PDSClient with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Returns(TTAGProcServerProcessResult.tpsprOK);

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY")).Returns("true");
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(
          new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprFailedEventDateValidation));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileNonDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, null, mockTRexTagFileProxy.Object, null, _customHeaders);

      var result = await submitter.ProcessAsync(request).ConfigureAwait(false);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage);
    }

    [TestMethod]
    public async Task NonDirectTagFileSubmitter_Raptor_UnSuccessful()
    {
      var projectUid = Guid.NewGuid();
      var resolvedLegacyProjectId = 544;
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      var request = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended
      (
        new CompactionTagFileRequest()
        {
          ProjectId = resolvedLegacyProjectId,
          ProjectUid = projectUid,
          FileName = "Machine Name--whatever --161230235959",
          Data = tagFileContent,
          OrgId = string.Empty
        },
        CreateAFence()
      );

      // create the mock PDSClient with Unsuccessful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Returns(TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions);

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY")).Returns("true");
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprOK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileNonDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, null, mockTRexTagFileProxy.Object, null, _customHeaders);

      var result = await submitter.ProcessAsync(request).ConfigureAwait(false);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Code == (int) TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions + 2000);
      Assert.AreEqual("Failed to process tagfile with error: OnChooseMachine. Machine Subscriptions Invalid.",
        result.Message);
    }

    [TestMethod]
    public async Task NonDirectTagFileSubmitter_Raptor_Exception()
    {
      var projectUid = Guid.NewGuid();
      var resolvedLegacyProjectId = 544;
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      var request = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended
      (
        new CompactionTagFileRequest()
        {
          ProjectId  = resolvedLegacyProjectId,
          ProjectUid = projectUid,
          FileName = "Machine Name--whatever --161230235959",
          Data = tagFileContent,
          OrgId = string.Empty
        },
        CreateAFence()
      );

      // create the mock PDSClient with Unsuccessful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Returns(TTAGProcServerProcessResult.tpsprFileReaderCorruptedTAGFileData);

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY")).Returns("true");
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprOK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileNonDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, null, mockTRexTagFileProxy.Object, null, _customHeaders);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await submitter.ProcessAsync(request))
        .ConfigureAwait(false);
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code, "executor threw exception but incorrect httpStatus");
      var result = ex.GetResult;
      Assert.AreEqual((int) TTAGProcServerProcessResult.tpsprFileReaderCorruptedTAGFileData + 2000, result.Code,
        "executor threw exception but incorrect code");
      Assert.AreEqual(
        "Failed to process tagfile with error: The TAG file was found to be corrupted on its pre-processing scan.",
        result.Message, "executor threw exception but incorrect message");
    }


    /// <summary>
    /// Tag files can be manually or automatically submitted.
    /// Manual submission requires a project Id be provided and the service determines the project boundary.
    /// Automatic submission requires no project Id be send, and the service therefore cannot resolve a boundary.
    ///
    /// Any variation of this combation is illegal and should be stopped by the executor.
    /// </summary>
    [TestMethod]
    [DataRow(544, false,
      "Failed to process tagfile with error: Manual tag file submissions must include a boundary fence.")]
    [DataRow(VelociraptorConstants.NO_PROJECT_ID, true,
      "Failed to process tagfile with error: Automatic tag file submissions cannot include boundary fence.")]
    public async Task NonDirectRaptor_Should_fail_if_project_and_boundary_dont_validate(long resolvedLegacyProjectId,
      bool createFence, string exceptionMessage)
    {
      var projectUid = Guid.NewGuid();
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      var request = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended
      (
        new CompactionTagFileRequest()
        {
          ProjectId = resolvedLegacyProjectId,
          ProjectUid = projectUid,
          FileName = "Machine Name--whatever --161230235959",
          Data = tagFileContent,
          OrgId = string.Empty
        },
        createFence ? CreateAFence() : null
      );

      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY")).Returns("false");
      var submitter = RequestExecutorContainerFactory.Build<TagFileNonDirectSubmissionExecutor>(_logger,
        mockRaptorClient.Object, mockTagProcessor.Object, mockConfigStore.Object);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await submitter.ProcessAsync(request))
        .ConfigureAwait(false);
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code, "executor threw exception but incorrect httpStatus");
      var result = ex.GetResult;
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, result.Code,
        "executor threw exception but incorrect code");
      Assert.AreEqual(exceptionMessage, result.Message, "executor threw exception but incorrect message");
    }


    private WGS84Fence CreateAFence()
    {
      var points = new List<WGSPoint3D>
      {
        new WGSPoint3D(0.631986074660308, -2.00757760231466),
        new WGSPoint3D(0.631907507374149, -2.00758733949739),
        new WGSPoint3D(0.631904485465203, -2.00744352879854),
        new WGSPoint3D(0.631987283352491, -2.00743753668608)
      };

      return new WGS84Fence(points.ToArray());
    }
  }
}
