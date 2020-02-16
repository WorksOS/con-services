using System;
using System.Collections.Generic;
//using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;

namespace VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers
{
  public class SNM940DeviceHandler : IDeviceHandlerStrategy
  {
    public SNM940DeviceHandler(IEnumerable<string> outboundEndpointNames)
    {
      OutboundEndpointNames = outboundEndpointNames;
    }

    public Type AssetIdConfigurationChangedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.SNM940, "AssetIdConfigurationChangedEventType")); }
    }

    public Type DigitalSwitchConfigurationEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.SNM940, "DigitalSwitchConfigurationEventType")); }
    }

    public Type DisableMaintenanceModeEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.SNM940, "DisableMaintenanceModeEventType")); }
    }

    public Type DiscreteInputConfigurationEventType
    {
      get { return typeof(MTSGateway.Interfaces.Events.IDiscreteInputConfigurationEvent); }
    }

    public Type EnableMaintenanceModeEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.SNM940, "EnableMaintenanceModeEventType")); }
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
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.SNM940, "SetDailyReportFrequencyEventType")); }
    }

    public Type DisableRapidReportingEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.SNM940, "DisableRapidReportingEventType")); }
    }

    public Type EnableRapidReportingEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.SNM940, "EnableRapidReportingEventType")); }
    }
    public Type SetReportFrequencyEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.SNM940, "SetReportFrequencyEventType")); }
    }

    public IEnumerable<string> OutboundEndpointNames { get; private set; }
  }
}
