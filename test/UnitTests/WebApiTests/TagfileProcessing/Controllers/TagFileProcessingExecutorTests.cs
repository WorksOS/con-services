using System;
using System.Collections.Generic;
using System.IO;
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
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
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
    private static IConfigurationStore _configStore;

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
      _configStore = _serviceProvider.GetRequiredService<IConfigurationStore>();
    }

    private WGS84Fence CreateAFence()
    {
      var points = new List<WGSPoint3D>
      {
        WGSPoint3D.CreatePoint(0.631986074660308, -2.00757760231466),
        WGSPoint3D.CreatePoint(0.631907507374149, -2.00758733949739),
        WGSPoint3D.CreatePoint(0.631904485465203, -2.00744352879854),
        WGSPoint3D.CreatePoint(0.631987283352491, -2.00743753668608)
      };

      return WGS84Fence.CreateWGS84Fence(points.ToArray());
    }

    private (TagFileRequestLegacy fileRequest, TWGS84FenceContainer container) CreateRequest(byte[] tagData, long projectId, bool createFence = true)
    {
      WGS84Fence fence = null;

      if (createFence)
      {
        fence = CreateAFence();
      }

      var request = TagFileRequestLegacy.CreateTagFile("0415J010SW--HOUK IR 29 16--170731225438.tag", tagData, projectId, fence, 1244020666025812, false, false);
      var fenceContainer = request.Boundary != null
        ? RaptorConverters.convertWGS84Fence(request.Boundary)
        : TWGS84FenceContainer.Null();

      return (fileRequest: request, container: fenceContainer);
    }

    [TestMethod]
    public void NonDirectRaptor__TagFileSubmitterSuccessful()
    {
      var requestData = CreateRequest(new byte[] { 0x1, 0x2, 0x3 }, 544);

      // create the mock PDSClient with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();

      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient().SubmitTAGFileToTAGFileProcessor(
         requestData.fileRequest.FileName,
         new MemoryStream(requestData.fileRequest.Data),
         requestData.fileRequest.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
         0,
         0,
         requestData.container,
         "")).Returns(TTAGProcServerProcessResult.tpsprOK);

      // create submitter
      var submitter = RequestExecutorContainerFactory.Build<TagFileExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object);

      // Act
      var result = submitter.Process(requestData.fileRequest);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage);
    }

    /// <summary>
    /// Tag files can be manually or automatically submitted.
    /// Manual submission requires a project Id be provided and the service determines the project boundary.
    /// Automatic submission requires no project Id be send, and the service therefore cannot resolve a boundary.
    ///
    /// Any variation of this combation is illegal and should be stopped by the executor.
    /// </summary>
    [TestMethod]
    [DataRow(544, false, "Failed to process tagfile with error: Manual tag file submissions must include a boundary fence.")]
    [DataRow(VelociraptorConstants.NO_PROJECT_ID, true, "Failed to process tagfile with error: Automatic tag file submissions cannot include boundary fence.")]
    public void NonDirectRaptor_Should_fail_if_project_and_boundary_dont_validate(long projectId, bool createFence, string exceptionMessage)
    {
      var requestData = CreateRequest(new byte[] { 0x1, 0x2, 0x3 }, projectId, createFence);

      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();

      var submitter = RequestExecutorContainerFactory.Build<TagFileExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object);

      Assert.ThrowsException<ServiceException>(() => submitter.Process(requestData.fileRequest), exceptionMessage);
    }

    [TestMethod]
    public async Task DirectTRex__TagFileSubmitterSuccessfull()
    {
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      var request = new CompactionTagFileRequest()
      {
        ProjectUid = null,
        FileName = "Machine Name--whatever --161230235959",
        Data = tagFileContent,
        OrgId = string.Empty
      };
      
      var configStore = new Mock<IConfigurationStore>();
      configStore.Setup(c => c.GetValueString("ENABLE_TFA_SERVICE")).Returns("true");

      // create the Trex mocks with successful result
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprOK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileDirect(request, It.IsAny <IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);
      var customHeaders = new Dictionary<string, string>();
      
      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionTRexExecutor>(_logger, null, null, null, null, null, null, null, null, mockTRexTagFileProxy.Object, customHeaders);

      var result = await submitter.ProcessAsync(request).ConfigureAwait(false);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage);
    }

    [TestMethod]
    public async Task DirectTRex__TagFileSubmitterUnsuccessfull()
    {
      var tagFileContent = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 };
      var request = new CompactionTagFileRequest()
      {
        ProjectUid = null,
        FileName = "Machine Name--whatever --161230235959",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      // create the Trex mocks with successful result
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprFailedEventDateValidation));
      mockTRexTagFileProxy.Setup(s => s.SendTagFileDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionTRexExecutor>(_logger, null, null, null, null, null, null, null, null, mockTRexTagFileProxy.Object, _customHeaders);

      var result = await submitter.ProcessAsync(request).ConfigureAwait(false);
      
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Code == (int) TTAGProcServerProcessResult.tpsprFailedEventDateValidation);
    }

    [TestMethod]
    public async Task DirectTRex__TagFileSubmitterThrowsException()
    {
      var tagFileContent = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 };
      var request = new CompactionTagFileRequest()
      {
        ProjectUid = null,
        FileName = "Machine Name--whatever --161230235959",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      // alternative logging mock
      //var mockLoggerFactory = new Mock<ILoggerFactory>();
      //mockLoggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>()))
      //  .Returns(Mock.Of<ILogger<RequestExecutorContainer>>());

      // create the Trex mocks with successful result
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ThrowsAsync(new NotImplementedException());
      
      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionTRexExecutor>(_logger, null, null, null, null, null, null, null, null, mockTRexTagFileProxy.Object, _customHeaders);

      var result = await submitter.ProcessAsync(request).ConfigureAwait(false);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Code == (int)TTAGProcServerProcessResult.tpsprUnknown);
    }

    [TestMethod]
    public void DirectRaptor__TagFileSubmitterSuccessfull()
    {
      // create the Raptor mocks with successful result
      var request = CreateRequest(new byte[] { 0x1, 0x2, 0x3, 0x4 }, 544);
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>()));

      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient().SubmitTAGFileToTAGFileProcessor(
        request.fileRequest.FileName,
        new MemoryStream(request.fileRequest.Data),
        request.fileRequest.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
        0,
        0,
        request.container,
        "")).Returns(TTAGProcServerProcessResult.tpsprOK);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionRaptorExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object, _configStore, null, null, null, null, mockTransferProxy.Object);

      var result = submitter.Process(request.fileRequest);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage);
    }

    [TestMethod]
    public void DirectRaptor__TagFileSubmitterUnsuccessfull()
    {
      // create the Raptor mocks with successful result
      var request = CreateRequest(new byte[] { 0x1, 0x2, 0x3, 0x4 }, 544);
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>()));

      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
        .SubmitTAGFileToTAGFileProcessor(
          It.IsAny<string>(),
          It.IsAny<MemoryStream>(),
          It.IsAny<long>(),
          It.IsAny<TDateTime>(),
          It.IsAny<TDateTime>(),
          It.IsAny<long>(),
          It.IsAny<TWGS84FenceContainer>(),
          It.IsAny<string>()
          )).Returns(TTAGProcServerProcessResult.tpsprFileReaderCorruptedTAGFileData);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionRaptorExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object, _configStore, null, null, null, null, mockTransferProxy.Object);

      var result = submitter.Process(request.fileRequest);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Code == (int) TTAGProcServerProcessResult.tpsprFileReaderCorruptedTAGFileData);
    }

    [TestMethod]
    public void DirectRaptor__TagFileSubmitterThrowsException()
    {
      // create the Raptor mocks with successful result
      var request = CreateRequest(new byte[] { 0x1, 0x2, 0x3, 0x4 }, 544);
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>()));

      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
        .SubmitTAGFileToTAGFileProcessor(
          It.IsAny<string>(),
          It.IsAny<MemoryStream>(),
          It.IsAny<long>(),
          It.IsAny<TDateTime>(),
          It.IsAny<TDateTime>(),
          It.IsAny<long>(),
          It.IsAny<TWGS84FenceContainer>(),
          It.IsAny<string>()
        )).Throws<NotImplementedException>();

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionRaptorExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object, _configStore, null, null, null, null, mockTransferProxy.Object);

      var result = submitter.Process(request.fileRequest);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Code == (int)TTAGProcServerProcessResult.tpsprUnknown);
    }
  }
}
