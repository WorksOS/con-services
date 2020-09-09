using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Cache.MemoryCache;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Entitlements.Abstractions.Interfaces;
using VSS.Productivity3D.Entitlements.Common.Clients;
using VSS.Serilog.Extensions;
using Xunit;
using ILogger = Serilog.ILogger;

namespace VSS.Productivity3D.Entitlements.UnitTests
{
  public class EmsClientTests
  {
    private string baseUrl = "http://nowhere.really";
    private IServiceCollection serviceCollection;
    private Mock<IWebRequest> mockWebRequest = new Mock<IWebRequest>();
    private Mock<IServiceResolution> mockServiceResolution = new Mock<IServiceResolution>();

    private IServiceProvider ServiceProvider { get; set; }
    private ILogger Log { get; set; }

    public EmsClientTests()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure($"Tests::{ GetType().Name}.log"));
      serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
      serviceCollection.AddSingleton<IMemoryCache, MemoryCache>();
      serviceCollection.AddSingleton<IDataCache, InMemoryDataCache>();
      serviceCollection.AddSingleton(mockWebRequest.Object);
      serviceCollection.AddSingleton(mockServiceResolution.Object);
      serviceCollection.AddTransient<IEmsClient, EmsClient>();
      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    [Theory]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.NoContent)]
    public async Task GetEntitlements_Success(HttpStatusCode statusCode)
    {
      var userUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var sku = "some sku";
      var feature = "some feature";

      var route = $"/entitlements/members/{userUid}/activations";
      var expectedUrl = $"{baseUrl}{route}";

      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      mockWebRequest.Setup(s => s.ExecuteRequest(
          It.IsAny<string>(),
          It.IsAny<Stream>(),
          It.IsAny<IHeaderDictionary>(),
          It.IsAny<HttpMethod>(),
          It.IsAny<int?>(),
          It.IsAny<int>(),
          It.IsAny<bool>()))
        .Returns(Task.FromResult(statusCode));

      var client = ServiceProvider.GetRequiredService<IEmsClient>();
      var result = await client.GetEntitlements(userUid, customerUid, sku, feature);
      Assert.Equal(statusCode, result);
    }
  }
}
