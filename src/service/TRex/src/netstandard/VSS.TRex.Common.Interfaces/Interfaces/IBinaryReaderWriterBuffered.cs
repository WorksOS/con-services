using System.IO;

namespace VSS.TRex.Common.Interfaces
{
  public interface IBinaryReaderWriterBuffered
  {
    void Read(BinaryReader reader, byte[] buffer);
    void Write(BinaryWriter writer, byte[] buffer);
  }
}
