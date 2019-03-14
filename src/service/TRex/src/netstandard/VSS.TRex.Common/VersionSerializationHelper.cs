using System.IO;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.Common
{
  public static class VersionSerializationHelper
  {
    public static void EmitVersionByte(IBinaryRawWriter writer, byte version)
    {
      writer.WriteByte(version);
    }

    public static byte CheckVersionByte(IBinaryRawReader reader, byte expectedVersion)
    {
      byte encounteredVersion = reader.ReadByte();

      if (encounteredVersion != expectedVersion)
        throw new TRexSerializationVersionException(expectedVersion, encounteredVersion);

      return encounteredVersion;
    }

    public static byte CheckVersionsByte(IBinaryRawReader reader, byte[] expectedVersions)
    {
      byte encounteredVersion = reader.ReadByte();

      if (!expectedVersions.Contains(encounteredVersion))
        throw new TRexSerializationVersionException(expectedVersions, encounteredVersion);

      return encounteredVersion;
    }

    public static void EmitVersionByte(BinaryWriter writer, byte version)
    {
      writer.Write(version);
    }

    public static byte CheckVersionByte(BinaryReader reader, byte expectedVersion)
    {
      byte encounteredVersion = reader.ReadByte();

      if (encounteredVersion != expectedVersion)
        throw new TRexSerializationVersionException(expectedVersion, encounteredVersion);

      return encounteredVersion;
    }

    public static byte CheckVersionsByte(BinaryReader reader, byte[] expectedVersions)
    {
      byte encounteredVersion = reader.ReadByte();

      if (!expectedVersions.Contains(encounteredVersion))
        throw new TRexSerializationVersionException(expectedVersions, encounteredVersion);

      return encounteredVersion;
    }
  }
}
