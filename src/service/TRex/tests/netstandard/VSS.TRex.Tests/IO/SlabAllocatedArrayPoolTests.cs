using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.IO;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.IO
{
  public class SlabAllocatedArrayPoolTests : IClassFixture<DILoggingFixture>
  {
    private const int DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE = 1024;

    [Fact]
    public void Creation_DefaultNumPools()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE);

      pool.Should().NotBeNull();
    }

    [Fact]
    public void Creation_ConfigurableNumPools_Success()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE);
      pool.Should().NotBeNull();
    }

    [Fact]
    public void Rent_AllPools()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE);

      for (int i = 0; i < DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE; i++)
      {
        var rental = pool.Rent(i);
        rental.Capacity.Should().BeGreaterOrEqualTo(i);
        rental.Count.Should().Be(0);
        rental.Elements.Should().NotBeNull();
        rental.Offset.Should().BeGreaterOrEqualTo(0);
        rental.OffsetPlusCount.Should().BeGreaterOrEqualTo(0);
        rental.SlabIndex.Should().Be((byte)(i == 0 ? TRexSpan<CellPass>.NO_SLAB_INDEX : 0));

        pool.Return(rental); // Release rental so as not to pollute expected pool allocated status
      }
    }


    [Fact]
    public void Rent_LargerThanAvailablePools()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE);

      var rental = pool.Rent(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE + 1);
      rental.Capacity.Should().Be(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE + 1); 
      rental.Count.Should().Be(0);
      rental.Elements.Should().NotBeNull();
      rental.Offset.Should().Be(0);
      rental.OffsetPlusCount.Should().BeGreaterOrEqualTo(0);
      rental.SlabIndex.Should().Be(TRexSpan<CellPass>.NO_SLAB_INDEX);
    }

    [Fact]
    public void Return_AllPools()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE);

      for (int i = 0; i < DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE; i++)
      {
        var rental = pool.Rent(i);
        rental.SlabIndex.Should().Be((byte)(i == 0 ? TRexSpan<CellPass>.NO_SLAB_INDEX : 0)); 

        pool.Return(rental);
        rental.MarkReturned();
      }
    }

    [Fact]
    public void Return_LargerThanAvailablePools()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE);

      var rental = pool.Rent(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE + 1);
      rental.Capacity.Should().Be(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE + 1);

      pool.Return(rental);
    }
  }
}
