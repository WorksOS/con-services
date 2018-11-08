using System;
using VSS.TRex.Caching;
using Xunit;

namespace VSS.TRex.Tests.Caching
{
  public class TRexCacheItemTests
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
      var theObject = new TRexSpatialMemoryCacheContextTests_Element();
      TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element> item = new TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element>(theObject, null, 100, 1, 2);

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
      object theObject = new Object();
      TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element> item = new TRexCacheItem<TRexSpatialMemoryCacheContextTests_Element>();
      
      item.Set(null, null, 100, 1, 2);
      item.GetPrevAndNext(out int prev, out int next);

      Assert.True(prev == 1);
      Assert.True(next == 2);
    }
  }
}
