using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class DCM300DeviceHandlerTests
  {
    [TestMethod]
    public void TestAssetIdConfigurationChangedEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.AssetIdConfigurationChangedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IAssetIdConfigurationChangedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);

    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDigitalSwitchConfigurationEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.DigitalSwitchConfigurationEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableMaintenanceModeEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.DisableMaintenanceModeEventType;
    }

    [TestMethod]
    public void TestDiscreteInputConfigurationEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.DiscreteInputConfigurationEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IDiscreteInputConfigurationEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableMaintenanceModeEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.EnableMaintenanceModeEventType;
    }

    [TestMethod]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestHourMeterModifiedEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.HourMeterModifiedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IHourMeterModifiedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestLocationUpdateRequestedEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ILocationStatusUpdateRequestedEvent),
        _deviceHandler.LocationUpdateRequestedEventType);
    }

    [TestMethod]
    public void TestMovingCriteriaConfigurationChangedEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.MovingCriteriaConfigurationChangedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestOdometerModifiedEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      
      Type type = _deviceHandler.OdometerModifiedEventType;
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IOdometerModifiedEvent).AssemblyQualifiedName, type.AssemblyQualifiedName);
    }

    [TestMethod]
    public void TestSiteDispatchedEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISiteDispatchedEvent), _deviceHandler.SiteDispatchedEventType);
    }

    [TestMethod]
    public void TestSiteRemovedEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      
      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISiteRemovedEvent), _deviceHandler.SiteRemovedEventType);
    }

    [TestMethod]
    public void TestGetStartModeEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());

      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IGetStartModeEvent), _deviceHandler.GetStartModeEventType);
    }

    [TestMethod]
    public void TestSetStartModeEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());

      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISetStartModeEvent), _deviceHandler.SetStartModeEventType);
    }

    [TestMethod]
    public void TestGetTamperLevelEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());

      Assert.AreEqual(typeof(DataOut.Interfaces.Events.IGetTamperLevelEvent), _deviceHandler.GetTamperLevelEventType);
    }

    [TestMethod]
    public void TestSetTamperLevelEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());

      Assert.AreEqual(typeof(DataOut.Interfaces.Events.ISetTamperLevelEvent), _deviceHandler.SetTamperLevelEventType);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetDailyReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableRapidReportingEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableRapidReportingEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }


    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestReportFrequencyChangedEventType()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetReportFrequencyEventType;
    }

    [TestMethod]
    public void TestOutboundEndpointNames()
    {
      DCM300DeviceHandler _deviceHandler = new DCM300DeviceHandler(Helpers.GetTestEndpointNames());
      
      IEnumerable<string> expectedEndpointNames = Helpers.GetTestEndpointNames();
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
      foreach (var expectedEndpointName in expectedEndpointNames)
      {
        Assert.IsTrue(actualEndpointNames.Contains(expectedEndpointName));
      }
    }
  }
}
