using System;
using VSS.Nighthawk.MasterDataSync.Interfaces;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class CreateDeviceEvent : IDeviceEvent
  {
    public Guid DeviceUID { get; set; }
    public string DeviceSerialNumber { get; set; }
    public string DeviceType { get; set; }
    public string DeviceState { get; set; }
    public DateTime? DeregisteredUTC { get; set; }
    public string ModuleType { get; set; }
    public string MainboardSoftwareVersion { get; set; }
    public string RadioFirmwarePartNumber { get; set; }
    public string GatewayFirmwarePartNumber { get; set; }
    public string DataLinkType { get; set; }
    public DateTime ActionUTC { get; set; }
  }
}
