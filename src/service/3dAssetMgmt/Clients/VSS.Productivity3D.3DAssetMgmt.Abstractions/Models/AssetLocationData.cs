using System;

namespace VSS.Productivity3D.AssetMgmt3D.Abstractions.Models
{
  public class AssetLocationData
  {
    public Guid AssetUid { get; set; }
    public DateTime? LocationLastUpdatedUtc { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
    public string AssetIdentifier { get; set; }
    public string MachineName { get; set; }
    public string AssetSerialNumber { get; set; }
    public string AssetType { get; set; }
  }
}
