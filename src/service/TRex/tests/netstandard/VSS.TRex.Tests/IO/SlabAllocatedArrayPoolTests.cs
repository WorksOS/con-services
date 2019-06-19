using System;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.IO;
using Xunit;

namespace VSS.TRex.Tests.IO
{
  public class SlabAllocatedArrayPoolTests
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
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE, 10);
      pool.Should().NotBeNull();

      var pool2 = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE, SlabAllocatedArrayPool<CellPass>.DefaultNumExponentialPoolsToProvide);
      pool2.Should().NotBeNull();

      var pool3 = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE, SlabAllocatedArrayPool<CellPass>.MaxNumExponentialPoolsToProvide);
      pool3.Should().NotBeNull();
    }

    [Fact]
    public void Creation_ConfigurableNumPools_Failure()
    {
      Action act = () => new SlabAllocatedArrayPool<CellPass>(1000, SlabAllocatedArrayPool<CellPass>.MaxNumExponentialPoolsToProvide + 1);
      act.Should().Throw<ArgumentException>().WithMessage("Cannot create slab allocated array pool with more than *");
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
        rental.PoolAllocated.Should().Be(i > 0 && i <= DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE);

        pool.Return(rental); // Release rental so as not to pollute expected pool allocated status
      }
    }


    [Fact]
    public void Rent_LargerThanAvailablePools()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE);

      var rental = pool.Rent(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE + 1);
      rental.Capacity.Should().Be(2 * DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE); // Will make an array pool element of the size it knows how to (power of two)
      rental.Count.Should().Be(0);
      rental.Elements.Should().NotBeNull();
      rental.Offset.Should().Be(0);
      rental.OffsetPlusCount.Should().BeGreaterOrEqualTo(0);
      rental.PoolAllocated.Should().Be(false);
    }

    [Fact]
    public void Return_AllPools()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE);

      for (int i = 0; i < DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE; i++)
      {
        var rental = pool.Rent(i);
        rental.PoolAllocated.Should().Be(i > 0 && i <= DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE); 

        pool.Return(rental);
        rental.MarkReturned();
      }
    }

    [Fact]
    public void Return_LargerThanAvailablePools()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE);

      var rental = pool.Rent(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE + 1);
      rental.Capacity.Should().Be(2 * DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE);  // Will make an array pool element of the size it knows how to (power of two)

      pool.Return(rental);
    }
  }
}
