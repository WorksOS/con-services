using System.IO;
using FluentAssertions;
using VSS.TRex.Common.Utilities.Interfaces;

namespace VSS.TRex.Tests.BinaryReaderWriter
{
  public static class TestBinary_ReaderWriterBufferedHelper
  {
    public static void RoundTripSerialise<T>(T instance) where T : IBinaryReaderWriterBuffered, new()
    {
      byte[]  buffer = new byte[10000];

      // Test using standard Read()/Write()
      var writer = new BinaryWriter(new MemoryStream());
      instance.Write(writer, buffer);

      (writer.BaseStream as MemoryStream).Position = 0;
      var instance2 = new T();
      instance2.Read(new BinaryReader(writer.BaseStream as MemoryStream), buffer);

      instance.Should().BeEquivalentTo(instance2);
    }
  }
}
