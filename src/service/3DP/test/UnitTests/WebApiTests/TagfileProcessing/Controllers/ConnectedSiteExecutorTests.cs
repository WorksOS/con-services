using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ShineOn.Rtl;
using TAGProcServiceDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.TagfileProcessing.Controllers
{
  [TestClass]
  public class ConnectedSiteExecutorTests
  {
    private static IServiceProvider _serviceProvider;
    private static ILoggerFactory _logger;
    private static Dictionary<string, string> _customHeaders;

    private static CompactionTagFileRequestExtended request = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended
    (
      new CompactionTagFileRequest()
      {
        ProjectId = 554,
        ProjectUid = Guid.NewGuid(),
        FileName = "Machine Name--whatever --161230235959",
        Data = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 },
        OrgId = string.Empty
      },
      new WGS84Fence(null)
    );

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
    public async Task NonDirectTagFileSubmitter_ConnectedSite_Switch()
    {
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Returns(TTAGProcServerProcessResult.tpsprOK);

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_CONNECTED_SITE_GATEWAY")).Returns("false");

      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileNonDirectToConnectedSite(request, It.IsAny<IDictionary<string, string>>()))
        .Throws(new Exception("I should not be called"));

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileConnectedSiteSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, null, mockTRexTagFileProxy.Object, null, _customHeaders);

      var result = await submitter.ProcessAsync(request).ConfigureAwait(false);

      result.Should().NotBeNull();
      result.Message.Should().Be(TagFileConnectedSiteSubmissionExecutor.DISABLED_MESSAGE);
      mockTRexTagFileProxy.Verify(s => s.SendTagFileNonDirectToConnectedSite(
        It.IsAny<CompactionTagFileRequestExtended>(), It.IsAny<IDictionary<string, string>>()), Times.Never());
    }


    [TestMethod]
    public async Task NonDirectTagFileSubmitter_ConnectedSite_Fail()
    {
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
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_CONNECTED_SITE_GATEWAY")).Returns("true");
      var connectedSiteResult =
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
        TagFileConnectedSiteSubmissionExecutor.DEFAULT_ERROR_MESSAGE);
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();

      mockTRexTagFileProxy.Setup(s => s.SendTagFileNonDirectToConnectedSite(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(connectedSiteResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileConnectedSiteSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, null, mockTRexTagFileProxy.Object, null, _customHeaders);

      var result = await submitter.ProcessAsync(request).ConfigureAwait(false);

      result.Should().NotBeNull();
      result.Message.Should().Be(TagFileConnectedSiteSubmissionExecutor.DEFAULT_ERROR_MESSAGE);

      mockTRexTagFileProxy.Verify(s => s.SendTagFileNonDirectToConnectedSite(
        request, new Dictionary<string, string>()), Times.Once());
    }


    [TestMethod]
    public async Task NonDirectTagFileSubmitter_ConnectedSite_Successful()
    {

      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient()
          .SubmitTAGFileToTAGFileProcessor(
            It.IsAny<string>(), It.IsAny<MemoryStream>(), It.IsAny<long>(), It.IsAny<TDateTime>(),
            It.IsAny<TDateTime>(),
            It.IsAny<long>(), It.IsAny<TWGS84FenceContainer>(), It.IsAny<string>()))
        .Returns(TTAGProcServerProcessResult.tpsprOK);

      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_CONNECTED_SITE_GATEWAY")).Returns("true");
      var trexGatewayResult =
        TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprOK));
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFileNonDirectToConnectedSite(request, It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileConnectedSiteSubmissionExecutor>(_logger, mockRaptorClient.Object, mockTagProcessor.Object,
          mockConfigStore.Object, null, null, null, null, null, mockTRexTagFileProxy.Object, null, _customHeaders);

      var result = await submitter.ProcessAsync(request).ConfigureAwait(false);

      result.Should().NotBeNull();
      result.Message.Should().Be(ContractExecutionResult.DefaultMessage);
      mockTRexTagFileProxy.Verify(s => s.SendTagFileNonDirectToConnectedSite(
        request, It.IsAny<IDictionary<string, string>>()), Times.Once());
    }
  }
}
