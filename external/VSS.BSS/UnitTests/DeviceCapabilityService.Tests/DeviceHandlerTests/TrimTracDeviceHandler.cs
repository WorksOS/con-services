using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class TrimTracDeviceHandlerTests
  {
    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestAssetIdConfigurationChangedEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.AssetIdConfigurationChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDigitalSwitchConfigurationEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.DigitalSwitchConfigurationEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableMaintenanceModeEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.DisableMaintenanceModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDiscreteInputConfigurationEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.DiscreteInputConfigurationEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableMaintenanceModeEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.EnableMaintenanceModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestHourMeterModifiedEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.HourMeterModifiedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestLocationUpdateRequestedEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.LocationUpdateRequestedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestMovingCriteriaConfigurationChangedEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.MovingCriteriaConfigurationChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestOdometerModifiedEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.OdometerModifiedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSiteDispatchedEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.SiteDispatchedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSiteRemovedEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.SiteRemovedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestGetStartModeEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.GetStartModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSetStartModeEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.SetStartModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestGetTamperLevelEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.GetTamperLevelEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSetTamperLevelEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.SetTamperLevelEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetDailyReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableRapidReportingEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableRapidReportingEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestReportFrequencyChangedEventType()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetReportFrequencyEventType;
    }

    [TestMethod]
    public void TestOutboundEndpointNames()
    {
      TrimTracDeviceHandler _deviceHandler = new TrimTracDeviceHandler(Helpers.GetTestEndpointNames());
      
      IEnumerable<string> expectedEndpointNames = Helpers.GetTestEndpointNames();
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
      foreach (var expectedEndpointName in expectedEndpointNames)
      {
        Assert.IsTrue(actualEndpointNames.Contains(expectedEndpointName));
      }
    }
  }
}
