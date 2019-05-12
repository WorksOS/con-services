using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.Repositories.ExtendedModels
{
  public class MatchingAssets
  {
    public string AssetUID2D { get; set; }
    public string AssetUID3D { get; set; }
    public string Name { get; set; }
    public string SerialNumber2D { get; set; }
    public string SerialNumber3D { get; set; }
    public string MakeCode2D { get; set; }
    public string MakeCode3D { get; set; }
    public string Model { get; set; }
    public string CustomerName { get; set; }

    public override bool Equals(object obj)
    {
      if (!(obj is MatchingAssets otherAsset)) return false;
      return otherAsset.AssetUID2D == AssetUID2D
             && otherAsset.AssetUID3D == AssetUID3D
             && otherAsset.Name == Name
             && otherAsset.SerialNumber2D == SerialNumber2D
             && otherAsset.SerialNumber3D == SerialNumber3D
             && otherAsset.MakeCode2D == MakeCode2D
             && otherAsset.MakeCode3D == MakeCode3D
             && otherAsset.Model == Model
             && otherAsset.CustomerName == CustomerName;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}

