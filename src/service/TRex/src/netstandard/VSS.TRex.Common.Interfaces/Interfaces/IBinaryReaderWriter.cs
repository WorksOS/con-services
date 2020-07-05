using System.IO;

namespace VSS.TRex.Common.Interfaces
{
  /// <summary>
  /// Interface detailing 'from' and 'to' byte array serialisation semantics
  /// </summary>
  public interface IBinaryReaderWriter
  {
    void Read(BinaryReader reader);
    void Write(BinaryWriter writer);
  }
}
