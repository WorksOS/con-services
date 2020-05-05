using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using System.Linq;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class DeviceListResponseModel : IMasterDataModel
  {
    public DeviceListResponseModel()
    {
      Devices = new List<DeviceFromListResponseModel>();
    }

    /// <summary>
    /// Devices
    /// </summary>
    [JsonProperty("devices")]
    public List<DeviceFromListResponseModel> Devices { get; set; }

    /// <summary>
    /// Returned as true if the result has more records to display. Helps in pagination. False implies that there are no more records to display.
    /// </summary>
    [JsonProperty("hasMore")]
    public bool HasMore { get; set; }

    public List<string> GetIdentifiers() => Devices?
                                              .SelectMany(d => d.GetIdentifiers())
                                              .Distinct()
                                              .ToList()
                                            ?? new List<string>();
  }

  /* Example response:   
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
