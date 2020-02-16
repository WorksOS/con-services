using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class TAP66DeviceHandlerTests
  {
    private static TAP66DeviceHandler _deviceHandler;

    [ClassInitialize]
    public static void Init(TestContext context)
    {
      _deviceHandler = new TAP66DeviceHandler(Helpers.GetTestEndpointNames());
    }

    [TestCleanup]
    public void Cleanup()
    {
      Init(null);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestAssetIdConfigurationChangedEventType()
    {
      Type type = _deviceHandler.AssetIdConfigurationChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDigitalSwitchConfigurationEventType()
    {
      Type type = _deviceHandler.DigitalSwitchConfigurationEventType;
    }

    [TestMethod]
    public void TestDisableMaintenanceModeEventType()
    {
      Type type = _deviceHandler.DisableMaintenanceModeEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IDisableMaintenanceModeEvent), type);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDiscreteInputConfigurationEventType()
    {
      Type type = _deviceHandler.DiscreteInputConfigurationEventType;
    }

    [TestMethod]
    public void TestEnableMaintenanceModeEventType()
    {
      Type type = _deviceHandler.EnableMaintenanceModeEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IEnableMaintenanceModeEvent), type);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      Type type = _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType;
    }

    [TestMethod]
    public void TestHourMeterModifiedEventType()
    {
      Type type = _deviceHandler.HourMeterModifiedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IHourMeterModifiedEvent), type);
    }

    [TestMethod]
    public void TestLocationUpdateRequestedEventType()
    {
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ILocationStatusUpdateRequestedEvent),
        _deviceHandler.LocationUpdateRequestedEventType);
    }

    [TestMethod]
    public void TestMovingCriteriaConfigurationChangedEventType()
    {
      Type type = _deviceHandler.MovingCriteriaConfigurationChangedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent), type);
    }

    [TestMethod]
    public void TestOdometerModifiedEventType()
    {
      Type type = _deviceHandler.OdometerModifiedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IOdometerModifiedEvent), type);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSiteDispatchedEventType()
    {
      var t = _deviceHandler.SiteDispatchedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSiteRemovedEventType()
    {
      var t = _deviceHandler.SiteRemovedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestGetStartModeEventType()
    {
      var t = _deviceHandler.GetStartModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSetStartModeEventType()
    {
      var t = _deviceHandler.SetStartModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestGetTamperLevelEventType()
    {
      var t = _deviceHandler.GetTamperLevelEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSetTamperLevelEventType()
    {
      var t = _deviceHandler.SetTamperLevelEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      Type type = _deviceHandler.SetDailyReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableRapidReportingEventType()
    {
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableRapidReportingEventType()
    {
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }
    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestReportFrequencyChangedEventType()
    {
      Type type = _deviceHandler.SetReportFrequencyEventType;
    }

    [TestMethod]
    public void TestOutboundEndpointNames()
    {
      IEnumerable<string> expectedEndpointNames = Helpers.GetTestEndpointNames();
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
      foreach (var expectedEndpointName in expectedEndpointNames)
      {
        Assert.IsTrue(actualEndpointNames.Contains(expectedEndpointName));
      }
    }
  }
}
