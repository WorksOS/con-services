using Apache.Ignite.Core.Binary;

namespace VSS.TRex.GridFabric
{
  public interface ISerialisedByteArrayWrapper
  {
    byte[] Bytes { get; set; }
    int Count { get; set; }

    void ToBinary(IBinaryRawWriter writer);
    void FromBinary(IBinaryRawReader reader);

    /// <summary>
    /// Implements the Ignite IBinarizable.WriteBinary interface Ignite will call to serialise this object.
    /// </summary>
    /// <param name="writer"></param>
    void WriteBinary(IBinaryWriter writer);

    /// <summary>
    /// Implements the Ignite IBinarizable.ReadBinary interface Ignite will call to serialise this object.
    /// </summary>
    void ReadBinary(IBinaryReader reader);
  }
}