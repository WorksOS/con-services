using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class Series521DeviceHandlerTests
  {
    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestAssetIdConfigurationChangedEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.AssetIdConfigurationChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDigitalSwitchConfigurationEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DigitalSwitchConfigurationEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableMaintenanceModeEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableMaintenanceModeEventType;
    }

    [TestMethod]
    public void TestDiscreteInputConfigurationEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DiscreteInputConfigurationEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IDiscreteInputConfigurationEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableMaintenanceModeEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableMaintenanceModeEventType;
    }

    [TestMethod]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestHourMeterModifiedEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.HourMeterModifiedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IHourMeterModifiedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestLocationUpdateRequestedEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ILocationStatusUpdateRequestedEvent),
        _deviceHandler.LocationUpdateRequestedEventType);
    }

    [TestMethod]
    public void TestMovingCriteriaConfigurationChangedEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.MovingCriteriaConfigurationChangedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestOdometerModifiedEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.OdometerModifiedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IOdometerModifiedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestSiteDispatchedEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISiteDispatchedEvent), _deviceHandler.SiteDispatchedEventType);
    }

    [TestMethod]
    public void TestSiteRemovedEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISiteRemovedEvent), _deviceHandler.SiteRemovedEventType);
    }

    [TestMethod]
    public void TestGetStartModeEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IGetStartModeEvent), _deviceHandler.GetStartModeEventType);
    }

    [TestMethod]
    public void TestSetStartModeEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISetStartModeEvent), _deviceHandler.SetStartModeEventType);
    }

    [TestMethod]
    public void TestGetTamperLevelEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IGetTamperLevelEvent), _deviceHandler.GetTamperLevelEventType);
    }

    [TestMethod]
    public void TestSetTamperLevelEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISetTamperLevelEvent), _deviceHandler.SetTamperLevelEventType);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetDailyReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableRapidReportingEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableRapidReportingEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestReportFrequencyChangedEventType()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetReportFrequencyEventType;
    }

    [TestMethod]
    public void TestOutboundEndpointNames()
    {
      Series521DeviceHandler _deviceHandler = new Series521DeviceHandler(Helpers.GetTestEndpointNames());
      IEnumerable<string> expectedEndpointNames = Helpers.GetTestEndpointNames();
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
      foreach (var expectedEndpointName in expectedEndpointNames)
      {
        Assert.IsTrue(actualEndpointNames.Contains(expectedEndpointName));
      }
    }
  }
}
