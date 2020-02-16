using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class Series522DeviceHandlerTests
  {
    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestAssetIdConfigurationChangedEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.AssetIdConfigurationChangedEventType;
    }

    [TestMethod]
    public void TestDigitalSwitchConfigurationEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DigitalSwitchConfigurationEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IDigitalSwitchConfigurationEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestDisableMaintenanceModeEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableMaintenanceModeEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IDisableMaintenanceModeEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestDiscreteInputConfigurationEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DiscreteInputConfigurationEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IDiscreteInputConfigurationEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestEnableMaintenanceModeEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableMaintenanceModeEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IEnableMaintenanceModeEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestHourMeterModifiedEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.HourMeterModifiedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IHourMeterModifiedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestLocationUpdateRequestedEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ILocationStatusUpdateRequestedEvent),
        _deviceHandler.LocationUpdateRequestedEventType);
    }

    [TestMethod]
    public void TestMovingCriteriaConfigurationChangedEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.MovingCriteriaConfigurationChangedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestOdometerModifiedEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.OdometerModifiedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IOdometerModifiedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestSiteDispatchedEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISiteDispatchedEvent), _deviceHandler.SiteDispatchedEventType);
    }

    [TestMethod]
    public void TestSiteRemovedEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISiteRemovedEvent), _deviceHandler.SiteRemovedEventType);
    }

    [TestMethod]
    public void TestGetStartModeEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IGetStartModeEvent), _deviceHandler.GetStartModeEventType);
    }

    [TestMethod]
    public void TestSetStartModeEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISetStartModeEvent), _deviceHandler.SetStartModeEventType);
    }

    [TestMethod]
    public void TestGetTamperLevelEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IGetTamperLevelEvent), _deviceHandler.GetTamperLevelEventType);
    }

    [TestMethod]
    public void TestSetTamperLevelEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISetTamperLevelEvent), _deviceHandler.SetTamperLevelEventType);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetDailyReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableRapidReportingEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableRapidReportingEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestReportFrequencyChangedEventType()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetReportFrequencyEventType;
    }

    [TestMethod]
    public void TestOutboundEndpointNames()
    {
      Series522DeviceHandler _deviceHandler = new Series522DeviceHandler(Helpers.GetTestEndpointNames());
      IEnumerable<string> expectedEndpointNames = Helpers.GetTestEndpointNames();
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
      foreach (var expectedEndpointName in expectedEndpointNames)
      {
        Assert.IsTrue(actualEndpointNames.Contains(expectedEndpointName));
      }
    }
  }
}
