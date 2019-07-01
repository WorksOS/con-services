using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.IO
{
  /// <summary>
  /// Provides a short term rental agency for arrays of elements of type T. It's external semantics are very close to
  /// SlabAllocatedArrayPool[T] in that arrays are rented and returned, and are represented to the renter as a
  /// TRexSpan[T]. Internally the semantics differ in that the individual exponential pools pack the rented arrays
  /// tightly, with no per element exponential pool size overhead except for remainder overhead when the pool page
  /// cannot support rental of another element of the minimum size supported by that pool 
  /// Characteristics:
  /// - Rented allocations are exact with no room to grow the content in the rented array. Ie: Capacity always equals the requested size
  /// - Rented arrays are packed together
  /// - Rentals are progressively allocated within pool pages rather than pre-defined rental spans being
  ///   issued in a stack based fashion
  /// - As soon as all rentals within a pool page are returned that page can be reused for additional array rentals
  /// - Pooled pages are relative;y small at 65536 elements. Larger requests are satisfied uniquely [Revisit this - it's only saving two bytes per rented TRexSpan<T></T>]
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class RollingExactSlabAllocatedArrayPool<T> 
  {
    public const int ABSOLUTE_MAXIMUM_NUMBER_OF_FREE_PAGES = 1024;

    private readonly int _maxNumFreePages;
    private readonly RollingExactSlabAllocatedPoolPage<T>[] _freePages;

    public RollingExactSlabAllocatedArrayPool(int maxNumFreePages)
    {
      if (maxNumFreePages < 0 || maxNumFreePages > ABSOLUTE_MAXIMUM_NUMBER_OF_FREE_PAGES)
      {
        throw new ArgumentException($"Maximum number of pool pages must be in the range 0..{ABSOLUTE_MAXIMUM_NUMBER_OF_FREE_PAGES}");
      }

      _maxNumFreePages = maxNumFreePages;
      _freePages = new RollingExactSlabAllocatedPoolPage<T>[_maxNumFreePages];
    }
  }
}
