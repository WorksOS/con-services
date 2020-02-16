using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions
{
  /// <summary>
  /// Movement Duration
  /// This represents the device configuration for movement duration.
  /// </summary>
  [Serializable]
  public class MovementDurationSeconds : Block
  {
    public double Value { get; set; } // The duration of movement required to consider the device as moving.
  }
}
