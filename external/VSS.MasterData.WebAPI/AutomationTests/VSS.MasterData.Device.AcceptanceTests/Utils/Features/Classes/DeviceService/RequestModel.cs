using Newtonsoft.Json;
using System;

namespace VSS.MasterData.Device.AcceptanceTests.Utils.Features.Classes.DeviceService
{
  public class CreateDeviceModel
  {
    public CreateDeviceEvent CreateDeviceEvent;
  }

  public class CreateDeviceEvent
  {
    public Guid DeviceUID { get; set; }
    public string DeviceSerialNumber { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DeviceType { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DeviceState { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime DeregisteredUTC { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ModuleType { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string MainboardSoftwareVersion { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string RadioFirmwarePartNumber { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string GatewayFirmwarePartNumber { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DataLinkType { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string FirmwarePartNumber { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string CellModemIMEI { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DevicePartNumber { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string CellularFirmwarePartnumber { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string NetworkFirmwarePartnumber { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string SatelliteFirmwarePartnumber { get; set; }
    public DateTime ActionUTC { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? ReceivedUTC { get; set; }

  }

  public class UpdateDeviceModel
  {
    public UpdateDeviceEvent UpdateDeviceEvent;
  }

  public class UpdateDeviceEvent
  {
    public Guid DeviceUID { get; set; }
    public string DeviceSerialNumber { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Guid OwningCustomerUID { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DeviceType { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DeviceState { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime DeregisteredUTC { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ModuleType { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string MainboardSoftwareVersion { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string RadioFirmwarePartNumber { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string GatewayFirmwarePartNumber { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DataLinkType { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string FirmwarePartNumber { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string CellModemIMEI { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DevicePartNumber { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string CellularFirmwarePartnumber { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string NetworkFirmwarePartnumber { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string SatelliteFirmwarePartnumber { get; set; }

    public DateTime ActionUTC { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? ReceivedUTC { get; set; }
  }
}
