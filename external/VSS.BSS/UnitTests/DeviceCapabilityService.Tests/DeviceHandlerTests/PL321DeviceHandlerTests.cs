using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class PL321HandlerTests
  {
    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestAssetIdConfigurationChangedEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.AssetIdConfigurationChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDigitalSwitchConfigurationEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DigitalSwitchConfigurationEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableMaintenanceModeEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableMaintenanceModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDiscreteInputConfigurationEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DiscreteInputConfigurationEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableMaintenanceModeEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableMaintenanceModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestHourMeterModifiedEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.HourMeterModifiedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestLocationUpdateRequestedEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.LocationUpdateRequestedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestMovingCriteriaConfigurationChangedEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.MovingCriteriaConfigurationChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestOdometerModifiedEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.OdometerModifiedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSiteDispatchedEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SiteDispatchedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSiteRemovedEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SiteRemovedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestGetStartModeEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.GetStartModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSetStartModeEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetStartModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestGetTamperLevelEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.GetTamperLevelEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSetTamperLevelEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetTamperLevelEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetDailyReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableRapidReportingEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableRapidReportingEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestReportFrequencyChangedEventType()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetReportFrequencyEventType;
    }

    [TestMethod]
    public void TestOutboundEndpointNames()
    {
      PL321DeviceHandler _deviceHandler = new PL321DeviceHandler(Helpers.GetTestEndpointNames());
      IEnumerable<string> expectedEndpointNames = Helpers.GetTestEndpointNames();
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
      foreach (var expectedEndpointName in expectedEndpointNames)
      {
        Assert.IsTrue(actualEndpointNames.Contains(expectedEndpointName));
      }
    }
  }
}
