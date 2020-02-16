using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class PL131HandlerTests
  {
    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestAssetIdConfigurationChangedEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.AssetIdConfigurationChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDigitalSwitchConfigurationEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DigitalSwitchConfigurationEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableMaintenanceModeEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableMaintenanceModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDiscreteInputConfigurationEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DiscreteInputConfigurationEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableMaintenanceModeEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableMaintenanceModeEventType;
    }

    [TestMethod]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent), _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestHourMeterModifiedEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.HourMeterModifiedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestLocationUpdateRequestedEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.LocationUpdateRequestedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestMovingCriteriaConfigurationChangedEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.MovingCriteriaConfigurationChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestOdometerModifiedEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.OdometerModifiedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSiteDispatchedEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SiteDispatchedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSiteRemovedEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SiteRemovedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestGetStartModeEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.GetStartModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSetStartModeEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetStartModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestGetTamperLevelEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.GetTamperLevelEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSetTamperLevelEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetTamperLevelEventType;
    }

    [TestMethod]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISetDailyReportFrequencyEvent), _deviceHandler.SetDailyReportFrequencyEventType);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableRapidReportingEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableRapidReportingEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }

    [TestMethod]
    public void TestReportFrequencyChangedEventType()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IReportingFrequencyChangedEvent), _deviceHandler.SetReportFrequencyEventType);
    }

    [TestMethod]
    public void TestOutboundEndpointNames()
    {
      PL131DeviceHandler _deviceHandler = new PL131DeviceHandler(Helpers.GetTestEndpointNames());
      IEnumerable<string> expectedEndpointNames = Helpers.GetTestEndpointNames();
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
      foreach (var expectedEndpointName in expectedEndpointNames)
      {
        Assert.IsTrue(actualEndpointNames.Contains(expectedEndpointName));
      }
    }
  }
}
