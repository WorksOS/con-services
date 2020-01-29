using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using CCSS.TagFileSplitter.Models;
using CCSS.TagFileSplitter.WebAPI.Common.Helpers;
using CCSS.TagFileSplitter.WebAPI.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using Serilog;
using VSS.Serilog.Extensions;
using Moq;
using Xunit;
using FluentAssertions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Models.Enums;

namespace CCSS.TagFileSplitter.UnitTests.Helpers
{
  public class TargetServiceHelperTests
  {
    private IServiceProvider _serviceProvider;
    private Microsoft.Extensions.Logging.ILogger _logger;

    public TargetServiceHelperTests()
    {
      _serviceProvider = new ServiceCollection()
          .AddLogging()
          .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.TagFileSplitter.WebApi.Tests.log")))
          .AddSingleton<IConfigurationStore, GenericConfiguration>()
          .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
          .BuildServiceProvider(); 

      _logger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<TargetServiceHelperTests>();
    }

    [Fact]
    public void SendTagFileToTargetVSSService_Success()
    {
      var tagFileContent = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 };
      var request = new CompactionTagFileRequest
      {
        ProjectId = null,
        ProjectUid = null,
        FileName = "Machine Name--whatever --161230235959",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      var targetVSSService = new TargetService(ApiService.Productivity3DVSS.ToString(), "tagfiles", "tagFiles/direct");
      var customHeaders = new Dictionary<string, string>();
      var expected3dPmResponseCode = 0;
      var mockProductivity3dV2ProxyVSS = new Mock<IProductivity3dV2ProxyVSS>();
      var threeDPmResult = new TargetServiceResponse(targetVSSService.ApiService.ToString(), expected3dPmResponseCode, ContractExecutionResult.DefaultMessage);
      mockProductivity3dV2ProxyVSS.Setup(p => p.ExecuteGenericV2Request<TargetServiceResponse>
        (It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(),
        It.IsAny<IDictionary<string, string>>(), It.IsAny<int?>()))
        .ReturnsAsync(threeDPmResult);

      var response = TargetServiceHelper.SendTagFileTo3dPmService(request, null, mockProductivity3dV2ProxyVSS.Object, targetVSSService.ApiService, targetVSSService.DirectRoute, _logger, customHeaders);

      response.Should().NotBeNull();
      response.Result.ApiService.Should().Be(targetVSSService.ApiService.ToString());
      response.Result.Code.Should().Be(expected3dPmResponseCode);
    }

    [Fact]
    public void SendTagFileToTargetCCSSService_Success()
    {
      var tagFileContent = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 };
      var request = new CompactionTagFileRequest
      {
        ProjectId = null,
        ProjectUid = null,
        FileName = "Machine Name--whatever --161230235959",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      var targetCCSSService = new TargetService(ApiService.Productivity3D.ToString(), "tagfiles", "tagFiles/direct");
      var customHeaders = new Dictionary<string, string>();
      var expected3dPmResponseCode = 0;
      var mockProductivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      var threeDPmResult = new TargetServiceResponse(targetCCSSService.ApiService.ToString(), expected3dPmResponseCode, ContractExecutionResult.DefaultMessage);
      mockProductivity3dV2ProxyNotification.Setup(p => p.ExecuteGenericV2Request<TargetServiceResponse>
        (It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>(), It.IsAny<int?>()))
        .ReturnsAsync(threeDPmResult);

      var response = TargetServiceHelper.SendTagFileTo3dPmService(request, mockProductivity3dV2ProxyNotification.Object, null, targetCCSSService.ApiService, targetCCSSService.DirectRoute, _logger, customHeaders);

      response.Should().NotBeNull();
      response.Result.ApiService.Should().Be(targetCCSSService.ApiService.ToString());
      response.Result.Code.Should().Be(expected3dPmResponseCode);
    }

    [Fact]
    public void SendTagFileToTargetService_Failed()
    {
      var tagFileContent = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 };
      var request = new CompactionTagFileRequest
      {
        ProjectId = null,
        ProjectUid = null,
        FileName = "Machine Name--whatever --161230235959",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      var targetService = new TargetService(ApiService.Productivity3D.ToString(), "tagfiles", "tagFiles/direct");
      var customHeaders = new Dictionary<string, string>();
      var expected3dPmResponseCode = (int)TAGProcServerProcessResultCode.FailedValidation;
      var threeDPmResult = new TargetServiceResponse(targetService.ApiService.ToString(), expected3dPmResponseCode, ContractExecutionResult.DefaultMessage);
      var mockProductivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      mockProductivity3dV2ProxyNotification.Setup(p => p.ExecuteGenericV2Request<TargetServiceResponse>
        (It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>(), It.IsAny<int?>()))
        .ReturnsAsync(threeDPmResult);

      var response = TargetServiceHelper.SendTagFileTo3dPmService(request, mockProductivity3dV2ProxyNotification.Object, null, targetService.ApiService, targetService.AutoRoute, _logger, customHeaders);

      response.Should().NotBeNull();
      response.Result.ApiService.Should().Be(targetService.ApiService.ToString());
      response.Result.Code.Should().Be(expected3dPmResponseCode);
    }

    [Fact]
    public void SendTagFileToTargetService_Exception()
    {
      var tagFileContent = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 };
      var request = new CompactionTagFileRequest
      {
        ProjectId = null,
        ProjectUid = null,
        FileName = "Machine Name--whatever --161230235959",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      var targetService = new TargetService(ApiService.Productivity3D.ToString(), "tagfiles", "tagFiles/direct");
      var customHeaders = new Dictionary<string, string>();
      var mockProductivity3dV2ProxyNotification = new Mock<IProductivity3dV2ProxyNotification>();
      var exception = new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Failed to process tagfile with error: Manual tag file submissions must include a boundary fence."));
      mockProductivity3dV2ProxyNotification.Setup(p => p.ExecuteGenericV2Request<TargetServiceResponse>
        (It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>(), It.IsAny<int?>()))
        .ThrowsAsync(exception);

      var response = TargetServiceHelper.SendTagFileTo3dPmService(request, mockProductivity3dV2ProxyNotification.Object, null, targetService.ApiService, targetService.AutoRoute, _logger, customHeaders);

      response.Should().NotBeNull();
      response.Result.ApiService.Should().Be(targetService.ApiService.ToString());
      response.Result.Code.Should().Be(exception.GetResult.Code);
      response.Result.Message.Should().Be(exception.GetResult.Message);
    }

  }
}
