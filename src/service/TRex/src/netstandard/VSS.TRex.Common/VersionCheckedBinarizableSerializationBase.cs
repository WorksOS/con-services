using System;
using Apache.Ignite.Core.Binary;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.Common
{
  public abstract class VersionCheckedBinarizableSerializationBase : IBinarizable, IFromToBinary, IVersionCheckedBinarizableSerializationBase
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<VersionCheckedBinarizableSerializationBase>();

    //public abstract byte[] GetVersionNumbers();

    public abstract void InternalFromBinary(IBinaryRawReader reader);

    public abstract void InternalToBinary(IBinaryRawWriter writer);

    public void ToBinary(IBinaryRawWriter writer)
    {
      try
      {
        InternalToBinary(writer);
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception in serialization");
      }
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      try
      {
        InternalFromBinary(reader);
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception in serialization");
      }
    }

    /// <summary>
    /// Implements the Ignite IBinarizable.WriteBinary interface Ignite will call to serialize this object.
    /// </summary>
    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    /// <summary>
    /// Implements the Ignite IBinarizable.ReadBinary interface Ignite will call to serialize this object.
    /// </summary>
    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());
  }
}
