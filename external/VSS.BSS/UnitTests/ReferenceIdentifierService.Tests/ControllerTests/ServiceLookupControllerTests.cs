using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Controllers;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.ControllerTests
{
  [TestClass]
  public class ServiceLookupControllerTests
  {
    [TestMethod]
    public void ServiceIdentifierControllerTests_GetAssetActiveServices_ByAssetUid_Success()
    {
      Mock<IServiceLookupManager> _mockServiceLookupManager = new Mock<IServiceLookupManager>();
      ServiceLookupController _controller = new ServiceLookupController(_mockServiceLookupManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      var result = new List<Guid?>();
      result.Add(Guid.NewGuid());
      _mockServiceLookupManager.Setup(o => o.GetAssetActiveServices(It.IsAny<Guid>()))
        .Returns(result);

      HttpResponseMessage response = _controller.GetAssetActiveServices(_httpRequestMessage, Guid.NewGuid());
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      var lookupResponse = (LookupResponse<List<Guid?>>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(result, lookupResponse.Data);
      Assert.IsNull(lookupResponse.Exception);
    }

    [TestMethod]
    public void ServiceIdentifierControllerTests_GetAssetActiveServices_ByAssetUid_WorkerThrowsException()
    {
      Mock<IServiceLookupManager> _mockServiceLookupManager = new Mock<IServiceLookupManager>();
      ServiceLookupController _controller = new ServiceLookupController(_mockServiceLookupManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      _mockServiceLookupManager.Setup(o => o.GetAssetActiveServices(It.IsAny<Guid>()))
        .Throws(new Exception("Ex"));

      HttpResponseMessage response = _controller.GetAssetActiveServices(_httpRequestMessage, Guid.NewGuid());
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<List<Guid?>>)((ObjectContent)response.Content).Value;
      Assert.IsNull(lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("Ex"));
    }

    [TestMethod]
    public void ServiceIdentifierControllerTests_GetAssetActiveServices_Success()
    {
      Mock<IServiceLookupManager> _mockServiceLookupManager = new Mock<IServiceLookupManager>();
      ServiceLookupController _controller = new ServiceLookupController(_mockServiceLookupManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      var result = new List<ServiceLookupItem>();
      result.Add(new ServiceLookupItem() { Type = "type", UID = Guid.NewGuid() });
      _mockServiceLookupManager.Setup(o => o.GetAssetActiveServices(It.IsAny<string>(), It.IsAny<string>()))
        .Returns(result);

      HttpResponseMessage response = _controller.GetAssetActiveServices(_httpRequestMessage, "serialNumber", "makeCode");
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      var lookupResponse = (LookupResponse<IList<ServiceLookupItem>>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(result, lookupResponse.Data);
      Assert.IsNull(lookupResponse.Exception);
    }

    [TestMethod]
    public void ServiceIdentifierControllerTests_GetAssetActiveServices_WorkerThrowsException()
    {
      Mock<IServiceLookupManager> _mockServiceLookupManager = new Mock<IServiceLookupManager>();
      ServiceLookupController _controller = new ServiceLookupController(_mockServiceLookupManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      _mockServiceLookupManager.Setup(o => o.GetAssetActiveServices(It.IsAny<string>(), It.IsAny<string>()))
        .Throws(new Exception("Ex"));

      HttpResponseMessage response = _controller.GetAssetActiveServices(_httpRequestMessage, "serialNumber", "makeCode");
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<IList<ServiceLookupItem>>)((ObjectContent)response.Content).Value;
      Assert.IsNull(lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("Ex"));
    }

    [TestMethod]
    public void ServiceIdentifierControllerTests_GetDeviceActiveServices_Success()
    {
      Mock<IServiceLookupManager> _mockServiceLookupManager = new Mock<IServiceLookupManager>();
      ServiceLookupController _controller = new ServiceLookupController(_mockServiceLookupManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      var result = new List<ServiceLookupItem>();
      result.Add(new ServiceLookupItem() { Type = "type", UID = Guid.NewGuid() });
      _mockServiceLookupManager.Setup(o => o.GetDeviceActiveServices(It.IsAny<string>(), It.IsAny<DeviceTypeEnum>()))
        .Returns(result);

      HttpResponseMessage response = _controller.GetDeviceActiveServices(_httpRequestMessage, "serialNumber", DeviceTypeEnum.PL641.ToString());
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      var lookupResponse = (LookupResponse<IList<ServiceLookupItem>>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(result, lookupResponse.Data);
      Assert.IsNull(lookupResponse.Exception);
    }

    [TestMethod]
    public void ServiceIdentifierControllerTests_GetDeviceActiveServices_Exception()
    {
      Mock<IServiceLookupManager> _mockServiceLookupManager = new Mock<IServiceLookupManager>();
      ServiceLookupController _controller = new ServiceLookupController(_mockServiceLookupManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      HttpResponseMessage response = _controller.GetDeviceActiveServices(_httpRequestMessage, "serialNumber", "NotAValidDeviceType");
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<IList<ServiceLookupItem>>)((ObjectContent)response.Content).Value;
      Assert.IsNull(lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("not found"));
    }
  }
}
