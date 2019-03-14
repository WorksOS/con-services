using System.IO;
using FluentAssertions;
using VSS.TRex.Common.Utilities.Interfaces;

namespace VSS.TRex.Tests.BinaryReaderWriter
{
  public static class TestBinary_ReaderWriterHelper
  {
    public static void RoundTripSerialise<T>(T instance) where T : IBinaryReaderWriter, new()
    {
      // Test using standard Read()/Write()
      var writer = new BinaryWriter(new MemoryStream());
      instance.Write(writer);

      (writer.BaseStream as MemoryStream).Position = 0;
      var instance2 = new T();
      instance2.Read(new BinaryReader(writer.BaseStream as MemoryStream));

      instance.Should().BeEquivalentTo(instance2);

      // Repeat using the buffered Write implementation
      var writerBuffered = new BinaryWriter(new MemoryStream());
      instance.Write(writerBuffered, new byte[10000]);

      (writerBuffered.BaseStream as MemoryStream).Position = 0;
      var instance3 = new T();
      instance3.Read(new BinaryReader(writerBuffered.BaseStream as MemoryStream));

      instance.Should().BeEquivalentTo(instance3);
    }
  }
}
