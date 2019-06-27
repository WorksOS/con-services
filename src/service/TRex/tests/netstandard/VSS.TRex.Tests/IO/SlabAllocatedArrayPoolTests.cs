using System;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common.Extensions;
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
    public void Creation_Fail_WithInvalidPageSize_Range()
    {
      Action act = () => new SlabAllocatedArrayPool<CellPass>(-1);
      act.Should().Throw<ArgumentException>().WithMessage("Allocation pool size must be in the range*");
    }

    [Fact]
    public void Creation_Fail_WithInvalidPageSize_PowerOfTwo()
    {
      Action act = () => new SlabAllocatedArrayPool<CellPass>(13);
      act.Should().Throw<ArgumentException>().WithMessage("Allocation pool page size must be a power of 2");
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
    }

    [Fact]
    public void Rent_Rail_WithNegativeSize()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE);
      Action act = () => pool.Rent(-1);
      act.Should().Throw<ArgumentException>().WithMessage("Negative buffer size not permitted*");
    }

    [Fact]
    public void Return_AllPools()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE);

      for (int i = 0; i < DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE; i++)
      {
        var rental = pool.Rent(i);

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

    [Fact]
    public void Statistics_DefaultEmptyPool()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE);
      var stats = pool.Statistics();

      stats.Should().NotBeNull();
      stats.Length.Should().Be(VSS.TRex.IO.Utilities.Log2(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE));

      stats.ForEach(x => x.rentedItems.Should().Be(0));
    }
  }
}
