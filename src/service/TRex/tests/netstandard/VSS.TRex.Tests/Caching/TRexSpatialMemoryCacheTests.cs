using System;
using System.Linq;
using VSS.TRex.Caching;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using Xunit.Abstractions;

namespace VSS.TRex.Tests.Caching
{
  public class TRexSpatialMemoryCacheTests : IClassFixture<DILoggingAndStorageProxyFixture>
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

      Assert.True(cache.MRUList != null, "MRU list is null");
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

    [Fact]
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
      ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(100, 1000000, 0.5);

      for (int i = 0; i < numberOfContexts; i++)
      {
        var context = cache.LocateOrCreateContext(Guid.Empty, $"fingerprint:{i}");

        Assert.True(cache.ContextCount == i + 1, $"Context count not {i + 1} as expect, it is {cache.ContextCount} instead");
        Assert.True(context != null, "Failed to create new context");
        Assert.True(context.ContextTokens != null, "Context does not have tokens storage");
        Assert.True(context.MRUList != null, "Context does not have MRU list reference");
        Assert.True(context.OwnerMemoryCache == cache, "Context does not have owner reference");
        Assert.True(context.TokenCount == 0, "New context has non-zero number of tokens");
      }
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_ContextCreation_MarkForRemovalOnInit()
    {
        ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(100, 1000000, 0.5);

        var context = cache.LocateOrCreateContext(Guid.Empty, "fingerprint");

        Assert.True(context != null, "Failed to create new context");
        Assert.True(context.MarkedForRemoval, "Context not marked for removal on creation in cache");
        Assert.True(context.MarkedForRemovalAtUtc > DateTime.UtcNow, "Marked for removal time earlier than now");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(10000)]
    public void Test_TRexSpatialMemoryCacheTests_ContextRetrieval(int numberOfContexts)
    {
      ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(100, 1000000, 0.5);

      var contexts = Enumerable.Range(0, numberOfContexts).Select(x => cache.LocateOrCreateContext(Guid.Empty, $"fingerprint:{x}")).ToArray();

      for (int i = 0; i < numberOfContexts; i++)
      {
        var context = cache.LocateOrCreateContext(Guid.Empty, $"fingerprint:{i}");

        Assert.True(ReferenceEquals(contexts[i], context), $"Failed to locate previously created context at index {i}");
      }
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_AddAndRetrieveItem_OneContext()
    {
      const uint _originX = 123;
      const uint _originY = 456;
      const int _size = 1000;

      ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(100, 1000000, 0.5);

      var context = cache.LocateOrCreateContext(Guid.Empty, "fingerprint");

      cache.Add(context, new TRexSpatialMemoryCacheContextTests_Element()
      {
        SizeInBytes = _size,
        CacheOriginX = _originX,
        CacheOriginY = _originY, 
      });

      Assert.True(context.TokenCount == 1, "Context token count not one after adding single item");

      var gotItem = context.Get(_originX, _originY);

      Assert.True(gotItem != null, "Failed to retrieve added entry");

      Assert.Equal(_originX, gotItem.CacheOriginX);
      Assert.Equal(_originY, gotItem.CacheOriginY);
      Assert.Equal(_size, gotItem.IndicativeSizeInBytes());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void Test_TRexSpatialMemoryCacheTests_AddAndRetrieveItem_ManyContexts(int numContexts)
    {
      const uint _originX = 123;
      const uint _originY = 456;
      const int _size = 10;

      // Create the cache with enough elements to hold one per context without eviction
      ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(numContexts, 1000000, 0.5);

      // Make the number ofg contexts requested, and a separate item to be placed in each one
      var contexts = Enumerable.Range(0, numContexts).Select(x => cache.LocateOrCreateContext(Guid.Empty, $"fingerprint:{x}")).ToArray();

      var items = Enumerable.Range(0, numContexts).Select(x => new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = _size,
        CacheOriginX = (uint) (_originX + x * SubGridTreeConsts.SubGridTreeDimension),
        CacheOriginY = (uint) (_originY + x * SubGridTreeConsts.SubGridTreeDimension)
      }).ToArray();

      for (int i = 0; i < numContexts; i++)
      {
        cache.Add(contexts[i], items[i]);

        Assert.True(contexts[i].TokenCount == 1, "Context token count not one after adding single item");
      }

      for (int i = 0; i < numContexts; i++)
      {
        var gotItem = contexts[i].Get(items[i].CacheOriginX, items[i].CacheOriginY);

        Assert.True(gotItem != null, "Failed to retrieve added entry");
        Assert.True(ReferenceEquals(items[i], gotItem), $"Got item not same as elements in items array at index {i}");
        Assert.Equal(items[i].CacheOriginX, gotItem.CacheOriginX);
        Assert.Equal(items[i].CacheOriginY, gotItem.CacheOriginY);
        Assert.Equal(items[i].IndicativeSizeInBytes(), gotItem.IndicativeSizeInBytes());
      }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void Test_TRexSpatialMemoryCacheTests_AddAndRemoveItem_ManyContexts(int numContexts)
    {
      const uint _originX = 123;
      const uint _originY = 456;
      const int _size = 10;

      // Create the cache with enough elements to hold one per context without eviction
      ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(numContexts, 1000000, 0.5);

      // Make the number ofg contexts requested, and a separate item to be placed in each one
      var contexts = Enumerable.Range(0, numContexts).Select(x => cache.LocateOrCreateContext(Guid.Empty, $"fingerprint:{x}")).ToArray();

      var items = Enumerable.Range(0, numContexts).Select(x => new TRexSpatialMemoryCacheContextTests_Element()
      {
        SizeInBytes = _size,
        CacheOriginX = (uint) (_originX + x * SubGridTreeConsts.SubGridTreeDimension),
        CacheOriginY = (uint) (_originY + x * SubGridTreeConsts.SubGridTreeDimension)
      }).ToArray();

      for (int i = 0; i < numContexts; i++)
      {
        cache.Add(contexts[i], items[i]);

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
      ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(1, 1000000, 0.5);

      // Make the number ofg contexts requested, and a separate item to be placed in each one
      var context = cache.LocateOrCreateContext(Guid.Empty, "fingerprint");

      var item1 = new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = _size,
        CacheOriginX = _originX,
        CacheOriginY = _originY
      };

      var item2 = new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = _size,
        CacheOriginX = _originX + SubGridTreeConsts.SubGridTreeDimension,
        CacheOriginY = _originY + SubGridTreeConsts.SubGridTreeDimension
      };

      cache.Add(context, item1);
      cache.Add(context, item2);

      Assert.True(context.TokenCount == 1, "Token count not one after addition of second element forcing eviction of first");
      Assert.True(context.Get(item1.CacheOriginX, item1.CacheOriginY) == null, "Able to request item1 after it should have been evicted for item2");
      Assert.True(context.Get(item2.CacheOriginX, item2.CacheOriginY) != null, "Unable to request item2 after it should have replaced item1");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_ItemReplacement()
    {
      const uint _originX = 123;
      const uint _originY = 456;
      const int _size = 1000;

      // Create the cache with enough elements to hold one per context without eviction
      ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(1, 1000000, 0.5);

      // Make the number ofg contexts requested, and a separate item to be placed in each one
      var context = cache.LocateOrCreateContext(Guid.Empty, "fingerprint");

      var item1 = new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = _size,
        CacheOriginX = _originX,
        CacheOriginY = _originY
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
      ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(capacity, maxNumBytes, 0.5);

      // Make the number ofg contexts requested, and a separate item to be placed in each one
      var context = cache.LocateOrCreateContext(Guid.Empty, "fingerprint");

      int numElementsToAdd = (maxNumBytes / elementSize) + 10;

      var items = Enumerable.Range(0, numElementsToAdd).Select(x => new TRexSpatialMemoryCacheContextTests_Element()
      {
        SizeInBytes = elementSize,
        CacheOriginX = (uint) (_originX + x * SubGridTreeConsts.SubGridTreeDimension),
        CacheOriginY = (uint) (_originY + x * SubGridTreeConsts.SubGridTreeDimension)
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
      ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(numElementsToAdd, maxNumBytes, 0.5);

      // Make the number ofg contexts requested, and a separate item to be placed in each one
      var context = cache.LocateOrCreateContext(Guid.Empty, "fingerprint");

      var items = Enumerable.Range(0, numElementsToAdd).Select(x => new TRexSpatialMemoryCacheContextTests_Element()
      {
        SizeInBytes = elementSize,
        CacheOriginX = (uint) (_originX + x * SubGridTreeConsts.SubGridTreeDimension),
        CacheOriginY = (uint) (_originY + x * SubGridTreeConsts.SubGridTreeDimension)
      }).ToArray();

      for (int i = 0; i < numElementsToAdd; i++)
      {
        cache.Add(context, items[i]);
        Assert.True(cache.CurrentSizeInBytes > 0, "Cache with elements has 0 size!");
        Assert.True(cache.CurrentSizeInBytes <= maxNumBytes, $"Current cache size of {cache.CurrentSizeInBytes} bytes is greater than limit of {maxNumBytes} bytes");
      }

      output.WriteLine($"Final cache size is {cache.CurrentSizeInBytes} compared to maximum of {maxNumBytes}");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_AddAndRetrieveManyItems_OneContext()
    {
      const uint _originX = 0;
      const uint _originY = 0;
      const int _size = 10;

      ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(100000, 1000000, 0.5);

      var context = cache.LocateOrCreateContext(Guid.Empty, "fingerprint");

      for (int i = 0; i < 100; i++)
      {
        for (int j = 0; j < 100; j++)
        {
          Assert.True(cache.Add(context, new TRexSpatialMemoryCacheContextTests_Element()
          {
            SizeInBytes = _size,
            CacheOriginX = (uint) (_originX + i * SubGridTreeConsts.SubGridTreeDimension),
            CacheOriginY = (uint) (_originY + j * SubGridTreeConsts.SubGridTreeDimension)
          }), $"Failed to add item to cache at {i}:{j}");
        }
      }

      Assert.True(context.TokenCount == 10000, $"Context token count not 10000 after adding 10000 items, it is {context.TokenCount} instead");

      for (int i = 0; i < 100; i++)
      for (int j = 0; j < 100; j++)
      {
        uint x = (uint) (_originX + i * SubGridTreeConsts.SubGridTreeDimension);
        uint y = (uint)(_originY + j * SubGridTreeConsts.SubGridTreeDimension);

        var gotItem = context.Get(x, y); 
        Assert.True(gotItem != null, "Failed to retrieve added entry");

        Assert.Equal(x, gotItem.CacheOriginX);
        Assert.Equal(y, gotItem.CacheOriginY);
        Assert.Equal(_size, gotItem.IndicativeSizeInBytes());

       }
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_ElementTimeBasedExpiry()
    {
      ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(10, 1000000, 0.5);

      // Create a context with an expiry time of one second
      var context = cache.LocateOrCreateContext(Guid.Empty, "fingerprint", new TimeSpan(0, 0, 0, 0, 500));

      // Add an item, verify it is there, wait for a seconds then attempt to get the
      // item. Verify the result is null and that the item is no longer present in the cache

      cache.Add(context, new TRexSpatialMemoryCacheContextTests_Element
      {
        CacheOriginX = 1000,
        CacheOriginY = 1000,
        SizeInBytes = 1000
      });

      Assert.True(context.TokenCount == 1, "Failed to add new item to context");

      System.Threading.Thread.Sleep(1000); // Allow the item to expire

      Assert.Null(context.Get(1000, 1000));
      Assert.True(context.TokenCount == 0, "Element not retired on Get() after expiry date");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_Get()
    {
      ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(10, 1000000, 0.5);

      // Create a context with an expiry time of one second
      var context = cache.LocateOrCreateContext(Guid.Empty, "fingerprint");
      cache.Add(context, new TRexSpatialMemoryCacheContextTests_Element
      {
        CacheOriginX = 1000,
        CacheOriginY = 1000,
        SizeInBytes = 1000
      });

      Assert.True(context.TokenCount == 1, "Failed to add new item to context");

      var item = cache.Get(context, 1000, 1000);

      Assert.NotNull(item);
      Assert.True(item.CacheOriginX == 1000 && item.CacheOriginY == 1000);
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_Remove()
    {
      ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(10, 1000000, 0.5);

      var context = cache.LocateOrCreateContext(Guid.Empty, "fingerprint");
      var item = new TRexSpatialMemoryCacheContextTests_Element
      {
        CacheOriginX = 1000,
        CacheOriginY = 1000,
        SizeInBytes = 1000
      };

      cache.Add(context, item);
      Assert.True(context.TokenCount == 1, "Token count incorrect after addition");

      cache.Remove(context, item);
      Assert.True(context.TokenCount == 0, "Token count incorrect after removal");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_ProductionDataIngestInvalidation()
    {
      ITRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(20000, 1000000, 0.5);

      // Create a context with default invalidation sensitivity, add some data to it
      // and validate that a change bitmask causes appropriate invalidation
      var context = cache.LocateOrCreateContext(Guid.Empty, "fingerprint");

      TRexSpatialMemoryCacheContextTests_Element[,] items = new TRexSpatialMemoryCacheContextTests_Element[100, 100];
      for (int i = 0; i < 100; i++)
      {
        for (int j = 0; j < 100; j++)
        {
          items[i, j] = new TRexSpatialMemoryCacheContextTests_Element
          {
            CacheOriginX = (uint) (i * SubGridTreeConsts.SubGridTreeDimension),
            CacheOriginY = (uint) (j * SubGridTreeConsts.SubGridTreeDimension),
            SizeInBytes = 1
          };

          cache.Add(context, items[i, j]);
        }
      }

      Assert.True(context.TokenCount == 10000, "Token count incorrect after addition");

      // Create the bitmask
      ISubGridTreeBitMask mask = new SubGridTreeSubGridExistenceBitMask();

      for (int i = 0; i < 100; i++)
      {
        for (int j = 0; j < 100; j++)
          cache.Add(context, new TRexSpatialMemoryCacheContextTests_Element
          {
            CacheOriginX = (uint)(i * SubGridTreeConsts.SubGridTreeDimension) >> SubGridTreeConsts.SubGridLocalKeyMask,
            CacheOriginY = (uint)(j * SubGridTreeConsts.SubGridTreeDimension) >> SubGridTreeConsts.SubGridLocalKeyMask,
            SizeInBytes = 1
          });
      }

      cache.InvalidateDueToProductionDataIngest(Guid.Empty, mask);

      int count = 10000;
      // Remove the items
      for (int i = 0; i < 100; i++)
      {
        for (int j = 0; j < 100; j++)
        {
          cache.Remove(context, items[i, j]);
          Assert.True(context.TokenCount == --count, $"Count incorrect at index {i}, {j}, count = {count}, tokenCount = {context.TokenCount}");
        }
      }

      Assert.True(context.TokenCount == 0, "Token count incorrect after invalidation");
    }
  }
}
