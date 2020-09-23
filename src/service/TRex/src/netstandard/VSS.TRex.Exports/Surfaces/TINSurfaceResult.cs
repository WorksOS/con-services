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

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteByteArray(data);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        data = reader.ReadByteArray();
      }
    }
  }
}
