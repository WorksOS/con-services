using System.IO;
using System.Linq;
using Apache.Ignite.Core.Binary;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.Common
{
  public static class VersionSerializationHelper
  {
    //private static readonly ILogger _log = Logging.Logger.CreateLogger("VersionSerializationHelper");

    public static void EmitVersionByte(IBinaryRawWriter writer, byte version)
    {
      writer.WriteByte(version);
    }

    public static byte CheckVersionByte(IBinaryRawReader reader, byte expectedVersion)
    {
      var encounteredVersion = reader.ReadByte();

      if (encounteredVersion != expectedVersion)
      {
         throw new TRexSerializationVersionException(expectedVersion, encounteredVersion);
        //_log.LogError(TRexSerializationVersionException.ErrorMessage(new[] { (uint)expectedVersion }, encounteredVersion));
      }

      return encounteredVersion;
    }

    public static byte CheckVersionsByte(IBinaryRawReader reader, byte[] expectedVersions)
    {
      var encounteredVersion = reader.ReadByte();

      if (!expectedVersions.Contains(encounteredVersion))
      {
         throw new TRexSerializationVersionException(expectedVersions, encounteredVersion);
        //_log.LogError(TRexSerializationVersionException.ErrorMessage(expectedVersions.Select(x => (uint)x).ToArray(), encounteredVersion));
      }

      return encounteredVersion;
    }

    public static void EmitVersionByte(BinaryWriter writer, byte version)
    {
      writer.Write(version);
    }

    public static byte CheckVersionByte(BinaryReader reader, byte expectedVersion)
    {
      var encounteredVersion = reader.ReadByte();

      if (encounteredVersion != expectedVersion)
      {
        throw new TRexSerializationVersionException(expectedVersion, encounteredVersion);
        //_log.LogError(TRexSerializationVersionException.ErrorMessage(new[] { (uint)expectedVersion }, encounteredVersion));
      }

      return encounteredVersion;
    }

    public static byte CheckVersionsByte(BinaryReader reader, byte[] expectedVersions)
    {
      var encounteredVersion = reader.ReadByte();

      if (!expectedVersions.Contains(encounteredVersion))
      {
        throw new TRexSerializationVersionException(expectedVersions, encounteredVersion);
        //_log.LogError(TRexSerializationVersionException.ErrorMessage(expectedVersions.Select(x => (uint)x).ToArray(), encounteredVersion));
      }

      return encounteredVersion;
    }
  }
}
