using System;
using FluentAssertions;
using VSS.TRex.IO;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.IO
{
  public class GenericArrayPoolCacheTests: IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Creation()
    {
      var cache = new GenericArrayPoolCaches<byte>();

      cache.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    [InlineData(128)]
    [InlineData(256)]
    [InlineData(512)]
    [InlineData(1024)]
    [InlineData(2048)]
    [InlineData(4096)]
    [InlineData(8192)]
    [InlineData(16384)]
    [InlineData(32768)]
    [InlineData(65536)]
    [InlineData(65536 << 1)]
    [InlineData(65536 << 2)]
    [InlineData(65536 << 3)]
    [InlineData(65536 << 4)]
    public void Rent_Success_ExponentialSizes_WithReturn(int minSize)
    {
      var cache = new GenericArrayPoolCaches<byte>();
      var item = cache.Rent(minSize);

      // The item rented is perfect power of two size so the size of the buffer should be exactly min size
      item.Length.Should().Be(minSize);

      cache.Return(item);

      if (minSize > 2)
      {
        var item2 = cache.Rent(minSize - 1);
        // The item rented is one less than perfect power of two size so the size of the buffer should be exactly min size
        item2.Length.Should().Be(minSize);

        cache.Return(item2);

        var item3 = cache.Rent(minSize + 1);
        // The item rented is one less than perfect power of two size so the size of the buffer should be exactly twice min size
        item3.Length.Should().Be(minSize + 1 > GenericArrayPoolCaches<byte>.MAX_BUFFER_SIZE_CACHED ? minSize + 1 : 2 * minSize);

        cache.Return(item3);
      }
    }

    [Fact]
    public void Rent_ZeroBuffer()
    {
      var cache = new GenericArrayPoolCaches<byte>();
      var item = cache.Rent(0);
      item.Length.Should().Be(0);
    }

    [Fact]
    public void Rent_LargerThanLargestBufferPool()
    {
      var cache = new GenericArrayPoolCaches<byte>();
      var item = cache.Rent(2000000);
      item.Length.Should().Be(2000000);
    }

    [Fact]
    public void Rent_Fail_WithNegativeSize()
    {
      var cache = new GenericArrayPoolCaches<byte>();
      Action act = () => cache.Rent(-1);
      act.Should().Throw<ArgumentException>().WithMessage("Negative buffer size not permitted*");
    }

    [Fact]
    public void Return_Fail_InvalidBufferSize()
    {
      var cache = new GenericArrayPoolCaches<byte>();

      // This should not throw an exception but may record an item in the log
      cache.Return(new byte[7]);
    }

    [Fact]
    public void Return_Fail_PoolCacheIsFull()
    {
      var cache = new GenericArrayPoolCaches<byte>();

      // Fill a small pool with returned elements of the correct size
      for (int i = 0; i < GenericArrayPoolCaches<byte>.SMALL_POOL_CACHE_SIZE; i++)
      {
        cache.Return(new byte[8]);
      }

      // Return an additional element. This should not throw an exception but may
      // record an item in the log
      cache.Return(new byte[8]);
    }
  }
}
