using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class AccountListResponseModel
  {
    public AccountListResponseModel()
    {
      Accounts = new List<AccountResponseModel>();
    }

    /// <summary>
    /// Accounts
    /// </summary>
    [JsonProperty("accounts")]
    public List<AccountResponseModel> Accounts { get; set; }

    /// <summary>
    /// Returned as true if the result has more records to display. Helps in pagination. False implies that there are no more records to display.
    /// </summary>
    [JsonProperty("hasMore")]
    public bool HasMore { get; set; }
  }

  /* Example response:
   {
    "hasMore": false,
    "accounts": [
        {
            "accountId": "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97",
            "accountName": "Berthoud",
            "profileImage": null,
            "deviceCount": 300,
            "userCount": 16,
            "projectCount": 18,
            "isValidOrgName": "true"
        }
    ]
  } 
   */
}
