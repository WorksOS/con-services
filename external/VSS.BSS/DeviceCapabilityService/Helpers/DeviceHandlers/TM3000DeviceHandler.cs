using System;
using System.Collections.Generic;
//using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;

namespace VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers
{
  public class TM3000DeviceHandler : IDeviceHandlerStrategy
  {
    public TM3000DeviceHandler(IEnumerable<string> outboundEndpointNames)
    {
      OutboundEndpointNames = outboundEndpointNames;
    }

    public Type AssetIdConfigurationChangedEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IAssetIdConfigurationChangedEvent); }
    }

    public Type DigitalSwitchConfigurationEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IDigitalSwitchConfigurationEvent); }
    }

    public Type DisableMaintenanceModeEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IDisableMaintenanceModeEvent); }
    }

    public Type DiscreteInputConfigurationEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IDiscreteInputConfigurationEvent); }
    }

    public Type EnableMaintenanceModeEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IEnableMaintenanceModeEvent); }
    }

    public Type FirstDailyReportStartTimeUtcChangedEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent); }
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
      get { return typeof(DataOut.Interfaces.Events.ISiteDispatchedEvent); }
    }

    public Type SiteRemovedEventType
    {
      get { return typeof(DataOut.Interfaces.Events.ISiteRemovedEvent); }
    }

    public Type SetStartModeEventType
    {
      get { return typeof(DataOut.Interfaces.Events.ISetStartModeEvent); }
    }

    public Type GetStartModeEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IGetStartModeEvent); }
    }

    public Type SetTamperLevelEventType
    {
      get { return typeof(DataOut.Interfaces.Events.ISetTamperLevelEvent); }
    }

    public Type GetTamperLevelEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IGetTamperLevelEvent); }
    }

    public Type SetDailyReportFrequencyEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.TM3000, "SetDailyReportFrequencyEventType")); }
    }

    public Type DisableRapidReportingEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.TM3000, "DisableRapidReportingEventType")); }
    }

    public Type EnableRapidReportingEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.TM3000, "EnableRapidReportingEventType")); }
    }
    public Type SetReportFrequencyEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.TM3000, "SetReportFrequencyEventType")); }
    }

    public IEnumerable<string> OutboundEndpointNames { get; private set; }
  }
}
