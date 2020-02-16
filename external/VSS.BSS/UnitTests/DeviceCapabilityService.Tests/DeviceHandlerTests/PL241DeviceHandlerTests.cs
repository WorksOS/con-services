using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class PL241DeviceHandlerTests
  {
    [TestMethod]
    public void TestAssetIdConfigurationChangedEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IAssetIdConfigurationChangedEvent), _deviceHandler.AssetIdConfigurationChangedEventType);
    }

    [TestMethod]
    public void TestDigitalSwitchConfigurationEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IDigitalSwitchConfigurationEvent), _deviceHandler.DigitalSwitchConfigurationEventType);
    }

    [TestMethod]
    public void TestDisableMaintenanceModeEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IDisableMaintenanceModeEvent), _deviceHandler.DisableMaintenanceModeEventType);
    }

    [TestMethod]
    public void TestDiscreteInputConfigurationEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IDiscreteInputConfigurationEvent), _deviceHandler.DiscreteInputConfigurationEventType);
    }

    [TestMethod]
    public void TestEnableMaintenanceModeEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IEnableMaintenanceModeEvent), _deviceHandler.EnableMaintenanceModeEventType);
    }

    [TestMethod]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent), _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType);
    }

    [TestMethod]
    public void TestHourMeterModifiedEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IHourMeterModifiedEvent), _deviceHandler.HourMeterModifiedEventType);
    }

    [TestMethod]
    public void TestLocationUpdateRequestedEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ILocationStatusUpdateRequestedEvent), _deviceHandler.LocationUpdateRequestedEventType);
    }

    [TestMethod]
    public void TestIMovingCriteriaConfigurationChangedEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent), _deviceHandler.MovingCriteriaConfigurationChangedEventType);
    }

    [TestMethod]
    public void TestOdometerModifiedEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IOdometerModifiedEvent), _deviceHandler.OdometerModifiedEventType);
    }

    [TestMethod]
    public void TestSiteDispatchedEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISiteDispatchedEvent), _deviceHandler.SiteDispatchedEventType);
    }

    [TestMethod]
    public void TestSiteRemovedEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISiteRemovedEvent), _deviceHandler.SiteRemovedEventType);
    }

    [TestMethod]
    public void TestSetTamperLevelEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISetTamperLevelEvent), _deviceHandler.SetTamperLevelEventType);
    }

    [TestMethod]
    public void TestGetTamperLevelEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IGetTamperLevelEvent), _deviceHandler.GetTamperLevelEventType);
    }

    [TestMethod]
    public void TestSetStartModeEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISetStartModeEvent), _deviceHandler.SetStartModeEventType);
    }

    [TestMethod]
    public void TestGetStartModeEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IGetStartModeEvent), _deviceHandler.GetStartModeEventType);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetDailyReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableRapidReportingEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableRapidReportingEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestReportFrequencyChangedEventType()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetReportFrequencyEventType;
    }

    [TestMethod]
    public void TestOutboundEndpointNames()
    {
      PL241DeviceHandler _deviceHandler = new PL241DeviceHandler(Helpers.GetTestEndpointNames());
      IEnumerable<string> expectedEndpointNames = Helpers.GetTestEndpointNames();
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
      foreach (var expectedEndpointName in expectedEndpointNames)
      {
        Assert.IsTrue(actualEndpointNames.Contains(expectedEndpointName));
      }
    }
  }
}
