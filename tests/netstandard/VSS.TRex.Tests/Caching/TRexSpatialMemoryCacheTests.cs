using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using VSS.TRex.Caching;
using VSS.TRex.SubGridTrees.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace VSS.TRex.Tests.Caching
{
  public class TRexSpatialMemoryCacheTests
  {
    private readonly ITestOutputHelper output;

    public TRexSpatialMemoryCacheTests(ITestOutputHelper output)
    {
      this.output = output;
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_Creation()
    {
      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(100, 1000000, 0.5);

      Assert.True(cache.MRUList != null, "MRUlist is null");
      Assert.Equal(0, cache.CurrentNumElements);
      Assert.Equal(0, cache.CurrentSizeInBytes);
      Assert.Equal(50, cache.MruNonUpdateableSlotCount);
      Assert.Equal(100, cache.MaxNumElements);
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_Creation_InvalidCacheSize()
    {
      Assert.Throws<ArgumentException>(() => new TRexSpatialMemoryCache(100, 0, 0.5));
      Assert.Throws<ArgumentException>(() => new TRexSpatialMemoryCache(100, 999, 0.5));
      Assert.Throws<ArgumentException>(() => new TRexSpatialMemoryCache(100, 100000000001, 0.5));
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_Creation_InvalidMRUBand()
    {
      Assert.Throws<ArgumentException>(() => new TRexSpatialMemoryCache(100, 1000000, -1.0));
      Assert.Throws<ArgumentException>(() => new TRexSpatialMemoryCache(100, 1000000, -0.0001));
      Assert.Throws<ArgumentException>(() => new TRexSpatialMemoryCache(100, 1000000, 1.01));
    }

    [Fact(Skip="Test hangs unexpectedly")]
    public void Test_TRexSpatialMemoryCacheTests_Creation_InvalidNumberOfElements()
    {
      Assert.Throws<ArgumentException>(() => new TRexSpatialMemoryCache(-1, 1000000, 0.50));
      Assert.Throws<ArgumentException>(() => new TRexSpatialMemoryCache(0, 1000000, 0.50));
      Assert.Throws<ArgumentException>(() => new TRexSpatialMemoryCache(1000000001, 1000000, 0.50));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(10000)]
    public void Test_TRexSpatialMemoryCacheTests_ContextCreation(int numberOfContexts)
    {
      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(100, 1000000, 0.5);

      for (int i = 0; i < numberOfContexts; i++)
      {
        var context = cache.LocateOrCreateContext($"fingerprint:{i}");

        Assert.True(cache.ContextCount() == i + 1, $"Context count not {i + 1} as expect, it is {cache.ContextCount()} instead");
        Assert.True(context != null, "Failed to create new context");
        Assert.True(context.ContextTokens != null, "Context does not have tokens storage");
        Assert.True(context.MRUList != null, "Context does not have MRU list reference");
        Assert.True(context.OwnerMemoryCache == cache, "Context does not have owner reference");
        Assert.True(context.TokenCount == 0, "New context has non-zero number of tokens");
      }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(10000)]
    public void Test_TRexSpatialMemoryCacheTests_ContextRetrieval(int numberOfContexts)
    {
      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(100, 1000000, 0.5);

      var contexts = Enumerable.Range(0, numberOfContexts).Select(x => cache.LocateOrCreateContext($"fingerprint:{x}")).ToArray();

      for (int i = 0; i < numberOfContexts; i++)
      {
        var context = cache.LocateOrCreateContext($"fingerprint:{i}");

        Assert.True(ReferenceEquals(contexts[i], context), $"Failed to locate previously created context at index {i}");
      }
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_AddAndRetrieveItem_OneContext()
    {
      const uint _originX = 123;
      const uint _originY = 456;
      const int _size = 1000;

      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(100, 1000000, 0.5);

      var context = cache.LocateOrCreateContext($"fingerprint");

      cache.Add(context, new TRexSpatialMemoryCacheContextTests_Element()
      {
        SizeInBytes = _size,
        OriginX = _originX,
        OriginY = _originY, 
      });

      Assert.True(context.TokenCount == 1, "Context token count not one after adding single item");

      var gotItem = context.Get(_originX, _originY);

      Assert.True(gotItem != null, "Failed to retrieve added entry");

      Assert.Equal(_originX, gotItem.OriginX);
      Assert.Equal(_originY, gotItem.OriginY);
      Assert.Equal(_size, gotItem.IndicativeSizeInBytes());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(1000)]
    [InlineData(100000)]
    public void Test_TRexSpatialMemoryCacheTests_AddAndRetrieveItem_ManyContexts(int numContexts)
    {
      const uint _originX = 123;
      const uint _originY = 456;
      const int _size = 1000;

      // Create the cache with enough elements to hold one per context without eviction
      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(numContexts, 1000000, 0.5);

      // Make the number ofg contexts requested, and a separate item to be placed in each one
      var contexts = Enumerable.Range(0, numContexts).Select(x => cache.LocateOrCreateContext($"fingerprint:{x}")).ToArray();

      var items = Enumerable.Range(0, numContexts).Select(x => new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = _size,
        OriginX = (uint) (_originX + x * SubGridTreeConsts.SubGridTreeDimension),
        OriginY = (uint) (_originY + x * SubGridTreeConsts.SubGridTreeDimension)
      }).ToArray();

      for (int i = 0; i < numContexts; i++)
      {
        contexts[i].Add(items[i]);

        Assert.True(contexts[i].TokenCount == 1, "Context token count not one after adding single item");
      }

      for (int i = 0; i < numContexts; i++)
      {
        var gotItem = contexts[i].Get(items[i].OriginX, items[i].OriginY);

        Assert.True(gotItem != null, "Failed to retrieve added entry");
        Assert.True(ReferenceEquals(items[i], gotItem), $"Got item not same as elements in items array at index {i}");
        Assert.Equal(items[i].OriginX, gotItem.OriginX);
        Assert.Equal(items[i].OriginY, gotItem.OriginY);
        Assert.Equal(items[i].IndicativeSizeInBytes(), gotItem.IndicativeSizeInBytes());
      }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(1000)]
    [InlineData(100000)]
    public void Test_TRexSpatialMemoryCacheTests_AddAndRemoveItem_ManyContexts(int numContexts)
    {
      const uint _originX = 123;
      const uint _originY = 456;
      const int _size = 1000;

      // Create the cache with enough elements to hold one per context without eviction
      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(numContexts, 1000000, 0.5);

      // Make the number ofg contexts requested, and a separate item to be placed in each one
      var contexts = Enumerable.Range(0, numContexts).Select(x => cache.LocateOrCreateContext($"fingerprint:{x}")).ToArray();

      var items = Enumerable.Range(0, numContexts).Select(x => new TRexSpatialMemoryCacheContextTests_Element()
      {
        SizeInBytes = _size,
        OriginX = (uint) (_originX + x * SubGridTreeConsts.SubGridTreeDimension),
        OriginY = (uint) (_originY + x * SubGridTreeConsts.SubGridTreeDimension)
      }).ToArray();

      for (int i = 0; i < numContexts; i++)
      {
        contexts[i].Add(items[i]);

        Assert.True(contexts[i].TokenCount == 1, "Context token count not one after adding single item");
      }

      Assert.True(contexts.Length == numContexts, $"Number of contexts not {numContexts} as expected, it is: {contexts.Length}");

      for (int i = 0; i < numContexts; i++)
      {
        contexts[i].Remove(items[i]);

        Assert.True(contexts[i].TokenCount == 0, "Token count not zero after removing only token in context");
      }
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_EvictedItemsRemoval()
    {
      const uint _originX = 123;
      const uint _originY = 456;
      const int _size = 1000;

      // Create the cache with enough elements to hold one per context without eviction
      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(1, 1000000, 0.5);

      // Make the number ofg contexts requested, and a separate item to be placed in each one
      var context = cache.LocateOrCreateContext($"fingerprint");

      var item1 = new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = _size,
        OriginX = (uint) (_originX),
        OriginY = (uint) (_originY)
      };

      var item2 = new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = _size,
        OriginX = (uint) (_originX + SubGridTreeConsts.SubGridTreeDimension),
        OriginY = (uint) (_originY + SubGridTreeConsts.SubGridTreeDimension)
      };

      cache.Add(context, item1);
      cache.Add(context, item2);

      Assert.True(context.TokenCount == 1, "Token count not one after addition of second element forcing eviction of first");
      Assert.True(context.Get(item1.OriginX, item1.OriginY) == null, "Able to request item1 after it should have been evicted for item2");
      Assert.True(context.Get(item2.OriginX, item2.OriginY) != null, "Unable to request item2 after it should have replaced item1");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_ItemReplacement()
    {
      const uint _originX = 123;
      const uint _originY = 456;
      const int _size = 1000;

      // Create the cache with enough elements to hold one per context without eviction
      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(1, 1000000, 0.5);

      // Make the number ofg contexts requested, and a separate item to be placed in each one
      var context = cache.LocateOrCreateContext($"fingerprint");

      var item1 = new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = _size,
        OriginX = (uint) (_originX),
        OriginY = (uint) (_originY)
      };

      Assert.True(cache.Add(context, item1), "Failed to add element for the first time");
      Assert.False(cache.Add(context, item1), "Succeeded overwriting element - bad!");
    }

    [Theory]
    [InlineData(11, 1000, 100)]
    [InlineData(21, 2000, 100)]
    [InlineData(100, 10000, 200)]
    [InlineData(1001, 1000000, 1000)]
    [InlineData(100000, 100000000, 10000)]
    public void Test_TRexSpatialMemoryCacheTests_SizeTrackingOnAddition(int capacity, int maxNumBytes, int elementSize)
    {
      const uint _originX = 123;
      const uint _originY = 456;

      // Create the cache with enough elements to hold one per context without eviction
      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(capacity, maxNumBytes, 0.5);

      // Make the number ofg contexts requested, and a separate item to be placed in each one
      var context = cache.LocateOrCreateContext("fingerprint");

      int numElementsToAdd = (maxNumBytes / elementSize) + 10;

      var items = Enumerable.Range(0, numElementsToAdd).Select(x => new TRexSpatialMemoryCacheContextTests_Element()
      {
        SizeInBytes = elementSize,
        OriginX = (uint) (_originX + x * SubGridTreeConsts.SubGridTreeDimension),
        OriginY = (uint) (_originY + x * SubGridTreeConsts.SubGridTreeDimension)
      }).ToArray();

      for (int i = 0; i < numElementsToAdd; i++)
      {
        var expectedSize = (i + 1) * elementSize > maxNumBytes ? maxNumBytes : (i + 1) * elementSize;
        cache.Add(context, items[i]);
        Assert.True(cache.CurrentSizeInBytes == expectedSize,
          $"Cache size is not correct, current = {cache.CurrentSizeInBytes}, elementSize = {elementSize}, expected = {expectedSize}, capacity = {capacity}, i = {i}, numElementsToAdd = {numElementsToAdd}");
      }
    }

    [Theory]
    [InlineData(1000, 100)]
    [InlineData(2000, 100)]
    [InlineData(10000, 200)]
    [InlineData(1000000, 1000)]
    [InlineData(100000000, 10000)]
    public void Test_TRexSpatialMemoryCacheTests_SizeConstraint(int maxNumBytes, int elementSize)
    {
      const uint _originX = 123;
      const uint _originY = 456;

      int numElementsToAdd = (maxNumBytes / elementSize) + 10;

      // Create the cache with enough elements to hold one per context without eviction
      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(numElementsToAdd, maxNumBytes, 0.5);

      // Make the number ofg contexts requested, and a separate item to be placed in each one
      var context = cache.LocateOrCreateContext("fingerprint");

      var items = Enumerable.Range(0, numElementsToAdd).Select(x => new TRexSpatialMemoryCacheContextTests_Element()
      {
        SizeInBytes = elementSize,
        OriginX = (uint) (_originX + x * SubGridTreeConsts.SubGridTreeDimension),
        OriginY = (uint) (_originY + x * SubGridTreeConsts.SubGridTreeDimension)
      }).ToArray();

      for (int i = 0; i < numElementsToAdd; i++)
      {
        cache.Add(context, items[i]);
        Assert.True(cache.CurrentSizeInBytes > 0, "Cache with elements has 0 size!");
        Assert.True(cache.CurrentSizeInBytes <= maxNumBytes, $"Current cache size of {cache.CurrentSizeInBytes} bytes is greater than limit of {maxNumBytes} bytes");
      }

      output.WriteLine($"Final cache size is {cache.CurrentSizeInBytes} compared to maximum of {maxNumBytes}");
    }
  }
}
