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
  public class SiteAdministrationControllerTests
  {
    [TestMethod]
    public void Test_Controller_Success()
    {
      Mock<ISiteAdministrationProcessor> _mockSiteAdministrationProcessor = new Mock<ISiteAdministrationProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      SiteAdministrationController _controller = new SiteAdministrationController(_mockSiteAdministrationProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      _mockFactoryTypeDescriptor.SetupGet(o => o.AssemblyQualifiedName).Returns("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent");
      _mockSiteAdministrationProcessor.Setup(o => o.GetSiteDispatchedEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockSiteAdministrationProcessor.Setup(o => o.GetSiteRemovedEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      HttpResponseMessage response = _controller.GetSiteDispatchedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      IFactoryOutboundEventTypeDescriptor actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetSiteRemovedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));
    }

    [TestMethod]
    public void Test_Controller_ThrowsUnknownDeviceException()
    {
      Mock<ISiteAdministrationProcessor> _mockSiteAdministrationProcessor = new Mock<ISiteAdministrationProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      SiteAdministrationController _controller = new SiteAdministrationController(_mockSiteAdministrationProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      _mockSiteAdministrationProcessor.Setup(o => o.GetSiteDispatchedEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockSiteAdministrationProcessor.Setup(o => o.GetSiteRemovedEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      HttpResponseMessage response = _controller.GetSiteDispatchedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      string message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetSiteRemovedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);
    }

    [TestMethod]
    public void Test_Controller_ThrowsNotImplementedException()
    {
      Mock<ISiteAdministrationProcessor> _mockSiteAdministrationProcessor = new Mock<ISiteAdministrationProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      SiteAdministrationController _controller = new SiteAdministrationController(_mockSiteAdministrationProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      _mockSiteAdministrationProcessor.Setup(o => o.GetSiteDispatchedEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockSiteAdministrationProcessor.Setup(o => o.GetSiteRemovedEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      HttpResponseMessage response = _controller.GetSiteDispatchedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      string message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetSiteRemovedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);
    }

    [TestMethod]
    public void Test_Controller_ThrowsException()
    {
      Mock<ISiteAdministrationProcessor> _mockSiteAdministrationProcessor = new Mock<ISiteAdministrationProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      SiteAdministrationController _controller = new SiteAdministrationController(_mockSiteAdministrationProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      _mockSiteAdministrationProcessor.Setup(o => o.GetSiteDispatchedEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockSiteAdministrationProcessor.Setup(o => o.GetSiteRemovedEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      HttpResponseMessage response = _controller.GetSiteDispatchedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetSiteRemovedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]
    public void Controller_ReturnsNotFoundFromDeviceQueryModelBinderError()
    {
      Mock<ISiteAdministrationProcessor> _mockSiteAdministrationProcessor = new Mock<ISiteAdministrationProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      SiteAdministrationController _controller = new SiteAdministrationController(_mockSiteAdministrationProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);
      _controller.ModelState.AddModelError(ModelBinderConstants.DeviceQueryModelBinderError, "Test Error");
      var response = _controller.GetSiteDispatchedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetSiteRemovedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
  }
}
