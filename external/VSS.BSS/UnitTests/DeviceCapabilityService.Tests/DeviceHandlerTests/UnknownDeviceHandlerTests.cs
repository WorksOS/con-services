using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using VSS.Nighthawk.DeviceCapabilityService.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class UnknownDeviceHandlerTests
  {
    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestAssetIdConfigurationChangedEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      Type type = _deviceHandler.AssetIdConfigurationChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestDigitalSwitchConfigurationEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      
      Type type = _deviceHandler.DigitalSwitchConfigurationEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestDisableMaintenanceModeEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      
      Type type = _deviceHandler.DisableMaintenanceModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestDiscreteInputConfigurationEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      
      Type type = _deviceHandler.DiscreteInputConfigurationEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestEnableMaintenanceModeEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      
      Type type = _deviceHandler.EnableMaintenanceModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      
      Type type = _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestHourMeterModifiedEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      
      Type type = _deviceHandler.HourMeterModifiedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestLocationUpdateRequestedEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      
      Type type = _deviceHandler.LocationUpdateRequestedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestMovingCriteriaConfigurationChangedEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      
      Type type = _deviceHandler.MovingCriteriaConfigurationChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestOdometerModifiedEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      
      Type type = _deviceHandler.OdometerModifiedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestSiteDispatchedEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      
      Type type = _deviceHandler.SiteDispatchedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestSiteRemovedEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      
      Type type = _deviceHandler.SiteRemovedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestGetStartModeEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();

      Type type = _deviceHandler.GetStartModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestSetStartModeEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();

      Type type = _deviceHandler.SetStartModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestGetTamperLevelEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();

      Type type = _deviceHandler.GetTamperLevelEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestSetTamperLevelEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();

      Type type = _deviceHandler.SetTamperLevelEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      Type type = _deviceHandler.SetDailyReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestEnableRapidReportingEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestDisableRapidReportingEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestReportFrequencyChangedEventType()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      Type type = _deviceHandler.SetReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownDeviceException))]
    public void TestOutboundEndpointNames()
    {
      UnknownDeviceHandler _deviceHandler = new UnknownDeviceHandler();
      
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
    }
  }
}
