using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Nighthawk.DeviceCapabilityService.Controllers;
using VSS.Nighthawk.DeviceCapabilityService.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.ControllerTests
{
  [TestClass]
  public class LocationUpdateRequestedControllerTests
  {
    [TestMethod]
    public void GetLocationUpdateRequestedEvent_GetLocationUpdateRequestedEvent_Called()
    {
      Mock<ILocationUpdateRequestedProcessor> mockLocationProcessor = new Mock<ILocationUpdateRequestedProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> descriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      LocationUpdateRequestedController _controller = new LocationUpdateRequestedController(mockLocationProcessor.Object) { Request = _httpRequestMessage };

      descriptor.Setup(e => e.AssemblyQualifiedName).Returns("Test");
      Mock<IDeviceQuery> device = new Mock<IDeviceQuery>();

      mockLocationProcessor.Setup(e => e.GetLocationUpdateRequestedEvent(It.IsAny<IDeviceQuery>())).Returns(
        descriptor.Object);
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
      _controller.GetLocationUpdateRequestedEvent(_httpRequestMessage, device.Object);
      mockLocationProcessor.Verify(e=>e.GetLocationUpdateRequestedEvent(It.IsAny<IDeviceQuery>()),Times.Once());
    }

    [TestMethod]
    public void GetLocationUpdateRequestedEvent_ResponseReturnsOK()
    {
      Mock<ILocationUpdateRequestedProcessor> mockLocationProcessor = new Mock<ILocationUpdateRequestedProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> descriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      LocationUpdateRequestedController _controller = new LocationUpdateRequestedController(mockLocationProcessor.Object) { Request = _httpRequestMessage };
      descriptor.Setup(e => e.AssemblyQualifiedName).Returns("Test");
      Mock<IDeviceQuery> device = new Mock<IDeviceQuery>();

      mockLocationProcessor.Setup(e => e.GetLocationUpdateRequestedEvent(It.IsAny<IDeviceQuery>())).Returns(
        descriptor.Object);
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
      var response = _controller.GetLocationUpdateRequestedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public void GetLocationUpdateRequestedEvent_ResponseReturnsCorrectFactoryTypeDescriptor()
    {
      Mock<ILocationUpdateRequestedProcessor> mockLocationProcessor = new Mock<ILocationUpdateRequestedProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> descriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      LocationUpdateRequestedController _controller = new LocationUpdateRequestedController(mockLocationProcessor.Object) { Request = _httpRequestMessage };
      descriptor.Setup(e => e.AssemblyQualifiedName).Returns("Test");
      Mock<IDeviceQuery> device = new Mock<IDeviceQuery>();

      mockLocationProcessor.Setup(e => e.GetLocationUpdateRequestedEvent(It.IsAny<IDeviceQuery>())).Returns(
        descriptor.Object);
      
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
      var response = _controller.GetLocationUpdateRequestedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual("Test", ((IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value).AssemblyQualifiedName);
    }

    [TestMethod]
    public void GetLocationUpdateRequestedEvent_UnkownDeviceReturnsNotFound()
    {
      Mock<ILocationUpdateRequestedProcessor> mockLocationProcessor = new Mock<ILocationUpdateRequestedProcessor>();
      Mock<IDeviceQuery> device = new Mock<IDeviceQuery>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      LocationUpdateRequestedController _controller = new LocationUpdateRequestedController(mockLocationProcessor.Object) { Request = _httpRequestMessage };
      mockLocationProcessor.Setup(e => e.GetLocationUpdateRequestedEvent(It.IsAny<IDeviceQuery>())).Throws(
        new UnknownDeviceException("Unknown Device"));
      
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
      var response = _controller.GetLocationUpdateRequestedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      Assert.AreEqual("Unknown Device", ((ObjectContent)response.Content).Value.ToString());
    }

    [TestMethod]
    public void GetLocationUpdateRequestedEvent_NotImplementedReturnsNotFound()
    {
      Mock<ILocationUpdateRequestedProcessor> mockLocationProcessor = new Mock<ILocationUpdateRequestedProcessor>();
      Mock<IDeviceQuery> device = new Mock<IDeviceQuery>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      LocationUpdateRequestedController _controller = new LocationUpdateRequestedController(mockLocationProcessor.Object) { Request = _httpRequestMessage };
      mockLocationProcessor.Setup(e => e.GetLocationUpdateRequestedEvent(It.IsAny<IDeviceQuery>())).Throws(
        new NotImplementedException("not implemented"));

      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
      var response = _controller.GetLocationUpdateRequestedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      Assert.AreEqual("not implemented", ((ObjectContent)response.Content).Value.ToString());
    }

    [TestMethod]
    public void GetLocationUpdateRequestedEvent_UnexpectedErrorReturnsInternalServerError()
    {
      Mock<ILocationUpdateRequestedProcessor> mockLocationProcessor = new Mock<ILocationUpdateRequestedProcessor>();
      Mock<IDeviceQuery> device = new Mock<IDeviceQuery>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      LocationUpdateRequestedController _controller = new LocationUpdateRequestedController(mockLocationProcessor.Object) { Request = _httpRequestMessage };
      mockLocationProcessor.Setup(e => e.GetLocationUpdateRequestedEvent(It.IsAny<IDeviceQuery>())).Throws(
        new Exception("bad exception"));
      
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
      var response = _controller.GetLocationUpdateRequestedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]
    public void Controller_ReturnsNotFoundFromDeviceQueryModelBinderError()
    {
      Mock<ILocationUpdateRequestedProcessor> mockLocationProcessor = new Mock<ILocationUpdateRequestedProcessor>();
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      LocationUpdateRequestedController _controller = new LocationUpdateRequestedController(mockLocationProcessor.Object) { Request = _httpRequestMessage };
      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
      _controller.ModelState.AddModelError(ModelBinderConstants.DeviceQueryModelBinderError, "Test Error");
      var response = _controller.GetLocationUpdateRequestedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
  }
}
