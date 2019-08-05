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
  public class SerialisedByteArrayWrapper : IBinarizable, IFromToBinary, ISerialisedByteArrayWrapper
  {
    private const byte VERSION_NUMBER = 1;

    public byte[] Bytes { get; set; }
    public int Count { get; set; }

    public SerialisedByteArrayWrapper() { }

    public SerialisedByteArrayWrapper(byte[] bytes, int count)
    {
      Bytes = bytes ?? new byte[0];
      Count = count;
    }

    public SerialisedByteArrayWrapper(byte[] bytes) : this(bytes, bytes?.Length ?? 0)
    {
    }

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteByteArray(Bytes);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      Bytes = reader.ReadByteArray();
      Count = Bytes?.Length ?? 0;
    }

    /// <summary>
    /// Implements the Ignite IBinarizable.WriteBinary interface Ignite will call to serialise this object.
    /// </summary>
    /// <param name="writer"></param>
    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    /// <summary>
    /// Implements the Ignite IBinarizable.ReadBinary interface Ignite will call to serialise this object.
    /// </summary>
    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());
  }
}
