using System;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.IO;
using Xunit;

namespace VSS.TRex.Tests.IO
{
  public class SlabAllocatedPoolTests
  {
    [Fact]
    public void Creation()
    {
      var slab = new SlabAllocatedPool<CellPass>(1024, 16);

      slab.Should().NotBeNull();
      slab.PoolSize.Should().Be(1024);
      slab.ArraySize.Should().Be(16);

      slab.Capacity.Should().Be(0); // Capacity always zero to start
    }

    [Fact]
    public void Creation_FailWithInvalidPoolSize()
    {
      Action act = () => new SlabAllocatedPool<CellPass>(1024, 10);
      act.Should().Throw<ArgumentException>("Pool size of 1024 is not a power of two as required.");
    }

    [Fact]
    public void Creation_FailWithInvalidArraySize()
    {
      Action act = () => new SlabAllocatedPool<CellPass>(1000, 16);
      act.Should().Throw<ArgumentException>("Array size of 16 is not a power of two as required.");
    }

    [Fact]
    public void Rent_Success_RentFromSlab()
    {
      var slab = new SlabAllocatedPool<CellPass>(1024, 16);

      var rental = slab.Rent();
      rental.Elements.Should().NotBeNull();
    }

    [Fact]
    public void Rent_Success_RentFromEmptySlab()
    {
      var slab = new SlabAllocatedPool<CellPass>(16, 16);

      var rental = slab.Rent();
      rental.Elements.Should().NotBeNull();

      slab.RentalTideLevel.Should().Be(0);

      var rental2 = slab.Rent();
      rental2.Elements.Should().NotBeNull();
      rental2.Elements.Length.Should().Be(slab.ArraySize);
      rental2.Elements.Should().NotBeSameAs(rental.Elements);
    }
  }
}
