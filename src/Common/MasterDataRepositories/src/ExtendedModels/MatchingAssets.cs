using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.Repositories.ExtendedModels
{
  public class MatchingAssets
  {
    public string AssetUID { get; set; }
    public string MatchingAssetUID { get; set; }
    public string Name { get; set; }
    public string SerialNumber { get; set; }
    public string MatchingSerialNumber { get; set; }
    public string MakeCode { get; set; }
    public string MatchingMakeCode { get; set; }
    public string Model { get; set; }
    public string CustomerName { get; set; }

    public override bool Equals(object obj)
    {
      var otherAsset = obj as MatchingAssets;
      if (otherAsset == null) return false;
      return otherAsset.AssetUID == AssetUID
             && otherAsset.MatchingAssetUID == MatchingAssetUID
             && otherAsset.Name == Name
             && otherAsset.SerialNumber == SerialNumber
             && otherAsset.MatchingSerialNumber == MatchingSerialNumber
             && otherAsset.MakeCode == MakeCode
             && otherAsset.MatchingMakeCode == MatchingMakeCode
             && otherAsset.Model == Model
             && otherAsset.CustomerName == CustomerName;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}
