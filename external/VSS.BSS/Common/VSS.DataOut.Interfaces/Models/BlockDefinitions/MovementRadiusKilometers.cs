using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions
{
  /// <summary>
  /// Movement Radius
  /// This represents the device configuration for movement radius.
  /// </summary>
  [Serializable]
  public class MovementRadiusKilometers : Block
  {
    public double Value { get; set; } // The radius to consider the device as moving.
  }
}
