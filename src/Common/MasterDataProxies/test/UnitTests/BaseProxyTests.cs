using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Cache.MemoryCache;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.MasterData.Proxies.UnitTests
{
  public class BaseProxyTests
  {
    private readonly IServiceCollection _serviceCollection;
    private readonly IServiceProvider _serviceProvider;

    private readonly Mock<IWebRequest> _mockWebRequest = new Mock<IWebRequest>();
    private readonly Mock<IConfigurationStore> _mockConfiguration = new Mock<IConfigurationStore>();
    private readonly Mock<IServiceResolution> _mockServiceResolution = new Mock<IServiceResolution>();

    public BaseProxyTests()
    {
      var name = GetType().FullName + ".log";
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure(name));

      _serviceCollection = new ServiceCollection()
        .AddLogging()
        .AddSingleton(loggerFactory)
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>()
        .AddMemoryCache()
        .AddSingleton<IDataCache, InMemoryDataCache>()
        .AddSingleton(_mockWebRequest.Object)
        .AddSingleton(_mockConfiguration.Object)
        .AddSingleton(_mockServiceResolution.Object);

      _serviceProvider = _serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public void EnsureHeadersOverrideInternal()
    {
      const string originalServiceName = "test-service";
      const string route = "/not-important";
      var proxy = ActivatorUtilities.CreateInstance<MockProxy>(_serviceProvider);
      Assert.NotNull(proxy);

      // Setup the proxy
      proxy.SetParameters(true, ApiService.Project, null, ApiVersion.V1, ApiType.Public);

      // Setup the mocks
      _mockServiceResolution.Setup(m => m.GetServiceName(It.IsAny<ApiService>())).Returns(originalServiceName);

      // First Test, ensure we get the original service name (no override)
      var x = proxy.DoACall(route, null);

      // And verify we called with the correct service name
      _mockServiceResolution.Verify(m =>
          m.ResolveLocalServiceEndpoint(
            It.Is<string>(s => string.Equals(s, originalServiceName, StringComparison.OrdinalIgnoreCase)),
            ApiType.Public,
            ApiVersion.V1,
            It.Is<string>(s => string.Equals(s, route, StringComparison.OrdinalIgnoreCase)),
            It.IsAny<IList<KeyValuePair<string, string>>>()),
        Times.Once);

      // Reset the calls
      _mockServiceResolution.Invocations.Clear();

      // Now make a call with the overriden service name
      const string overrideServiceHeader = HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + originalServiceName;
      const string newServiceName = "doesnt-matter";

      var x2 = proxy.DoACall(route, new HeaderDictionary { { overrideServiceHeader, newServiceName } });

      // Verify that we called the method with the correct name
      _mockServiceResolution.Verify(m =>
          m.ResolveLocalServiceEndpoint(
            It.Is<string>(s => string.Equals(s, newServiceName, StringComparison.OrdinalIgnoreCase)),
            ApiType.Public,
            ApiVersion.V1,
            It.Is<string>(s => string.Equals(s, route, StringComparison.OrdinalIgnoreCase)),
            It.IsAny<IList<KeyValuePair<string, string>>>()),
        Times.Once);

      // And didn't call the old method with the original service name
      _mockServiceResolution.Verify(m =>
          m.ResolveLocalServiceEndpoint(
            It.Is<string>(s => string.Equals(s, originalServiceName, StringComparison.OrdinalIgnoreCase)),
            ApiType.Public,
            ApiVersion.V1,
            It.Is<string>(s => string.Equals(s, route, StringComparison.OrdinalIgnoreCase)),
            It.IsAny<IList<KeyValuePair<string, string>>>()),
        Times.Never);
    }
  }
}
