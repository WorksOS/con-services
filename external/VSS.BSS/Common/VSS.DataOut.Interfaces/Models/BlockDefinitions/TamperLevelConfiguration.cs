using System;
using VSS.Nighthawk.DataOut.Interfaces.Enums;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions
{
  /// <summary>
  /// Tamper Level Configuration
  /// This notifies the target about VisionLink’s tamper level configuration for each asset.
  /// This is used to configure the asset’s security system’s level of sensitivity.
  /// </summary>
  [Serializable]
  public class TamperLevelConfiguration : Block
  {
    public TamperResistanceStatus TamperLevel { get; set; }
  }
}
