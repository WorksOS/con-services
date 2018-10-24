using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.TRex.Caching;
using VSS.TRex.SubGridTrees.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.Caching
{
  public class TRexSpatialMemoryCacheTests
  {
    [Fact]
    public void Test_TRexSpatialMemoryCacheTests_Creation()
    {
      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(100, 0.5);

      Assert.True(cache.MRUList != null, "MRUlist is null");
      Assert.Equal(0, cache.CurrentNumElements);
      Assert.Equal(0, cache.CurrentSizeInBytes);
      Assert.Equal(50, cache.MruNonUpdateableSlotCount);
      Assert.Equal(100, cache.MaxNumElements);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(10000)]
    public void Test_TRexSpatialMemoryCacheTests_ContextCreation(int numberOfContexts)
    {
      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(100, 0.5);

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
      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(100, 0.5);

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

      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(100, 0.5);

      var context = cache.LocateOrCreateContext($"fingerprint");

      context.Add(new TRexSpatialMemoryCacheContextTests_Element()
      {
        SizeInBytes = _size,
        OriginX = _originX,
        OriginY = _originY
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
      TRexSpatialMemoryCache cache = new TRexSpatialMemoryCache(numContexts, 0.5);

      // Make the number ofg contexts requested, and a separate item to be placed in each one
      var contexts = Enumerable.Range(0, numContexts).Select(x => cache.LocateOrCreateContext($"fingerprint:{x}")).ToArray();

      var items = Enumerable.Range(0, numContexts).Select(x => new TRexSpatialMemoryCacheContextTests_Element()
      {
        SizeInBytes = _size,
        OriginX = (uint)(_originX + x * SubGridTreeConsts.SubGridTreeDimension),
        OriginY = (uint)(_originY + x * SubGridTreeConsts.SubGridTreeDimension)
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
  }
}
