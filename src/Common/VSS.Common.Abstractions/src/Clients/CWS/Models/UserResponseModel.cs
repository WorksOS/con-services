using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class UserResponseModel : IMasterDataModel
  {
    private string _trn;

    /// <summary>
    /// User TRN ID
    /// </summary>
    [JsonProperty("userId")]
    public string TRN
    {
      get => _trn;
      set
      {
        _trn = value;
        Id = TRNHelper.ExtractGuidAsString(value);
      }
    }

    /// <summary>
    /// WorksOS user ID; the Guid extracted from the TRN.
    /// </summary>
    public string Id { get; private set; }

    [JsonProperty("firstName")]
    public string FirstName { get; set; }

    [JsonProperty("lastName")]
    public string LastName { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    /// <summary>
    /// Language for user preferences
    /// </summary>
    [JsonProperty("language")]
    public string Language { get; set; }

    public List<string> GetIdentifiers() => new List<string> { TRN, Id };
  }
}
