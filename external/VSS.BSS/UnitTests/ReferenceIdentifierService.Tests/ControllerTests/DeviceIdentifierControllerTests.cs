using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Controllers;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Constants;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.ControllerTests
{
  [TestClass]
  public class DeviceIdentifierControllerTests
  {
    [TestMethod]
    public void DeviceIdentifierControllerTests_Retrieve_Success()
    {
      Mock<IDeviceIdentifierManager> _mockDeviceIdentifierManager = new Mock<IDeviceIdentifierManager>();
      DeviceIdentifierController _controller = new DeviceIdentifierController(_mockDeviceIdentifierManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      var guid = new UUIDSequentialGuid().CreateGuid();
      _mockDeviceIdentifierManager.Setup(o => o.Retrieve(It.IsAny<IdentifierDefinition>())).Returns(guid);
      IdentifierDefinition idDef = GetTestIdentifierDefinition();
      HttpResponseMessage response = _controller.Retrieve(_httpRequestMessage, idDef);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      var lookupResponse = (LookupResponse<Guid?>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(guid, lookupResponse.Data);
      Assert.IsNull(lookupResponse.Exception);
    }

    [TestMethod]
    public void DeviceIdentifierControllerTests_Retrieve_ManagerThrowsException()
    {
      Mock<IDeviceIdentifierManager> _mockDeviceIdentifierManager = new Mock<IDeviceIdentifierManager>();
      DeviceIdentifierController _controller = new DeviceIdentifierController(_mockDeviceIdentifierManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      IdentifierDefinition idDef = GetTestIdentifierDefinition();
      _mockDeviceIdentifierManager.Setup(o => o.Retrieve(It.IsAny<IdentifierDefinition>()))
        .Throws(new Exception("an exception"));
      HttpResponseMessage response = _controller.Retrieve(_httpRequestMessage, idDef);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<Guid?>)((ObjectContent)response.Content).Value;
      Assert.IsNull(lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("an exception"));
    }

    [TestMethod]
    public void DeviceIdentifierControllerTests_Retrieve_Missing_A_Parameter()
    {
      Mock<IDeviceIdentifierManager> _mockDeviceIdentifierManager = new Mock<IDeviceIdentifierManager>();
      DeviceIdentifierController _controller = new DeviceIdentifierController(_mockDeviceIdentifierManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      IdentifierDefinition idDef = GetTestIdentifierDefinition();
      idDef.Alias = null;
      _controller.ModelState.AddModelError(ModelBinderConstants.IdentifierDefinitionModelBinderError,
          "Invalid request: query string does not have valid alias");
      HttpResponseMessage response = _controller.Retrieve(_httpRequestMessage, idDef);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<Guid?>)((ObjectContent)response.Content).Value;
      Assert.IsNull(lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("Invalid request: query string does not have valid alias"));
    }

    [TestMethod]
    public void DeviceIdentifierControllerTests_Create_Success()
    {
      Mock<IDeviceIdentifierManager> _mockDeviceIdentifierManager = new Mock<IDeviceIdentifierManager>();
      DeviceIdentifierController _controller = new DeviceIdentifierController(_mockDeviceIdentifierManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      var guid = new UUIDSequentialGuid().CreateGuid();
      IdentifierDefinition idDef = GetTestIdentifierDefinition();
      idDef.UID = guid;
      HttpResponseMessage response = _controller.Create(_httpRequestMessage, idDef);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      var lookupResponse = (LookupResponse<Guid?>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(guid, lookupResponse.Data);
      Assert.IsNull(lookupResponse.Exception);
    }

    [TestMethod]
    public void DeviceIdentifierControllerTests_Create_ManagerThrowsException()
    {
      Mock<IDeviceIdentifierManager> _mockDeviceIdentifierManager = new Mock<IDeviceIdentifierManager>();
      DeviceIdentifierController _controller = new DeviceIdentifierController(_mockDeviceIdentifierManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      var guid = new UUIDSequentialGuid().CreateGuid();
      IdentifierDefinition idDef = GetTestIdentifierDefinition();
      idDef.UID = guid;
      _mockDeviceIdentifierManager.Setup(o => o.Create(It.IsAny<IdentifierDefinition>()))
        .Throws(new Exception("an exception"));
      HttpResponseMessage response = _controller.Create(_httpRequestMessage, idDef);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<Guid?>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(guid, lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("an exception"));
    }

    [TestMethod]
    public void DeviceIdentifierControllerTests_Create_Missing_A_Parameter()
    {
      Mock<IDeviceIdentifierManager> _mockDeviceIdentifierManager = new Mock<IDeviceIdentifierManager>();
      DeviceIdentifierController _controller = new DeviceIdentifierController(_mockDeviceIdentifierManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      var guid = new UUIDSequentialGuid().CreateGuid();
      IdentifierDefinition idDef = GetTestIdentifierDefinition();
      idDef.UID = guid;
      idDef.Alias = null;
      _controller.ModelState.AddModelError(ModelBinderConstants.IdentifierDefinitionModelBinderError,
          "Invalid request: query string does not have valid alias");
      HttpResponseMessage response = _controller.Create(_httpRequestMessage, idDef);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<Guid?>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(guid, lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("Invalid request: query string does not have valid alias"));
    }

    [TestMethod]
    public void DeviceIdentifierControllerTests_Create_Missing_Uid()
    {
      Mock<IDeviceIdentifierManager> _mockDeviceIdentifierManager = new Mock<IDeviceIdentifierManager>();
      DeviceIdentifierController _controller = new DeviceIdentifierController(_mockDeviceIdentifierManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      IdentifierDefinition idDef = GetTestIdentifierDefinition();
      HttpResponseMessage response = _controller.Create(_httpRequestMessage, idDef);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<Guid?>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(Guid.Empty, lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("UID cannot be empty"));
    }

    [TestMethod]
    public void DeviceIdentifierControllerTests_GetAssociatedAsset_Success()
    {
      Mock<IDeviceIdentifierManager> _mockDeviceIdentifierManager = new Mock<IDeviceIdentifierManager>();
      DeviceIdentifierController _controller = new DeviceIdentifierController(_mockDeviceIdentifierManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      var uid = Guid.NewGuid();
      _mockDeviceIdentifierManager.Setup(o => o.GetAssociatedAsset(It.IsAny<Guid>())).Returns(uid);
      HttpResponseMessage response = _controller.GetAssociatedAsset(_httpRequestMessage, Guid.NewGuid());
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      var lookupResponse = (LookupResponse<Guid?>)((ObjectContent)response.Content).Value;
      Assert.AreEqual(uid, lookupResponse.Data);
      Assert.IsNull(lookupResponse.Exception);
    }

    [TestMethod]
    public void DeviceIdentifierControllerTests_GetAssociatedAsset_WorkerThrowsException()
    {
      Mock<IDeviceIdentifierManager> _mockDeviceIdentifierManager = new Mock<IDeviceIdentifierManager>();
      DeviceIdentifierController _controller = new DeviceIdentifierController(_mockDeviceIdentifierManager.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
    
      _mockDeviceIdentifierManager.Setup(o => o.GetAssociatedAsset(It.IsAny<Guid>())).Throws(new Exception("Ex"));
      HttpResponseMessage response = _controller.GetAssociatedAsset(_httpRequestMessage, Guid.NewGuid());
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
      var lookupResponse = (LookupResponse<Guid?>)((ObjectContent)response.Content).Value;
      Assert.IsNull(lookupResponse.Data);
      Assert.IsNotNull(lookupResponse.Exception);
      Assert.IsTrue(lookupResponse.Exception.Message.Contains("Ex"));
    }

    private IdentifierDefinition GetTestIdentifierDefinition()
    {
      return new IdentifierDefinition
      {
        StoreId = 1,
        Alias = "abc",
        Value = "xyz"
      };
    }
  }
}
