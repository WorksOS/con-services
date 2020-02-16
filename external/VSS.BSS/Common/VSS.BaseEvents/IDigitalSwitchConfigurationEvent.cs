using System;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.BaseEvents
{
  public interface IDigitalSwitchConfigurationEvent : IEndpointDestinedEvent
  {
    string DeviceId { get; set; }
    DeviceTypeEnum DeviceType { get; set; }
    DateTime TimestampUtc { get; set; }

    int SwitchNumber { get; set; }
    string SwitchOnDescription { get; set; }
    double Sensitivity { get; set; }
    SwitchState DefaultState { get; set; }
    DigitalInputMonitoringConditions MonitoredWhen { get; set; }
  }
}
