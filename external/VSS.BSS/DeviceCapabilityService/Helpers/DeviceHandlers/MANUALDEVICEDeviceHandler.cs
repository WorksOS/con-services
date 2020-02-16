using System;
using System.Collections.Generic;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;

namespace VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers
{
  public class MANUALDEVICEDeviceHandler : IDeviceHandlerStrategy
  {
    public MANUALDEVICEDeviceHandler(IEnumerable<string> outboundEndpointNames)
    {
      OutboundEndpointNames = outboundEndpointNames;
    }

    public Type AssetIdConfigurationChangedEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type DigitalSwitchConfigurationEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type DisableMaintenanceModeEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type DiscreteInputConfigurationEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type EnableMaintenanceModeEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type FirstDailyReportStartTimeUtcChangedEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type HourMeterModifiedEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type LocationUpdateRequestedEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type MovingCriteriaConfigurationChangedEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type OdometerModifiedEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type SiteDispatchedEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type SiteRemovedEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type SetStartModeEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type GetStartModeEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type SetTamperLevelEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type GetTamperLevelEventType
    {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<string> OutboundEndpointNames { get; private set; }


    public Type SetDailyReportFrequencyEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type DisableRapidReportingEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type EnableRapidReportingEventType
    {
      get { throw new NotImplementedException(); }
    }

    public Type SetReportFrequencyEventType
    {
      get { throw new NotImplementedException(); }
    }
  }
}
