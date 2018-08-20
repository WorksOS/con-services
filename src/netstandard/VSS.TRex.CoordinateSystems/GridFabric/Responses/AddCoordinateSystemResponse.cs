using System;

namespace VSS.TRex.CoordinateSystems.GridFabric.Responses
{
  /// <summary>
  /// The response state return from th add coordinate system operation
  /// </summary>
  [Serializable]
  public class AddCoordinateSystemResponse
  {
    /// <summary>
    /// Indicates overall success of the operation
    /// </summary>
    public bool Succeeded { get; set; }
  }
}
