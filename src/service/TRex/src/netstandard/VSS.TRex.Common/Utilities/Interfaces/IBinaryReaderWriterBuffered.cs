using System.IO;

namespace VSS.TRex.Common.Utilities.Interfaces
{
  public interface IBinaryReaderWriterBuffered
  {
    void Read(BinaryReader reader, byte[] buffer);
    void Write(BinaryWriter writer, byte[] buffer);
  }
}
