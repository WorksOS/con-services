using System;

namespace VSS.Productivity3D._3DAssetMgmt.Abstractions
{
  public class BaseAsset
  {
    public long AssetId { get; set; }
    public Guid AssetUid { get; set; }
    public string AssetName { get; set; }
    public string SerialNumber { get; set; }
  }
}