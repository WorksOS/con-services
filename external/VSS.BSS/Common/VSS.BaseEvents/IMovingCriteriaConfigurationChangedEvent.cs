using System;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.BaseEvents
{
  public interface IMovingCriteriaConfigurationChangedEvent : IEndpointDestinedEvent
  {
    string DeviceId { get; set; }
    DeviceTypeEnum DeviceType { get; set; }
    DateTime TimestampUtc { get; set; }

    double MovementRadiusInFeet { get; set; }
    double MovementDurationSeconds { get; set; }
    double MovementSpeedMPH  { get; set; }
  }
}
