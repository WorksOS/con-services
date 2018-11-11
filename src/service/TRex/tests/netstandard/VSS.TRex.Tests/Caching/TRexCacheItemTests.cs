using VSS.TRex.Caching;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Caching
{
  public class TRexCacheItemTests : IClassFixture<DILoggingAndStorgeProxyFixture>
  {
    [Fact]
    public void Test_TRexCacheItem_Creation_Default()
    {
      TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element> item = new TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element>();

      Assert.True(item.Item == null);
      Assert.True(item.Next == 0);
      Assert.True(item.Prev == 0);
      Assert.True(item.MRUEpochToken == 0);
    }

    [Fact]
    public void Test_TRexCacheItem_Creation_Specific()
    {
      var storage = new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(10, 5);
      var dummyCache = new TRexSpatialMemoryCacheContext(new TRexSpatialMemoryCache(1000, 1000, 0), storage);

      var theObject = new TRexSpatialMemoryCacheContextTests_Element();
      TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element> item = new TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element>(theObject, dummyCache, 100, 1, 2);

      Assert.True(ReferenceEquals(item.Item, theObject));
      Assert.True(item.Prev == 1);
      Assert.True(item.Next == 2);
      Assert.True(item.MRUEpochToken == 100);
    }

    [Fact]
    public void Test_TRexCacheItem_Set()
    {
      var theObject = new TRexSpatialMemoryCacheContextTests_Element();
      TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element> item = new TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element>();

      item.Set(theObject, null, 100, 1, 2);

      Assert.True(ReferenceEquals(item.Item, theObject));
      Assert.True(item.Prev == 1);
      Assert.True(item.Next == 2);
      Assert.True(item.MRUEpochToken == 100);
    }

    [Fact]
    public void Test_TRexCacheItem_GetPrevAndNext()
    {
      TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element> item = new TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element>();

      item.Set(null, null, 100, 1, 2);
      item.GetPrevAndNext(out int prev, out int next);

      Assert.True(prev == 1);
      Assert.True(next == 2);
    }

    [Fact]
    public void Test_TRexCacheItem_Validate()
    {
      TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element> item = new TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element>();

      item.Set(null, null, 100, 1, 2);

      Assert.False(item.Valid, "Newly created item is not valid");
      bool previousValid = item.Validate();

      Assert.False(previousValid, "previousValid not false after Validate");
      Assert.True(item.Valid, "Item still invalid after Validate");
    }

    [Fact]
    public void Test_TRexCacheItem_Invalidate()
    {
      TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element> item = new TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element>();

      item.Set(null, null, 100, 1, 2);
      item.Validate();

      Assert.True(item.Valid, "Newly created item is not valid");

      bool previousValid = item.Invalidate();

      Assert.True(previousValid, "Previous validity state not returned");
      Assert.False(item.Valid, "Item still valid after Invalidate");
    }
  }
}
