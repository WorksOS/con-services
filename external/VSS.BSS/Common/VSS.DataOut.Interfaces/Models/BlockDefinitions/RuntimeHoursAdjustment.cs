using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions
{
  /// <summary>
  /// Runtime Hours Adjustment
  /// This enables the user to specify an adjustment to our service meter hours.
  /// </summary>
  [Serializable]
  public class RuntimeHoursAdjustment : Block
  {
    public double? Before { get; set; } // The absolute runtime hour meter value before the adjustment. Optional
    public double After { get; set; } //The absolute runtime hour meter value after the adjustment.
  }
}
