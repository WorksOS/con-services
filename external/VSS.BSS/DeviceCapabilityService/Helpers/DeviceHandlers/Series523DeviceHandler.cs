using System;
using System.Collections.Generic;
//using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;

namespace VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers
{
  public class Series523DeviceHandler : IDeviceHandlerStrategy
  {
    public Series523DeviceHandler(IEnumerable<string> outboundEndpointNames)
    {
      OutboundEndpointNames = outboundEndpointNames;
    }

    public Type AssetIdConfigurationChangedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.Series523, "AssetIdConfigurationChangedEventType")); }
    }

    public Type DigitalSwitchConfigurationEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.IDigitalSwitchConfigurationEvent); }
    }

    public Type DisableMaintenanceModeEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.IDisableMaintenanceModeEvent); }
    }

    public Type DiscreteInputConfigurationEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.IDiscreteInputConfigurationEvent); }
    }

    public Type EnableMaintenanceModeEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.IEnableMaintenanceModeEvent); }
    }

    public Type FirstDailyReportStartTimeUtcChangedEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent); }
    }

    public Type HourMeterModifiedEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.IHourMeterModifiedEvent); }
    }

    public Type LocationUpdateRequestedEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.ILocationStatusUpdateRequestedEvent); }
    }

    public Type MovingCriteriaConfigurationChangedEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent); }
    }

    public Type OdometerModifiedEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.IOdometerModifiedEvent); }
    }

    public Type SiteDispatchedEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.ISiteDispatchedEvent); }
    }

    public Type SiteRemovedEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.ISiteRemovedEvent); }
    }

    public Type SetStartModeEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.ISetStartModeEvent); }
    }

    public Type GetStartModeEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.IGetStartModeEvent); }
    }

    public Type SetTamperLevelEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.ISetTamperLevelEvent); }
    }

    public Type GetTamperLevelEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.IGetTamperLevelEvent); }
    }

    public Type SetDailyReportFrequencyEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.Series523, "SetDailyReportFrequencyEventType")); }
    }

    public Type DisableRapidReportingEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.Series523, "DisableRapidReportingEventType")); }
    }

    public Type EnableRapidReportingEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.Series523, "EnableRapidReportingEventType")); }
    }
    public Type SetReportFrequencyEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.Series523, "SetReportFrequencyEventType")); }
    }

    public IEnumerable<string> OutboundEndpointNames { get; private set; }
  }
}
