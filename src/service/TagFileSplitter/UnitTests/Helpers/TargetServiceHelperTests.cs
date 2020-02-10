using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using CCSS.TagFileSplitter.Models;
using CCSS.TagFileSplitter.WebAPI.Common.Executors;
using CCSS.TagFileSplitter.WebAPI.Common.Helpers;
using CCSS.TagFileSplitter.WebAPI.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using Serilog;
using VSS.Serilog.Extensions;
using Moq;
using Xunit;
using FluentAssertions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Constants;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Models;
using VSS.Common.Exceptions;
using VSS.MasterData.Proxies.Interfaces;
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
        FileName = "Serial--Machine Name--161230235959.tag",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      var vssServiceName = ServiceNameConstants.PRODUCTIVITY3D_VSS_SERVICE;
      var targetVssService = new TargetService(vssServiceName, ApiVersion.V2.ToString(), "tagfiles", "tagFiles/direct");
      var customHeaders = new Dictionary<string, string>();
      var expected3dPmResponseCode = 0;

      var serviceResult = new ServiceResult() {Endpoint = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-3dproductivityvss/2.0", Type = ServiceResultType.Configuration};
      var targetServiceUrl = $"{serviceResult}/{targetVssService.DirectRoute}";
      var mockServiceResolution = new Mock<IServiceResolution>();
      mockServiceResolution.Setup(r => r.ResolveService(It.IsAny<string>())).ReturnsAsync(serviceResult);
      mockServiceResolution.Setup(r => r.ResolveRemoteServiceEndpoint(It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), It.IsAny<string>(), It.IsAny<IList<KeyValuePair<string, string>>>()))
        .ReturnsAsync(targetServiceUrl);

      var mockGenericHttpProxy = new Mock<IGenericHttpProxy>();
      var threeDPmResult = new TargetServiceResponse(vssServiceName, expected3dPmResponseCode, ContractExecutionResult.DefaultMessage);
      mockGenericHttpProxy.Setup(p => p.ExecuteGenericHttpRequest<TargetServiceResponse>
        (It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(),
        It.IsAny<IDictionary<string, string>>(), It.IsAny<int?>()))
        .ReturnsAsync(threeDPmResult);

      var response = TargetServiceHelper.SendTagFileTo3dPmService(request, mockServiceResolution.Object, mockGenericHttpProxy.Object, 
        targetVssService.ServiceName, targetVssService.TargetApiVersion, 
        targetVssService.DirectRoute, _logger, customHeaders);

      response.Should().NotBeNull();
      response.Result.ServiceName.Should().Be(targetVssService.ServiceName);
      response.Result.StatusCode.Should().Be(HttpStatusCode.OK); // todoJeannie
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
        FileName = "Serial--Machine Name--161230235959.tag",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      var ccssServiceName = ServiceNameConstants.PRODUCTIVITY3D_SERVICE;
      var targetCcssService = new TargetService(ccssServiceName, ApiVersion.V2.ToString(), "tagfiles", "tagFiles/direct");
      var customHeaders = new Dictionary<string, string>();
      var expected3dPmResponseCode = 0;

      var serviceResult = new ServiceResult() { Endpoint = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-3dproductivityccss/2.0", Type = ServiceResultType.Configuration };
      var targetServiceUrl = $"{serviceResult}/{targetCcssService.DirectRoute}";
      var mockServiceResolution = new Mock<IServiceResolution>();
      mockServiceResolution.Setup(r => r.ResolveService(It.IsAny<string>())).ReturnsAsync(serviceResult);
      mockServiceResolution.Setup(r => r.ResolveRemoteServiceEndpoint(It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), It.IsAny<string>(), It.IsAny<IList<KeyValuePair<string, string>>>()))
        .ReturnsAsync(targetServiceUrl);

      var mockGenericHttpProxy = new Mock<IGenericHttpProxy>();
      var threeDPmResult = new TargetServiceResponse(ccssServiceName, expected3dPmResponseCode, ContractExecutionResult.DefaultMessage);
      mockGenericHttpProxy.Setup(p => p.ExecuteGenericHttpRequest<TargetServiceResponse>
        (It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>(), It.IsAny<int?>()))
        .ReturnsAsync(threeDPmResult);

      var response = TargetServiceHelper.SendTagFileTo3dPmService(request, mockServiceResolution.Object, mockGenericHttpProxy.Object,
        targetCcssService.ServiceName, targetCcssService.TargetApiVersion, 
        targetCcssService.DirectRoute, _logger, customHeaders);

      response.Should().NotBeNull();
      response.Result.ServiceName.Should().Be(targetCcssService.ServiceName);
      response.Result.StatusCode.Should().Be(HttpStatusCode.OK); 
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
        FileName = "Serial--Machine Name--161230235959.tag",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      var ccssServiceName = ServiceNameConstants.PRODUCTIVITY3D_SERVICE;
      var targetCcssService = new TargetService(ccssServiceName, ApiVersion.V2.ToString(), "tagfiles", "tagFiles/direct");
      var customHeaders = new Dictionary<string, string>();
      var expected3dPmResponseCode = (int)TAGProcServerProcessResultCode.FailedValidation;

      var serviceResult = new ServiceResult() { Endpoint = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-3dproductivityvss/2.0", Type = ServiceResultType.Configuration };
      var targetServiceUrl = $"{serviceResult}/{targetCcssService.DirectRoute}";
      var mockServiceResolution = new Mock<IServiceResolution>();
      mockServiceResolution.Setup(r => r.ResolveService(It.IsAny<string>())).ReturnsAsync(serviceResult);
      mockServiceResolution.Setup(r => r.ResolveRemoteServiceEndpoint(It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), It.IsAny<string>(), It.IsAny<IList<KeyValuePair<string, string>>>()))
        .ReturnsAsync(targetServiceUrl);

      var threeDPmResult = new TargetServiceResponse(ccssServiceName, expected3dPmResponseCode, ContractExecutionResult.DefaultMessage);
      var mockGenericHttpProxy = new Mock<IGenericHttpProxy>();
      mockGenericHttpProxy.Setup(p => p.ExecuteGenericHttpRequest<TargetServiceResponse>
        (It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>(), It.IsAny<int?>()))
        .ReturnsAsync(threeDPmResult);

      var response = TargetServiceHelper.SendTagFileTo3dPmService(request, mockServiceResolution.Object, mockGenericHttpProxy.Object,
        targetCcssService.ServiceName, targetCcssService.TargetApiVersion, targetCcssService.AutoRoute, _logger, customHeaders);

      response.Should().NotBeNull();
      response.Result.ServiceName.Should().Be(targetCcssService.ServiceName);
      response.Result.StatusCode.Should().Be(threeDPmResult.StatusCode); 
      response.Result.Code.Should().Be(expected3dPmResponseCode);
    }

    [Fact]
    public void SendTagFileToTargetService_TargetServiceException()
    {
      var tagFileContent = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 };
      var request = new CompactionTagFileRequest
      {
        ProjectId = null,
        ProjectUid = null,
        FileName = "Serial--Machine Name--161230235959.tag",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      var ccssServiceName = ServiceNameConstants.PRODUCTIVITY3D_SERVICE;
      var targetCcssService = new TargetService(ccssServiceName, ApiVersion.V2.ToString(), "tagfiles", "tagFiles/direct");
      var customHeaders = new Dictionary<string, string>();

      var serviceResult = new ServiceResult() { Endpoint = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-3dproductivityccss/2.0", Type = ServiceResultType.Configuration };
      var targetServiceUrl = $"{serviceResult}/{targetCcssService.DirectRoute}";
      var mockServiceResolution = new Mock<IServiceResolution>();
      mockServiceResolution.Setup(r => r.ResolveService(It.IsAny<string>())).ReturnsAsync(serviceResult);
      mockServiceResolution.Setup(r => r.ResolveRemoteServiceEndpoint(It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), It.IsAny<string>(), It.IsAny<IList<KeyValuePair<string, string>>>()))
        .ReturnsAsync(targetServiceUrl);

      var mockGenericHttpProxy = new Mock<IGenericHttpProxy>();
      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Failed to process tagfile with error: Manual tag file submissions must include a boundary fence."));
      mockGenericHttpProxy.Setup(p => p.ExecuteGenericHttpRequest<TargetServiceResponse>
        (It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>(), It.IsAny<int?>()))
        .ThrowsAsync(exception);

      var response = TargetServiceHelper.SendTagFileTo3dPmService(request, mockServiceResolution.Object, mockGenericHttpProxy.Object,
        targetCcssService.ServiceName, targetCcssService.TargetApiVersion, targetCcssService.AutoRoute, _logger, customHeaders);

      response.Should().NotBeNull();
      response.Result.ServiceName.Should().Be(targetCcssService.ServiceName);
      response.Result.StatusCode.Should().Be(exception.Code);
      response.Result.Code.Should().Be(exception.GetResult.Code);
      response.Result.Message.Should().Be(exception.GetResult.Message);
    }

    [Fact]
    public void SendTagFileToTargetService_ServiceResolverException()
    {
      var tagFileContent = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 };
      var request = new CompactionTagFileRequest
      {
        ProjectId = null,
        ProjectUid = null,
        FileName = "Serial--Machine Name--161230235959.tag",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      var ccssServiceName = ServiceNameConstants.PRODUCTIVITY3D_SERVICE;
      var targetCcssService = new TargetService(ccssServiceName, ApiVersion.V2.ToString(), "tagfiles", "tagFiles/direct");
      var customHeaders = new Dictionary<string, string>();

      var serviceResult = new ServiceResult() { Endpoint = string.Empty, Type = ServiceResultType.Configuration };
      var targetServiceUrl = $"{serviceResult}/{targetCcssService.DirectRoute}";
      var mockServiceResolution = new Mock<IServiceResolution>();
      mockServiceResolution.Setup(r => r.ResolveService(It.IsAny<string>())).ReturnsAsync(serviceResult);
      mockServiceResolution.Setup(r => r.ResolveRemoteServiceEndpoint(It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), It.IsAny<string>(), It.IsAny<IList<KeyValuePair<string, string>>>()))
        .ReturnsAsync(targetServiceUrl);

      var mockGenericHttpProxy = new Mock<IGenericHttpProxy>();
      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          $"SendTagFileTo3dPmService: Unable to resolve target service url. serviceName: {ccssServiceName}"));
      mockGenericHttpProxy.Setup(p => p.ExecuteGenericHttpRequest<TargetServiceResponse>
        (It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>(), It.IsAny<int?>()))
        .ThrowsAsync(exception);

      var response = TargetServiceHelper.SendTagFileTo3dPmService(request, mockServiceResolution.Object, mockGenericHttpProxy.Object,
        targetCcssService.ServiceName, targetCcssService.TargetApiVersion, targetCcssService.AutoRoute, _logger, customHeaders);

      response.Should().NotBeNull();
      response.Result.ServiceName.Should().Be(targetCcssService.ServiceName);
      response.Result.StatusCode.Should().Be(exception.Code);
      response.Result.Code.Should().Be(exception.GetResult.Code);
      response.Result.Message.Should().Be(exception.GetResult.Message);
    }

    [Fact]
    public void SendTagFileToTargetService_ResolveEndpointException()
    {
      var tagFileContent = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 };
      var request = new CompactionTagFileRequest
      {
        ProjectId = null,
        ProjectUid = null,
        FileName = "Serial--Machine Name--161230235959.tag",
        Data = tagFileContent,
        OrgId = string.Empty
      };

      var ccssServiceName = ServiceNameConstants.PRODUCTIVITY3D_SERVICE;
      var targetCcssService = new TargetService(ccssServiceName, ApiVersion.V2.ToString(), "tagfiles", "tagFiles/direct");
      var customHeaders = new Dictionary<string, string>();

      var targetServiceUrl = "https://api-stg.trimble.com/t/trimble.com/vss-alpha-3dproductivityccss/2.0/tagfiles";
      var mockServiceResolution = new Mock<IServiceResolution>();
      mockServiceResolution.Setup(r => r.ResolveRemoteServiceEndpoint(It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), It.IsAny<string>(), It.IsAny<IList<KeyValuePair<string, string>>>()))
        .ReturnsAsync(targetServiceUrl);

      var mockGenericHttpProxy = new Mock<IGenericHttpProxy>();
      var exception = new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          $"SendTagFileTo3dPmService: Unable to resolve target service endpoint: {ccssServiceName}"));
      mockGenericHttpProxy.Setup(p => p.ExecuteGenericHttpRequest<TargetServiceResponse>
        (It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>(), It.IsAny<int?>()))
        .ThrowsAsync(exception);

      var response = TargetServiceHelper.SendTagFileTo3dPmService(request, mockServiceResolution.Object, mockGenericHttpProxy.Object,
        targetCcssService.ServiceName, targetCcssService.TargetApiVersion, targetCcssService.AutoRoute, _logger, customHeaders);

      response.Should().NotBeNull();
      response.Result.ServiceName.Should().Be(targetCcssService.ServiceName);
      response.Result.StatusCode.Should().Be(exception.Code);
      response.Result.Code.Should().Be(exception.GetResult.Code);
      response.Result.Message.Should().Be(exception.GetResult.Message);
    }

  }
}
