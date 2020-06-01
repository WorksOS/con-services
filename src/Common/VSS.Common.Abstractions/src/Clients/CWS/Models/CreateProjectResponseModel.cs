using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class CreateProjectResponseModel : IMasterDataModel
  {
    private string _trn;

    /// <summary>
    /// Project TRN ID
    /// </summary>
    [JsonProperty("projectId")]
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
    /// WorksOS project ID; the Guid extracted from the TRN.
    /// </summary>
    public string Id { get; private set; }

    public List<string> GetIdentifiers() => new List<string> { TRN, Id };
  }

  /* example
    {
      "projectId": "trn::profilex:us-west-2:project:815b84bf-13c7-43dd-b80e-3f36at"
    }
   */
}
