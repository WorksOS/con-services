using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using System.Linq;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class DeviceAccountListResponseModel : IMasterDataModel
  {
    public DeviceAccountListResponseModel()
    {
      Accounts = new List<DeviceAccountResponseModel>();
    }

    /// <summary>
    /// Devices
    /// </summary>
    [JsonProperty("accounts")]
    public List<DeviceAccountResponseModel> Accounts { get; set; }

    /// <summary>
    /// Returned as true if the result has more records to display. Helps in pagination. False implies that there are no more records to display.
    /// </summary>
    [JsonProperty("hasMore")]
    public bool HasMore { get; set; }

    public List<string> GetIdentifiers() => Accounts?
                                              .SelectMany(d => d.GetIdentifiers())
                                              .Distinct()
                                              .ToList()
                                            ?? new List<string>();
  }

  /* Example response:
  {
    "accounts": [
        {
            "accountId": "trn::profilex:us-west-2:account:d8f56cce-fa04-4e7b-9560-3e28f283a554",
            "accountName": "TrimbleChennai",
            "relationStatus": "PENDING",
            "tccDeviceId": null,
            "tccDeviceStatus": null
        },
        {
            "accountId": "trn::profilex:us-west-2:account:af5a9262-1e29-42df-b199-e2b881a89c88",
            "accountName": "TRIMBLECECCHN",
            "relationStatus": "ACTIVE",
            "tccDeviceId": "u96012200-2b30-4637-a4a2-fad11eda6dbf",
            "tccDeviceStatus": "Registered"
        }
    ]
  }
*/
}
