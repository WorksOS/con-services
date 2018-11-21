using VSS.TRex.Exceptions;

namespace VSS.TRex.Common.Exceptions
{
  public class TRexSerializationVersionException : TRexException
  {
    public TRexSerializationVersionException(int expectedVersion, int encounteredVersion) : base($"Invalid version read during deserialization: {encounteredVersion}, expected version {expectedVersion}")
    {
    }
  }
}
