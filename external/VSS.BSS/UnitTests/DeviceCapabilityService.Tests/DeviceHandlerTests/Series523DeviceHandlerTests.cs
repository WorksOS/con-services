using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class Series523DeviceHandlerTests
  {
    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestAssetIdConfigurationChangedEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
    
      Type type = _deviceHandler.AssetIdConfigurationChangedEventType;
    }

    [TestMethod]
    public void TestDigitalSwitchConfigurationEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
    
      Type type = _deviceHandler.DigitalSwitchConfigurationEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IDigitalSwitchConfigurationEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestDisableMaintenanceModeEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
    
      Type type = _deviceHandler.DisableMaintenanceModeEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IDisableMaintenanceModeEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestDiscreteInputConfigurationEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
    
      Type type = _deviceHandler.DiscreteInputConfigurationEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IDiscreteInputConfigurationEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestEnableMaintenanceModeEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
    
      Type type = _deviceHandler.EnableMaintenanceModeEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IEnableMaintenanceModeEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
    
      Type type = _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestHourMeterModifiedEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
    
      Type type = _deviceHandler.HourMeterModifiedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IHourMeterModifiedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestLocationUpdateRequestedEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
    
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ILocationStatusUpdateRequestedEvent),
        _deviceHandler.LocationUpdateRequestedEventType);
    }

    [TestMethod]
    public void TestMovingCriteriaConfigurationChangedEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
    
      Type type = _deviceHandler.MovingCriteriaConfigurationChangedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestOdometerModifiedEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
    
      Type type = _deviceHandler.OdometerModifiedEventType;
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IOdometerModifiedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestSiteDispatchedEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
    
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISiteDispatchedEvent), _deviceHandler.SiteDispatchedEventType);
    }

    [TestMethod]
    public void TestSiteRemovedEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
    
      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISiteRemovedEvent), _deviceHandler.SiteRemovedEventType);
    }

    [TestMethod]
    public void TestGetStartModeEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());

      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IGetStartModeEvent), _deviceHandler.GetStartModeEventType);
    }

    [TestMethod]
    public void TestSetStartModeEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());

      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISetStartModeEvent), _deviceHandler.SetStartModeEventType);
    }

    [TestMethod]
    public void TestGetTamperLevelEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());

      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.IGetTamperLevelEvent), _deviceHandler.GetTamperLevelEventType);
    }

    [TestMethod]
    public void TestSetTamperLevelEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());

      Assert.AreEqual(typeof(MTSGateway.Interfaces.Events.ISetTamperLevelEvent), _deviceHandler.SetTamperLevelEventType);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetDailyReportFrequencyEventType;
    }
    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestReportFrequencyChangedEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableRapidReportingEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableRapidReportingEventType()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }

    [TestMethod]
    public void TestOutboundEndpointNames()
    {
      Series523DeviceHandler _deviceHandler = new Series523DeviceHandler(Helpers.GetTestEndpointNames());
    
      IEnumerable<string> expectedEndpointNames = Helpers.GetTestEndpointNames();
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
      foreach (var expectedEndpointName in expectedEndpointNames)
      {
        Assert.IsTrue(actualEndpointNames.Contains(expectedEndpointName));
      }
    }
  }
}
