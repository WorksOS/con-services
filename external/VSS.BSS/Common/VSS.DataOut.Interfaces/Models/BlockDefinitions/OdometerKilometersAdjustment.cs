using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions
{
  /// <summary>
  /// Odometer Adjustment
  /// This enables the user to specify an adjustment to the odometer.
  /// This value will always be sent in the SI unit of kilometers,
  /// the unit will not change based on user preferences.
  /// </summary>
  [Serializable]
  public class OdometerKilometersAdjustment : Block
  {
    public double? Before { get; set; } // The absolute odometer value before the adjustment. Optional
    public double After { get; set; } // The absolute odometer value after the adjustment.
  }
}
