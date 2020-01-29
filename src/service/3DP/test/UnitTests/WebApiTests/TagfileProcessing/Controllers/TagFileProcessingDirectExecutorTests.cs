using System;
using System.Collections.Generic;
using System.IO;
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
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApiTests.TagfileProcessing.Controllers
{
  [TestClass]
  public class TagFileProcessingDirectExecutorTests
  {
    private static IServiceProvider _serviceProvider;
    private static ILoggerFactory _logger;
    private static Dictionary<string, string> _customHeaders;
    private string _s3BucketName = "ccss-stg-directtagfile-archives";

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
    public async Task DirectTagFileSubmitter_RaptorAndTRex_Successful()
    {
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      long legacyProjectId = 1;
      var request = new CompactionTagFileRequest
                    {
        ProjectId = legacyProjectId,
        ProjectUid = null,
        FileName = "Serial--Machine Name--161230235959.tag",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      // We need to validate the tag file is uploaded correctly to both TCC and S3
      const string expectedS3Filename = "Serial--Machine Name/Production-Data (Archived)/Serial--Machine Name--161230/Serial--Machine Name--161230235959.tag";
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

#if RAPTOR
      // create the Raptor mocks with successful result
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
      mockConfigStore.Setup(x => x.GetValueString("AWS_DIRECT_TAGFILE_BUCKET_NAME", It.IsAny<string>())).Returns(_s3BucketName);
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TAGProcServerProcessResultCode.OK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionExecutor>(_logger,
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
    public async Task DirectTagFileSubmitter_TRex_Unsuccessful()
    {
      // trex result is ignored in overall result to final return is the Raptor success

      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      long legacyProjectId = 1;
      var request = new CompactionTagFileRequest
                    {
        ProjectId = legacyProjectId,
        ProjectUid = null,
        FileName = "Serial--Machine Name--161230235959.tag",
        Data = tagFileContent,
        OrgId = string.Empty
      };
#if RAPTOR
      // create the Raptor mocks with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
#endif
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));
#if RAPTOR
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Returns(TTAGProcServerProcessResult.tpsprOK);
#endif
      // create the Trex mocks with UNsuccessful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_RAPTOR_GATEWAY_TAGFILE")).Returns(false);
      mockConfigStore.Setup(x => x.GetValueString("AWS_DIRECT_TAGFILE_BUCKET_NAME", It.IsAny<string>())).Returns(_s3BucketName);
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      var trexGatewayResult =
        new ContractExecutionResult((int)TRexTagFileResultCode.TRexInvalidTagfile); 
      mockTRexTagFileProxy.Setup(s => s.SendTagFileDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionExecutor>(_logger,
#if RAPTOR
          mockRaptorClient.Object, mockTagProcessor.Object,
#endif
          mockConfigStore.Object, transferProxy: mockTransferProxy.Object, tRexTagFileProxy: mockTRexTagFileProxy.Object, customHeaders: _customHeaders);

      var result = await submitter.ProcessAsync(request);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == "The TAG file was found to be corrupted on its pre-processing scan.");
    }

#if RAPTOR
    [TestMethod]
    public async Task DirectTagFileSubmitter_Raptor_Unsuccessful()
    {
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      long legacyProjectId = 1;
      var request = new CompactionTagFileRequest
                    {
        ProjectId = legacyProjectId,
        ProjectUid = null,
        FileName = "Serial--Machine Name--161230235959.tag",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      // create the Raptor mocks with UNsuccessful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()
          ))
        .Returns(TTAGProcServerProcessResult.tpsprFileReaderCorruptedTAGFileData);

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_RAPTOR_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueString("AWS_DIRECT_TAGFILE_BUCKET_NAME", It.IsAny<string>())).Returns(_s3BucketName);
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TAGProcServerProcessResultCode.OK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, mockTransferProxy.Object, mockTRexTagFileProxy.Object, null,
          customHeaders:_customHeaders);

      var result = await submitter.ProcessAsync(request);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Code == (int) TTAGProcServerProcessResult.tpsprFileReaderCorruptedTAGFileData);
    }

    [TestMethod]
    public async Task DirectTagFileSubmitter_TRex_ThrowsException()
    {
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      long legacyProjectId = 1;
      var request = new CompactionTagFileRequest
                    {
        ProjectId = legacyProjectId,
        ProjectUid = null,
        FileName = "Serial--Machine Name--161230235959.tag",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      // create the Raptor mocks with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();

      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Returns(TTAGProcServerProcessResult.tpsprOK);

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_RAPTOR_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueString("AWS_DIRECT_TAGFILE_BUCKET_NAME", It.IsAny<string>())).Returns(_s3BucketName);
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ThrowsAsync(new NotImplementedException());

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionExecutor>(_logger,
          mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, transferProxy: mockTransferProxy.Object, tRexTagFileProxy: mockTRexTagFileProxy.Object, customHeaders: _customHeaders);

      var result = await submitter.ProcessAsync(request);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage);
    }

    [TestMethod]
    public async Task DirectTagFileSubmitter_Raptor_ThrowsException()
    {
      long legacyProjectId = 1; //  todo projectID validation rejects  0 and -1. Also projectId or UID MUST have a valid value;
                                //   VelociraptorConstants.NO_PROJECT_ID = -1
      var tagFileContent = new byte[] {0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9};
      var request = new CompactionTagFileRequest
                    {
        ProjectId = legacyProjectId, 
        ProjectUid = null,
        FileName = "Serial--Machine Name--161230235959.tag",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      // create the Raptor mocks with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockTransferProxy = new Mock<ITransferProxy>();
      mockTransferProxy.Setup(t => t.UploadToBucket(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));

      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Throws<NotImplementedException>();

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_TREX_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueBool("ENABLE_RAPTOR_GATEWAY_TAGFILE")).Returns(true);
      mockConfigStore.Setup(x => x.GetValueString("AWS_DIRECT_TAGFILE_BUCKET_NAME", It.IsAny<string>())).Returns(_s3BucketName);
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TAGProcServerProcessResultCode.OK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileDirect(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileDirectSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, mockTransferProxy.Object, mockTRexTagFileProxy.Object, null,
          customHeaders: _customHeaders);

      var result = await submitter.ProcessAsync(request);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Code == (int) TTAGProcServerProcessResult.tpsprUnknown);
    }
#endif
    
  }
}
