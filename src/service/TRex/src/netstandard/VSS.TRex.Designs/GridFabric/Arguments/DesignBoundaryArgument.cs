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
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);
    }
  }
}
