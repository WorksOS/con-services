using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.DeviceHandlerTests
{
  [TestClass]
  public class ManualDeviceHandlerTests
  {
    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestAssetIdConfigurationChangedEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.AssetIdConfigurationChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDigitalSwitchConfigurationEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DigitalSwitchConfigurationEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableMaintenanceModeEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableMaintenanceModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDiscreteInputConfigurationEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DiscreteInputConfigurationEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableMaintenanceModeEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableMaintenanceModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestFirstDailyReportStartTimeUtcChangedEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.FirstDailyReportStartTimeUtcChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestHourMeterModifiedEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.HourMeterModifiedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestLocationUpdateRequestedEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.LocationUpdateRequestedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestMovingCriteriaConfigurationChangedEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.MovingCriteriaConfigurationChangedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestOdometerModifiedEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.OdometerModifiedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSiteDispatchedEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SiteDispatchedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSiteRemovedEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SiteRemovedEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestGetStartModeEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.GetStartModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSetStartModeEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetStartModeEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestGetTamperLevelEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.GetTamperLevelEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestSetTamperLevelEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetTamperLevelEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDailyReportFrequencyModifiedEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetDailyReportFrequencyEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestEnableRapidReportingEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.EnableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestDisableRapidReportingEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.DisableRapidReportingEventType;
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void TestReportFrequencyChangedEventType()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      Type type = _deviceHandler.SetReportFrequencyEventType;
    }

    [TestMethod]
    public void TestOutboundEndpointNames()
    {
      MANUALDEVICEDeviceHandler _deviceHandler = new MANUALDEVICEDeviceHandler(Helpers.GetTestEndpointNames());
      IEnumerable<string> expectedEndpointNames = Helpers.GetTestEndpointNames();
      IEnumerable<string> actualEndpointNames = _deviceHandler.OutboundEndpointNames;
      foreach (var expectedEndpointName in expectedEndpointNames)
      {
        Assert.IsTrue(actualEndpointNames.Contains(expectedEndpointName));
      }
    }
  }
}
