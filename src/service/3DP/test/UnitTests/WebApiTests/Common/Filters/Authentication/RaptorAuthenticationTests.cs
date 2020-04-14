using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Serilog.Extensions;

namespace VSS.Productivity3D.WebApiTests.Common.Filters.Authentication
{
  [TestClass]
  public class RaptorAuthenticationTests
  {
    [TestMethod]
    public void RequireCustomer_Yes_PatchesButHasACustomer()
    {
      var mockHttpContext = new Mock<HttpContext>();
      Task MockRequestDelegate(HttpContext context) => Task.FromResult(mockHttpContext.Object);

      var mockCwsAccountClient = new Mock<ICwsAccountClient>();
      var mockConfigStoreProxy = new Mock<IConfigurationStore>();
      var mockLoggerFactoryProxy = new Mock<ILoggerFactory>();
      var mockServiceExceptionProxy = new Mock<IServiceExceptionHandler>();
      var mockProjectProxy = new Mock<IProjectProxy>();

      var raptorAuthentication = new RaptorAuthentication(MockRequestDelegate,
        mockCwsAccountClient.Object, mockConfigStoreProxy.Object, mockLoggerFactoryProxy.Object, mockServiceExceptionProxy.Object, mockProjectProxy.Object);

      var request = new DefaultHttpContext().Request;
      request.Path = "/device/patches";
      request.Headers.Add(new KeyValuePair<string, StringValues>("X-VisionLink-CustomerUid", "gotACustomer"));
      request.Method = "GET";
      var isCustomerUidRequired = raptorAuthentication.RequireCustomerUid(request.HttpContext);
      isCustomerUidRequired.Should().BeTrue();
    }

    [TestMethod]
    public void RequireCustomer_Yes_PatchesButWrongMethod()
    {
      var mockHttpContext = new Mock<HttpContext>();
      Task MockRequestDelegate(HttpContext context) => Task.FromResult(mockHttpContext.Object);

      var mockCwsAccountClient = new Mock<ICwsAccountClient>();
      var mockConfigStoreProxy = new Mock<IConfigurationStore>();
      var mockLoggerFactoryProxy = new Mock<ILoggerFactory>();
      var mockServiceExceptionProxy = new Mock<IServiceExceptionHandler>();
      var mockProjectProxy = new Mock<IProjectProxy>();

      var raptorAuthentication = new RaptorAuthentication(MockRequestDelegate,
        mockCwsAccountClient.Object, mockConfigStoreProxy.Object, mockLoggerFactoryProxy.Object, mockServiceExceptionProxy.Object, mockProjectProxy.Object);

      var request = new DefaultHttpContext().Request;
      request.Path = "/device/patches";
      request.Headers.Add(new KeyValuePair<string, StringValues>("X-VisionLink-Not", "somethingElse"));
      request.Method = "POST";
      var isCustomerUidRequired = raptorAuthentication.RequireCustomerUid(request.HttpContext);
      isCustomerUidRequired.Should().BeTrue();
    }

    [TestMethod]
    public void RequireCustomer_Yes_ProductionPatches()
    {
      var mockHttpContext = new Mock<HttpContext>();
      Task MockRequestDelegate(HttpContext context) => Task.FromResult(mockHttpContext.Object);

      var mockCwsAccountClient = new Mock<ICwsAccountClient>();
      var mockConfigStoreProxy = new Mock<IConfigurationStore>();
      var mockLoggerFactoryProxy = new Mock<ILoggerFactory>();
      var mockServiceExceptionProxy = new Mock<IServiceExceptionHandler>();
      var mockProjectProxy = new Mock<IProjectProxy>();

      var raptorAuthentication = new RaptorAuthentication(MockRequestDelegate,
        mockCwsAccountClient.Object, mockConfigStoreProxy.Object, mockLoggerFactoryProxy.Object, mockServiceExceptionProxy.Object, mockProjectProxy.Object);

      var request = new DefaultHttpContext().Request;
      request.Path = "/productiondata/patches";
      request.Headers.Add(new KeyValuePair<string, StringValues>("X-VisionLink-SomethingElse", "dontCare"));
      request.Method = "GET";
      var isCustomerUidRequired = raptorAuthentication.RequireCustomerUid(request.HttpContext);
      isCustomerUidRequired.Should().BeTrue();
    }

    [TestMethod]
    public void RequireCustomer_Yes_TagFilesAndHasACustomer()
    {
      var mockHttpContext = new Mock<HttpContext>();
      Task MockRequestDelegate(HttpContext context) => Task.FromResult(mockHttpContext.Object);

      var mockCwsAccountClient = new Mock<ICwsAccountClient>(); ;
      var mockConfigStoreProxy = new Mock<IConfigurationStore>();
      var loggerFactoryProxy = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.WebApi.Tests.log"));
      var mockServiceExceptionProxy = new Mock<IServiceExceptionHandler>();
      var mockProjectProxy = new Mock<IProjectProxy>();

      var raptorAuthentication = new RaptorAuthentication(MockRequestDelegate,
        mockCwsAccountClient.Object, mockConfigStoreProxy.Object, loggerFactoryProxy, mockServiceExceptionProxy.Object, mockProjectProxy.Object);

      var request = new DefaultHttpContext().Request;
      request.Path = "/api/v2/tagfiles";
      request.Headers.Add(new KeyValuePair<string, StringValues>("X-VisionLink-CustomerUid", "gotACustomer"));
      request.Method = "POST";
      var isCustomerUidRequired = raptorAuthentication.RequireCustomerUid(request.HttpContext);
      isCustomerUidRequired.Should().BeTrue();
    }

    [TestMethod]
    public void RequireCustomer_No_PatchesAndHasNoCustomer()
    {
      var mockHttpContext = new Mock<HttpContext>();
      Task MockRequestDelegate(HttpContext context) => Task.FromResult(mockHttpContext.Object);

      var mockCwsAccountClient = new Mock<ICwsAccountClient>();
      var mockConfigStoreProxy = new Mock<IConfigurationStore>();
      var loggerFactoryProxy = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.WebApi.Tests.log"));
      var mockServiceExceptionProxy = new Mock<IServiceExceptionHandler>();
      var mockProjectProxy = new Mock<IProjectProxy>();

      var raptorAuthentication = new RaptorAuthentication(MockRequestDelegate,
        mockCwsAccountClient.Object, mockConfigStoreProxy.Object,loggerFactoryProxy, mockServiceExceptionProxy.Object, mockProjectProxy.Object);

      var request = new DefaultHttpContext().Request;
      request.Path = "/device/patches";
      request.Headers.Add(new KeyValuePair<string, StringValues>("X-VisionLink-SomethingElse", "SomethingElse"));
      request.Method = "GET";
      var isCustomerUidRequired = raptorAuthentication.RequireCustomerUid(request.HttpContext);
      isCustomerUidRequired.Should().BeFalse();
    }

    [TestMethod]
    public void RequireCustomer_No_TagFilesAndHasNoCustomer()
    {
      var mockHttpContext = new Mock<HttpContext>();
      Task MockRequestDelegate(HttpContext context) => Task.FromResult(mockHttpContext.Object);

      var mockCwsAccountClient = new Mock<ICwsAccountClient>();
      var mockConfigStoreProxy = new Mock<IConfigurationStore>();
      var loggerFactoryProxy = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Productivity3D.WebApi.Tests.log"));
      var mockServiceExceptionProxy = new Mock<IServiceExceptionHandler>();
      var mockProjectProxy = new Mock<IProjectProxy>();

      var raptorAuthentication = new RaptorAuthentication(MockRequestDelegate,
        mockCwsAccountClient.Object, mockConfigStoreProxy.Object, loggerFactoryProxy, mockServiceExceptionProxy.Object, mockProjectProxy.Object);

      var request = new DefaultHttpContext().Request;
      request.Path = "/api/v2/tagfiles";
      request.Headers.Add(new KeyValuePair<string, StringValues>("X-VisionLink-SomethingElse", "SomethingElse"));
      request.Method = "POST";
      var isCustomerUidRequired = raptorAuthentication.RequireCustomerUid(request.HttpContext);
      isCustomerUidRequired.Should().BeFalse();
    }

  }
}
