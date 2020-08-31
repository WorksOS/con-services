using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Cache.MemoryCache;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Request;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Response;
using VSS.Productivity3D.Entitlements.Proxy;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.Productivity3D.Entitlements.UnitTests
{
    public class EntitlementProxyTests
    {
      private readonly ILoggerFactory _loggerFactory;

      private Mock<IWebRequest> _mockWebRequest = new Mock<IWebRequest>();
      private Mock<IConfigurationStore> _mockConfiguration = new Mock<IConfigurationStore>();
      private Mock<IServiceResolution> _mockServiceResolution = new Mock<IServiceResolution>();
      private readonly IDataCache _dataCache;

      public EntitlementProxyTests()
      {
        var name = GetType().FullName + ".log";
        _loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure(name));
        _dataCache = new InMemoryDataCache(_loggerFactory, new MemoryCache(new MemoryCacheOptions()));
      }

      [Fact]
      public void ShouldCallIfDisabled()
      {
        // We should still get an OK Result when entitlements are checking
        // So we at least hit the entitlements service everytime
        var response = new EntitlementResponseModel {Feature = "test-feature", UserEmail = "test-email", UserUid = "test-uuid", OrganizationIdentifier = "test-org", IsEntitled = true};

        _mockConfiguration
          .Setup(m => m.GetValueBool(It.Is<string>(s => s == "ENABLE_ENTITLEMENTS_CHECKING"), It.IsAny<bool>()))
          .Returns(false);

        _mockWebRequest.Setup(m => m.ExecuteRequest<EntitlementResponseModel>(It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<IHeaderDictionary>(),
            It.Is<HttpMethod>(m => m == HttpMethod.Post),
            It.IsAny<int?>(),
            It.IsAny<int>(),
            It.IsAny<bool>()))
          .Returns(Task.FromResult(response));

        var proxy = new EntitlementProxy(_mockWebRequest.Object, _mockConfiguration.Object, _loggerFactory, _dataCache, _mockServiceResolution.Object);
        var result = proxy.IsEntitled(new EntitlementRequestModel 
          {Feature = "test-feature", Sku = "test-sku", OrganizationIdentifier = "test-org", UserEmail = "test-email", UserUid = "test-uuid"}).Result;

        result.Should().NotBeNull();
        result.IsEntitled.Should().BeTrue();
        result.UserEmail.Should().Be("test-email");
        result.UserUid.Should().Be("test-uuid");
        result.Feature.Should().Be("test-feature");
        result.Sku.Should().Be("test-sku");
        result.OrganizationIdentifier.Should().Be("test-org");
      }

      [Fact]
      public void ShouldCallIfEnabled()
      {
        var response = new EntitlementResponseModel();
        _mockConfiguration
          .Setup(m => m.GetValueBool(It.Is<string>(s => s == "ENABLE_ENTITLEMENTS_CHECKING"), It.IsAny<bool>()))
          .Returns(true);

        _mockWebRequest.Setup(m => m.ExecuteRequest<EntitlementResponseModel>(It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<IHeaderDictionary>(),
            It.Is<HttpMethod>(m => m == HttpMethod.Post),
            It.IsAny<int?>(),
            It.IsAny<int>(),
            It.IsAny<bool>()))
          .Returns(Task.FromResult(response));

        var proxy = new EntitlementProxy(_mockWebRequest.Object, _mockConfiguration.Object, _loggerFactory, _dataCache, _mockServiceResolution.Object);
        var result = proxy.IsEntitled(new EntitlementRequestModel
        {
          Feature = "test-feature",
          Sku = "test-sku",
          OrganizationIdentifier = "test-org",
          UserEmail = "test-email",
          UserUid = "test-uuid"
        }).Result;

        result.Should().NotBeNull();

        _mockWebRequest.Verify(m => m.ExecuteRequest<EntitlementResponseModel>(It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<IHeaderDictionary>(),
            It.Is<HttpMethod>(m => m == HttpMethod.Post),
            It.IsAny<int?>(),
            It.IsAny<int>(),
            It.IsAny<bool>()),
          Times.Once);
      }
    }
}
