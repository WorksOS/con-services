using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class DeviceAccountResponseModel : IMasterDataModel
  {
    private string _trn;

    //Note: There are other properties returned (tccDeviceId) but we only want some of it
    /// <summary>
    /// Account TRN ID
    /// </summary>
    [JsonProperty("accountId")]
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
    /// WorksOS account ID; the Guid extracted from the TRN.
    /// </summary>
    public string Id { get; private set; }

    [JsonProperty("accountName")]
    public string AccountName { get; set; }

    [JsonProperty("relationStatus")]
    public RelationStatusEnum RelationStatus { get; set; }

    [JsonProperty("tccDeviceStatus")]
    public TCCDeviceStatusEnum TccDeviceStatus { get; set; }
    

    public List<string> GetIdentifiers() => new List<string> { TRN, Id };
  }
}
