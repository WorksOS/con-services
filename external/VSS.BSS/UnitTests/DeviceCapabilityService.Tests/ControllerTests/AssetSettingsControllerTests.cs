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
  public class AssetSettingsControllerTests
  {
    [TestMethod]
    public void Test_Controller_Success()
    {
      Mock<IAssetSettingsProcessor> _mockAssetSettingsProcessor = new Mock<IAssetSettingsProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      AssetSettingsController _controller = new AssetSettingsController(_mockAssetSettingsProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
      _mockFactoryTypeDescriptor.SetupGet(o => o.AssemblyQualifiedName).Returns("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent");
      _mockAssetSettingsProcessor.Setup(o => o.GetAssetIdConfigurationChangedEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      HttpResponseMessage response = _controller.GetAssetIdConfigurationChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      IFactoryOutboundEventTypeDescriptor actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));
    }

    [TestMethod]
    public void Test_Controller_ThrowsUnknownDeviceException()
    {
      Mock<IAssetSettingsProcessor> _mockAssetSettingsProcessor = new Mock<IAssetSettingsProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      AssetSettingsController _controller = new AssetSettingsController(_mockAssetSettingsProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
      _mockAssetSettingsProcessor.Setup(o => o.GetAssetIdConfigurationChangedEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      HttpResponseMessage response = _controller.GetAssetIdConfigurationChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      string message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);
    }

    [TestMethod]
    public void Test_Controller_ThrowsNotImplementedException()
    {
      Mock<IAssetSettingsProcessor> _mockAssetSettingsProcessor = new Mock<IAssetSettingsProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      AssetSettingsController _controller = new AssetSettingsController(_mockAssetSettingsProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
      _mockAssetSettingsProcessor.Setup(o => o.GetAssetIdConfigurationChangedEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      HttpResponseMessage response = _controller.GetAssetIdConfigurationChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      string message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);
    }

    [TestMethod]
    public void Test_Controller_ThrowsException()
    {
      Mock<IAssetSettingsProcessor> _mockAssetSettingsProcessor = new Mock<IAssetSettingsProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      AssetSettingsController _controller = new AssetSettingsController(_mockAssetSettingsProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
      _mockAssetSettingsProcessor.Setup(o => o.GetAssetIdConfigurationChangedEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      HttpResponseMessage response = _controller.GetAssetIdConfigurationChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]
    public void Controller_ReturnsNotFoundFromDeviceQueryModelBinderError()
    {
      Mock<IAssetSettingsProcessor> _mockAssetSettingsProcessor = new Mock<IAssetSettingsProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      AssetSettingsController _controller = new AssetSettingsController(_mockAssetSettingsProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);
      _controller.ModelState.AddModelError(ModelBinderConstants.DeviceQueryModelBinderError, "Test Error");
      var response = _controller.GetAssetIdConfigurationChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
  }
}
