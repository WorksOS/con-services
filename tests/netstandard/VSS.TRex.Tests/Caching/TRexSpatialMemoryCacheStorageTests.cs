using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Caching;
using VSS.TRex.SubGridTrees.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.Caching
{
  public class TRexSpatialMemoryCacheStorageTests
  {
    [Fact]
    public void Test_TRexSpatialMemoryCacheStorageTests_FillWithElements()
    {
      var storage = new TRexSpatialMemoryCacheStorage<ITRexMemoryCacheItem>(100, 50);

      // Fill all available slots
      for (int i = 0; i < 100; i++)
        storage.Add(new TRexSpatialMemoryCacheContextTests_Element
        {
          SizeInBytes = 1000,
          OriginX = (uint)(2000 + i * SubGridTreeConsts.SubGridTreeDimension),
          OriginY = (uint)(3000 + i * SubGridTreeConsts.SubGridTreeDimension)
        });

      Assert.True(storage.TokenCount == 1, $"Element count incorrect (= {storage.TokenCount})");

      Assert.True(false);
    }

  }
}
