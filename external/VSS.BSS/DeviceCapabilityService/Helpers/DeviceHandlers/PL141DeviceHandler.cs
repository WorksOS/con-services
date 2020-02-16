using System;
using System.Collections.Generic;
//using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;

namespace VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers
{
  public class PL141DeviceHandler : IDeviceHandlerStrategy
  {
    public PL141DeviceHandler(IEnumerable<string> outboundEndpointNames)
    {
      OutboundEndpointNames = outboundEndpointNames;
    }

    public Type AssetIdConfigurationChangedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "AssetIdConfigurationChangedEventType")); }
    }

    public Type DigitalSwitchConfigurationEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "DigitalSwitchConfigurationEventType")); }
    }

    public Type DisableMaintenanceModeEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "DisableMaintenanceModeEventType")); }
    }

    public Type DiscreteInputConfigurationEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "DiscreteInputConfigurationEventType")); }
    }

    public Type EnableMaintenanceModeEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "EnableMaintenanceModeEventType")); }
    }

    public Type FirstDailyReportStartTimeUtcChangedEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent); }
    }

    public Type HourMeterModifiedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "HourMeterModifiedEventType")); }
    }

    public Type LocationUpdateRequestedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "LocationUpdateRequestedEventType")); }
    }

    public Type MovingCriteriaConfigurationChangedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "MovingCriteriaConfigurationChangedEventType")); }
    }

    public Type OdometerModifiedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "OdometerModifiedEventType")); }
    }

    public Type SiteDispatchedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "SiteDispatchedEventType")); }
    }

    public Type SiteRemovedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "SiteRemovedEventType")); }
    }

    public Type SetTamperLevelEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "SetTamperLevelEventType")); }
    }

    public Type GetTamperLevelEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "GetTamperLevelEventType")); }
    }

    public Type SetStartModeEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "SetStartModeEventType")); }
    }

    public Type GetStartModeEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL121, "GetStartModeEventType")); }
    }

    public Type SetDailyReportFrequencyEventType
    {
      get { return typeof(DataOut.Interfaces.Events.ISetDailyReportFrequencyEvent); }
    }

    public Type DisableRapidReportingEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.PL141, "DisableRapidReportingEventType")); }
    }

    public Type EnableRapidReportingEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.PL141, "EnableRapidReportingEventType")); }
    }

    public Type SetReportFrequencyEventType
    {
      get { return typeof(DataOut.Interfaces.Events.IReportingFrequencyChangedEvent); }
    }

    public IEnumerable<string> OutboundEndpointNames { get; private set; }

    #region IDeviceHandlerStrategy Members

    #endregion
  }
}

