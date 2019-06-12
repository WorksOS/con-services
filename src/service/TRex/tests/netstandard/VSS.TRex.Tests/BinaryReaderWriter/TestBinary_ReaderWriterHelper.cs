using System.IO;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities.Interfaces;

namespace VSS.TRex.Tests.BinaryReaderWriter
{
  public static class TestBinary_ReaderWriterHelper
  {
    public static void RoundTripSerialise<T>(T instance) where T : IBinaryReaderWriter, new()
    {
      // Test using standard Read()/Write()
      var writer = new BinaryWriter(new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION));
      instance.Write(writer);

      (writer.BaseStream as MemoryStream).Position = 0;
      var instance2 = new T();
      instance2.Read(new BinaryReader(writer.BaseStream as MemoryStream));

      instance.Should().BeEquivalentTo(instance2);
    }
  }
}
