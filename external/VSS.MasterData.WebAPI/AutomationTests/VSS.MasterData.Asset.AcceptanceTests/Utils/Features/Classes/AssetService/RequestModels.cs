using Newtonsoft.Json;
using System;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetService
{
  #region Valid AssetServiceCreateRequest

  public class CreateAssetModel
  {
    public CreateAssetEvent CreateAssetEvent;
  }

  public class CreateAssetEvent
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string AssetName { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public long? LegacyAssetID { get; set; }
    public string SerialNumber { get; set; }
    public string MakeCode { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Model { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string AssetType { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? IconKey { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string EquipmentVIN { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? ModelYear { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Guid OwningCustomerUID { get; set; }
    public Guid AssetUID { get; set; }
    public DateTime ActionUTC { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? ReceivedUTC { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ObjectType { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Category { get; set; }

    //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    //public string Project { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ProjectStatus { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string SortField { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Source { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string UserEnteredRuntimeHours { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Classification { get; set; }

  }

  #endregion

  #region Valid AssetServiceUpdateRequest

  public class UpdateAssetModel
  {
    public UpdateAssetEvent UpdateAssetEvent;
  }

  public class UpdateAssetEvent
  {
    public Guid AssetUID { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string AssetName { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public long? LegacyAssetID { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Model { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string AssetType { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? IconKey { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string EquipmentVIN { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Guid OwningCustomerUID { get; set; }
    public int? ModelYear { get; set; }
    public DateTime ActionUTC { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? ReceivedUTC { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ObjectType { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Category { get; set; }

    //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    //public string Project { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ProjectStatus { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string SortField { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Source { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string UserEnteredRuntimeHours { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Classification { get; set; }
  }

  #endregion

  #region Valid AssetServiceDeleteRequest

  public class DeleteAssetModel
  {
    public DeleteAssetEvent DeleteAssetEvent;
  }

  public class DeleteAssetEvent
  {
    public Guid AssetUID { get; set; }
    public DateTime ActionUTC { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? ReceivedUTC { get; set; }
  }

  #endregion

  #region InValid AssetServiceCreateRequest

  public class InValidCreateAssetEvent
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string AssetName { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string LegacyAssetID { get; set; }
    public string SerialNumber { get; set; }
    public string MakeCode { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Model { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string AssetType { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string IconKey { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string EquipmentVIN { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ModelYear { get; set; }
    public string AssetUID { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string OwningCustomerUID { get; set; }
    public string ActionUTC { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ReceivedUTC { get; set; }
  }
  #endregion

  #region InValid AssetServiceUpdateRequest

  public class InValidUpdateAssetEvent
  {
    public string AssetUID { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string AssetName { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string LegacyAssetID { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Model { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string AssetType { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string IconKey { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string EquipmentVIN { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ModelYear { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string OwningCustomerUID { get; set; }
    public string ActionUTC { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ReceivedUTC { get; set; }
  }
  #endregion

  #region InValid AssetServiceDeleteRequest

  public class InValidDeleteAssetEvent
  {
    public string AssetUID { get; set; }
    public string ActionUTC { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ReceivedUTC { get; set; }
  }

  #endregion
}
