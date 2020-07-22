using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApiTests.TagfileProcessing.Controllers
{
  [TestClass]
  public class TagFileProcessingExecutorTests
  {
    private static IServiceProvider _serviceProvider;
    private static ILoggerFactory _logger;
    private static HeaderDictionary _customHeaders;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      _serviceProvider = new ServiceCollection()
                        .AddLogging()
                        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.WebApi.Tests.log")))
                        .AddSingleton<IConfigurationStore, GenericConfiguration>()
                        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
                        .BuildServiceProvider();

      _logger = _serviceProvider.GetRequiredService<ILoggerFactory>();
      _customHeaders = new HeaderDictionary();
    }

    [TestMethod]
    public async Task NonDirectTagFileSubmitter_TRex_Successful()
    {
      var projectUid = Guid.NewGuid();
      var resolvedLegacyProjectId = 544;
      var tagFileContent = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 };
      var request = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended
      (
        new CompactionTagFileRequest
        {
          ProjectId = resolvedLegacyProjectId,
          ProjectUid = projectUid,
          FileName = "Machine Name--whatever --161230235959",
          Data = tagFileContent,
          OrgId = string.Empty
        },
        CreateAFence()
      );

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      var trexGatewayResult = new ContractExecutionResult();
      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFile(request, It.IsAny<IHeaderDictionary>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(_logger,
          mockConfigStore.Object, tRexTagFileProxy: mockTRexTagFileProxy.Object, customHeaders: _customHeaders);

      var result = await submitter.ProcessAsync(request);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage);
    }

    [TestMethod]
    public async Task NonDirectTagFileSubmitter_TRex_UnSuccessful()
    {
      var projectUid = Guid.NewGuid();
      var resolvedLegacyProjectId = 544;
      var tagFileContent = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 };
      var request = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended
      (
        new CompactionTagFileRequest
        {
          ProjectId = resolvedLegacyProjectId,
          ProjectUid = projectUid,
          FileName = "Machine Name--whatever --161230235959",
          Data = tagFileContent,
          OrgId = string.Empty
        },
        CreateAFence()
      );

      // create the Trex mocks with successful result
      var mockConfigStore = new Mock<IConfigurationStore>();
      var trexGatewayResult =
        new ContractExecutionResult((int)TRexTagFileResultCode.TFAManualProjectNotFound, "Unable to find the Project requested");

      var mockTRexTagFileProxy = new Mock<ITRexTagFileProxy>();
      mockTRexTagFileProxy.Setup(s => s.SendTagFile(request, It.IsAny<IHeaderDictionary>()))
        .ReturnsAsync(trexGatewayResult);

      var submitter = RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(_logger,
          mockConfigStore.Object, tRexTagFileProxy: mockTRexTagFileProxy.Object, customHeaders: _customHeaders);

      var result = await submitter.ProcessAsync(request);

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Code == (int)TRexTagFileResultCode.TFAManualProjectNotFound);
      Assert.IsTrue(result.Message == "Unable to find the Project requested");
    }


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
