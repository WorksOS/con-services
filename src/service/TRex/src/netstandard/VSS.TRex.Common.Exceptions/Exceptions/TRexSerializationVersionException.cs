using System;
using System.Linq;

namespace VSS.TRex.Common.Exceptions
{
  public class TRexSerializationVersionException : TRexException
  {
    public static string ErrorMessage(uint [] expectedVersions, uint encounteredVersion)
    {
      return $"Invalid version read during deserialization: {encounteredVersion}, expected version in [{string.Join(", ", expectedVersions)}]";
    }

    public TRexSerializationVersionException(string message): base(message)
    {
    }

    public TRexSerializationVersionException(string message, Exception e) : base(message, e)
    {
    }

    public TRexSerializationVersionException(uint expectedVersion, uint encounteredVersion) : base(TRexSerializationVersionException.ErrorMessage(new []{expectedVersion}, encounteredVersion))
    {
    }

    public TRexSerializationVersionException(uint [] expectedVersions, uint encounteredVersion) : base(TRexSerializationVersionException.ErrorMessage(expectedVersions, encounteredVersion))
    {
    }

    public TRexSerializationVersionException(byte[] expectedVersions, uint encounteredVersion) : base(TRexSerializationVersionException.ErrorMessage(expectedVersions.Select(x => (uint)x).ToArray(), encounteredVersion))
    {
    }
  }
}
