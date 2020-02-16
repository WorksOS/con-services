using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions
{
  /// <summary>
  /// Movement Speed
  /// This represents the device configuration for movement speed.
  /// This value will always be sent in the SI unit of kilometers,
  /// the unit will not change based on user preferences
  /// </summary>
  [Serializable]
  public class MovementSpeedKilometers : Block
  {
    public double Value { get; set; } // The speed at which to consider the device as moving.
  }
}
