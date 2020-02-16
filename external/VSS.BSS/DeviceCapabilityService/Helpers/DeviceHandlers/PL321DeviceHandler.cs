using System;
using System.Collections.Generic;
//using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;

namespace VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers
{
  public class PL321DeviceHandler : IDeviceHandlerStrategy
  {
    public PL321DeviceHandler(IEnumerable<string> outboundEndpointNames)
    {
      OutboundEndpointNames = outboundEndpointNames;
    }

    public Type AssetIdConfigurationChangedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "AssetIdConfigurationChangedEventType")); }
    }

    public Type DigitalSwitchConfigurationEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "DigitalSwitchConfigurationEventType")); }
    }

    public Type DisableMaintenanceModeEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "DisableMaintenanceModeEventType")); }
    }

    public Type DiscreteInputConfigurationEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "DiscreteInputConfigurationEventType")); }
    }

    public Type EnableMaintenanceModeEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "EnableMaintenanceModeEventType")); }
    }

    public Type FirstDailyReportStartTimeUtcChangedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "FirstDailyReportStartTimeUtcChangedEventType")); }
    }

    public Type HourMeterModifiedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "HourMeterModifiedEventType")); }
    }

    public Type LocationUpdateRequestedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "LocationUpdateRequestedEventType")); }
    }

    public Type MovingCriteriaConfigurationChangedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "MovingCriteriaConfigurationChangedEventType")); }
    }

    public Type OdometerModifiedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "OdometerModifiedEventType")); }
    }

    public Type SiteDispatchedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "SiteDispatchedEventType")); }
    }

    public Type SiteRemovedEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "SiteRemovedEventType")); }
    }

    public Type SetStartModeEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "SetStartModeEventType")); }
    }

    public Type GetStartModeEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "GetStartModeEventType")); }
    }

    public Type SetTamperLevelEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "SetTamperLevelEventType")); }
    }

    public Type GetTamperLevelEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "GetTamperLevelEventType")); }
    }
    public Type SetDailyReportFrequencyEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "SetDailyReportFrequencyEventType")); }
    }
    
    public Type DisableRapidReportingEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.PL321, "DisableRapidReportingEventType")); }
    }

    public Type EnableRapidReportingEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotSupported(DeviceTypeEnum.PL321, "EnableRapidReportingEventType")); }
    }
    
    public Type SetReportFrequencyEventType
    {
      get { throw new NotImplementedException(DeviceHandlerExceptionHelper.NotCurrentlyImplemented(DeviceTypeEnum.PL321, "SetReportFrequencyEventType")); }
    }

    
    public IEnumerable<string> OutboundEndpointNames { get; private set; }
  }
}
