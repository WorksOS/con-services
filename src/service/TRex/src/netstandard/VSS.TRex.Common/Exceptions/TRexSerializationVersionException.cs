namespace VSS.TRex.Common.Exceptions
{
  public class TRexSerializationVersionException : TRexException
  {
    public static string ErrorMessage(int [] expectedVersions, int encounteredVersion)
    {
      return $"Invalid version read during deserialization: {encounteredVersion}, expected version {expectedVersions}";
    }

    public TRexSerializationVersionException(int expectedVersion, int encounteredVersion) : base(TRexSerializationVersionException.ErrorMessage(new []{expectedVersion}, encounteredVersion))
    {
    }

    public TRexSerializationVersionException(int [] expectedVersions, int encounteredVersion) : base(TRexSerializationVersionException.ErrorMessage(expectedVersions, encounteredVersion))
    {
    }
  }
}
