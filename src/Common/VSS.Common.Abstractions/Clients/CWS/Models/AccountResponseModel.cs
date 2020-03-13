using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class AccountResponseModel
  {
    //Note: There are other properties returned but we only want the account id and name
    /// <summary>
    /// Account TRN ID
    /// </summary>
    [JsonProperty("accountId")]
    public string Id { get; set; }

    /// <summary>
    /// Account Name
    /// </summary>
    [JsonProperty("accountName")]
    public string Name { get; set; }
  }
}
