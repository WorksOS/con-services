using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
#if RAPTOR
using ShineOn.Rtl;
using TAGProcServiceDecls;
using VLPDDecls;
#endif
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApiTests.TagfileProcessing.Controllers
{
  [TestClass]
  public class TagFileProcessingNonDirectExecutorTests
  {
    private static IServiceProvider _serviceProvider;
    private static ILoggerFactory _logger;
    private static Dictionary<string, string> _customHeaders;
    private string _s3BucketName = "ccss-stg-gcs900tagfile-archives";

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
      _customHeaders = new Dictionary<string, string>();
    }

    [TestMethod]
    public async Task NonDirectTagFileSubmitter_RaptorAndTRex_Manual_Successful()
    {
      var projectUid = Guid.NewGuid();
      var resolvedLegacyProjectId = 544;
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      var request = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended
      (
        new CompactionTagFileRequest
        {
          ProjectId = resolvedLegacyProjectId,
          ProjectUid = projectUid,
          FileName = "Serial--Machine Name--161230235959.tag",
          Data = tagFileContent,
          OrgId = string.Empty
        },
        CreateAFence()
      );

      // We need to validate the tag file is uploaded correctly to both TCC and S3
      const string expectedS3Filename = "whatever";
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

#if RAPTOR
      // create the mock PDSClient with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Returns(TTAGProcServerProcessResult.tpsprOK);
#endif

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueString("AWS_NONDIRECT_TAGFILE_BUCKET_NAME", It.IsAny<string>())).Returns(_s3BucketName);
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TAGProcServerProcessResultCode.OK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileNonDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(_logger,
#if RAPTOR
          mockRaptorClient.Object, mockTagProcessor.Object,
#endif
          mockConfigStore.Object, transferProxy: mockTransferProxy.Object, tRexTagFileProxy: mockTRexTagFileProxy.Object, customHeaders: _customHeaders);

      var result = await submitter.ProcessAsync(request);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage);

      // Ensure we have uploaded our file to S3, with the correct filename
      mockTransferProxy.Verify(m => m.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task NonDirectTagFileSubmitter_RaptorAndTRex_Auto_Successful()
    {
      Guid? projectUid = null;
      var resolvedLegacyProjectId = -1;
      var tagFileContent = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 };
      var request = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended
      (
        new CompactionTagFileRequest
        {
          ProjectId = resolvedLegacyProjectId,
          ProjectUid = projectUid,
          FileName = "Serial--Machine Name--161230235959.tag",
          Data = tagFileContent,
          OrgId = Guid.NewGuid().ToString()
        },
        CreateAFence()
      );

      // We need to validate the tag file is uploaded correctly to both TCC and S3
      string expectedS3Filename = $"{request.OrgId}/Machine Name/Production-Data (Archived)/Serial--Machine Name--161230/Serial--Machine Name--161230235959.tag";
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

#if RAPTOR
      // create the mock PDSClient with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Returns(TTAGProcServerProcessResult.tpsprOK);
#endif

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueString("AWS_NONDIRECT_TAGFILE_BUCKET_NAME", It.IsAny<string>())).Returns(_s3BucketName);
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TAGProcServerProcessResultCode.OK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileNonDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(_logger,
#if RAPTOR
          mockRaptorClient.Object, mockTagProcessor.Object,
#endif
          mockConfigStore.Object, transferProxy: mockTransferProxy.Object, tRexTagFileProxy: mockTRexTagFileProxy.Object, customHeaders: _customHeaders);

      var result = await submitter.ProcessAsync(request);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage);

      // Ensure we have uploaded our file to S3, with the correct filename
      mockTransferProxy.Verify(m => m.UploadToBucket(It.IsAny<Stream>(), It.Is<string>(s => s == expectedS3Filename), It.Is<string>(s => s == _s3BucketName)), Times.Once);
    }

    [TestMethod]
    public async Task NonDirectTagFileSubmitter_TRex_UnSuccessful()
    {
      var projectUid = Guid.NewGuid();
      var resolvedLegacyProjectId = 544;
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      var request = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended
      (
        new CompactionTagFileRequest
        {
          ProjectId = resolvedLegacyProjectId,
          ProjectUid = projectUid,
          FileName = "Serial--Machine Name--161230235959.tag",
          Data = tagFileContent,
          OrgId = string.Empty
        },
        CreateAFence()
      );

      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

#if RAPTOR
      // create the mock PDSClient with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Returns(TTAGProcServerProcessResult.tpsprOK);
#endif
      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_RAPTOR_GATEWAY_TAGFILE")).Returns(false);
      mockConfigStore.Setup(x => x.GetValueString("AWS_NONDIRECT_TAGFILE_BUCKET_NAME", It.IsAny<string>())).Returns(_s3BucketName);
      var trexGatewayResult =
        new ContractExecutionResult((int)TRexTagFileResultCode.TFAManualProjectNotFound, "Unable to find the Project requested");

      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileNonDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(_logger,
#if RAPTOR
          mockRaptorClient.Object, mockTagProcessor.Object,
#endif
          mockConfigStore.Object, transferProxy: mockTransferProxy.Object, tRexTagFileProxy: mockTRexTagFileProxy.Object, customHeaders: _customHeaders);

      var result = await submitter.ProcessAsync(request);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Code == (int)TRexTagFileResultCode.TFAManualProjectNotFound);
      Assert.IsTrue(result.Message == "Unable to find the Project requested");
    }

#if RAPTOR
    [TestMethod]
    public async Task NonDirectTagFileSubmitter_Raptor_UnSuccessful()
    {
      var projectUid = Guid.NewGuid();
      var resolvedLegacyProjectId = 544;
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      var request = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended
      (
        new CompactionTagFileRequest
        {
          ProjectId = resolvedLegacyProjectId,
          ProjectUid = projectUid,
          FileName = "Serial--Machine Name--161230235959.tag",
          Data = tagFileContent,
          OrgId = string.Empty
        },
        CreateAFence()
      );

      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

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
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_RAPTOR_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueString("AWS_NONDIRECT_TAGFILE_BUCKET_NAME", It.IsAny<string>())).Returns(_s3BucketName);
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TAGProcServerProcessResultCode.OK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileNonDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, mockTransferProxy.Object, mockTRexTagFileProxy.Object, null, customHeaders: _customHeaders);

      var result = await submitter.ProcessAsync(request);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Code == (int) TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions + 2000);
      Assert.AreEqual($"Failed to process tagfile '{request.FileName}', with error: OnChooseMachine. Machine Subscriptions Invalid.",
        result.Message);
    }

    [TestMethod]
    public async Task NonDirectTagFileSubmitter_Raptor_ThrowsException()
    {
      var projectUid = Guid.NewGuid();
      var resolvedLegacyProjectId = 544;
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      var request = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended
      (
        new CompactionTagFileRequest
        {
          ProjectId  = resolvedLegacyProjectId,
          ProjectUid = projectUid,
          FileName = "Serial--Machine Name--161230235959.tag",
          Data = tagFileContent,
          OrgId = string.Empty
        },
        CreateAFence()
      );

      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

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
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_RAPTOR_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueString("AWS_NONDIRECT_TAGFILE_BUCKET_NAME", It.IsAny<string>())).Returns(_s3BucketName);
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TAGProcServerProcessResultCode.OK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileNonDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, transferProxy: mockTransferProxy.Object, mockTRexTagFileProxy.Object, null, customHeaders: _customHeaders);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await submitter.ProcessAsync(request));
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code, "executor threw exception but incorrect httpStatus");
      var result = ex.GetResult;
      Assert.AreEqual((int) TTAGProcServerProcessResult.tpsprFileReaderCorruptedTAGFileData + 2000, result.Code,
        "executor threw exception but incorrect code");
      Assert.AreEqual(
        $"CallRaptorEndpoint: Failed to process tagfile '{request.FileName}', The TAG file was found to be corrupted on its pre-processing scan.",
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
        new CompactionTagFileRequest
        {
          ProjectId = resolvedLegacyProjectId,
          ProjectUid = projectUid,
          FileName = "Serial--Machine Name--161230235959.tag",
          Data = tagFileContent,
          OrgId = string.Empty
        },
        createFence ? CreateAFence() : null
      );

      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_TAGFILE")).Returns(false);
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_RAPTOR_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueString("AWS_NONDIRECT_TAGFILE_BUCKET_NAME", It.IsAny<string>())).Returns(_s3BucketName);
      var submitter = RequestExecutorContainerFactory.Build<TagFileNonDirectSubmissionExecutor>(_logger,
        mockRaptorClient.Object, mockTagProcessor.Object, mockConfigStore.Object);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await submitter.ProcessAsync(request));
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code, "executor threw exception but incorrect httpStatus");
      var result = ex.GetResult;
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, result.Code,
        "executor threw exception but incorrect code");
      Assert.AreEqual(exceptionMessage, result.Message, "executor threw exception but incorrect message");
    }
#endif

    private static WGS84Fence CreateAFence()
    {
      var points = new List<WGSPoint>
      {
        new WGSPoint(0.631986074660308, -2.00757760231466),
        new WGSPoint(0.631907507374149, -2.00758733949739),
        new WGSPoint(0.631904485465203, -2.00744352879854),
        new WGSPoint(0.631987283352491, -2.00743753668608)
      };

      return new WGS84Fence(points.ToArray());
    }
  }
}
