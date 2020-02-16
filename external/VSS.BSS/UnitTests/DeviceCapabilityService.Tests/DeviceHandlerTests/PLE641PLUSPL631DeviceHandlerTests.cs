using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class PLE641PLUSPL631DeviceHandlerTests
  {
    [TestMethod]
    public void TestAssetIdConfigurationChangedEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IAssetIdConfigurationChangedEvent), _deviceHandler.AssetIdConfigurationChangedEventType);
    }

    [TestMethod]
    public void TestDigitalSwitchConfigurationEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IDigitalSwitchConfigurationEvent), _deviceHandler.DigitalSwitchConfigurationEventType);
    }

    [TestMethod]
    public void TestDisableMaintenanceModeEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IDisableMaintenanceModeEvent), _deviceHandler.DisableMaintenanceModeEventType);
    }

    [TestMethod]
    public void TestDiscreteInputConfigurationEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IDiscreteInputConfigurationEvent), _deviceHandler.DiscreteInputConfigurationEventType);
    }

    [TestMethod]
    public void TestEnableMaintenanceModeEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IEnableMaintenanceModeEvent), _deviceHandler.EnableMaintenanceModeEventType);
    }

    [TestMethod]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent), _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType);
    }

    [TestMethod]
    public void TestHourMeterModifiedEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IHourMeterModifiedEvent), _deviceHandler.HourMeterModifiedEventType);
    }

    [TestMethod]
    public void TestLocationUpdateRequestedEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ILocationStatusUpdateRequestedEvent), _deviceHandler.LocationUpdateRequestedEventType);
    }

    [TestMethod]
    public void TestIMovingCriteriaConfigurationChangedEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent), _deviceHandler.MovingCriteriaConfigurationChangedEventType);
    }

    [TestMethod]
    public void TestOdometerModifiedEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IOdometerModifiedEvent), _deviceHandler.OdometerModifiedEventType);
    }

    [TestMethod]
    public void TestSiteDispatchedEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISiteDispatchedEvent), _deviceHandler.SiteDispatchedEventType);
    }

    [TestMethod]
    public void TestSiteRemovedEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISiteRemovedEvent), _deviceHandler.SiteRemovedEventType);
    }

    [TestMethod]
    public void TestSetTamperLevelEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISetTamperLevelEvent), _deviceHandler.SetTamperLevelEventType);
    }

    [TestMethod]
    public void TestGetTamperLevelEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IGetTamperLevelEvent), _deviceHandler.GetTamperLevelEventType);
    }

    [TestMethod]
    public void TestSetStartModeEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISetStartModeEvent), _deviceHandler.SetStartModeEventType);
    }

    [TestMethod]
    public void TestGetStartModeEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IGetStartModeEvent), _deviceHandler.GetStartModeEventType);
    }
    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetDailyReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableRapidReportingEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableRapidReportingEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestReportFrequencyChangedEventType()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetReportFrequencyEventType;
    }

    [TestMethod]
    public void TestOutboundEndpointNames()
    {
      PLE641PLUSPL631DeviceHandler _deviceHandler = new PLE641PLUSPL631DeviceHandler(Helpers.GetTestEndpointNames());
      IEnumerable<string> expectedEndpointNames = Helpers.GetTestEndpointNames();
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
      foreach (var expectedEndpointName in expectedEndpointNames)
      {
        Assert.IsTrue(actualEndpointNames.Contains(expectedEndpointName));
      }
    }
  }
}
