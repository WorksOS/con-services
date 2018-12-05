using System;
using VSS.TRex.Caching;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Caching
{
  public class TRexSpatialMemoryCacheContextTests : IClassFixture<DILoggingAndStorgeProxyFixture>
  {
    [Fact]
    public void Test_TRexSpatialMemoryCacheContext_Creation_Default()
    {
      ITRexSpatialMemoryCacheContext context = new TRexSpatialMemoryCacheContext(null, null);

      Assert.True(context.ContextTokens != null, "No index subgrid tree created");
      Assert.True(context.MRUList == null);
      Assert.True(context.OwnerMemoryCache == null);
      Assert.False(context.MarkedForRemoval);
      Assert.True(context.MarkedForRemovalAtUtc == DateTime.MinValue);
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheContext_Creation_Default_Sensitivity()
    {
      ITRexSpatialMemoryCacheContext context = new TRexSpatialMemoryCacheContext(null, null);

      Assert.True(context.Sensitivity == TRexSpatialMemoryCacheInvalidationSensitivity.ProductionDataIngest,
        "Default cache invalidation sensitivity is not production data ingest.");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheContext_Creation_Default_Project()
    {
      ITRexSpatialMemoryCacheContext context = new TRexSpatialMemoryCacheContext(null, null);

      Assert.True(context.ProjectUID == Guid.Empty, "Default projectUID not set.");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheContext_Creation_NonDefault_Project()
    {
      Guid _projectUID = Guid.NewGuid();

      ITRexSpatialMemoryCacheContext context = new TRexSpatialMemoryCacheContext(null, null, TimeSpan.Zero, "", _projectUID);

      Assert.True(context.ProjectUID == _projectUID, "Default projectUID not set.");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheContext_Creation_WithOwnerAndMRU()
    {
      ITRexSpatialMemoryCacheContext context =
        new TRexSpatialMemoryCacheContext(new TRexSpatialMemoryCache(100, 1000000, 0.5),
          new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(100, 50));

      Assert.True(context.ContextTokens != null, "No index subgrid tree created");
      Assert.True(context.MRUList != null, "No MRU list available");
      Assert.True(context.OwnerMemoryCache != null, "No owning memory cache available");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheContext_AddOneElement()
    {
      ITRexSpatialMemoryCacheContext context =
        new TRexSpatialMemoryCacheContext(new TRexSpatialMemoryCache(100, 1000000, 0.5),
          new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(100, 50));

      var element = new TRexSpatialMemoryCacheContextTests_Element {SizeInBytes = 1000, CacheOriginX = 2000, CacheOriginY = 3000};
      context.Add(element);

      Assert.True(context.TokenCount == 1, $"Element count incorrect (= {context.TokenCount})");
      Assert.True(context.MRUList.TokenCount == 1, $"MRU list count incorrect (= {context.MRUList.TokenCount})");

      // Check the newly added element in the context is present in the context map with a 1-based index
      int token = context.ContextTokens[element.CacheOriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel, element.CacheOriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel];
      Assert.True(token == 1, "Single newly added element does not have index of 1 present in ContextTokens");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheContext_RemoveOneElement()
    {
      ITRexSpatialMemoryCacheContext context =
        new TRexSpatialMemoryCacheContext(new TRexSpatialMemoryCache(100, 1000000, 0.5),
          new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(100, 50));

      var element = new TRexSpatialMemoryCacheContextTests_Element {SizeInBytes = 1000, CacheOriginX = 2000, CacheOriginY = 3000};
      context.OwnerMemoryCache.Add(context, element);

      Assert.True(context.TokenCount == 1, $"Element count incorrect (= {context.TokenCount})");
      Assert.True(context.MRUList.TokenCount == 1, $"MRU list count incorrect (= {context.MRUList.TokenCount})");

      // Check the newly added element in the context is present in the context map with a 1-based index
      Assert.True(context.ContextTokens[element.CacheOriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel, element.CacheOriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel] == 1,
        "Single newly added element does not have index of 1 present in ContextTokens");

      context.Remove(element);

      Assert.True(context.ContextTokens[element.CacheOriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel, element.CacheOriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel] == 0, "Removed element did not reset ContextTokens index to 0");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheContext_FailOverwriteOfExistingElement()
    {
      ITRexSpatialMemoryCacheContext context =
        new TRexSpatialMemoryCacheContext(new TRexSpatialMemoryCache(100, 1000000, 0.5),
          new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(100, 50));

      var element = new TRexSpatialMemoryCacheContextTests_Element {SizeInBytes = 1000, CacheOriginX = 2000, CacheOriginY = 3000};

      Assert.True(context.OwnerMemoryCache.Add(context, element), "Result is false on addition of first element");
      Assert.False(context.OwnerMemoryCache.Add(context, element), "Result is true on second addition of same element");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheContext_MarkForRemovalDirect()
    {
      ITRexSpatialMemoryCacheContext context =
        new TRexSpatialMemoryCacheContext(new TRexSpatialMemoryCache(100, 1000000, 0.5),
          new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(100, 50));

      Assert.False(context.MarkedForRemoval);

      var currentDate = DateTime.UtcNow;

      context.MarkForRemoval(DateTime.UtcNow);
      Assert.True(context.MarkedForRemoval, "Marking context for removal did not set state");
      Assert.True(context.MarkedForRemovalAtUtc >= currentDate && context.MarkedForRemovalAtUtc <= DateTime.UtcNow, "Marking context for removal did not set state");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheContext_MarkForRemovalViaLastElementRemoved()
    {
      ITRexSpatialMemoryCacheContext context =
        new TRexSpatialMemoryCacheContext(new TRexSpatialMemoryCache(100, 1000000, 0.5),
          new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(100, 50));

      Assert.False(context.MarkedForRemoval);

      var currentDate = DateTime.UtcNow;
      var element = new TRexSpatialMemoryCacheContextTests_Element { SizeInBytes = 1000, CacheOriginX = 2000, CacheOriginY = 3000 };

      Assert.True(context.OwnerMemoryCache.Add(context, element), "Result is false on addition of first element");
      Assert.True(!context.MarkedForRemoval && context.TokenCount == 1);

      context.Remove(element);

      Assert.True(context.MarkedForRemoval, "Removing last element in context for removal did not mark for removal");
      Assert.True(context.MarkedForRemovalAtUtc >= currentDate && context.MarkedForRemovalAtUtc <= DateTime.UtcNow, "Removal date not expected");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheContext_ReanimateDirect()
    {
      ITRexSpatialMemoryCacheContext context =
        new TRexSpatialMemoryCacheContext(new TRexSpatialMemoryCache(100, 1000000, 0.5),
          new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(100, 50));

      var element = new TRexSpatialMemoryCacheContextTests_Element {SizeInBytes = 1000, CacheOriginX = 2000, CacheOriginY = 3000};

      Assert.True(context.OwnerMemoryCache.Add(context, element), "Result is false on addition of first element");

      context.MarkForRemoval(DateTime.UtcNow);
      Assert.True(context.MarkedForRemoval, "Marking context for removal did not set state");

      context.Reanimate();
      Assert.False(context.MarkedForRemoval, "Marking context for removal did not set state");
      Assert.True(context.MarkedForRemovalAtUtc == DateTime.MinValue, "Marking context for removal did not set date");
    }

    [Fact]
    public void Test_TRexSpatialMemoryCacheContext_ReanimateViaItemAddition()
    {
      ITRexSpatialMemoryCacheContext context =
        new TRexSpatialMemoryCacheContext(new TRexSpatialMemoryCache(100, 1000000, 0.5),
          new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(100, 50));

      var element = new TRexSpatialMemoryCacheContextTests_Element { SizeInBytes = 1000, CacheOriginX = 2000, CacheOriginY = 3000 };

      context.MarkForRemoval(DateTime.UtcNow);
      Assert.True(context.MarkedForRemoval, "Marking context for removal did not set state");

      // Reanimate by adding an element
      Assert.True(context.OwnerMemoryCache.Add(context, element), "Result is false on addition of first element");

      Assert.False(context.MarkedForRemoval, "Marking context for removal did not set state");
      Assert.True(context.MarkedForRemovalAtUtc == DateTime.MinValue, "Marking context for removal did not set date");
    }
  }
}
