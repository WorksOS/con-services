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
      TRexCacheItem<object> item = new TRexCacheItem<object>();

      Assert.True(item.Item == null);
      Assert.True(item.Next == -1);
      Assert.True(item.Prev == -1);
      Assert.True(item.MRUEpochToken == -1);
    }

    [Fact]
    public void Test_TRexCacheItem_Creation_Specific()
    {
      object theObject = new Object();
      TRexCacheItem<object> item = new TRexCacheItem<object>(theObject, 0, 1, 2);

      Assert.True(ReferenceEquals(item.Item, theObject));
      Assert.True(item.Next == 1);
      Assert.True(item.Prev == 2);
      Assert.True(item.MRUEpochToken == 0);
    }
  }
}
