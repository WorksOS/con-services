using System;

namespace VSS.TRex.CoordinateSystems.GridFabric.Arguments
{
  /// <summary>
  /// Contains a coordinate system expressed as a CSIB (Coordinate System Information Block) encoded as a string to s project
  /// </summary>
  public class AddCoordinateSystemArgument
  {
    /// <summary>
    /// The ID of the project to assign the coordinate system to
    /// </summary>
    public Guid ProjectID;

    /// <summary>
    /// The CSIB encoded as a string
    /// </summary>
    public string CSIB;
  }
}
