using System;
using VSS.Nighthawk.DataOut.Interfaces.Enums;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions
{
  /// <summary>
  /// Start Mode Configuration
  /// This notifies the target about VisionLink’s start mode configuration for each asset.
  /// This is used to configure the asset’s security system to a number of different settings.
  /// These values are selected via business rules, and primarily consider whether the equipment
  /// is being disabled or derated for repossession purposes.
  /// </summary>
  [Serializable]
  public class StartModeConfiguration : Block
  {
    public MachineStartStatus StartMode { get; set; }
  }
}
