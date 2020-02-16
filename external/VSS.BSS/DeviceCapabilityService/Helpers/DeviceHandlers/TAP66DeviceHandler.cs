using System;
using System.Collections.Generic;
//using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;

namespace VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers
{
  public class TAP66DeviceHandler : IDeviceHandlerStrategy
  {
    public TAP66DeviceHandler(IEnumerable<string> outboundEndpointNames)
    {
      OutboundEndpointNames = outboundEndpointNames;
    }

    public Type AssetIdConfigurationChangedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.TAP66, "AssetIdConfigurationChangedEventType")); }
    }

    public Type DigitalSwitchConfigurationEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.TAP66, "DigitalSwitchConfigurationEventType")); }
    }

    public Type DisableMaintenanceModeEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IDisableMaintenanceModeEvent); }
    }

    public Type DiscreteInputConfigurationEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.TAP66, "DiscreteInputConfigurationEventType")); }
    }

    public Type EnableMaintenanceModeEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IEnableMaintenanceModeEvent); }
    }

    public Type FirstDailyReportStartTimeUtcChangedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.TAP66, "FirstDailyReportStartTimeUtcChangedEventType")); }
    }

    public Type HourMeterModifiedEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IHourMeterModifiedEvent); }
    }

    public Type LocationUpdateRequestedEventType
    {
      get { return typeof(DataOut.Interfaces.Events.ILocationStatusUpdateRequestedEvent); }
    }

    public Type MovingCriteriaConfigurationChangedEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent); }
    }

    public Type OdometerModifiedEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IOdometerModifiedEvent); }
    }

    public Type SiteDispatchedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.TAP66, "SiteDispatchedEventType")); }
    }

    public Type SiteRemovedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.TAP66, "SiteRemovedEventType")); }
    }

    public Type SetStartModeEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.TAP66, "SetStartModeEventType")); }
    }

    public Type GetStartModeEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.TAP66, "GetStartModeEventType")); }
    }

    public Type SetTamperLevelEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.TAP66, "SetTamperLevelEventType")); }
    }

    public Type GetTamperLevelEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.TAP66, "GetTamperLevelEventType")); }
    }

    public Type SetDailyReportFrequencyEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.TAP66, "SetDailyReportFrequencyEventType")); }
    }

    public Type DisableRapidReportingEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.TAP66, "DisableRapidReportingEventType")); }
    }

    public Type EnableRapidReportingEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.TAP66, "EnableRapidReportingEventType")); }
    }

    public Type SetReportFrequencyEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.TAP66, "SetReportFrequencyEventType")); }
    }

    public IEnumerable<string> OutboundEndpointNames { get; private set; }
  }
}
