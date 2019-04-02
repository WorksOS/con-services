using System;

namespace VSS.Productivity3D.AssetMgmt3D.Abstractions
{
  public class BaseAsset
  {
    public long AssetId { get; set; }
    public Guid AssetUid { get; set; }
    public string AssetName { get; set; }
    public string SerialNumber { get; set; }
  }
}