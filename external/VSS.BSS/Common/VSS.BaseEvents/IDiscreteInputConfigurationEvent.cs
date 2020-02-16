using System;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.BaseEvents
{
  public interface IDiscreteInputConfigurationEvent : IEndpointDestinedEvent
  {
    string DeviceId { get; set; }
    DeviceTypeEnum DeviceType { get; set; }
    DateTime TimestampUtc { get; set; }

    int SwitchNumber { get; set; }
    string Name { get; set; }
    string OpenDescription { get; set; }
    string ClosedDescription { get; set; }
    double Sensitivity { get; set; }
    DigitalInputMonitoringConditions? MonitoredWhen { get; set; }
    bool Enabled { get; set; }
    bool WakeUpWhenClosed { get; set; }
  }
}
