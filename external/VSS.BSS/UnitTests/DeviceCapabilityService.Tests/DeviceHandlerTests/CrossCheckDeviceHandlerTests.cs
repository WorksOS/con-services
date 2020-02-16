using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class CrossCheckDeviceHandlerTests
  {
    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestAssetIdConfigurationChangedEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.AssetIdConfigurationChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDigitalSwitchConfigurationEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DigitalSwitchConfigurationEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableMaintenanceModeEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableMaintenanceModeEventType;
    }

    [TestMethod]
    public void TestDiscreteInputConfigurationEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DiscreteInputConfigurationEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IDiscreteInputConfigurationEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableMaintenanceModeEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableMaintenanceModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType;
    }

    [TestMethod]
    public void TestHourMeterModifiedEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.HourMeterModifiedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IHourMeterModifiedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestLocationUpdateRequestedEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ILocationStatusUpdateRequestedEvent),
        _deviceHandler.LocationUpdateRequestedEventType);
    }

    [TestMethod]
    public void TestMovingCriteriaConfigurationChangedEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.MovingCriteriaConfigurationChangedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestOdometerModifiedEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.OdometerModifiedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IOdometerModifiedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestSiteDispatchedEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISiteDispatchedEvent), _deviceHandler.SiteDispatchedEventType);
    }

    [TestMethod]
    public void TestSiteRemovedEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISiteRemovedEvent), _deviceHandler.SiteRemovedEventType);
    }

    [TestMethod]
    public void TestGetTamperLevelEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IGetTamperLevelEvent), _deviceHandler.GetTamperLevelEventType);
    }

    [TestMethod]
    public void TestSetTamperLevelEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISetTamperLevelEvent), _deviceHandler.SetTamperLevelEventType);
    }

    [TestMethod]
    public void TestGetStartModeEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IGetStartModeEvent), _deviceHandler.GetStartModeEventType);
    }

    [TestMethod]
    public void TestSetStartModeEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISetStartModeEvent), _deviceHandler.SetStartModeEventType);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetDailyReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableRapidReportingEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableRapidReportingEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestReportFrequencyChangedEventType()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetReportFrequencyEventType;
    }

    [TestMethod]
    public void TestOutboundEndpointNames()
    {
      CrossCheckDeviceHandler _deviceHandler = new CrossCheckDeviceHandler(Helpers.GetTestEndpointNames());
      IEnumerable<string> expectedEndpointNames = Helpers.GetTestEndpointNames();
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
      foreach (var expectedEndpointName in expectedEndpointNames)
      {
        Assert.IsTrue(actualEndpointNames.Contains(expectedEndpointName));
      }
    }
  }
}
