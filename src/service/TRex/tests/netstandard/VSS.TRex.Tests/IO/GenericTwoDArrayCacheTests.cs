using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.IO;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.IO
{
  public class GenericTwoDArrayCacheTests: IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Creation()
    {
      var cache = new GenericTwoDArrayCache<Cell_NonStatic>(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension, 1);
      cache.Should().NotBeNull();
    }

    [Fact]
    public void RentReturnRent_SameElement()
    {
      var cache = new GenericTwoDArrayCache<Cell_NonStatic>(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension, 10);
      var rental = cache.Rent();
      var savedRental = rental;
      cache.Return(ref rental);
      rental = cache.Rent();

      savedRental.Should().BeSameAs(rental);
    }

    [Fact]
    public void Rent_DifferentElements()
    {
      var cache = new GenericTwoDArrayCache<Cell_NonStatic>(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension, 10);
      var rental1 = cache.Rent();
      var rental2 = cache.Rent();

      rental1.Should().NotBeSameAs(rental2);
    }

    [Fact]
    public void Rent_FreeOfRentedElements()
    {
      var cache = new GenericTwoDArrayCache<Cell_NonStatic>(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension, 10);

      var rental = cache.Rent();
      cache.Should().NotBeNull();

      var pool = new SlabAllocatedArrayPool<CellPass>();

      // Rent and use all cells
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        rental[x, y].Passes = pool.Rent(5);
        rental[x, y].Passes.Add(new CellPass());
      });

      // Give all the cells back
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        pool.Return(ref rental[x, y].Passes);
      });

      // the 2D array back
      cache.Return(ref rental);

      rental.Should().BeNull();

      // Use the RentEx call to validate the content of the rental element being returned before being supplied to the calling context
      rental = cache.RentEx (r => SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        r[x, y].Passes.IsRented.Should().BeFalse();
        r[x, y].Passes.Count.Should().Be(0);
      }));

      rental.Should().NotBeNull();
    }
  }
}
