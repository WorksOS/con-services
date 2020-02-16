using VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Query
{
  public class DeviceQuery : IDeviceQuery
  {
    public long? AssetID { get; set; }
    public long? ID { get; set; }
    public string GPSDeviceID { get; set; }
    public DeviceTypeEnum? DeviceType { get; set; }
  }
}
