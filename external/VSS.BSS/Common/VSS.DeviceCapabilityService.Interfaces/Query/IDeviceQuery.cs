using VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query
{
  public interface IDeviceQuery
  {
    long? AssetID { get; set; }

    long? ID { get; set; }
    
    string GPSDeviceID { get; set; }
    
    DeviceTypeEnum? DeviceType { get; set; }
  }
}
