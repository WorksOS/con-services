using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Exports.Patches.GridFabric.PatchRequest;

namespace VSS.TRex.Exports.Patches.GridFabric.PatchRequestWithColors
{
  /// <summary>
  /// The response returned from the Patches with colors request executor that contains the response code and the set of
  /// sub grids extracted for the patch in question
  /// </summary>
  public class PatchRequestWithColorsResponse : PatchRequestResponse
  {
    private static byte VERSION_NUMBER = 1;

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
