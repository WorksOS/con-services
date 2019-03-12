using System.IO;
using FluentAssertions;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public static class TestBinarizable_ReaderWriterHelper
  {
    public static void RoundTripSerialise<T>(T instance) where T : IFromToBinary, new()
    {
      var writer = new TestBinaryWriter();
      instance.ToBinary(writer);

      var instance2 = new T();
      instance2.FromBinary(new TestBinaryReader(writer._stream.BaseStream as MemoryStream));

      instance.Should().BeEquivalentTo(instance2);
    }
  }
}
