using VSS.BaseEvents;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Factories
{
  public interface IDeviceConfigFactory
  {
    IDigitalSwitchConfigurationEvent BuildDigitalSwitchConfigurationEventForDevice(IDeviceQuery deviceQuery);
    IDisableMaintenanceModeEvent BuildDisableMaintenanceModeEventForDevice(IDeviceQuery deviceQuery);
    IDiscreteInputConfigurationEvent BuildDiscreteInputConfigurationEventForDevice(IDeviceQuery deviceQuery);
    IEnableMaintenanceModeEvent BuildEnableMaintenanceModeEventForDevice(IDeviceQuery deviceQuery);
    IFirstDailyReportStartTimeUtcChangedEvent BuildFirstDailyReportStartTimeUtcChangedEventForDevice(IDeviceQuery deviceQuery);
    IHourMeterModifiedEvent BuildHourMeterModifiedEventForDevice(IDeviceQuery deviceQuery);
    IMovingCriteriaConfigurationChangedEvent BuildMovingCriteriaConfigurationChangedEventForDevice(IDeviceQuery deviceQuery);
    IOdometerModifiedEvent BuildOdometerModifiedEventForDevice(IDeviceQuery deviceQuery);
    IGetStartModeEvent BuildGetStartModeEventForDevice(IDeviceQuery deviceQuery);
    ISetStartModeEvent BuildSetStartModeEventForDevice(IDeviceQuery deviceQuery);
    IGetTamperLevelEvent BuildGetTamperLevelEventForDevice(IDeviceQuery deviceQuery);
    ISetTamperLevelEvent BuildSetTamperLevelEventForDevice(IDeviceQuery deviceQuery);
    ISetDailyReportFrequencyEvent BuildSetDailyReportFrequencyEvent(IDeviceQuery deviceQuery);
    IDisableRapidReportingEvent BuildDisableRapidReportingEventForDevice(IDeviceQuery deviceQuery);
    IEnableRapidReportingEvent BuildEnableRapidReportingEventForDevice(IDeviceQuery deviceQuery);
    IReportingFrequencyChangedEvent BuildReportingFrequencyChangedEvent(IDeviceQuery deviceQuery);
  }
}
