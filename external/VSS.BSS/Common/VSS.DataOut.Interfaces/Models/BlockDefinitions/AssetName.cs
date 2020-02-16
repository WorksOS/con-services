using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions
{
  /// <summary>
  /// Asset ID (aka Asset Name, Asset TAG) Change
  /// This data will be attributed to an asset,
  /// so the destination block in the header will represent
  /// the asset this change relates to
  /// </summary>
  [Serializable]
  public class AssetName : Block
  {
    /// <remarks>A string with max length of 200 alphanumeric chars</remarks>
    public string Value { get; set; } // The asset name after the change.
  }
}
