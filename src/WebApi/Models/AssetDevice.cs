namespace VSS.TagFileAuth.Service.WebApi.Models
{
  public class AssetDevice
  {
    public string AssetUid { get; set; }
    public long LegacyAssetId { get; set; }
    public string OwnerCustomerUid { get; set; }
    public string DeviceUid { get; set; }
    public string RadioSerial { get; set; }
    public string DeviceType { get; set; }

    public override bool Equals(object obj)
    {
      var otherAsset = obj as AssetDevice;
      if (otherAsset == null) return false;
      return otherAsset.AssetUid == AssetUid
        && otherAsset.LegacyAssetId == LegacyAssetId
        && otherAsset.OwnerCustomerUid == OwnerCustomerUid
        && otherAsset.DeviceUid == DeviceUid
        && otherAsset.RadioSerial == RadioSerial
        && otherAsset.DeviceType == DeviceType;
    }
    public override int GetHashCode() { return 0; }
  }
}