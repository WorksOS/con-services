using System;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Common.Extensions;
using VSS.TRex.DI;
using VSS.TRex.IO;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.RecyclableMemoryStream
{
  public class RecyclableMemoryStreamTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Prerequisites()
    {
      RecyclableMemoryStreamManager.DefaultBlockSize.Should().Be(1024 * 128);
      DIContext.Obtain<RecyclableMemoryStreamManager>().BlockSize.Should()
        .Be(RecyclableMemoryStreamManager.DefaultBlockSize);
    }

    [Fact]
    public void Creation_Default()
    {
      var stream = DIContext.Obtain<RecyclableMemoryStreamManager>().GetStream();

      stream.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(100, 1)]
    [InlineData(255, 1)]
    [InlineData(0, 1024 * 128 + 1)] // Ensure default block boundary is crossed
    [InlineData(1, 1024 * 128 + 1)]
    [InlineData(100, 1024 * 128 + 1)]
    [InlineData(255, 1024 * 128 + 1)]
    public void ReadWriteOneByte(byte value, int count)
    {
      var stream = DIContext.Obtain<RecyclableMemoryStreamManager>().GetStream();

      for (int i = 0; i < count; i++)
        stream.WriteByte(value);

      stream.Position.Should().Be(count);
      stream.Length.Should().Be(count);

      stream.Position = 0;

      for (int i = 0; i < count; i++)
        stream.ReadByte().Should().Be(value);

      stream.Position.Should().Be(count);
      stream.Length.Should().Be(count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(255)]
    [InlineData(1024)]
    [InlineData(32768)]
    [InlineData(65536)]
    [InlineData(1024 * 128 - 1)]
    [InlineData(1024 * 128)] // Default block size
    [InlineData(1024 * 128 + 1)] 
    [InlineData(1000000)]
    public void ReadWriteByteBuffer(int size)
    {
      var stream = DIContext.Obtain<RecyclableMemoryStreamManager>().GetStream();

      var buffer = new byte [size];
      for (int i = 0; i < size; i++)
        buffer[i] = (byte)(i % 256);

      stream.Write(buffer, 0, size);

      stream.Position.Should().Be(size);
      stream.Length.Should().Be(size);

      var buffer2 = new byte[size];
      stream.Position = 0;
      stream.Read(buffer2, 0, size).Should().Be(size);
      buffer2.Should().BeEquivalentTo(buffer);

      stream.Position.Should().Be(size);
      stream.Length.Should().Be(size);
    }

    [Fact]
    public void DoNotReadPastEndOfStream()
    {
      var stream = DIContext.Obtain<RecyclableMemoryStreamManager>().GetStream();

      stream.WriteByte(255);

      stream.Length.Should().Be(1);
      stream.Position.Should().Be(1);
      stream.Position = 0;

      var buffer = new Byte[10];
      var numBytes = stream.Read(buffer, 0, buffer.Length);

      numBytes.Should().Be(1);
      stream.Position.Should().Be(1);
      stream.Length.Should().Be(1);

      buffer[0].Should().Be(255);
      buffer.Skip(1).Take(buffer.Length - 1).ForEach(x => x.Should().Be(0));
    }
  }
}
