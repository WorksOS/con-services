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
  public class DeviceConfigControllerTests
  {
    [TestMethod]
    public void Test_Controller_Success()
    {
      Mock<IDeviceConfigProcessor> _mockDeviceConfigProcessor = new Mock<IDeviceConfigProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      DeviceConfigController _controller = new DeviceConfigController(_mockDeviceConfigProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
      _mockFactoryTypeDescriptor.SetupGet(o => o.AssemblyQualifiedName).Returns("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent");
      _mockDeviceConfigProcessor.Setup(o => o.GetDigitalSwitchConfigurationEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockDeviceConfigProcessor.Setup(o => o.GetDisableMaintenanceModeEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockDeviceConfigProcessor.Setup(o => o.GetDiscreteInputConfigurationEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockDeviceConfigProcessor.Setup(o => o.GetEnableMaintenanceModeEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockDeviceConfigProcessor.Setup(o => o.GetFirstDailyReportStartTimeUtcChangedEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockDeviceConfigProcessor.Setup(o => o.GetHourMeterModifiedEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockDeviceConfigProcessor.Setup(o => o.GetMovingCriteriaConfigurationChangedEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockDeviceConfigProcessor.Setup(o => o.GetOdometerModifiedEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockDeviceConfigProcessor.Setup(o => o.SetTamperLevelEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockDeviceConfigProcessor.Setup(o => o.GetTamperLevelEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockDeviceConfigProcessor.Setup(o => o.SetStartModeEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockDeviceConfigProcessor.Setup(o => o.GetStartModeEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockDeviceConfigProcessor.Setup(o => o.SetDailyReportFrequencyEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockDeviceConfigProcessor.Setup(o => o.EnableRapidReportingEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      _mockDeviceConfigProcessor.Setup(o => o.DisableRapidReportingEvent(It.IsAny<IDeviceQuery>())).Returns(_mockFactoryTypeDescriptor.Object);
      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      HttpResponseMessage response = _controller.GetDigitalSwitchConfigurationEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      IFactoryOutboundEventTypeDescriptor actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetDigitalSwitchConfigurationEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetDisableMaintenanceModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetDiscreteInputConfigurationEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetEnableMaintenanceModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetFirstDailyReportStartTimeUtcChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetHourMeterModifiedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetMovingCriteriaConfigurationChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetOdometerModifiedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetSetTamperLevelEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetTamperLevelEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetSetStartModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetStartModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetSetDailyReportFrequencyEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetEnableRapidReportingEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));

      response = _controller.GetDisableRapidReportingEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
      actualFactoryTypeDescriptor = (IFactoryOutboundEventTypeDescriptor)((ObjectContent)response.Content).Value;
      Assert.IsTrue(actualFactoryTypeDescriptor.AssemblyQualifiedName.Contains("VSS.Nighthawk.DataOut.Interfaces.Events.IEvent"));
    }

    [TestMethod]
    public void Test_Controller_ThrowsUnknownDeviceException()
    {
      Mock<IDeviceConfigProcessor> _mockDeviceConfigProcessor = new Mock<IDeviceConfigProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      DeviceConfigController _controller = new DeviceConfigController(_mockDeviceConfigProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      _mockDeviceConfigProcessor.Setup(o => o.GetDigitalSwitchConfigurationEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockDeviceConfigProcessor.Setup(o => o.GetDisableMaintenanceModeEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockDeviceConfigProcessor.Setup(o => o.GetDiscreteInputConfigurationEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockDeviceConfigProcessor.Setup(o => o.GetEnableMaintenanceModeEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockDeviceConfigProcessor.Setup(o => o.GetFirstDailyReportStartTimeUtcChangedEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockDeviceConfigProcessor.Setup(o => o.GetHourMeterModifiedEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockDeviceConfigProcessor.Setup(o => o.GetMovingCriteriaConfigurationChangedEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockDeviceConfigProcessor.Setup(o => o.GetOdometerModifiedEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockDeviceConfigProcessor.Setup(o => o.SetTamperLevelEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockDeviceConfigProcessor.Setup(o => o.GetTamperLevelEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockDeviceConfigProcessor.Setup(o => o.SetStartModeEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockDeviceConfigProcessor.Setup(o => o.GetStartModeEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockDeviceConfigProcessor.Setup(o => o.SetDailyReportFrequencyEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockDeviceConfigProcessor.Setup(o => o.EnableRapidReportingEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));
      _mockDeviceConfigProcessor.Setup(o => o.DisableRapidReportingEvent(It.IsAny<IDeviceQuery>())).Throws(new UnknownDeviceException("my message"));

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      HttpResponseMessage response = _controller.GetDigitalSwitchConfigurationEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      string message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetDisableMaintenanceModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetDiscreteInputConfigurationEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetEnableMaintenanceModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetFirstDailyReportStartTimeUtcChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetHourMeterModifiedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetMovingCriteriaConfigurationChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetOdometerModifiedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetSetTamperLevelEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetTamperLevelEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetSetStartModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetStartModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetSetDailyReportFrequencyEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetEnableRapidReportingEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);

      response = _controller.GetDisableRapidReportingEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("my message", message);
    }

    [TestMethod]
    public void Test_Controller_ThrowsNotImplementedException()
    {
      Mock<IDeviceConfigProcessor> _mockDeviceConfigProcessor = new Mock<IDeviceConfigProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      DeviceConfigController _controller = new DeviceConfigController(_mockDeviceConfigProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      _mockDeviceConfigProcessor.Setup(o => o.GetDigitalSwitchConfigurationEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockDeviceConfigProcessor.Setup(o => o.GetDisableMaintenanceModeEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockDeviceConfigProcessor.Setup(o => o.GetDiscreteInputConfigurationEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockDeviceConfigProcessor.Setup(o => o.GetEnableMaintenanceModeEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockDeviceConfigProcessor.Setup(o => o.GetFirstDailyReportStartTimeUtcChangedEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockDeviceConfigProcessor.Setup(o => o.GetHourMeterModifiedEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockDeviceConfigProcessor.Setup(o => o.GetMovingCriteriaConfigurationChangedEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockDeviceConfigProcessor.Setup(o => o.GetOdometerModifiedEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockDeviceConfigProcessor.Setup(o => o.SetTamperLevelEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockDeviceConfigProcessor.Setup(o => o.GetTamperLevelEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockDeviceConfigProcessor.Setup(o => o.SetStartModeEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockDeviceConfigProcessor.Setup(o => o.GetStartModeEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockDeviceConfigProcessor.Setup(o => o.SetDailyReportFrequencyEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockDeviceConfigProcessor.Setup(o => o.EnableRapidReportingEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));
      _mockDeviceConfigProcessor.Setup(o => o.DisableRapidReportingEvent(It.IsAny<IDeviceQuery>())).Throws(new NotImplementedException("not implemented"));

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      HttpResponseMessage response = _controller.GetDigitalSwitchConfigurationEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      string message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetDisableMaintenanceModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetDiscreteInputConfigurationEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetEnableMaintenanceModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetFirstDailyReportStartTimeUtcChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetHourMeterModifiedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetMovingCriteriaConfigurationChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetOdometerModifiedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetSetTamperLevelEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetTamperLevelEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetSetStartModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetStartModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetSetDailyReportFrequencyEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetEnableRapidReportingEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);

      response = _controller.GetDisableRapidReportingEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
      message = (string)((ObjectContent)response.Content).Value;
      Assert.AreEqual("not implemented", message);
    }

    [TestMethod]
    public void Test_Controller_ThrowsException()
    {
      Mock<IDeviceConfigProcessor> _mockDeviceConfigProcessor = new Mock<IDeviceConfigProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      DeviceConfigController _controller = new DeviceConfigController(_mockDeviceConfigProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      _mockDeviceConfigProcessor.Setup(o => o.GetDigitalSwitchConfigurationEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockDeviceConfigProcessor.Setup(o => o.GetDisableMaintenanceModeEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockDeviceConfigProcessor.Setup(o => o.GetDiscreteInputConfigurationEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockDeviceConfigProcessor.Setup(o => o.GetEnableMaintenanceModeEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockDeviceConfigProcessor.Setup(o => o.GetFirstDailyReportStartTimeUtcChangedEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockDeviceConfigProcessor.Setup(o => o.GetHourMeterModifiedEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockDeviceConfigProcessor.Setup(o => o.GetMovingCriteriaConfigurationChangedEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockDeviceConfigProcessor.Setup(o => o.GetOdometerModifiedEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockDeviceConfigProcessor.Setup(o => o.SetTamperLevelEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockDeviceConfigProcessor.Setup(o => o.GetTamperLevelEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockDeviceConfigProcessor.Setup(o => o.SetStartModeEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockDeviceConfigProcessor.Setup(o => o.GetStartModeEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockDeviceConfigProcessor.Setup(o => o.SetDailyReportFrequencyEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockDeviceConfigProcessor.Setup(o => o.DisableRapidReportingEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      _mockDeviceConfigProcessor.Setup(o => o.EnableRapidReportingEvent(It.IsAny<IDeviceQuery>())).Throws(new Exception("bad exception"));
      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);

      HttpResponseMessage response = _controller.GetDigitalSwitchConfigurationEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetDisableMaintenanceModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetDiscreteInputConfigurationEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetEnableMaintenanceModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetFirstDailyReportStartTimeUtcChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetHourMeterModifiedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetMovingCriteriaConfigurationChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetOdometerModifiedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetSetTamperLevelEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetTamperLevelEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetSetStartModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetStartModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetSetDailyReportFrequencyEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetEnableRapidReportingEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);

      response = _controller.GetDisableRapidReportingEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]
    public void Controller_AllActionsReturnsNotFoundFromDeviceQueryModelBinderError()
    {
      Mock<IDeviceConfigProcessor> _mockDeviceConfigProcessor = new Mock<IDeviceConfigProcessor>();
      Mock<IFactoryOutboundEventTypeDescriptor> _mockFactoryTypeDescriptor = new Mock<IFactoryOutboundEventTypeDescriptor>();
      DeviceConfigController _controller = new DeviceConfigController(_mockDeviceConfigProcessor.Object);
      HttpRequestMessage _httpRequestMessage = new HttpRequestMessage();
      _controller.Request = _httpRequestMessage;
      _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

      var device = new Mock<IDeviceQuery>();
      device.SetupGet(e => e.ID).Returns(123);
      _controller.ModelState.AddModelError(ModelBinderConstants.DeviceQueryModelBinderError, "Test Error");

      var response = _controller.GetDigitalSwitchConfigurationEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetDisableMaintenanceModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetDiscreteInputConfigurationEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetEnableMaintenanceModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetFirstDailyReportStartTimeUtcChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetHourMeterModifiedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetMovingCriteriaConfigurationChangedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetOdometerModifiedEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetSetTamperLevelEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetTamperLevelEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetSetStartModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetStartModeEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetSetDailyReportFrequencyEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetEnableRapidReportingEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

      response = _controller.GetDisableRapidReportingEvent(_httpRequestMessage, device.Object);
      Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
  }
}
