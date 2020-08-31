using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.GridFabric
{
  /// <summary>
  /// Represents a byte array that is has serialisation semantics controlled by TRex rather than Ignite.
  /// This will allow array pools and similar cached constructs to be used for these byte arrays when these
  /// facilities become available in the platform and usable by the Ignite C# client serialised.
  /// </summary>
  public class SerialisedByteArrayWrapper : VersionCheckedBinarizableSerializationBase, ISerialisedByteArrayWrapper
  {
    private const byte VERSION_NUMBER = 1;

    public byte[] Bytes { get; set; }
    public int Count { get; set; }

    public SerialisedByteArrayWrapper() { }

    public SerialisedByteArrayWrapper(byte[] bytes, int count)
    {
      Bytes = bytes;
      Count = count;
    }

    public SerialisedByteArrayWrapper(byte[] bytes) : this(bytes, bytes?.Length ?? 0)
    {
    }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteByteArray(Bytes);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        Bytes = reader.ReadByteArray();
        Count = Bytes?.Length ?? 0;
      }
    }
  }
}
