using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  /// <summary>
  /// Argument containing the parameters required for a design boundary request
  /// </summary>    
  public class DesignBoundaryArgument : DesignSubGridRequestArgumentBase
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);
    }
  }
}
