using System;
using System.Collections.Generic;
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
    private readonly IServiceCollection serviceCollection;
    private readonly IServiceProvider serviceProvider;

    private Mock<IWebRequest> mockWebRequest = new Mock<IWebRequest>();
    private Mock<IConfigurationStore> mockConfiguration = new Mock<IConfigurationStore>();
    private Mock<IServiceResolution> mockServiceResolution = new Mock<IServiceResolution>();

    public BaseProxyTests()
    {
      var name = this.GetType().FullName + ".log";
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure(name));

      serviceCollection = new ServiceCollection()
        .AddLogging()
        .AddSingleton(loggerFactory)
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>()
        .AddMemoryCache()
        .AddSingleton<IDataCache, InMemoryDataCache>()
        .AddSingleton(mockWebRequest.Object)
        .AddSingleton(mockConfiguration.Object)
        .AddSingleton(mockServiceResolution.Object);

      serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public void EnsureHeadersOverrideInternal()
    {
      const string originalServiceName = "test-service";
      const string route = "/not-important";
      var proxy = ActivatorUtilities.CreateInstance<MockProxy>(serviceProvider);
      Assert.NotNull(proxy);

      // Setup the proxy
      proxy.SetParameters(true, ApiService.Project, null, ApiVersion.V1, ApiType.Public);

      // Setup the mocks
      mockServiceResolution.Setup(m => m.GetServiceName(It.IsAny<ApiService>())).Returns(originalServiceName);

      // First Test, ensure we get the original service name (no override)
      var x = proxy.DoACall(route, null);

      // And verify we called with the correct service name
      mockServiceResolution.Verify(m =>
          m.ResolveLocalServiceEndpoint(
            It.Is<string>(s => string.Compare(s, originalServiceName, StringComparison.OrdinalIgnoreCase) == 0),
            ApiType.Public,
            ApiVersion.V1,
            It.Is<string>(s => string.Compare(s, route, StringComparison.OrdinalIgnoreCase) == 0),
            It.IsAny<IList<KeyValuePair<string, string>>>()), 
        Times.Once);

      // Reset the calls
      mockServiceResolution.Invocations.Clear();
      
      // Now make a call with the overriden service name
      var overrideServiceHeader = HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + originalServiceName;
      var newServiceName = "doesnt-matter";
      var x2 = proxy.DoACall(route, new Dictionary<string, string> {{overrideServiceHeader, newServiceName}});

      // Verify that we called the method with the correct name
      mockServiceResolution.Verify(m =>
          m.ResolveLocalServiceEndpoint(
            It.Is<string>(s => string.Compare(s, newServiceName, StringComparison.OrdinalIgnoreCase) == 0),
            ApiType.Public,
            ApiVersion.V1,
            It.Is<string>(s => string.Compare(s, route, StringComparison.OrdinalIgnoreCase) == 0),
            It.IsAny<IList<KeyValuePair<string, string>>>()), 
        Times.Once);

      // And didn't call the old method with the original service name
      mockServiceResolution.Verify(m =>
          m.ResolveLocalServiceEndpoint(
            It.Is<string>(s => string.Compare(s, originalServiceName, StringComparison.OrdinalIgnoreCase) == 0),
            ApiType.Public,
            ApiVersion.V1,
            It.Is<string>(s => string.Compare(s, route, StringComparison.OrdinalIgnoreCase) == 0),
            It.IsAny<IList<KeyValuePair<string, string>>>()), 
        Times.Never);
    }
  }
}
