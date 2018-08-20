using System;

namespace VSS.TRex.CoordinateSystems.GridFabric.Arguments
{
  /// <summary>
  /// Contains a coordiante system expressed as a CSIB (Coordinate System Informatio Block) encoded as a string to s project
  /// </summary>
  [Serializable]
    public class AddCoordinateSystemArgument
  {
    /// <summary>
    /// The ID of the project to assign the coordiante system to
    /// </summary>
    public Guid ProjectID;

    /// <summary>
    /// The CSIB encoded as a string
    /// </summary>
    public string CSIB;
  }
}
