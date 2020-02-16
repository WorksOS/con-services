using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class TM3000DeviceHandlerTests
  {
    [TestMethod]
    public void TestAssetIdConfigurationChangedEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.AssetIdConfigurationChangedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IAssetIdConfigurationChangedEvent), type);
    }

    [TestMethod]
    public void TestDigitalSwitchConfigurationEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.DigitalSwitchConfigurationEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IDigitalSwitchConfigurationEvent), type);
    }

    [TestMethod]
    public void TestDisableMaintenanceModeEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.DisableMaintenanceModeEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IDisableMaintenanceModeEvent), type);
    }

    [TestMethod]
    public void TestDiscreteInputConfigurationEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.DiscreteInputConfigurationEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IDiscreteInputConfigurationEvent), type);
    }

    [TestMethod]
    public void TestEnableMaintenanceModeEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.EnableMaintenanceModeEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IEnableMaintenanceModeEvent), type);
    }

    [TestMethod]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent), type);
    }

    [TestMethod]
    public void TestHourMeterModifiedEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.HourMeterModifiedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IHourMeterModifiedEvent), type);
    }

    [TestMethod]
    public void TestLocationUpdateRequestedEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.LocationUpdateRequestedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ILocationStatusUpdateRequestedEvent), type);
    }

    [TestMethod]
    public void TestMovingCriteriaConfigurationChangedEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.MovingCriteriaConfigurationChangedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent), type);
    }

    [TestMethod]
    public void TestOdometerModifiedEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.OdometerModifiedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IOdometerModifiedEvent), type);
    }

    [TestMethod]
    public void TestSiteDispatchedEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.SiteDispatchedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISiteDispatchedEvent), type);
    }

    [TestMethod]
    public void TestSiteRemovedEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.SiteRemovedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISiteRemovedEvent), type);
    }

    [TestMethod]
    public void TestGetStartModeEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.GetStartModeEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IGetStartModeEvent), type);
    }

    [TestMethod]
    public void TestSetStartModeEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.SetStartModeEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISetStartModeEvent), type);
    }

    [TestMethod]
    public void TestGetTamperLevelEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.GetTamperLevelEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IGetTamperLevelEvent), type);
    }

    [TestMethod]
    public void TestSetTamperLevelEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      Type type = _deviceHandler.SetTamperLevelEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISetTamperLevelEvent), type);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetDailyReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableRapidReportingEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableRapidReportingEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestReportFrequencyChangedEventType()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetReportFrequencyEventType;
    }

    [TestMethod]
    public void TestOutboundEndpointNames()
    {
      TM3000DeviceHandler _deviceHandler = new TM3000DeviceHandler(Helpers.GetTestEndpointNames());

      IEnumerable<string> expectedEndpointNames = Helpers.GetTestEndpointNames();
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
      foreach (var expectedEndpointName in expectedEndpointNames)
      {
        Assert.IsTrue(actualEndpointNames.Contains(expectedEndpointName));
      }
    }
  }
}
