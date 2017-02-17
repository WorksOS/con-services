namespace VSS.TagFileAuth.Service.WebApi.Models
{
  public class AssetDevice
  {
    public string AssetUid { get; set; }
    public long LegacyAssetId { get; set; }
    public string OwningCustomerUid { get; set; }
    public string DeviceUid { get; set; }
    public string DeviceType { get; set; }
    public string RadioSerial { get; set; }



    public override bool Equals(object obj)
    {
      var otherAsset = obj as AssetDevice;
      if (otherAsset == null) return false;
      return otherAsset.AssetUid == AssetUid
        && otherAsset.LegacyAssetId == LegacyAssetId
        && otherAsset.OwningCustomerUid == OwningCustomerUid
        && otherAsset.DeviceUid == DeviceUid
        && otherAsset.DeviceType == DeviceType
        && otherAsset.RadioSerial == RadioSerial;
    }
    public override int GetHashCode() { return 0; }
  }
}