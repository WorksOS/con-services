using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class DeviceListResponseModel
  {
    public DeviceListResponseModel()
    {
      Devices = new List<DeviceResponseModel>();
    }

    /// <summary>
    /// Devices
    /// </summary>
    [JsonProperty("devices")]
    public List<DeviceResponseModel> Devices { get; set; }

    /// <summary>
    /// Returned as true if the result has more records to display. Helps in pagination. False implies that there are no more records to display.
    /// </summary>
    [JsonProperty("hasMore")]
    public bool HasMore { get; set; }
  }

  /* Example response:
   * todoMaverick format not available yet
   {
    "hasMore": true,
    "devices": [
        {
          "deviceId": "trn::profilex:us-west-2:device:08d60-4a00010001f6",
          "deviceName": "WowzerMyDozer",
          "deviceNickname": "WowzerMyDozer",
          "deviceType": "CB460",
          "serialNumber": "2203J009SW",
          "accountName": "My New Mansion",
          "description": "Lavendar is a color",
          "tccDeviceId": "ub60c4d6e-18224-836b-9b5e325a2a57",
          "tccDeviceStatus": "Registered",
          "accountRegistrationStatus": "PENDING"  // if includeTccRegistrationStatus
        }
    ]
  } 
   */
}
