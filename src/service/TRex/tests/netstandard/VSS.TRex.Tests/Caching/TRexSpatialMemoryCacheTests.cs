using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Caching;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Extensions;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
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
      using (var cache = new TRexSpatialMemoryCache(100, 1000000, 0.5))
      {
        Assert.True(cache.MRUList != null, "MRU list is null");
        Assert.Equal(0, cache.CurrentNumElements);
        Assert.Equal(0, cache.CurrentSizeInBytes);
        Assert.Equal(50, cache.MruNonUpdateableSlotCount);
        Assert.Equal(100, cache.MaxNumElements);

        cache.ProjectCount.Should().Be(0);
        cache.ContextRemovalCount.Should().Be(0);
      }
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
      using (var cache = new TRexSpatialMemoryCache(100, 1000000, 0.5))
      {
        for (int i = 0; i < numberOfContexts; i++)
        {
          var context = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, $"fingerprint:{i}");

          Assert.True(cache.ContextCount == i + 1, $"Context count not {i + 1} as expect, it is {cache.ContextCount} instead");
          Assert.True(context != null, "Failed to create new context");
          Assert.True(context.ContextTokens != null, "Context does not have tokens storage");
          Assert.True(context.MRUList != null, "Context does not have MRU list reference");
          Assert.True(context.OwnerMemoryCache == cache, "Context does not have owner reference");
          Assert.True(context.TokenCount == 0, "New context has non-zero number of tokens");
          Assert.True(context.GridDataType == GridDataType.Height, "Grid data type is not height as expected.");
        }

        cache.ProjectCount.Should().Be(1);
      }
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_ContextCreation_MarkForRemovalOnInit()
    {
      using (var cache = new TRexSpatialMemoryCache(100, 1000000, 0.5))
      {
        var context = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, "fingerprint");

        Assert.True(context != null, "Failed to create new context");
        Assert.True(context.MarkedForRemoval, "Context not marked for removal on creation in cache");
        Assert.True(context.MarkedForRemovalAtUtc > DateTime.UtcNow, "Marked for removal time earlier than now");
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
      using (var cache = new TRexSpatialMemoryCache(100, 1000000, 0.5))
      {
        var contexts = Enumerable.Range(0, numberOfContexts).Select(x => cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, $"fingerprint:{x}")).ToArray();

        for (int i = 0; i < numberOfContexts; i++)
        {
          var context = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, $"fingerprint:{i}");

          Assert.True(ReferenceEquals(contexts[i], context), $"Failed to locate previously created context at index {i}");
        }
      }
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_AddAndRetrieveItem_OneContext()
    {
      const int _originX = 123;
      const int _originY = 456;
      const int _size = 1000;

      using (var cache = new TRexSpatialMemoryCache(100, 1000000, 0.5))
      { 
        var context = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, "fingerprint");

      cache.Add(context, new TRexSpatialMemoryCacheContextTests_Element
      {
        SizeInBytes = _size,
        CacheOriginX = _originX,
        CacheOriginY = _originY,
      });

      Assert.True(context.TokenCount == 1, "Context token count not one after adding single item");

      var gotItem = cache.Get(context, _originX, _originY);

      Assert.True(gotItem != null, "Failed to retrieve added entry");

      Assert.Equal(_originX, gotItem.CacheOriginX);
      Assert.Equal(_originY, gotItem.CacheOriginY);
      Assert.Equal(_size, gotItem.IndicativeSizeInBytes());
      }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void Test_TRexSpatialMemoryCacheTests_AddAndRetrieveItem_ManyContexts(int numContexts)
    {
      const int _originX = 123;
      const int _originY = 456;
      const int _size = 10;

      // Create the cache with enough elements to hold one per context without eviction
      using (var cache = new TRexSpatialMemoryCache(numContexts, 1000000, 0.5))
      {
        // Make the number ofg contexts requested, and a separate item to be placed in each one
        var contexts = Enumerable.Range(0, numContexts).Select(x => cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, $"fingerprint:{x}")).ToArray();

        var items = Enumerable.Range(0, numContexts).Select(x => new TRexSpatialMemoryCacheContextTests_Element
        {
          SizeInBytes = _size,
          CacheOriginX = (int) (_originX + x * SubGridTreeConsts.SubGridTreeDimension),
          CacheOriginY = (int) (_originY + x * SubGridTreeConsts.SubGridTreeDimension)
        }).ToArray();

        for (int i = 0; i < numContexts; i++)
        {
          cache.Add(contexts[i], items[i]);

          Assert.True(contexts[i].TokenCount == 1, "Context token count not one after adding single item");
        }

        for (int i = 0; i < numContexts; i++)
        {
          var gotItem = cache.Get(contexts[i], items[i].CacheOriginX, items[i].CacheOriginY);

          Assert.True(gotItem != null, "Failed to retrieve added entry");
          Assert.True(ReferenceEquals(items[i], gotItem), $"Got item not same as elements in items array at index {i}");
          Assert.Equal(items[i].CacheOriginX, gotItem.CacheOriginX);
          Assert.Equal(items[i].CacheOriginY, gotItem.CacheOriginY);
          Assert.Equal(items[i].IndicativeSizeInBytes(), gotItem.IndicativeSizeInBytes());
        }
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
      const int _originX = 123;
      const int _originY = 456;
      const int _size = 10;

      // Create the cache with enough elements to hold one per context without eviction
      using (var cache = new TRexSpatialMemoryCache(numContexts, 1000000, 0.5))
      {
        // Make the number ofg contexts requested, and a separate item to be placed in each one
        var contexts = Enumerable.Range(0, numContexts).Select(x => cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, $"fingerprint:{x}")).ToArray();

        var items = Enumerable.Range(0, numContexts).Select(x => new TRexSpatialMemoryCacheContextTests_Element()
        {
          SizeInBytes = _size,
          CacheOriginX = (int) (_originX + x * SubGridTreeConsts.SubGridTreeDimension),
          CacheOriginY = (int) (_originY + x * SubGridTreeConsts.SubGridTreeDimension)
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

        cache.RemoveContextsMarkedForRemoval(0);
        cache.ContextRemovalCount.Should().Be(numContexts);
      }
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_EvictedItemsRemoval()
    {
      const int _originX = 123;
      const int _originY = 456;
      const int _size = 1000;

      // Create the cache with enough elements to hold one per context without eviction
      using (var cache = new TRexSpatialMemoryCache(1, 1000000, 0.5))
      {
        // Make the number of contexts requested, and a separate item to be placed in each one
        var context = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, "fingerprint");

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
        Assert.True(cache.Get(context, item1.CacheOriginX, item1.CacheOriginY) == null, "Able to request item1 after it should have been evicted for item2");
        Assert.True(cache.Get(context, item2.CacheOriginX, item2.CacheOriginY) != null, "Unable to request item2 after it should have replaced item1");
      }
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_ItemReplacement()
    {
      const int _originX = 123;
      const int _originY = 456;
      const int _size = 1000;

      // Create the cache with enough elements to hold one per context without eviction
      using (var cache = new TRexSpatialMemoryCache(1, 1000000, 0.5))
      {
        // Make the number ofg contexts requested, and a separate item to be placed in each one
        var context = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, "fingerprint");

        var item1 = new TRexSpatialMemoryCacheContextTests_Element
        {
          SizeInBytes = _size,
          CacheOriginX = _originX,
          CacheOriginY = _originY
        };

        Assert.True(cache.Add(context, item1), "Failed to add element for the first time");
        Assert.False(cache.Add(context, item1), "Succeeded overwriting element - bad!");
      }
    }

    [Theory]
    [InlineData(11, 1000, 100)]
    [InlineData(21, 2000, 100)]
    [InlineData(100, 10000, 200)]
    [InlineData(1001, 1000000, 1000)]
    [InlineData(100000, 100000000, 10000)]
    public void Test_TRexSpatialMemoryCacheTests_SizeTrackingOnAddition(int capacity, int maxNumBytes, int elementSize)
    {
      const int _originX = 123;
      const int _originY = 456;

      // Create the cache with enough elements to hold one per context without eviction
      using (var cache = new TRexSpatialMemoryCache(capacity, maxNumBytes, 0.5))
      {
        // Make the number ofg contexts requested, and a separate item to be placed in each one
        var context = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, "fingerprint");

        int numElementsToAdd = (maxNumBytes / elementSize) + 10;

        var items = Enumerable.Range(0, numElementsToAdd).Select(x => new TRexSpatialMemoryCacheContextTests_Element()
        {
          SizeInBytes = elementSize,
          CacheOriginX = (int) (_originX + x * SubGridTreeConsts.SubGridTreeDimension),
          CacheOriginY = (int) (_originY + x * SubGridTreeConsts.SubGridTreeDimension)
        }).ToArray();

        for (int i = 0; i < numElementsToAdd; i++)
        {
          var expectedSize = (i + 1) * elementSize > maxNumBytes ? maxNumBytes : (i + 1) * elementSize;
          cache.Add(context, items[i]);
          Assert.True(cache.CurrentSizeInBytes == expectedSize,
            $"Cache size is not correct, current = {cache.CurrentSizeInBytes}, elementSize = {elementSize}, expected = {expectedSize}, capacity = {capacity}, i = {i}, numElementsToAdd = {numElementsToAdd}");
        }
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
      const int _originX = 123;
      const int _originY = 456;

      int numElementsToAdd = (maxNumBytes / elementSize) + 10;

      // Create the cache with enough elements to hold one per context without eviction
      using (var cache = new TRexSpatialMemoryCache(numElementsToAdd, maxNumBytes, 0.5))
      {
        // Make the number ofg contexts requested, and a separate item to be placed in each one
        var context = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, "fingerprint");

        var items = Enumerable.Range(0, numElementsToAdd).Select(x => new TRexSpatialMemoryCacheContextTests_Element()
        {
          SizeInBytes = elementSize,
          CacheOriginX = (int) (_originX + x * SubGridTreeConsts.SubGridTreeDimension),
          CacheOriginY = (int) (_originY + x * SubGridTreeConsts.SubGridTreeDimension)
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

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_AddAndRetrieveManyItems_OneContext()
    {
      const int _originX = 0;
      const int _originY = 0;
      const int _size = 10;

      using (var cache = new TRexSpatialMemoryCache(100000, 1000000, 0.5))
      {
        var context = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, "fingerprint");

        for (int i = 0; i < 100; i++)
        {
          for (int j = 0; j < 100; j++)
          {
            Assert.True(cache.Add(context, new TRexSpatialMemoryCacheContextTests_Element()
            {
              SizeInBytes = _size,
              CacheOriginX = (int) (_originX + i * SubGridTreeConsts.SubGridTreeDimension),
              CacheOriginY = (int) (_originY + j * SubGridTreeConsts.SubGridTreeDimension)
            }), $"Failed to add item to cache at {i}:{j}");
          }
        }

        Assert.True(context.TokenCount == 10000, $"Context token count not 10000 after adding 10000 items, it is {context.TokenCount} instead");

        for (int i = 0; i < 100; i++)
        for (int j = 0; j < 100; j++)
        {
          int x = (int) (_originX + i * SubGridTreeConsts.SubGridTreeDimension);
          int y = (int) (_originY + j * SubGridTreeConsts.SubGridTreeDimension);

          var gotItem = cache.Get(context, x, y);
          Assert.True(gotItem != null, "Failed to retrieve added entry");

          Assert.Equal(x, gotItem.CacheOriginX);
          Assert.Equal(y, gotItem.CacheOriginY);
          Assert.Equal(_size, gotItem.IndicativeSizeInBytes());
        }
      }
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_ElementTimeBasedExpiry()
    {
      using (var cache = new TRexSpatialMemoryCache(10, 1000000, 0.5))
      {
        // Create a context with an expiry time of one second
        var context = cache.LocateOrCreateContext(Guid.Empty, GridDataType.All, "fingerprint", new TimeSpan(0, 0, 0, 0, 500));

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

        Assert.Null(cache.Get(context, 1000, 1000));
        Assert.True(context.TokenCount == 0, "Element not retired on Get() after expiry date");
      }
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_Get()
    {
      using (var cache = new TRexSpatialMemoryCache(10, 1000000, 0.5))
      {
        // Create a context with an expiry time of one second
        var context = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, "fingerprint");
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
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_Remove()
    {
      using (var cache = new TRexSpatialMemoryCache(10, 1000000, 0.5))
      {
        var context = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, "fingerprint");
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
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_ProductionDataIngestInvalidation()
    {
      using (var cache = new TRexSpatialMemoryCache(20000, 1000000, 0.5))
      {
        // Create a context with default invalidation sensitivity, add some data to it
        // and validate that a change bitmask causes appropriate invalidation
        var context = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, "fingerprint");

        var items = new TRexSpatialMemoryCacheContextTests_Element[100, 100];
        for (var i = 0; i < 100; i++)
        {
          for (var j = 0; j < 100; j++)
          {
            items[i, j] = new TRexSpatialMemoryCacheContextTests_Element
            {
              CacheOriginX = i * SubGridTreeConsts.SubGridTreeDimension,
              CacheOriginY = j * SubGridTreeConsts.SubGridTreeDimension,
              SizeInBytes = 1
            };

            cache.Add(context, items[i, j]);
          }
        }

        Assert.True(context.TokenCount == 10000, "Token count incorrect after addition");

        // Create the bitmask
        ISubGridTreeBitMask mask = new SubGridTreeSubGridExistenceBitMask();

        for (var i = 0; i < 100; i++)
        {
          for (var j = 0; j < 100; j++)
          {
            mask[(i * SubGridTreeConsts.SubGridTreeDimension) >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
                 (j * SubGridTreeConsts.SubGridTreeDimension) >> SubGridTreeConsts.SubGridIndexBitsPerLevel] = true;
          }
        }

        cache.MRUList.TokenCount.Should().Be(context.TokenCount);

        cache.InvalidateDueToProductionDataIngest(Guid.Empty, mask);

        cache.MRUList.TokenCount.Should().Be(0);
      }
    }

    [Fact(Skip = "Slow test - better if run locally")]
    [Trait("Category", "Slow")]
    public void Test_TRexSpatialMemoryCacheTests_ConcurrentAccessWithDesignAndProductionDataIngestInvalidation()
    {
      using var cache = new TRexSpatialMemoryCache(20000, 1000000, 0.5);

      // Create a context with default invalidation sensitivity, add some data to it
      // and validate that a change bitmask causes appropriate invalidation in concurrent operations
      var contextHeight = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, "fingerprintHeight");
      var contextPassCount = cache.LocateOrCreateContext(Guid.Empty, GridDataType.PassCount, "fingerprintPasscount");

      var contexts = new[] {contextHeight, contextPassCount};

      var itemsHeight = new TRexSpatialMemoryCacheContextTests_Element[100, 100];
      var itemsPassCount = new TRexSpatialMemoryCacheContextTests_Element[100, 100];

      var items = new[] {itemsHeight, itemsPassCount};

      // Create the bitmask
      ISubGridTreeBitMask mask = new SubGridTreeSubGridExistenceBitMask();

      for (var i = 0; i < 100; i++)
      {
        for (var j = 0; j < 100; j++)
        {
          mask[(i * SubGridTreeConsts.SubGridTreeDimension) >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
            (j * SubGridTreeConsts.SubGridTreeDimension) >> SubGridTreeConsts.SubGridIndexBitsPerLevel] = true;

          items.ForEach(x => x[i, j] = new TRexSpatialMemoryCacheContextTests_Element { CacheOriginX = i * SubGridTreeConsts.SubGridTreeDimension, CacheOriginY = j * SubGridTreeConsts.SubGridTreeDimension, SizeInBytes = 1 });
        }
      }

      var additionComplete = false;
      var invalidationComplete = false;

      // Progressively add and invalidate the elements in the cache in separate tasks

      var additionTask = Task.Run(() =>{
        for (var loopCount = 0; loopCount < 100; loopCount++)
        {
          for (var i = 0; i < 100; i++)
          {
            for (var j = 0; j < 100; j++)
            {
              for (var contextIndex = 0; contextIndex < contexts.Length; contextIndex++)
              {
                if (cache.Get(contexts[contextIndex], i, j) == null)
                  cache.Add(contexts[contextIndex], items[contextIndex][i, j]);
              }
            }
          }
        }

        additionComplete = true;
      });

      var invalidationTask = Task.Run(() =>
      {
        for (var loopCount = 0; loopCount < 100; loopCount++)
        {
          for (var i = 99; i >= 0; i--)
          {
            for (var j = 99; j >= 0; j--)
            {
              foreach (var context in contexts)
              {
                // Empty contexts are ignored
                if (context.TokenCount > 0)
                {
                  context.InvalidateSubGrid(i * SubGridTreeConsts.SubGridTreeDimension, j * SubGridTreeConsts.SubGridTreeDimension, out var subGridPresentForInvalidation);
                }
              }
            }
          }
        }

        invalidationComplete = true;
      });

      var numDesignInvalidations = 0;
      var designUpdateTask = Task.Run(async () =>
      {
        while (!additionComplete || !invalidationComplete)
        {
          // Mimic design invalidation by periodically invalidating all sub grids
          await Task.Delay(10);
          contexts[numDesignInvalidations % contexts.Length].InvalidateAllSubGrids();

          numDesignInvalidations++;
        }
      });

      Task.WaitAll(new[] { additionTask, invalidationTask, designUpdateTask });

      numDesignInvalidations.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_DesignChangeInvalidation()
    {
      var projectUid = Guid.NewGuid();
      var gridDataType = Types.GridDataType.PassCount;
      var designUid = Guid.NewGuid();
      var designUid2 = Guid.NewGuid();
      Guid[] includedSurveyedSurfaces = new Guid[] { designUid };
      Guid[] includedSurveyedSurfaces2 = new Guid[] { designUid2};

      using (var cache = new TRexSpatialMemoryCache(20000, 1000000, 0.5))
      {
        // Create a context with a included design and validate that a design change causes the appropriate invalidation
        var testGuid = Guid.NewGuid();
        var context =  cache.LocateOrCreateContext(projectUid, gridDataType, SpatialCacheFingerprint.ConstructFingerprint(projectUid, gridDataType,null, includedSurveyedSurfaces));
        // this content will remain in cache as it uses a different design
        var context2 = cache.LocateOrCreateContext(projectUid, gridDataType, SpatialCacheFingerprint.ConstructFingerprint(projectUid, gridDataType, null, includedSurveyedSurfaces2));

        Assert.True(context.MarkedForRemoval == true, "Empty contents should be marked for removal");

        TRexSpatialMemoryCacheContextTests_Element[,] items = new TRexSpatialMemoryCacheContextTests_Element[100, 100];
        TRexSpatialMemoryCacheContextTests_Element[,] items2 = new TRexSpatialMemoryCacheContextTests_Element[100, 100];

        // Add content to our 2 contexts
        for (var k = 1; k < 3; k++)
        {
          for (var i = 0; i < 100; i++)
          {
            for (var j = 0; j < 100; j++)
            {
              if (k == 1)
              {
                items[i, j] = new TRexSpatialMemoryCacheContextTests_Element
                {
                  CacheOriginX = (int)(i * SubGridTreeConsts.SubGridTreeDimension),
                  CacheOriginY = (int)(j * SubGridTreeConsts.SubGridTreeDimension),
                  SizeInBytes = 1
                };
                cache.Add(context, items[i, j]);
              }
              else
              {
                items2[i, j] = new TRexSpatialMemoryCacheContextTests_Element
                {
                  CacheOriginX = (int)(i * SubGridTreeConsts.SubGridTreeDimension),
                  CacheOriginY = (int)(j * SubGridTreeConsts.SubGridTreeDimension),
                  SizeInBytes = 1
                };
                cache.Add(context2, items2[i, j]);
              }
            }
          }
        }

        // verify items added OK
        Assert.True(context.MarkedForRemoval == false, "Context should not be marked for removal");
        Assert.True(context.TokenCount == 10000, "Token count incorrect after addition");
        Assert.True(context2.MarkedForRemoval == false, "Context2 should not be marked for removal");
        Assert.True(context2.TokenCount == 10000, "Token count incorrect after addition of context2");

        // invalidate first of the 2 contexts added
        cache.InvalidateDueToDesignChange(projectUid, designUid);

        cache.MRUList.TokenCount.Should().Be(10000);

        int counter = 0;
        for (var i = 0; i < 100; i++)
        {
          for (var j = 0; j < 100; j++)
          {
            cache.MRUList.Get(counter++).Should().NotBe(items[i, j]);
          }
        }

        for (var i = 0; i < 100; i++)
        {
          for (var j = 0; j < 100; j++)
          {
            cache.MRUList.Get(counter++).Should().Be(items2[i, j]);
          }
        }

        //Test context is removed
        Assert.True(context.MarkedForRemoval == true, "Empty context should be marked for removal");
        Assert.True(context.TokenCount == 0, "Token count incorrect after invalidation");
        // Test context2 remains
        Assert.True(context2.MarkedForRemoval == false, "Context2 should not be marked for removal");
        Assert.True(context2.TokenCount == 10000, "Token count incorrect after test");
      }
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_Disposable()
    {
      using (var cache = new TRexSpatialMemoryCache(10, 1000000, 0.5))
      {
        var _ = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, "fingerprint");

        cache.ContextCount.Should().Be(1);
      }
    }

    [Fact]
    public void ItemRemovedFromContext_FailWithExceptionOnNegativeResult()
    {
      using (var cache = new TRexSpatialMemoryCache(10, 1000000, 0.5))
      {
        Action act = () => cache.ItemRemovedFromContext(100);
        act.Should().Throw<TRexException>().WithMessage("CurrentSizeInBytes < 0! Consider using Cache.Add(context, item).");
      }
    }

    [Fact]
   // [Trait("Category", "Slow")]
    public void CacheContext_Tenancy_IsUpheld_OnConcurrentRequests_WithLRUEvictionOnSmallCacheSize()
    {
      using var cache = new TRexSpatialMemoryCache(100, 1000000, 0.1);

      // Create a context with default invalidation sensitivity, add some data to it
      // and validate that a change bitmask causes appropriate invalidation in concurrent operations
      var contextHeight = cache.LocateOrCreateContext(Guid.Empty, GridDataType.Height, "fingerprintHeight");
      var contextPassCount = cache.LocateOrCreateContext(Guid.Empty, GridDataType.PassCount, "fingerprintPasscount");

      var contexts = new[] { contextHeight, contextPassCount };

      var itemsHeight = new TRexSpatialMemoryCacheContextTests_Element[100, 100];
      var itemsPassCount = new TRexSpatialMemoryCacheContextTests_Element[100, 100];

      var items = new[] { itemsHeight, itemsPassCount };

      // Create the bitmask
      ISubGridTreeBitMask mask = new SubGridTreeSubGridExistenceBitMask();

      for (var i = 0; i < 100; i++)
      {
        for (var j = 0; j < 100; j++)
        {
          mask[(i * SubGridTreeConsts.SubGridTreeDimension) >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
            (j * SubGridTreeConsts.SubGridTreeDimension) >> SubGridTreeConsts.SubGridIndexBitsPerLevel] = true;

          items.ForEach((x, index) => x[i, j] = new TRexSpatialMemoryCacheContextTests_Element
          {
            Context = contexts[index],
            CacheOriginX = i * SubGridTreeConsts.SubGridTreeDimension,
            CacheOriginY = j * SubGridTreeConsts.SubGridTreeDimension, SizeInBytes = 1
          });
        }
      }

//      var additionComplete = false;
//      var invalidationComplete = false;

      // Progressively add elements in the cache forcing elements to be removed to accomodate them given the cache's small size
      var additionTask = Task.Run(() => {
        for (var loopCount = 0; loopCount < 100; loopCount++)
        {
          for (var i = 0; i < 100; i++)
          {
            for (var j = 0; j < 100; j++)
            {
              for (var contextIndex = 0; contextIndex < contexts.Length; contextIndex++)
              {
                TRexSpatialMemoryCacheContextTests_Element elem;
                if ((elem = (TRexSpatialMemoryCacheContextTests_Element)cache.Get(contexts[contextIndex], i, j)) != null)
                {
                  elem.Context.Should().Be(contexts[contextIndex]);
                }
                else
                {
                  cache.Add(contexts[contextIndex], items[contextIndex][i, j]).Should().BeTrue();
                }
              }
            }
          }
        }

//        additionComplete = true;
      });

      /*
      var invalidationTask = Task.Run(() =>
      {
        for (var loopCount = 0; loopCount < 100; loopCount++)
        {
          for (var i = 99; i >= 0; i--)
          {
            for (var j = 99; j >= 0; j--)
            {
              foreach (var context in contexts)
              {
                // Empty contexts are ignored
                if (context.TokenCount > 0)
                {
                  context.InvalidateSubGrid(i * SubGridTreeConsts.SubGridTreeDimension, j * SubGridTreeConsts.SubGridTreeDimension, out var subGridPresentForInvalidation);
                }
              }
            }
          }
        }

        invalidationComplete = true;
      });
      */

/*      var numDesignInvalidations = 0;
      var designUpdateTask = Task.Run(async () =>
      {
        while (!additionComplete || !invalidationComplete)
        {
          // Mimic design invalidation by periodically invalidating all sub grids
          await Task.Delay(10);
          contexts[numDesignInvalidations % contexts.Length].InvalidateAllSubGrids();

          numDesignInvalidations++;
        }
      });

  */
      Task.WaitAll(new[] { additionTask /*, invalidationTask, designUpdateTask*/ });

//      numDesignInvalidations.Should().BeGreaterThan(0);
    }
  }
}
