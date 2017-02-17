using System;

namespace VSS.TagFileAuth.Service.Models
{
  public class Asset
  {
    public string AssetUid { get; set; }
    public long LegacyAssetID { get; set; }
    public string OwningCustomerUID { get; set; }
    public string Name { get; set; }
    public string MakeCode { get; set; }
    public string SerialNumber { get; set; }

    public string Model { get; set; }
    public int? IconKey { get; set; }
    public string AssetType { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? LastActionedUtc { get; set; }

    public override bool Equals(object obj)
    {
      var otherAsset = obj as Asset;
      if (otherAsset == null) return false;
      return otherAsset.AssetUid == AssetUid
        && otherAsset.Name == Name
        && otherAsset.MakeCode == MakeCode
        && otherAsset.SerialNumber == SerialNumber
        && otherAsset.Model == Model
        && otherAsset.IconKey == IconKey
        && otherAsset.AssetType == AssetType
        && otherAsset.IsDeleted == IsDeleted
        && otherAsset.LastActionedUtc == LastActionedUtc;
    }
    public override int GetHashCode() { return 0; }
  }
}