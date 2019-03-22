using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.Exports.Surfaces
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public class TINSurfaceResult : SubGridsPipelinedResponseBase
  {
    private static byte VERSION_NUMBER = 1;

    public byte[] data;

    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteByteArray(data);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      data = reader.ReadByteArray();
    }
  }
}
