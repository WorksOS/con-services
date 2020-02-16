using VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces
{
  public interface IDeviceConfigProcessor
  {
    IFactoryOutboundEventTypeDescriptor GetDigitalSwitchConfigurationEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor GetDisableMaintenanceModeEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor GetDiscreteInputConfigurationEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor GetEnableMaintenanceModeEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor GetFirstDailyReportStartTimeUtcChangedEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor GetHourMeterModifiedEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor GetMovingCriteriaConfigurationChangedEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor GetOdometerModifiedEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor SetStartModeEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor GetStartModeEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor SetTamperLevelEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor GetTamperLevelEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor SetDailyReportFrequencyEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor DisableRapidReportingEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor EnableRapidReportingEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor GetReportingFrequencyChangedEvent(IDeviceQuery device);
  }
}
