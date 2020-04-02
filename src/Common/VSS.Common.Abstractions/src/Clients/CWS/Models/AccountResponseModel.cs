using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class AccountResponseModel : IMasterDataModel
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

    public List<string> GetIdentifiers() => new List<string>
    {
      Id
    };
  }
}
