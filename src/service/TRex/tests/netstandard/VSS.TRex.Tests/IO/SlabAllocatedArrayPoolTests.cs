using System;
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
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE, 1, 10);

      pool.Should().NotBeNull();
    }

    [Fact]
    public void Creation_ConfigurableNumPools_Success()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE, 1, 10);
      pool.Should().NotBeNull();

      var pool2 = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE, 1, SlabAllocatedArrayPool<CellPass>.DefaultLargestSizeExponentialPoolToProvide);
      pool2.Should().NotBeNull();

      var pool3 = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE, 1, SlabAllocatedArrayPool<CellPass>.LargestSizeExponentialPoolToProvide);
      pool3.Should().NotBeNull();
    }

    [Fact]
    public void Creation_ConfigurableNumPools_Failure_OutOfRangeSmall()
    {
      Action act = () => new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE, 1, SlabAllocatedArrayPool<CellPass>.LargestSizeExponentialPoolToProvide + 1);
      act.Should().Throw<ArgumentException>().WithMessage("Min/Max exponential pool range must be in*");
    }

    [Fact]
    public void Creation_ConfigurableNumPools_Failure_OutOfRangeLarge()
    {
      Action act = () => new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE, 0, SlabAllocatedArrayPool<CellPass>.LargestSizeExponentialPoolToProvide);
      act.Should().Throw<ArgumentException>().WithMessage("Min/Max exponential pool range must be in*");
    }

    [Fact]
    public void Creation_ConfigurableNumPools_Failure_SmallGreaterThanLarge()
    {
      Action act = () => new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE, 2, 1);
      act.Should().Throw<ArgumentException>().WithMessage("Smallest exponential pool must be less than or equal to largest exponential pool to provide");
    }

    [Fact]
    public void Rent_AllPools()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE, 1, 10);

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
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE, 1, 10);

      var rental = pool.Rent(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE + 1);
      rental.Capacity.Should().Be(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE + 1); 
      rental.Count.Should().Be(0);
      rental.Elements.Should().NotBeNull();
      rental.Offset.Should().Be(0);
      rental.OffsetPlusCount.Should().BeGreaterOrEqualTo(0);
      rental.PoolAllocated.Should().Be(false);
    }

    [Fact]
    public void Return_AllPools()
    {
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE, 1, 10);

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
      var pool = new SlabAllocatedArrayPool<CellPass>(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE, 1, 10);

      var rental = pool.Rent(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE + 1);
      rental.Capacity.Should().Be(DEFAULT_TEST_SLAB_ALLOCATED_POOL_SIZE + 1);

      pool.Return(rental);
    }
  }
}
