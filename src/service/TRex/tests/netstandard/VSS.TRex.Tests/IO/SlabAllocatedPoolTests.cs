using System;
using System.Collections.Generic;
using System.Text;
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
      var slab = new SlabAllocatedPool<CellPass>(1000, 10);

      slab.Should().NotBeNull();
      slab.PoolSize.Should().Be(1000);
      slab.ArraySize.Should().Be(10);

      slab.AvailCount.Should().Be(100);
    }

    [Fact]
    public void Rent_Success_RentFromSlab()
    {
      var slab = new SlabAllocatedPool<CellPass>(1000, 10);

      var rental = slab.Rent();
      rental.Elements.Should().NotBeNull();
      rental.PoolAllocated.Should().BeTrue();
    }

    [Fact]
    public void Rent_Success_RentFromEmptySlab()
    {
      var slab = new SlabAllocatedPool<CellPass>(10, 10);

      var rental = slab.Rent();
      rental.Elements.Should().NotBeNull();
      rental.PoolAllocated.Should().BeTrue();

      slab.AvailCount.Should().Be(0);

      var rental2 = slab.Rent();
      rental2.Elements.Should().NotBeNull();
      rental2.Elements.Length.Should().Be(slab.ArraySize);
      rental2.Elements.Should().NotBeSameAs(rental.Elements);
      rental2.PoolAllocated.Should().BeFalse();
    }
  }
}
