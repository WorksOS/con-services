using System;
using System.Collections.Generic;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;

namespace VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers
{
  public class UnknownDeviceHandler : IDeviceHandlerStrategy
  {
    public Type AssetIdConfigurationChangedEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("AssetIdConfigurationChangedEventType")); }
    }

    public Type DigitalSwitchConfigurationEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("DigitalSwitchConfigurationEventType")); }
    }

    public Type DisableMaintenanceModeEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("DisableMaintenanceModeEventType")); }
    }

    public Type DiscreteInputConfigurationEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("DiscreteInputConfigurationEventType")); }
    }

    public Type EnableMaintenanceModeEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("EnableMaintenanceModeEventType")); }
    }

    public Type FirstDailyReportStartTimeUtcChangedEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("FirstDailyReportStartTimeUtcChangedEventType")); }
    }

    public Type HourMeterModifiedEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("HourMeterModifiedEventType")); }
    }

    public Type LocationUpdateRequestedEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("LocationUpdateRequestedEventType")); }
    }

    public Type MovingCriteriaConfigurationChangedEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("MovingCriteriaConfigurationChangedEventType")); }
    }

    public Type OdometerModifiedEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("OdometerModifiedEventType")); }
    }

    public Type SiteDispatchedEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("SiteDispatchedEventType")); }
    }

    public Type SiteRemovedEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("SiteRemovedEventType")); }
    }

    public Type SetStartModeEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("SetStartModeEventType")); }
    }

    public Type GetStartModeEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("GetStartModeEventType")); }
    }

    public Type SetTamperLevelEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("SetTamperLevelEventType")); }
    }

    public Type GetTamperLevelEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("GetTamperLevelEventType")); }
    }

    public IEnumerable<string> OutboundEndpointNames
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("OutboundEndpointNames")); }
    }

    public Type SetDailyReportFrequencyEventType
    {
      get
      {
        throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("SetDailyReportFrequencyEventType"));
      }
    }

    public Type DisableRapidReportingEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("DisableRapidReportingEventType")); }
    }

    public Type EnableRapidReportingEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("EnableRapidReportingEventType")); }
    }

    public Type SetReportFrequencyEventType
    {
      get { throw new UnknownDeviceException(DeviceHandlerExceptionHelper.Unknown("SetReportFrequencyEventType")); }
    }

  }
}
