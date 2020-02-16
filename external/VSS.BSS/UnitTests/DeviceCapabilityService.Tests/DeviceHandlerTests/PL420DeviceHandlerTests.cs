using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class PL420DeviceHandlerTests
  {
    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestAssetIdConfigurationChangedEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.AssetIdConfigurationChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDigitalSwitchConfigurationEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DigitalSwitchConfigurationEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableMaintenanceModeEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableMaintenanceModeEventType;
    }

    [TestMethod]
    public void TestDiscreteInputConfigurationEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DiscreteInputConfigurationEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IDiscreteInputConfigurationEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableMaintenanceModeEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableMaintenanceModeEventType;
    }

    [TestMethod]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestHourMeterModifiedEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.HourMeterModifiedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IHourMeterModifiedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestLocationUpdateRequestedEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ILocationStatusUpdateRequestedEvent),
        _deviceHandler.LocationUpdateRequestedEventType);
    }

    [TestMethod]
    public void TestMovingCriteriaConfigurationChangedEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.MovingCriteriaConfigurationChangedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestOdometerModifiedEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.OdometerModifiedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IOdometerModifiedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestSiteDispatchedEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISiteDispatchedEvent), _deviceHandler.SiteDispatchedEventType);
    }

    [TestMethod]
    public void TestSiteRemovedEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISiteRemovedEvent), _deviceHandler.SiteRemovedEventType);
    }

    [TestMethod]
    public void TestGetStartModeEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IGetStartModeEvent), _deviceHandler.GetStartModeEventType);
    }

    [TestMethod]
    public void TestSetStartModeEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISetStartModeEvent), _deviceHandler.SetStartModeEventType);
    }

    [TestMethod]
    public void TestGetTamperLevelEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IGetTamperLevelEvent), _deviceHandler.GetTamperLevelEventType);
    }

    [TestMethod]
    public void TestSetTamperLevelEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISetTamperLevelEvent), _deviceHandler.SetTamperLevelEventType);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetDailyReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableRapidReportingEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableRapidReportingEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestReportFrequencyChangedEventType()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetReportFrequencyEventType;
    }

    [TestMethod]
    public void TestOutboundEndpointNames()
    {
      PL420DeviceHandler _deviceHandler = new PL420DeviceHandler(Helpers.GetTestEndpointNames());
      IEnumerable<string> expectedEndpointNames = Helpers.GetTestEndpointNames();
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
      foreach (var expectedEndpointName in expectedEndpointNames)
      {
        Assert.IsTrue(actualEndpointNames.Contains(expectedEndpointName));
      }
    }
  }
}
