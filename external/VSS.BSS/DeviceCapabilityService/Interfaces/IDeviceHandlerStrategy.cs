using System;
using System.Collections.Generic;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces
{
  public interface IDeviceHandlerStrategy
  {
    Type AssetIdConfigurationChangedEventType { get; }
    Type DigitalSwitchConfigurationEventType { get; }
    Type DisableMaintenanceModeEventType { get; }
    Type DiscreteInputConfigurationEventType { get; }
    Type EnableMaintenanceModeEventType { get; }
    Type FirstDailyReportStartTimeUtcChangedEventType { get; }
    Type HourMeterModifiedEventType { get; }
    Type LocationUpdateRequestedEventType { get; }
    Type MovingCriteriaConfigurationChangedEventType { get; }
    Type OdometerModifiedEventType { get; }
    Type SiteDispatchedEventType { get; }
    Type SiteRemovedEventType { get; }
    Type SetStartModeEventType { get; }
    Type GetStartModeEventType { get; }
    Type SetTamperLevelEventType { get; }
    Type GetTamperLevelEventType { get; }
    Type SetDailyReportFrequencyEventType { get; }
    Type DisableRapidReportingEventType { get; }
    Type EnableRapidReportingEventType { get; }
    Type SetReportFrequencyEventType { get; }
    IEnumerable<string> OutboundEndpointNames { get; }
  }
}
