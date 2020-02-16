using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
//using VSS.Hosted.VLCommon.Events;
using VSS.BaseEvents;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Factories;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.InterfaceTests
{
  /// <summary>
  /// Summary description for DeviceConfigFactoryTests
  /// </summary>
  [TestClass]
  public class DeviceConfigFactoryTests
  {
    [TestMethod]
    public void BuildDigitalSwitchConfigurationEventForDeviceTest_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.IDigitalSwitchConfigurationEvent>();

      mockEventTypeHelper.Setup(
        o =>
        o.QueryServiceForTypeAndBuildInstance<IDigitalSwitchConfigurationEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      PrivateObject target = new PrivateObject(typeof (DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      IDigitalSwitchConfigurationEvent actualEvent = (IDigitalSwitchConfigurationEvent) target.Invoke("BuildDigitalSwitchConfigurationEventForDevice", new object[] {mockDeviceQuery.Object});
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildDisableMaintenanceModeEventForDeviceTest_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.IDisableMaintenanceModeEvent>();

      mockEventTypeHelper.Setup(
        o =>
        o.QueryServiceForTypeAndBuildInstance<IDisableMaintenanceModeEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      PrivateObject target = new PrivateObject(typeof (DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      IDisableMaintenanceModeEvent actualEvent = (IDisableMaintenanceModeEvent)target.Invoke("BuildDisableMaintenanceModeEventForDevice", new object[] {mockDeviceQuery.Object});
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildDiscreteInputConfigurationEventForDeviceTest_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.IDiscreteInputConfigurationEvent>();

      mockEventTypeHelper.Setup(
        o =>
        o.QueryServiceForTypeAndBuildInstance<IDiscreteInputConfigurationEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      PrivateObject target = new PrivateObject(typeof (DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      IDiscreteInputConfigurationEvent actualEvent = (IDiscreteInputConfigurationEvent)target.Invoke("BuildDiscreteInputConfigurationEventForDevice", new object[] {mockDeviceQuery.Object});
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildEnableMaintenanceModeEventForDeviceTest_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.IEnableMaintenanceModeEvent>();

      mockEventTypeHelper.Setup(
        o =>
        o.QueryServiceForTypeAndBuildInstance<IEnableMaintenanceModeEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      PrivateObject target = new PrivateObject(typeof (DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      IEnableMaintenanceModeEvent actualEvent = (IEnableMaintenanceModeEvent)target.Invoke("BuildEnableMaintenanceModeEventForDevice", new object[] {mockDeviceQuery.Object});
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildFirstDailyReportStartTimeUtcChangedEventForDeviceTest_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent>();

      mockEventTypeHelper.Setup(
        o =>
        o.QueryServiceForTypeAndBuildInstance<IFirstDailyReportStartTimeUtcChangedEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      PrivateObject target = new PrivateObject(typeof (DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      IFirstDailyReportStartTimeUtcChangedEvent actualEvent = (IFirstDailyReportStartTimeUtcChangedEvent)target.Invoke("BuildFirstDailyReportStartTimeUtcChangedEventForDevice", new object[] {mockDeviceQuery.Object});
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildHourMeterModifiedEventForDeviceTest_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.IHourMeterModifiedEvent>();

      mockEventTypeHelper.Setup(o =>
                                o.QueryServiceForTypeAndBuildInstance<IHourMeterModifiedEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      PrivateObject target = new PrivateObject(typeof (DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      IHourMeterModifiedEvent actualEvent = (IHourMeterModifiedEvent)target.Invoke("BuildHourMeterModifiedEventForDevice", new object[] {mockDeviceQuery.Object});
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildMovingCriteriaConfigurationChangedEventForDeviceTest_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent>();

      mockEventTypeHelper.Setup(
        o =>
        o.QueryServiceForTypeAndBuildInstance<IMovingCriteriaConfigurationChangedEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      PrivateObject target = new PrivateObject(typeof (DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      IMovingCriteriaConfigurationChangedEvent actualEvent = (IMovingCriteriaConfigurationChangedEvent) target.Invoke("BuildMovingCriteriaConfigurationChangedEventForDevice", new object[] {mockDeviceQuery.Object});
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildOdometerModifiedEventForDeviceTest_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.IOdometerModifiedEvent>();

      mockEventTypeHelper.Setup(
        o =>
        o.QueryServiceForTypeAndBuildInstance<IOdometerModifiedEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      PrivateObject target = new PrivateObject(typeof(DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      IOdometerModifiedEvent actualEvent = (IOdometerModifiedEvent)target.Invoke("BuildOdometerModifiedEventForDevice", new object[] { mockDeviceQuery.Object });
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildGetTamperLevelEventForDeviceTest_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.IGetTamperLevelEvent>();

      mockEventTypeHelper.Setup(
        o =>
        o.QueryServiceForTypeAndBuildInstance<IGetTamperLevelEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      var target = new PrivateObject(typeof(DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      var actualEvent = (IGetTamperLevelEvent)target.Invoke("BuildGetTamperLevelEventForDevice", new object[] { mockDeviceQuery.Object });
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildSetTamperLevelEventForDeviceTest_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.ISetTamperLevelEvent>();

      mockEventTypeHelper.Setup(
        o =>
        o.QueryServiceForTypeAndBuildInstance<ISetTamperLevelEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      var target = new PrivateObject(typeof(DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      var actualEvent = (ISetTamperLevelEvent)target.Invoke("BuildSetTamperLevelEventForDevice", new object[] { mockDeviceQuery.Object });
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildGetStartModeEventForDeviceTest_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.IGetStartModeEvent>();

      mockEventTypeHelper.Setup(
        o =>
        o.QueryServiceForTypeAndBuildInstance<IGetStartModeEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      var target = new PrivateObject(typeof(DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      var actualEvent = (IGetStartModeEvent)target.Invoke("BuildGetStartModeEventForDevice", new object[] { mockDeviceQuery.Object });
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildSetStartModeEventForDeviceTest_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.ISetStartModeEvent>();

      mockEventTypeHelper.Setup(
        o =>
        o.QueryServiceForTypeAndBuildInstance<ISetStartModeEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      var target = new PrivateObject(typeof(DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      var actualEvent = (ISetStartModeEvent)target.Invoke("BuildSetStartModeEventForDevice", new object[] { mockDeviceQuery.Object });
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildDailyReportFrequencyEventForDeviceTest_Success()
      {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.ISetDailyReportFrequencyEvent>();

      mockEventTypeHelper.Setup(
        o =>
        o.QueryServiceForTypeAndBuildInstance<ISetDailyReportFrequencyEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      var target = new PrivateObject(typeof(DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      var actualEvent = (ISetDailyReportFrequencyEvent)target.Invoke("BuildSetDailyReportFrequencyEvent", new object[] { mockDeviceQuery.Object });
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildEnableRapidReportingEventForDeviceTest_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.IEnableRapidReportingEvent>();

      mockEventTypeHelper.Setup(
        o =>
        o.QueryServiceForTypeAndBuildInstance<IEnableRapidReportingEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      var target = new PrivateObject(typeof(DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      var actualEvent = (IEnableRapidReportingEvent)target.Invoke("BuildEnableRapidReportingEventForDevice", new object[] { mockDeviceQuery.Object });
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }

    [TestMethod]
    public void BuildDisableRapidReportingEventForDeviceTest_Success()
    {
      var mockEventTypeHelper = new Mock<IEventTypeHelper>();
      var mockDeviceQuery = new Mock<IDeviceQuery>();
      var mockEvent = new Mock<DataOut.Interfaces.Events.IDisableRapidReportingEvent>();

      mockEventTypeHelper.Setup(
        o =>
        o.QueryServiceForTypeAndBuildInstance<IDisableRapidReportingEvent>(It.IsAny<string>(), It.IsAny<string>(), mockDeviceQuery.Object)).Returns(mockEvent.Object);

      var target = new PrivateObject(typeof(DeviceConfigFactory));
      target.SetFieldOrProperty("_deviceCapabilitySvcUri", string.Empty);
      target.SetFieldOrProperty("_eventTypeHelper", mockEventTypeHelper.Object);

      var actualEvent = (IDisableRapidReportingEvent)target.Invoke("BuildDisableRapidReportingEventForDevice", new object[] { mockDeviceQuery.Object });
      Assert.AreEqual(mockEvent.Object, actualEvent);
    }
  }
}
