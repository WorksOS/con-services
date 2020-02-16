using EasyHttp.Http;
using System.Configuration;
using VSS.BaseEvents;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Factories;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Factories
{
  public class DeviceConfigFactory : IDeviceConfigFactory
  {
    private readonly string _deviceCapabilitySvcUri;
    private readonly IEventTypeHelper _eventTypeHelper;

    static DeviceConfigFactory()
    {
      FixReferencedTypesAtCompileTime();
    }

    private static void FixReferencedTypesAtCompileTime()
    {
      // Need to load all the referenced types for compiler to force copy
      // dependent type assemblies to clients of this assembly;
      // otherwise, clients of this assembly must reference assemblies with these types explicitly
      var referencedTypes = new[]
                              {
                                typeof(DataOut.Interfaces.Events.IDigitalSwitchConfigurationEvent).AssemblyQualifiedName,
                                typeof(MTSGateway.Interfaces.Events.IDigitalSwitchConfigurationEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.IDisableMaintenanceModeEvent).AssemblyQualifiedName,
                                typeof(MTSGateway.Interfaces.Events.IDisableMaintenanceModeEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.IDiscreteInputConfigurationEvent).AssemblyQualifiedName,
                                typeof(MTSGateway.Interfaces.Events.IDiscreteInputConfigurationEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.IEnableMaintenanceModeEvent).AssemblyQualifiedName,
                                typeof(MTSGateway.Interfaces.Events.IEnableMaintenanceModeEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent).AssemblyQualifiedName,
                                typeof(MTSGateway.Interfaces.Events.IFirstDailyReportStartTimeUtcChangedEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.IHourMeterModifiedEvent).AssemblyQualifiedName,
                                typeof(MTSGateway.Interfaces.Events.IHourMeterModifiedEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent).AssemblyQualifiedName,
                                typeof(MTSGateway.Interfaces.Events.IMovingCriteriaConfigurationChangedEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.IOdometerModifiedEvent).AssemblyQualifiedName,
                                typeof(MTSGateway.Interfaces.Events.IOdometerModifiedEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.IGetStartModeEvent).AssemblyQualifiedName,
                                typeof(MTSGateway.Interfaces.Events.IGetStartModeEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.ISetStartModeEvent).AssemblyQualifiedName,
                                typeof(MTSGateway.Interfaces.Events.ISetStartModeEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.IGetTamperLevelEvent).AssemblyQualifiedName,
                                typeof(MTSGateway.Interfaces.Events.IGetTamperLevelEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.ISetTamperLevelEvent).AssemblyQualifiedName,
                                typeof(MTSGateway.Interfaces.Events.ISetTamperLevelEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.ISetDailyReportFrequencyEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.IEnableRapidReportingEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.IDisableRapidReportingEvent).AssemblyQualifiedName,
                                typeof(DataOut.Interfaces.Events.IReportingFrequencyChangedEvent).AssemblyQualifiedName
                              };
    }

    public DeviceConfigFactory()
    {
      _deviceCapabilitySvcUri = ConfigurationManager.AppSettings["DeviceCapabilityServiceBaseUri"] + "/" + ControllerConstants.DeviceConfigControllerRouteName;
      _eventTypeHelper = new EventTypeHelper(new HttpClientWrapper(new HttpClient() { Request = { Accept = HttpContentTypes.ApplicationJson } }));
    }

    public IDigitalSwitchConfigurationEvent BuildDigitalSwitchConfigurationEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<IDigitalSwitchConfigurationEvent>(ActionConstants.GetDigitalSwitchConfigurationEvent, _deviceCapabilitySvcUri, deviceQuery);
    }

    public IDisableMaintenanceModeEvent BuildDisableMaintenanceModeEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<IDisableMaintenanceModeEvent>(ActionConstants.GetDisableMaintenanceModeEvent, _deviceCapabilitySvcUri, deviceQuery);
    }

    public IDiscreteInputConfigurationEvent BuildDiscreteInputConfigurationEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<IDiscreteInputConfigurationEvent>(ActionConstants.GetDiscreteInputConfigurationEvent, _deviceCapabilitySvcUri, deviceQuery);
    }

    public IEnableMaintenanceModeEvent BuildEnableMaintenanceModeEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<IEnableMaintenanceModeEvent>(ActionConstants.GetEnableMaintenanceModeEvent, _deviceCapabilitySvcUri, deviceQuery);
    }

    public IFirstDailyReportStartTimeUtcChangedEvent BuildFirstDailyReportStartTimeUtcChangedEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<IFirstDailyReportStartTimeUtcChangedEvent>(ActionConstants.GetFirstDailyReportStartTimeUtcChangedEvent, _deviceCapabilitySvcUri, deviceQuery);
    }

    public IHourMeterModifiedEvent BuildHourMeterModifiedEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<IHourMeterModifiedEvent>(ActionConstants.GetHourMeterModifiedEvent, _deviceCapabilitySvcUri, deviceQuery);
    }

    public IMovingCriteriaConfigurationChangedEvent BuildMovingCriteriaConfigurationChangedEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<IMovingCriteriaConfigurationChangedEvent>(ActionConstants.GetMovingCriteriaConfigurationChangedEvent, _deviceCapabilitySvcUri, deviceQuery);
    }

    public IOdometerModifiedEvent BuildOdometerModifiedEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<IOdometerModifiedEvent>(ActionConstants.GetOdometerModifiedEvent, _deviceCapabilitySvcUri, deviceQuery);
    }

    public IGetStartModeEvent BuildGetStartModeEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<IGetStartModeEvent>(ActionConstants.GetStartModeEvent, _deviceCapabilitySvcUri, deviceQuery);
    }

    public ISetStartModeEvent BuildSetStartModeEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<ISetStartModeEvent>(ActionConstants.SetStartModeEvent, _deviceCapabilitySvcUri, deviceQuery);
    }

    public IGetTamperLevelEvent BuildGetTamperLevelEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<IGetTamperLevelEvent>(ActionConstants.GetTamperLevelEvent, _deviceCapabilitySvcUri, deviceQuery);
    }

    public ISetTamperLevelEvent BuildSetTamperLevelEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<ISetTamperLevelEvent>(ActionConstants.SetTamperLevelEvent, _deviceCapabilitySvcUri, deviceQuery);
    }


    public ISetDailyReportFrequencyEvent BuildSetDailyReportFrequencyEvent(IDeviceQuery deviceQuery)
    {
     return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<ISetDailyReportFrequencyEvent>(ActionConstants.SetDailyReportFrequencyEvent, _deviceCapabilitySvcUri, deviceQuery);
    }


    public IDisableRapidReportingEvent BuildDisableRapidReportingEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<IDisableRapidReportingEvent>(ActionConstants.DisableRapidReportingEvent, _deviceCapabilitySvcUri, deviceQuery);
    }

    public IEnableRapidReportingEvent BuildEnableRapidReportingEventForDevice(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<IEnableRapidReportingEvent>(ActionConstants.EnableRapidReportingEvent, _deviceCapabilitySvcUri, deviceQuery);
    }

    public IReportingFrequencyChangedEvent BuildReportingFrequencyChangedEvent(IDeviceQuery deviceQuery)
    {
      return _eventTypeHelper.QueryServiceForTypeAndBuildInstance<IReportingFrequencyChangedEvent>(ActionConstants.ReportingFrequencyChangedEvent, _deviceCapabilitySvcUri, deviceQuery);
    }
  }
}
